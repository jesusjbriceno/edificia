import { describe, expect, it } from 'vitest';
import { normalizeAiContentToHtml } from '@/lib/normalizeAiContent';
import { sanitizeRichHtml } from '@/lib/sanitizeHtml';

describe('AI Markdown pipeline', () => {
  it('convierte markdown enriquecido y preserva estructura útil de memoria', () => {
    const markdown = `# Memoria técnica

| Norma | Aplicación |
| --- | --- |
| CTE DB-SI | Evacuación |

Consultar [CTE](https://www.codigotecnico.org).

Fórmula térmica: $q = m · c · ΔT$`;

    const html = normalizeAiContentToHtml(markdown);
    const sanitized = sanitizeRichHtml(html);

    expect(sanitized).toContain('<table>');
    expect(sanitized).toContain('https://www.codigotecnico.org');
    expect(sanitized).toContain('$q = m · c · ΔT$');
  });

  it('elimina payloads XSS mezclados en markdown/html', () => {
    const mixed = `Texto previo\n\n<img src=x onerror=alert(1)>\n\n[malicioso](javascript:alert(1))\n\n<script>alert('xss')</script>`;

    const html = normalizeAiContentToHtml(mixed);
    const sanitized = sanitizeRichHtml(html);

    expect(sanitized).not.toContain('onerror');
    expect(sanitized).not.toContain('<script>');
    expect(sanitized).not.toContain('href="javascript:');
    expect(sanitized).toContain('Texto previo');
  });
});
