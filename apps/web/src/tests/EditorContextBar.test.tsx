import { describe, it, expect, vi } from 'vitest';
import { render, screen, fireEvent } from '@testing-library/react';
import { EditorContextBar } from '@/components/Editor/EditorContextBar';
import type { EditorContextBarProps } from '@/components/Editor/EditorContextBar';

const defaultProps: EditorContextBarProps = {
  activePath: ['Proyecto de Ejecución', '1. Memoria', '1.1 Memoria Descriptiva'],
  syncStatus: 'synced',
  pendingCount: 0,
  isReadonly: false,
  canSubmitForReview: false,
  submittingReview: false,
  aiPanelOpen: false,
  exporting: false,
  projectId: 'proj-abc',
  onSubmitForReview: vi.fn(),
  onToggleAi: vi.fn(),
  onExport: vi.fn(),
};

function renderBar(overrides: Partial<EditorContextBarProps> = {}) {
  return render(<EditorContextBar {...defaultProps} {...overrides} />);
}

// ── Breadcrumbs ───────────────────────────────────────────

describe('EditorContextBar — breadcrumbs', () => {
  it('renderiza todos los segmentos de la ruta', () => {
    renderBar();
    expect(screen.getByText('Proyecto de Ejecución')).toBeInTheDocument();
    expect(screen.getByText('1. Memoria')).toBeInTheDocument();
    expect(screen.getByText('1.1 Memoria Descriptiva')).toBeInTheDocument();
  });

  it('el último segmento tiene aria-current="page"', () => {
    renderBar();
    const last = screen.getByText('1.1 Memoria Descriptiva');
    expect(last).toHaveAttribute('aria-current', 'page');
  });

  it('la nav tiene aria-label descriptivo', () => {
    renderBar();
    expect(screen.getByRole('navigation', { name: 'Ruta de la sección activa' })).toBeInTheDocument();
  });
});

// ── Badge Solo lectura ────────────────────────────────────

describe('EditorContextBar — modo solo lectura', () => {
  it('muestra el badge "Solo lectura" cuando isReadonly=true', () => {
    renderBar({ isReadonly: true });
    expect(screen.getByText('Solo lectura')).toBeInTheDocument();
  });

  it('no muestra el badge cuando isReadonly=false', () => {
    renderBar({ isReadonly: false });
    expect(screen.queryByText('Solo lectura')).not.toBeInTheDocument();
  });
});

// ── Botón Enviar a Revisión ───────────────────────────────

describe('EditorContextBar — Enviar a Revisión', () => {
  it('no renderiza el botón cuando canSubmitForReview=false', () => {
    renderBar({ canSubmitForReview: false });
    expect(screen.queryByRole('button', { name: /enviar/i })).not.toBeInTheDocument();
  });

  it('renderiza el botón cuando canSubmitForReview=true', () => {
    renderBar({ canSubmitForReview: true });
    expect(screen.getByRole('button', { name: /enviar memoria a revisión/i })).toBeInTheDocument();
  });

  it('llama a onSubmitForReview al hacer click', () => {
    const onSubmitForReview = vi.fn();
    renderBar({ canSubmitForReview: true, onSubmitForReview });
    fireEvent.click(screen.getByRole('button', { name: /enviar memoria a revisión/i }));
    expect(onSubmitForReview).toHaveBeenCalledOnce();
  });

  it('está deshabilitado y tiene aria-busy cuando submittingReview=true', () => {
    renderBar({ canSubmitForReview: true, submittingReview: true });
    const btn = screen.getByRole('button', { name: /enviando a revisión/i });
    expect(btn).toBeDisabled();
    expect(btn).toHaveAttribute('aria-busy', 'true');
  });
});

// ── Botón IA (toggle) ─────────────────────────────────────

describe('EditorContextBar — botón IA', () => {
  it('tiene aria-pressed=false cuando el panel está cerrado', () => {
    renderBar({ aiPanelOpen: false });
    const btn = screen.getByRole('button', { name: /abrir asistente de ia/i });
    expect(btn).toHaveAttribute('aria-pressed', 'false');
  });

  it('tiene aria-pressed=true cuando el panel está abierto', () => {
    renderBar({ aiPanelOpen: true });
    const btn = screen.getByRole('button', { name: /cerrar asistente de ia/i });
    expect(btn).toHaveAttribute('aria-pressed', 'true');
  });

  it('llama a onToggleAi al hacer click', () => {
    const onToggleAi = vi.fn();
    renderBar({ onToggleAi });
    fireEvent.click(screen.getByRole('button', { name: /abrir asistente de ia/i }));
    expect(onToggleAi).toHaveBeenCalledOnce();
  });
});

// ── Botón Exportar ────────────────────────────────────────

describe('EditorContextBar — botón Exportar', () => {
  it('está habilitado cuando hay projectId', () => {
    renderBar({ projectId: 'proj-123' });
    expect(screen.getByRole('button', { name: /exportar memoria a docx/i })).toBeEnabled();
  });

  it('está deshabilitado cuando projectId es null', () => {
    renderBar({ projectId: null });
    expect(screen.getByRole('button', { name: /exportar/i })).toBeDisabled();
  });

  it('tiene title explicativo cuando projectId es null', () => {
    renderBar({ projectId: null });
    const btn = screen.getByRole('button', { name: /exportar/i });
    expect(btn).toHaveAttribute('title');
  });

  it('llama a onExport al hacer click', () => {
    const onExport = vi.fn();
    renderBar({ onExport });
    fireEvent.click(screen.getByRole('button', { name: /exportar memoria a docx/i }));
    expect(onExport).toHaveBeenCalledOnce();
  });

  it('está deshabilitado y tiene aria-busy cuando exporting=true', () => {
    renderBar({ exporting: true });
    const btn = screen.getByRole('button', { name: /exportando documento/i });
    expect(btn).toBeDisabled();
    expect(btn).toHaveAttribute('aria-busy', 'true');
  });

  it('contiene texto sr-only al exportar para lectores de pantalla', () => {
    renderBar({ exporting: true });
    expect(screen.getByText(/Generando el documento DOCX/i)).toBeInTheDocument();
  });
});

// ── Toolbar accesible ─────────────────────────────────────

describe('EditorContextBar — toolbar ARIA', () => {
  it('el contenedor de acciones tiene role=toolbar con aria-label', () => {
    renderBar();
    expect(screen.getByRole('toolbar', { name: 'Acciones del editor' })).toBeInTheDocument();
  });
});
