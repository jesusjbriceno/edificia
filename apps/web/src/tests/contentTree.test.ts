import { describe, it, expect } from 'vitest';
import { filterTree, parseContentTree, flattenTreeContent } from '@/lib/contentTree';
import { InterventionType } from '@/lib/types';
import type { ContentTreeNode, TreeFilterConfig } from '@/lib/types';

// ─── Test Data ───────────────────────────────────────────

const sampleTree: ContentTreeNode[] = [
  {
    id: 'MD',
    title: 'Memoria Descriptiva',
    requiresNewWork: false,
    content: null,
    sections: [
      {
        id: 'MD.01',
        title: 'Agentes intervinientes',
        requiresNewWork: false,
        content: '<p>Promotor: Test</p>',
        sections: [],
      },
      {
        id: 'MD.02',
        title: 'Información previa',
        requiresNewWork: false,
        content: null,
        sections: [],
      },
    ],
  },
  {
    id: 'MC',
    title: 'Memoria Constructiva',
    requiresNewWork: false,
    content: null,
    sections: [
      {
        id: 'MC.01',
        title: 'Sustentación del edificio',
        requiresNewWork: true,
        content: '<p>Geotécnico</p>',
        sections: [
          {
            id: 'MC.01.01',
            title: 'Estudio geotécnico',
            requiresNewWork: true,
            content: null,
            sections: [],
          },
        ],
      },
      {
        id: 'MC.02',
        title: 'Sistema envolvente',
        requiresNewWork: false,
        content: null,
        sections: [],
      },
    ],
  },
  {
    id: 'CN',
    title: 'Cumplimiento del CTE',
    requiresNewWork: false,
    content: null,
    sections: [
      {
        id: 'CN.01',
        title: 'DB-SE Seguridad Estructural',
        requiresNewWork: true,
        content: null,
        sections: [],
      },
      {
        id: 'CN.02',
        title: 'DB-SI Seguridad en caso de incendio',
        requiresNewWork: false,
        content: '<p>SI content</p>',
        sections: [],
      },
    ],
  },
];

// ─── filterTree Tests ────────────────────────────────────

describe('filterTree', () => {
  it('returns all nodes for NewConstruction', () => {
    const config: TreeFilterConfig = {
      interventionType: InterventionType.NewConstruction,
      isLoeRequired: true,
    };

    const result = filterTree(sampleTree, config);

    expect(result).toEqual(sampleTree);
    expect(result).toHaveLength(3);
  });

  it('returns all nodes for Extension (same as NewConstruction)', () => {
    const config: TreeFilterConfig = {
      interventionType: InterventionType.Extension,
      isLoeRequired: true,
    };

    const result = filterTree(sampleTree, config);

    expect(result).toEqual(sampleTree);
  });

  it('filters out requiresNewWork nodes for Reform', () => {
    const config: TreeFilterConfig = {
      interventionType: InterventionType.Reform,
      isLoeRequired: false,
    };

    const result = filterTree(sampleTree, config);

    // MD stays (requiresNewWork: false)
    expect(result).toHaveLength(3);

    // MC.01 (Sustentación, requiresNewWork: true) should be removed
    const mc = result.find((n) => n.id === 'MC');
    expect(mc).toBeDefined();
    expect(mc!.sections).toHaveLength(1);
    expect(mc!.sections[0].id).toBe('MC.02');

    // CN.01 (DB-SE, requiresNewWork: true) should be removed
    const cn = result.find((n) => n.id === 'CN');
    expect(cn).toBeDefined();
    expect(cn!.sections).toHaveLength(1);
    expect(cn!.sections[0].id).toBe('CN.02');
  });

  it('removes top-level nodes if requiresNewWork is true', () => {
    const topLevelNewWork: ContentTreeNode[] = [
      {
        id: 'STRUCT',
        title: 'Estructura',
        requiresNewWork: true,
        content: null,
        sections: [],
      },
      {
        id: 'DESC',
        title: 'Descripción',
        requiresNewWork: false,
        content: null,
        sections: [],
      },
    ];

    const config: TreeFilterConfig = {
      interventionType: InterventionType.Reform,
      isLoeRequired: false,
    };

    const result = filterTree(topLevelNewWork, config);

    expect(result).toHaveLength(1);
    expect(result[0].id).toBe('DESC');
  });

  it('handles empty tree', () => {
    const config: TreeFilterConfig = {
      interventionType: InterventionType.Reform,
      isLoeRequired: false,
    };

    const result = filterTree([], config);

    expect(result).toEqual([]);
  });

  it('does not mutate the original tree', () => {
    const config: TreeFilterConfig = {
      interventionType: InterventionType.Reform,
      isLoeRequired: false,
    };

    const original = JSON.parse(JSON.stringify(sampleTree));
    filterTree(sampleTree, config);

    expect(sampleTree).toEqual(original);
  });
});

// ─── parseContentTree Tests ──────────────────────────────

describe('parseContentTree', () => {
  it('parses valid JSON with chapters', () => {
    const json = JSON.stringify({ chapters: sampleTree });
    const result = parseContentTree(json);

    expect(result).toHaveLength(3);
    expect(result[0].id).toBe('MD');
  });

  it('returns empty array for null input', () => {
    expect(parseContentTree(null)).toEqual([]);
  });

  it('returns empty array for invalid JSON', () => {
    expect(parseContentTree('{invalid')).toEqual([]);
  });

  it('returns empty array if chapters key is missing', () => {
    expect(parseContentTree('{"data": []}')).toEqual([]);
  });

  it('returns empty array for empty string', () => {
    expect(parseContentTree('')).toEqual([]);
  });
});

// ─── flattenTreeContent Tests ────────────────────────────

describe('flattenTreeContent', () => {
  it('extracts content from all nodes with content', () => {
    const result = flattenTreeContent(sampleTree);

    expect(result['MD.01']).toBe('<p>Promotor: Test</p>');
    expect(result['MC.01']).toBe('<p>Geotécnico</p>');
    expect(result['CN.02']).toBe('<p>SI content</p>');
  });

  it('skips nodes with null content', () => {
    const result = flattenTreeContent(sampleTree);

    expect(result).not.toHaveProperty('MD');
    expect(result).not.toHaveProperty('MD.02');
    expect(result).not.toHaveProperty('MC');
    expect(result).not.toHaveProperty('CN.01');
  });

  it('returns empty object for empty tree', () => {
    expect(flattenTreeContent([])).toEqual({});
  });

  it('extracts content count correctly', () => {
    const result = flattenTreeContent(sampleTree);

    expect(Object.keys(result)).toHaveLength(3);
  });
});
