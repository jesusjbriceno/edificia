/**
 * Utilities for determining whether a section contains meaningful content.
 * Used to compute completion indicators in SidebarNavigation.
 */

/**
 * Returns `true` if the given HTML string contains visible, non-empty text.
 *
 * TipTap emits `<p></p>` or `<p><br></p>` for empty sections, which should
 * be treated as "no content". Any string with actual text after stripping
 * HTML tags and trimming whitespace is considered filled.
 *
 * @param html - Raw HTML string from the editor (may be undefined/null).
 */
export function hasContent(html: string | null | undefined): boolean {
  if (!html) return false;
  // Strip all HTML tags and collapse whitespace
  const text = html.replaceAll(/<[^>]*>/g, '').trim();
  return text.length > 0;
}

/**
 * Counts completed and total leaf nodes within a tree node recursively.
 * A leaf is a node with no `sections` children.
 * A leaf is "completed" if `hasContent(content[node.id])` is true.
 *
 * @param node    - The tree node to traverse.
 * @param content - The content map from the editor store.
 * @returns `{ completed, total }` counts of leaf nodes.
 */
export function getLeafStats(
  node: { id: string; sections: { id: string; sections: unknown[] }[] },
  content: Record<string, string>,
): { completed: number; total: number } {
  if (node.sections.length === 0) {
    // Leaf node
    return { completed: hasContent(content[node.id]) ? 1 : 0, total: 1 };
  }

  return node.sections.reduce(
    (acc, child) => {
      const childStats = getLeafStats(
        child as { id: string; sections: { id: string; sections: unknown[] }[] },
        content,
      );
      return {
        completed: acc.completed + childStats.completed,
        total: acc.total + childStats.total,
      };
    },
    { completed: 0, total: 0 },
  );
}
