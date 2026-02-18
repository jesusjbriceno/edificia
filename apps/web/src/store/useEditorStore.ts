import { create } from 'zustand';
import { get, set } from 'idb-keyval';
import type { ContentTreeNode } from '@/lib/types';

interface EditorState {
  /** Currently loaded project ID. */
  projectId: string | null;
  /** Filtered tree rendered in sidebar. */
  tree: ContentTreeNode[];
  /** Active section being edited. */
  activeSectionId: string | null;
  /** Section content keyed by section ID (HTML strings). */
  content: Record<string, string>;
  /** Whether a save operation is in progress. */
  isSaving: boolean;

  /** Initialize the store for a specific project with its filtered tree and content. */
  initProject: (projectId: string, tree: ContentTreeNode[], content: Record<string, string>) => void;
  /** Set the active section being edited. */
  setActiveSection: (id: string) => void;
  /** Update content for a section (persists to IndexedDB). */
  updateContent: (id: string, html: string) => void;
  /** Load saved content from IndexedDB for a given project. */
  loadFromLocal: (projectId: string) => Promise<Record<string, string>>;
  /** Reset editor state when leaving the editor. */
  reset: () => void;
}

export const useEditorStore = create<EditorState>((setStore, getStore) => ({
  projectId: null,
  tree: [],
  activeSectionId: null,
  content: {},
  isSaving: false,

  initProject: (projectId, tree, content) => {
    setStore({
      projectId,
      tree,
      content,
      activeSectionId: null,
      isSaving: false,
    });
  },

  setActiveSection: (id) => setStore({ activeSectionId: id }),

  updateContent: (id, html) => {
    const { projectId } = getStore();
    setStore((state) => ({
      content: { ...state.content, [id]: html },
      isSaving: true,
    }));

    // Persist to IndexedDB with project-scoped key
    const key = projectId ? `project-${projectId}-section-${id}` : `section-${id}`;
    void set(key, html).then(() => setStore({ isSaving: false }));
  },

  loadFromLocal: async (projectId: string) => {
    // Load all sections from IndexedDB for this project
    // This is a placeholder — full implementation loads all section keys
    // For now individual sections are loaded on demand via idb-keyval `get()`
    const cached = await get<Record<string, string>>(`project-${projectId}-content`);
    return cached ?? {};
  },

  reset: () =>
    setStore({
      projectId: null,
      tree: [],
      activeSectionId: null,
      content: {},
      isSaving: false,
    }),
}))
