import { create } from 'zustand';
import type { ContentTreeNode } from '@/lib/types';
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

  /** Initialize the store for a specific project with its filtered tree and content. */
  initProject: (projectId: string, tree: ContentTreeNode[], content: Record<string, string>) => void;
  /** Set the active section being edited. */
  setActiveSection: (id: string) => void;
  /** Update content for a section (notifies SyncManager for debounced persistence). */
  updateContent: (id: string, html: string) => void;
  /** Update sync status and pending count (called by SyncManager). */
  setSyncInfo: (status: SyncStatus, pendingCount: number) => void;
  /** Reset editor state when leaving the editor. */
  reset: () => void;
}

export const useEditorStore = create<EditorState>((setStore) => ({
  projectId: null,
  tree: [],
  activeSectionId: null,
  content: {},
  syncStatus: 'idle',
  pendingCount: 0,

  initProject: (projectId, tree, content) => {
    setStore({
      projectId,
      tree,
      content,
      activeSectionId: null,
      syncStatus: 'idle',
      pendingCount: 0,
    });
  },

  setActiveSection: (id) => setStore({ activeSectionId: id }),

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
    }),
}))
