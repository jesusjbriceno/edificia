import { describe, it, expect, beforeEach, afterEach } from 'vitest';
import { render, screen } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import SidebarNavigation from '@/components/Editor/SidebarNavigation';
import { useEditorStore } from '@/store/useEditorStore';
import type { ContentTreeNode } from '@/lib/types';

// ─── Fixtures ────────────────────────────────────────────

const LEAF_A: ContentTreeNode = {
  id: 'MD.01',
  title: 'Agentes intervinientes',
  requiresNewWork: false,
  content: null,
  sections: [],
};

const LEAF_B: ContentTreeNode = {
  id: 'MD.02',
  title: 'Información previa',
  requiresNewWork: false,
  content: null,
  sections: [],
};

const LEAF_C: ContentTreeNode = {
  id: 'MC.01',
  title: 'Sustentación del edificio',
  requiresNewWork: false,
  content: null,
  sections: [],
};

const PARENT_MD: ContentTreeNode = {
  id: 'MD',
  title: 'Memoria Descriptiva',
  requiresNewWork: false,
  content: null,
  sections: [LEAF_A, LEAF_B],
};

const PARENT_MC: ContentTreeNode = {
  id: 'MC',
  title: 'Memoria Constructiva',
  requiresNewWork: false,
  content: null,
  sections: [LEAF_C],
};

const TREE = [PARENT_MD, PARENT_MC];

// ─── Setup ───────────────────────────────────────────────

function setup(content: Record<string, string> = {}) {
  useEditorStore.getState().initProject('proj-test', TREE, content, 'Test Project', 0, 'Draft');
  return render(<SidebarNavigation tree={TREE} />);
}

beforeEach(() => {
  useEditorStore.getState().reset();
});

afterEach(() => {
  useEditorStore.getState().reset();
});

// ─── Rendering ───────────────────────────────────────────

describe('SidebarNavigation — renderizado', () => {
  it('muestra todos los títulos del árbol', () => {
    setup();
    expect(screen.getByText('Memoria Descriptiva')).toBeInTheDocument();
    expect(screen.getByText('Memoria Constructiva')).toBeInTheDocument();
    expect(screen.getByText('Agentes intervinientes')).toBeInTheDocument();
    expect(screen.getByText('Información previa')).toBeInTheDocument();
    expect(screen.getByText('Sustentación del edificio')).toBeInTheDocument();
  });

  it('muestra el buscador de apartados', () => {
    setup();
    expect(screen.getByPlaceholderText(/buscar apartado/i)).toBeInTheDocument();
  });
});

// ─── Indicadores de hojas ─────────────────────────────────

describe('SidebarNavigation — nodo hoja', () => {
  it('nodo hoja sin contenido no tiene clase de completado', () => {
    setup({});
    const btn = screen.getByRole('button', { name: /agentes intervinientes/i });
    expect(btn.className).not.toContain('text-white/80');
  });

  it('nodo hoja con contenido tiene clase text-white/80', () => {
    setup({ 'MD.01': '<p>Contenido real</p>' });
    const btn = screen.getByRole('button', { name: /agentes intervinientes/i });
    expect(btn.className).toContain('text-white/80');
  });

  it('nodo hoja con HTML vacío (<p></p>) no tiene clase de completado', () => {
    setup({ 'MD.01': '<p></p>' });
    const btn = screen.getByRole('button', { name: /agentes intervinientes/i });
    expect(btn.className).not.toContain('text-white/80');
  });

  it('otro nodo hoja sin contenido no se ve afectado por el contenido de un vecino', () => {
    setup({ 'MD.01': '<p>Contenido</p>' });
    const btn = screen.getByRole('button', { name: /información previa/i });
    expect(btn.className).not.toContain('text-white/80');
  });
});

// ─── Barra de progreso en nodos padre ────────────────────

describe('SidebarNavigation — progress bar nodo padre', () => {
  it('muestra 0/2 cuando ninguna hoja tiene contenido', () => {
    setup({});
    expect(screen.getByText('0/2')).toBeInTheDocument();
  });

  it('muestra 1/2 cuando una de dos hojas tiene contenido', () => {
    setup({ 'MD.01': '<p>Texto</p>' });
    expect(screen.getByText('1/2')).toBeInTheDocument();
  });

  it('muestra 2/2 cuando todas las hojas tienen contenido', () => {
    setup({ 'MD.01': '<p>Texto A</p>', 'MD.02': '<p>Texto B</p>' });
    expect(screen.getByText('2/2')).toBeInTheDocument();
  });

  it('varios nodos padre muestran sus contadores independientes', () => {
    setup({ 'MD.01': '<p>Texto</p>', 'MC.01': '<p>Texto</p>' });
    // MD tiene 1/2, MC tiene 1/1
    expect(screen.getByText('1/2')).toBeInTheDocument();
    expect(screen.getByText('1/1')).toBeInTheDocument();
  });

  it('contador de padre completo tiene clase text-emerald-400', () => {
    setup({ 'MC.01': '<p>Texto C</p>' });
    const counter = screen.getByText('1/1');
    expect(counter.className).toContain('text-emerald-400');
  });

  it('contador de padre parcial tiene clase text-gray-500', () => {
    setup({ 'MD.01': '<p>Texto</p>' });
    const counter = screen.getByText('1/2');
    expect(counter.className).toContain('text-gray-500');
  });
});

// ─── Búsqueda ────────────────────────────────────────────

describe('SidebarNavigation — búsqueda', () => {
  it('filtra las secciones por título', async () => {
    const user = userEvent.setup();
    setup();
    await user.type(screen.getByPlaceholderText(/buscar apartado/i), 'Agentes');
    expect(screen.getByText('Agentes intervinientes')).toBeInTheDocument();
    expect(screen.queryByText('Información previa')).not.toBeInTheDocument();
  });

  it('muestra mensaje vacío cuando no hay resultados', async () => {
    const user = userEvent.setup();
    setup();
    await user.type(screen.getByPlaceholderText(/buscar apartado/i), 'zzznomatch');
    expect(screen.getByText(/no se han encontrado apartados/i)).toBeInTheDocument();
  });
});

// ─── Interacción ─────────────────────────────────────────

describe('SidebarNavigation — interacción', () => {
  it('llamar a setActiveSection al hacer clic en una hoja', async () => {
    const user = userEvent.setup();
    setup();
    await user.click(screen.getByRole('button', { name: /agentes intervinientes/i }));
    expect(useEditorStore.getState().activeSectionId).toBe('MD.01');
  });

  it('el nodo activo recibe clase bg-brand-primary/10', async () => {
    const user = userEvent.setup();
    setup();
    await user.click(screen.getByRole('button', { name: /agentes intervinientes/i }));
    const btn = screen.getByRole('button', { name: /agentes intervinientes/i });
    expect(btn.className).toContain('bg-brand-primary/10');
  });
});
