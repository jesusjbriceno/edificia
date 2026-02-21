import { describe, it, expect, vi, beforeEach, afterEach } from 'vitest';
import { render, screen, fireEvent, waitFor } from '@testing-library/react';
import EditorShell from '@/components/Editor/EditorShell';
import { useEditorStore } from '@/store/useEditorStore';

// ── Mocks ────────────────────────────────────────────────

vi.mock('@/lib/services/projectService.js', () => ({
  projectService: {
    exportDocx: vi.fn(),
  },
}));

import { projectService } from '@/lib/services/projectService.js';

// Mock TipTap — provide minimal working editor stubs
vi.mock('@tiptap/react', () => ({
  useEditor: () => ({
    commands: { setContent: vi.fn(), focus: vi.fn(), insertContent: vi.fn() },
    getHTML: () => '<p>test</p>',
    isActive: () => false,
    can: () => ({ chain: () => ({ focus: () => ({ toggleBold: () => true, toggleItalic: () => true, toggleUnderline: () => true, toggleHeading: () => true, toggleBulletList: () => true, toggleOrderedList: () => true }) }) }),
    chain: () => ({ focus: () => ({ toggleBold: () => ({ run: vi.fn() }), toggleItalic: () => ({ run: vi.fn() }), toggleUnderline: () => ({ run: vi.fn() }), toggleHeading: () => ({ run: vi.fn() }), toggleBulletList: () => ({ run: vi.fn() }), toggleOrderedList: () => ({ run: vi.fn() }) }) }),
  }),
  EditorContent: ({ editor }: { editor: unknown }) =>
    editor ? <div data-testid="editor-content" /> : null,
}));

vi.mock('@tiptap/starter-kit', () => ({ default: {} }));
vi.mock('@tiptap/extension-underline', () => ({ default: {} }));

vi.mock('@/components/Editor/EditorToolbar.js', () => ({
  EditorToolbar: () => <div data-testid="toolbar" />,
}));

vi.mock('@/components/Editor/AiAssistantPanel.js', () => ({
  default: () => null,
}));

// ── Helpers ──────────────────────────────────────────────

function setupStore() {
  const store = useEditorStore.getState();
  store.initProject(
    'proj-export-1', 
    [
      { 
        id: 'sec-1', 
        title: 'Memoria Descriptiva', 
        content: '<p>Hello</p>', 
        sections: [],
        requiresNewWork: false,
        order: 1
      }
    ], 
    { 'sec-1': '<p>Hello</p>' },
    'Proyecto de Prueba',
    0, // OBRA_NUEVA
    'Draft'
  );
  store.setActiveSection('sec-1');
}

// ── URL spies ────────────────────────────────────────────

let createObjectURLSpy: ReturnType<typeof vi.fn>;
let revokeObjectURLSpy: ReturnType<typeof vi.fn>;

beforeEach(() => {
  createObjectURLSpy = vi.fn(() => 'blob:http://localhost/fake');
  revokeObjectURLSpy = vi.fn();
  globalThis.URL.createObjectURL = createObjectURLSpy as unknown as typeof URL.createObjectURL;
  globalThis.URL.revokeObjectURL = revokeObjectURLSpy as unknown as typeof URL.revokeObjectURL;
});

afterEach(() => {
  vi.restoreAllMocks();
});

// ── Tests ────────────────────────────────────────────────

describe('Export DOCX button', () => {
  beforeEach(() => {
    vi.clearAllMocks();
    setupStore();
  });

  it('should render "Exportar" button when a section is active', () => {
    render(<EditorShell />);
    expect(screen.getByText('Exportar')).toBeInTheDocument();
  });

  it('should not render export button when no section is selected', () => {
    useEditorStore.getState().setActiveSection(null);
    render(<EditorShell />);
    expect(screen.queryByText('Exportar')).not.toBeInTheDocument();
  });

  it('should show loading state while exporting', async () => {
    // Make exportDocx hang until we resolve it
    let resolveExport!: (value: { blob: Blob; fileName: string }) => void;
    (projectService.exportDocx as ReturnType<typeof vi.fn>).mockImplementation(
      () => new Promise((resolve) => { resolveExport = resolve; }),
    );

    render(<EditorShell />);
    fireEvent.click(screen.getByText('Exportar'));

    await waitFor(() => {
      expect(screen.getByText('Exportando...')).toBeInTheDocument();
    });

    // Resolve to clean up
    resolveExport({ blob: new Blob(['test']), fileName: 'test.docx' });

    await waitFor(() => {
      expect(screen.getByText('Exportar')).toBeInTheDocument();
    });
  });

  it('should trigger file download on successful export', async () => {
    const fakeBlob = new Blob(['docx content'], { type: 'application/vnd.openxmlformats-officedocument.wordprocessingml.document' });
    (projectService.exportDocx as ReturnType<typeof vi.fn>).mockResolvedValue({
      blob: fakeBlob,
      fileName: 'memoria-test.docx',
    });

    render(<EditorShell />);
    fireEvent.click(screen.getByText('Exportar'));

    await waitFor(() => {
      expect(projectService.exportDocx).toHaveBeenCalledWith('proj-export-1');
    });

    await waitFor(() => {
      // Verify URL lifecycle (blob created for download, then revoked)
      expect(createObjectURLSpy).toHaveBeenCalledWith(fakeBlob);
      expect(revokeObjectURLSpy).toHaveBeenCalledWith('blob:http://localhost/fake');
    });
  });

  it('should recover from export errors gracefully', async () => {
    (projectService.exportDocx as ReturnType<typeof vi.fn>).mockRejectedValue(
      new Error('Network error'),
    );

    render(<EditorShell />);
    fireEvent.click(screen.getByText('Exportar'));

    // Should go to loading
    await waitFor(() => {
      expect(screen.getByText('Exportando...')).toBeInTheDocument();
    });

    // Should recover back to normal button
    await waitFor(() => {
      expect(screen.getByText('Exportar')).toBeInTheDocument();
    });
  });
});
