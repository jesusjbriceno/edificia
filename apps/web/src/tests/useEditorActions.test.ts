import { describe, it, expect, vi, beforeEach, afterEach } from 'vitest';
import { renderHook, act, waitFor } from '@testing-library/react';
import { useEditorActions } from '@/lib/hooks/useEditorActions';
import { useEditorStore } from '@/store/useEditorStore';

// ── Mocks de dependencias externas ───────────────────────

vi.mock('@/lib/services/projectService.js', () => ({
  projectService: {
    exportDocx: vi.fn(),
    submitForReview: vi.fn(),
  },
}));

vi.mock('@/store/useToastStore', () => ({
  toast: {
    success: vi.fn(),
    error: vi.fn(),
  },
}));

import { projectService } from '@/lib/services/projectService.js';
import { toast } from '@/store/useToastStore';

// ── Mock editor TipTap mínimo ─────────────────────────────

const mockEditor = {
  commands: {
    setContent: vi.fn(),
    focus: vi.fn(),
    insertContent: vi.fn(),
  },
  getHTML: vi.fn(() => '<p>contenido</p>'),
};

// ── Helpers ───────────────────────────────────────────────

function setupStore(status = 'Draft') {
  useEditorStore.getState().initProject(
    'proj-actions-1',
    [],
    { 'sec-1': '<p>Hola</p>' },
    'Test Project',
    0,
    status,
  );
  useEditorStore.getState().setActiveSection('sec-1');
}

let createObjectURLSpy: ReturnType<typeof vi.fn>;
let revokeObjectURLSpy: ReturnType<typeof vi.fn>;
let clickSpy: ReturnType<typeof vi.fn>;

// Anchor real del DOM para que renderHook no se rompa
let fakeAnchor: HTMLAnchorElement;

beforeEach(() => {
  vi.clearAllMocks();
  setupStore();

  createObjectURLSpy = vi.fn(() => 'blob:fake');
  revokeObjectURLSpy = vi.fn();
  globalThis.URL.createObjectURL = createObjectURLSpy as unknown as typeof URL.createObjectURL;
  globalThis.URL.revokeObjectURL = revokeObjectURLSpy as unknown as typeof URL.revokeObjectURL;

  clickSpy = vi.fn();

  // Solo interceptar <a>, dejar que el resto de createElement funcione normalmente
  const originalCreateElement = document.createElement.bind(document);
  vi.spyOn(document, 'createElement').mockImplementation((tagName: string, ...args) => {
    if (tagName === 'a') {
      fakeAnchor = originalCreateElement('a');
      fakeAnchor.click = clickSpy;
      return fakeAnchor;
    }
    return originalCreateElement(tagName, ...args as [ElementCreationOptions?]);
  });

  vi.spyOn(document.body, 'appendChild').mockImplementation((node) => node);
  vi.spyOn(document.body, 'removeChild').mockImplementation((node) => node);
});

afterEach(() => {
  vi.restoreAllMocks();
});

// ── Derivados de estado ───────────────────────────────────

describe('useEditorActions — estado derivado', () => {
  it('canSubmitForReview=true cuando el estado es Draft', () => {
    setupStore('Draft');
    const { result } = renderHook(() => useEditorActions(null));
    expect(result.current.canSubmitForReview).toBe(true);
  });

  it('canSubmitForReview=true cuando el estado es InProgress', () => {
    setupStore('InProgress');
    const { result } = renderHook(() => useEditorActions(null));
    expect(result.current.canSubmitForReview).toBe(true);
  });

  it('canSubmitForReview=false cuando el estado es PendingReview', () => {
    setupStore('PendingReview');
    const { result } = renderHook(() => useEditorActions(null));
    expect(result.current.canSubmitForReview).toBe(false);
  });

  it('isReadonly=true cuando el estado es PendingReview', () => {
    setupStore('PendingReview');
    const { result } = renderHook(() => useEditorActions(null));
    expect(result.current.isReadonly).toBe(true);
  });

  it('isReadonly=false cuando el estado es Draft', () => {
    setupStore('Draft');
    const { result } = renderHook(() => useEditorActions(null));
    expect(result.current.isReadonly).toBe(false);
  });
});

// ── handleExport ──────────────────────────────────────────

