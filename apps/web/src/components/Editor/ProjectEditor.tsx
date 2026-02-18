import { useEffect, useState } from 'react';
import { Loader2, AlertTriangle } from 'lucide-react';
import { projectService } from '@/lib/services';
import { filterTree, parseContentTree, flattenTreeContent } from '@/lib/contentTree';
import { useEditorStore } from '@/store/useEditorStore';
import { InterventionType } from '@/lib/types';
import type { ContentTreeNode, TreeFilterConfig, ProjectResponse } from '@/lib/types';
import SidebarNavigation from './SidebarNavigation';
import EditorShell from './EditorShell';

interface ProjectEditorProps {
  projectId: string;
}

/** Fetch the CTE 2024 template from the static public folder. */
async function loadCteTemplate(): Promise<ContentTreeNode[]> {
  const res = await fetch('/normativa/cte_2024.json');
  if (!res.ok) throw new Error('No se pudo cargar la plantilla CTE 2024.');
  const data = (await res.json()) as { chapters: ContentTreeNode[] };
  return data.chapters;
}

/**
 * Container component that orchestrates:
 * 1. Fetches project + content tree from API
 * 2. Falls back to cte_2024.json if project has no tree
 * 3. Applies filterTree based on intervention strategy
 * 4. Initializes the editor Zustand store
 * 5. Renders Sidebar + EditorShell
 */
export default function ProjectEditor({ projectId }: Readonly<ProjectEditorProps>) {
  const [project, setProject] = useState<ProjectResponse | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const initProject = useEditorStore((s) => s.initProject);
  const tree = useEditorStore((s) => s.tree);

  useEffect(() => {
    let cancelled = false;

    async function load() {
      setLoading(true);
      setError(null);

      try {
        // Fetch project metadata and content tree in parallel
        const [projectData, treeData] = await Promise.all([
          projectService.getById(projectId),
          projectService.getContentTree(projectId),
        ]);

        if (cancelled) return;

        setProject(projectData);

        // Parse the content tree — use API data or fallback to CTE template
        let chapters: ContentTreeNode[];
        if (treeData.contentTreeJson) {
          chapters = parseContentTree(treeData.contentTreeJson);
        } else {
          // Project doesn't have a tree yet → use CTE 2024 template
          chapters = await loadCteTemplate();

          // Initialize the project tree in the backend with the template
          const templateJson = JSON.stringify({ chapters });
          await projectService.updateContentTree(projectId, {
            contentTreeJson: templateJson,
          });
        }

        // Determine intervention type for filtering
        const interventionTypeMap: Record<string, InterventionType> = {
          NewConstruction: InterventionType.NewConstruction,
          Reform: InterventionType.Reform,
          Extension: InterventionType.Extension,
        };

        const config: TreeFilterConfig = {
          interventionType:
            interventionTypeMap[treeData.interventionType] ??
            InterventionType.NewConstruction,
          isLoeRequired: treeData.isLoeRequired,
        };

        // Apply filtering based on project strategy
        const filteredTree = filterTree(chapters, config);

        // Extract existing content from the tree nodes
        const existingContent = flattenTreeContent(chapters);

        // Initialize the editor store
        initProject(projectId, filteredTree, existingContent);
      } catch (err) {
        if (!cancelled) {
          setError(
            err instanceof Error
              ? err.message
              : 'Error al cargar el proyecto.',
          );
        }
      } finally {
        if (!cancelled) setLoading(false);
      }
    }

    load();
    return () => {
      cancelled = true;
    };
  }, [projectId]);

  if (loading) {
    return (
      <div className="flex-1 flex items-center justify-center bg-dark-bg/20">
        <div className="text-center space-y-4">
          <Loader2 size={40} className="animate-spin text-brand-primary mx-auto" />
          <p className="text-sm text-gray-400">Cargando proyecto...</p>
        </div>
      </div>
    );
  }

  if (error) {
    return (
      <div className="flex-1 flex items-center justify-center bg-dark-bg/20">
        <div className="text-center space-y-4 max-w-sm">
          <AlertTriangle size={40} className="text-red-400 mx-auto" />
          <p className="text-sm text-red-300">{error}</p>
          <button
            onClick={() => globalThis.location.reload()}
            className="px-4 py-2 text-sm bg-white/10 hover:bg-white/20 text-white rounded-lg transition-colors"
          >
            Reintentar
          </button>
        </div>
      </div>
    );
  }

  return (
    <>
      <SidebarNavigation tree={tree} />
      <EditorShell projectTitle={project?.title} />
    </>
  );
}
