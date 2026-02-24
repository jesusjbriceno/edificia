import { describe, it, expect } from 'vitest';
import { render, screen } from '@testing-library/react';
import { SyncBadge } from '@/components/Editor/SyncBadge';
import { EditorEmptyState } from '@/components/Editor/EditorEmptyState';

// ── SyncBadge ─────────────────────────────────────────────

describe('SyncBadge', () => {
  it('muestra "Sincronizando..." con role=status en estado syncing', () => {
    render(<SyncBadge status="syncing" pendingCount={0} />);
    const badge = screen.getByRole('status');
    expect(badge).toHaveTextContent('Sincronizando...');
    expect(badge).toHaveAttribute('aria-label', 'Sincronizando contenido con el servidor');
  });

  it('muestra "Sincronizado" con role=status en estado synced', () => {
    render(<SyncBadge status="synced" pendingCount={0} />);
    const badge = screen.getByRole('status');
    expect(badge).toHaveTextContent('Sincronizado');
    expect(badge).toHaveAttribute('aria-label', 'Contenido sincronizado');
  });

  it('muestra conteo de pendientes en estado modified — singular', () => {
    render(<SyncBadge status="modified" pendingCount={1} />);
    const badge = screen.getByRole('status');
    expect(badge).toHaveTextContent('1 pendiente');
    expect(badge).toHaveAttribute('aria-label', '1 cambio pendiente de sincronizar');
  });

  it('muestra conteo de pendientes en estado modified — plural', () => {
    render(<SyncBadge status="modified" pendingCount={3} />);
    const badge = screen.getByRole('status');
    expect(badge).toHaveTextContent('3 pendientes');
    expect(badge).toHaveAttribute('aria-label', '3 cambios pendientes de sincronizar');
  });

  it('muestra "Modo Local" con role=status en estado offline', () => {
    render(<SyncBadge status="offline" pendingCount={0} />);
    const badge = screen.getByRole('status');
    expect(badge).toHaveTextContent('Modo Local');
    expect(badge).toHaveAttribute('aria-label', 'Modo local: sin conexión al servidor');
  });

  it('muestra "Error" con role=alert en estado error', () => {
    render(<SyncBadge status="error" pendingCount={0} />);
    const badge = screen.getByRole('alert');
    expect(badge).toHaveTextContent('Error');
    expect(badge).toHaveAttribute('aria-label', 'Error al sincronizar el contenido');
  });

  it('muestra "Borrador Local" con role=status en estado idle (default)', () => {
    render(<SyncBadge status="idle" pendingCount={0} />);
    const badge = screen.getByRole('status');
    expect(badge).toHaveTextContent('Borrador Local');
    expect(badge).toHaveAttribute('aria-label', 'Borrador guardado localmente');
  });

  it('los iconos llevan aria-hidden para no duplicar la información auditiva', () => {
    render(<SyncBadge status="syncing" pendingCount={0} />);
    const icons = document.querySelectorAll('[aria-hidden="true"]');
    expect(icons.length).toBeGreaterThan(0);
  });
});

// ── EditorEmptyState ──────────────────────────────────────

describe('EditorEmptyState', () => {
  it('renderiza el título del editor', () => {
    render(<EditorEmptyState />);
    expect(screen.getByText('Editor de Memoria')).toBeInTheDocument();
  });

  it('renderiza el texto de guía', () => {
    render(<EditorEmptyState />);
    expect(
      screen.getByText(/Selecciona un capítulo/i),
    ).toBeInTheDocument();
  });

  it('tiene role=status con aria-label accesible', () => {
    render(<EditorEmptyState />);
    const container = screen.getByRole('status');
    expect(container).toHaveAttribute('aria-label', 'Ningún capítulo seleccionado');
  });
});