describe('useEditorActions — handleExport', () => {
  it('llama a projectService.exportDocx y dispara la descarga', async () => {
    const mockBlob = new Blob(['content']);
    vi.mocked(projectService.exportDocx).mockResolvedValueOnce({
      blob: mockBlob,
      fileName: 'memoria.docx',
    });

    const { result } = renderHook(() => useEditorActions(null));

    await act(async () => {
      await result.current.handleExport();
    });

    expect(projectService.exportDocx).toHaveBeenCalledWith('proj-actions-1');
    expect(createObjectURLSpy).toHaveBeenCalledWith(mockBlob);
    expect(clickSpy).toHaveBeenCalled();
    expect(revokeObjectURLSpy).toHaveBeenCalled();
    expect(toast.success).toHaveBeenCalledWith(expect.stringContaining('memoria.docx'));
  });

  it('muestra toast.error si la exportación falla', async () => {
    vi.mocked(projectService.exportDocx).mockRejectedValueOnce(new Error('Network error'));

    const { result } = renderHook(() => useEditorActions(null));

    await act(async () => {
      await result.current.handleExport();
    });

    expect(toast.error).toHaveBeenCalledWith(expect.stringContaining('exportar'));
  });

  it('no llama a la API si projectId es null', async () => {
    useEditorStore.getState().initProject(null as unknown as string, [], {}, 'Sin ID', 0, 'Draft');

    const { result } = renderHook(() => useEditorActions(null));

    await act(async () => {
      await result.current.handleExport();
    });

    expect(projectService.exportDocx).not.toHaveBeenCalled();
  });

  it('no hace doble envío si exporting ya es true', async () => {
    let resolveExport!: (v: { blob: Blob; fileName: string }) => void;
    vi.mocked(projectService.exportDocx).mockImplementation(
      () => new Promise((resolve) => { resolveExport = resolve; }),
    );

    const { result } = renderHook(() => useEditorActions(null));

    // Primera llamada: inicia la exportación
    act(() => { void result.current.handleExport(); });

    // Esperar hasta que exporting sea true para que la guarda actúe
    await waitFor(() => expect(result.current.exporting).toBe(true));

    // Segunda llamada: debe ser ignorada por la guarda
    await act(async () => {
      await result.current.handleExport();
    });

    expect(projectService.exportDocx).toHaveBeenCalledTimes(1);

    // Limpiar la promesa pendiente
    resolveExport({ blob: new Blob(), fileName: 'doc.docx' });
  });
});

// ── handleSubmitForReview ─────────────────────────────────

describe('useEditorActions — handleSubmitForReview', () => {
  it('llama a projectService.submitForReview y actualiza el estado', async () => {
    vi.mocked(projectService.submitForReview).mockResolvedValueOnce(undefined);

    const { result } = renderHook(() => useEditorActions(null));

    await act(async () => {
      await result.current.handleSubmitForReview();
    });

    expect(projectService.submitForReview).toHaveBeenCalledWith('proj-actions-1');
    expect(toast.success).toHaveBeenCalledWith(expect.stringContaining('revisión'));
    // El estado debe haber cambiado a PendingReview
    expect(useEditorStore.getState().projectStatus).toBe('PendingReview');
  });

  it('muestra toast.error si el envío falla', async () => {
    vi.mocked(projectService.submitForReview).mockRejectedValueOnce(new Error('API error'));

    const { result } = renderHook(() => useEditorActions(null));

    await act(async () => {
      await result.current.handleSubmitForReview();
    });

    expect(toast.error).toHaveBeenCalledWith(expect.stringContaining('revisión'));
  });

  it('no hace nada si canSubmitForReview=false (estado PendingReview)', async () => {
    setupStore('PendingReview');

    const { result } = renderHook(() => useEditorActions(null));

    await act(async () => {
      await result.current.handleSubmitForReview();
    });

    expect(projectService.submitForReview).not.toHaveBeenCalled();
  });
});

// ── handleAiInsertContent ─────────────────────────────────

describe('useEditorActions — handleAiInsertContent', () => {
  it('reemplaza el contenido del editor en modo replace', () => {
    const { result } = renderHook(() =>
      useEditorActions(mockEditor as unknown as Parameters<typeof useEditorActions>[0]),
    );

    act(() => {
      result.current.handleAiInsertContent('<p>Nuevo contenido</p>', 'replace');
    });

    expect(mockEditor.commands.setContent).toHaveBeenCalledWith('<p>Nuevo contenido</p>');
  });

  it('añade al final del editor en modo append', () => {
    const { result } = renderHook(() =>
      useEditorActions(mockEditor as unknown as Parameters<typeof useEditorActions>[0]),
    );

    act(() => {
      result.current.handleAiInsertContent('<p>Añadido</p>', 'append');
    });

    expect(mockEditor.commands.focus).toHaveBeenCalledWith('end');
    expect(mockEditor.commands.insertContent).toHaveBeenCalledWith('<p>Añadido</p>');
  });

  it('no hace nada si el editor es null', () => {
    const { result } = renderHook(() => useEditorActions(null));

    act(() => {
      result.current.handleAiInsertContent('<p>Texto</p>', 'replace');
    });

    expect(mockEditor.commands.setContent).not.toHaveBeenCalled();
  });
});
