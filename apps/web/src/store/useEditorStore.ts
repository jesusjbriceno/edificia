import { create } from 'zustand';
import { get, set } from 'idb-keyval';

interface EditorState {
  activeSectionId: string | null;
  content: Record<string, string>;
  isSaving: boolean;
  setActiveSection: (id: string) => void;
  updateContent: (id: string, html: string) => void;
  loadFromLocal: () => Promise<void>;
}

export const useEditorStore = create<EditorState>((setStore, getStore) => ({
  activeSectionId: null,
  content: {},
  isSaving: false,

  setActiveSection: (id) => setStore({ activeSectionId: id }),

  updateContent: async (id, html) => {
    setStore((state) => ({
      content: { ...state.content, [id]: html },
      isSaving: true,
    }));

    // Auto-save persistente en IndexedDB
    await set(`section-${id}`, html);
    
    setStore({ isSaving: false });
  },

  loadFromLocal: async () => {
    // Aquí se cargaría el contenido inicial de IndexedDB si fuera necesario
    // Por ahora es un placeholder para la lógica de persistencia
  }
}));
