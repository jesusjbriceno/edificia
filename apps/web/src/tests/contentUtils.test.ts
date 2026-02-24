import { describe, it, expect } from 'vitest';
import { hasContent, getLeafStats } from '@/lib/contentUtils';
import type { ContentTreeNode } from '@/lib/types';

// ─── hasContent ───────────────────────────────────────────

describe('hasContent', () => {
  it('returns false for null', () => {
    expect(hasContent(null)).toBe(false);
  });

  it('returns false for undefined', () => {
    expect(hasContent(undefined)).toBe(false);
  });

  it('returns false for empty string', () => {
    expect(hasContent('')).toBe(false);
  });

  it('returns false for a single empty paragraph', () => {
    expect(hasContent('<p></p>')).toBe(false);
  });

  it('returns false for a paragraph with only a line break', () => {
    expect(hasContent('<p><br></p>')).toBe(false);
  });

  it('returns false for whitespace-only content', () => {
    expect(hasContent('<p>   </p>')).toBe(false);
  });

  it('returns false for a self-closing br tag', () => {
    expect(hasContent('<br/>')).toBe(false);
  });

  it('returns true for a paragraph with text', () => {
    expect(hasContent('<p>Texto de prueba</p>')).toBe(true);
  });

  it('returns true for multiple paragraphs with text', () => {
    expect(hasContent('<p>Primero</p><p>Segundo</p>')).toBe(true);
  });

  it('returns true for content with nested tags', () => {
    expect(hasContent('<p><strong>Negrita</strong></p>')).toBe(true);
  });

  it('returns true for a single character', () => {
    expect(hasContent('<p>.</p>')).toBe(true);
  });
});

// ─── getLeafStats ─────────────────────────────────────────

const makeLeaf = (id: string): ContentTreeNode => ({
  id,
  title: id,
  requiresNewWork: false,
  content: null,
  sections: [],
});

const makeParent = (id: string, children: ContentTreeNode[]): ContentTreeNode => ({
  id,
  title: id,
  requiresNewWork: false,
  content: null,
  sections: children,
});

describe('getLeafStats — nodo hoja', () => {
  it('hoja con contenido → completed=1, total=1', () => {
    const node = makeLeaf('A');
    expect(getLeafStats(node, { A: '<p>Texto</p>' })).toEqual({ completed: 1, total: 1 });
  });

  it('hoja sin contenido → completed=0, total=1', () => {
    const node = makeLeaf('A');
    expect(getLeafStats(node, {})).toEqual({ completed: 0, total: 1 });
  });

  it('hoja con contenido vacío (<p></p>) → completed=0, total=1', () => {
    const node = makeLeaf('A');
    expect(getLeafStats(node, { A: '<p></p>' })).toEqual({ completed: 0, total: 1 });
  });
});

describe('getLeafStats — nodo padre', () => {
  it('todas las hojas completas → completed=total', () => {
    const tree = makeParent('P', [makeLeaf('A'), makeLeaf('B'), makeLeaf('C')]);
    const content = { A: '<p>a</p>', B: '<p>b</p>', C: '<p>c</p>' };
    expect(getLeafStats(tree, content)).toEqual({ completed: 3, total: 3 });
  });

  it('ninguna hoja completa → completed=0', () => {
    const tree = makeParent('P', [makeLeaf('A'), makeLeaf('B')]);
    expect(getLeafStats(tree, {})).toEqual({ completed: 0, total: 2 });
  });

  it('parcialmente completo → cuenta correctamente', () => {
    const tree = makeParent('P', [makeLeaf('A'), makeLeaf('B'), makeLeaf('C')]);
    const content = { A: '<p>a</p>', C: '<p>c</p>' };
    expect(getLeafStats(tree, content)).toEqual({ completed: 2, total: 3 });
  });

  it('árbol con 2 niveles de profundidad', () => {
    const tree = makeParent('R', [
      makeParent('P1', [makeLeaf('A'), makeLeaf('B')]),
      makeParent('P2', [makeLeaf('C')]),
    ]);
    const content = { A: '<p>a</p>', C: '<p>c</p>' };
    expect(getLeafStats(tree, content)).toEqual({ completed: 2, total: 3 });
  });

  it('árbol con 3 niveles de profundidad', () => {
    const tree = makeParent('R', [
      makeParent('P1', [
        makeParent('P1.1', [makeLeaf('A'), makeLeaf('B')]),
        makeLeaf('C'),
      ]),
    ]);
    const content = { A: '<p>a</p>', B: '<p>b</p>', C: '<p>c</p>' };
    expect(getLeafStats(tree, content)).toEqual({ completed: 3, total: 3 });
  });
});
