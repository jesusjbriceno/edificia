import { InterventionType } from '@/lib/types';
import type { ContentTreeNode, TreeFilterConfig } from '@/lib/types';

/**
 * Recursively filters a content tree based on project intervention strategy.
 *
 * Rules:
 * - If interventionType == Reform and a node has requiresNewWork: true → excluded.
 * - Extension uses the same tree as NewConstruction (no filtering).
 * - Parent nodes are kept if at least one child survives filtering.
 * - Parent nodes with requiresNewWork: true are still removed entirely for Reform.
 *
 * @param nodes  - Array of ContentTreeNode to filter.
 * @param config - Project strategy config (interventionType, isLoeRequired).
 * @returns A new filtered array (does NOT mutate the original).
 */
export function filterTree(
  nodes: ContentTreeNode[],
  config: TreeFilterConfig,
): ContentTreeNode[] {
  // Only Reform projects filter out requiresNewWork nodes
  if (config.interventionType !== InterventionType.Reform) {
    return nodes;
  }

  return nodes.reduce<ContentTreeNode[]>((acc, node) => {
    // Skip nodes that explicitly require new construction work
    if (node.requiresNewWork) {
      return acc;
    }

    // Recursively filter children
    const filteredSections = filterTree(node.sections, config);

    acc.push({
      ...node,
      sections: filteredSections,
    });

    return acc;
  }, []);
}

/**
 * Parses the raw contentTreeJson string into a typed ContentTreeNode array.
 * Returns an empty array if the JSON is null/invalid.
 */
export function parseContentTree(json: string | null): ContentTreeNode[] {
  if (!json) return [];

  try {
    const parsed = JSON.parse(json) as { chapters?: ContentTreeNode[] };
    return parsed.chapters ?? [];
  } catch {
    return [];
  }
}

/**
 * Flattens a content tree into a flat map of { [sectionId]: content }.
 * Useful for hydrating the editor store with section content.
 */
export function flattenTreeContent(
  nodes: ContentTreeNode[],
): Record<string, string> {
  const result: Record<string, string> = {};

  function walk(nodeList: ContentTreeNode[]) {
    for (const node of nodeList) {
      if (node.content) {
        result[node.id] = node.content;
      }
      if (node.sections.length > 0) {
        walk(node.sections);
      }
    }
  }

  walk(nodes);
  return result;
}
