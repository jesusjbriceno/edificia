import { create } from 'zustand';
import type { ContentTreeNode, ProjectStatus } from '@/lib/types';
import type { SyncStatus } from '@/lib/syncManager';

// ── Sync notification bridge ─────────────────────────────
// Module-level callback set by ProjectEditor to route content changes to SyncManager.
let _syncNotify: ((sectionId: string, html: string) => void) | null = null;

/** Register a callback invoked on every content change (used by SyncManager). */
export function registerSyncNotify(
  fn: ((sectionId: string, html: string) => void) | null,
): void {
  _syncNotify = fn;
}

// ── Store ────────────────────────────────────────────────

interface EditorState {
  /** Currently loaded project ID. */
  projectId: string | null;
  /** Filtered tree rendered in sidebar. */
  tree: ContentTreeNode[];
  /** Active section being edited. */
  activeSectionId: string | null;
  /** Section content keyed by section ID (HTML strings). */
  content: Record<string, string>;

  /** Current synchronization status. */
  syncStatus: SyncStatus;
  /** Number of sections with unsaved changes. */
  pendingCount: number;

  /** Project metadata for display. */
  projectTitle: string | null;
  interventionType: number | null; // Using number to match InterventionType enum/type
  projectStatus: ProjectStatus | null;

  /** UI State */
  aiPanelOpen: boolean;
  activePath: string[];

  /** Initialize the store for a specific project with its filtered tree and content. */
  initProject: (projectId: string, tree: ContentTreeNode[], content: Record<string, string>, title: string, interventionType: number, status: ProjectStatus) => void;
  /** Update the project status in the store (after review actions). */
  setProjectStatus: (status: ProjectStatus) => void;
  /** Set the active section being edited. */
  setActiveSection: (id: string | null) => void;
  /** Toggle AI assistant panel. */
  setAiPanelOpen: (open: boolean) => void;
  /** Update content for a section (notifies SyncManager for debounced persistence). */
  updateContent: (id: string, html: string) => void;
  /** Update sync status and pending count (called by SyncManager). */
  setSyncInfo: (status: SyncStatus, pendingCount: number) => void;
  /** Reset editor state when leaving the editor. */
  reset: () => void;
}

// ── Helpers ──────────────────────────────────────────────

/** Walk the tree recursively to find the path of nodes to a specific ID. */
function findNodePath(nodes: ContentTreeNode[], id: string, path: string[] = []): string[] | null {
  for (const node of nodes) {
    const currentPath = [...path, node.title];
    if (node.id === id) return currentPath;
    const found = findNodePath(node.sections, id, currentPath);
    if (found) return found;
  }
  return null;
}

export const useEditorStore = create<EditorState>((setStore, getStore) => ({
  projectId: null,
  tree: [],
  activeSectionId: null,
  content: {},
  syncStatus: 'idle',
  pendingCount: 0,
  projectTitle: null,
  interventionType: null,
  projectStatus: null,
  aiPanelOpen: false,
  activePath: [],

  initProject: (projectId, tree, content, title, interventionType, status) => {
    setStore({
      projectId,
      tree,
      content,
      projectTitle: title,
      interventionType,
      projectStatus: status,
      activeSectionId: null,
      activePath: [],
      syncStatus: 'idle',
      pendingCount: 0,
      aiPanelOpen: false,
    });
  },

  setProjectStatus: (status) => setStore({ projectStatus: status }),

  setActiveSection: (id: string | null) => {
    const { tree } = getStore();
    const path = id ? (findNodePath(tree, id) ?? []) : [];
    setStore({ activeSectionId: id, activePath: path });
  },

  setAiPanelOpen: (open) => setStore({ aiPanelOpen: open }),

  updateContent: (id, html) => {
    setStore((state) => ({
      content: { ...state.content, [id]: html },
    }));
    // Delegate persistence to SyncManager (debounced IndexedDB + periodic API sync)
    _syncNotify?.(id, html);
  },

  setSyncInfo: (status, pendingCount) => {
    setStore({ syncStatus: status, pendingCount });
  },

  reset: () =>
    setStore({
      projectId: null,
      tree: [],
      activeSectionId: null,
      content: {},
      syncStatus: 'idle',
      pendingCount: 0,
      projectTitle: null,
      interventionType: null,
      projectStatus: null,
      aiPanelOpen: false,
      activePath: [],
    }),
}));
