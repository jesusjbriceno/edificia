import { marked } from 'marked';

function looksLikeHtml(input: string): boolean {
  return /<\/?[a-z][\s\S]*>/i.test(input);
}

export function normalizeAiContentToHtml(content: string): string {
  if (!content) return '';

  if (looksLikeHtml(content)) {
    return content;
  }

  return marked.parse(content, {
    gfm: true,
    breaks: true,
    async: false,
  }) as string;
}
