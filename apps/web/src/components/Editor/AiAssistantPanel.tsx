import { useState, useCallback } from 'react';
import { Sparkles, X, Loader2, Send, RotateCcw, ArrowDownToLine, AlertTriangle } from 'lucide-react';
import { Button } from '@/components/ui/Button';
import { aiService } from '@/lib/services/aiService.js';
import { useEditorStore } from '@/store/useEditorStore';

// ── Prompt Suggestions ───────────────────────────────────

const PROMPT_SUGGESTIONS = [
  'Redacta este apartado según la normativa CTE vigente.',
  'Amplía el contenido existente con más detalle técnico.',
  'Genera la justificación normativa para este capítulo.',
  'Redacta la descripción constructiva de esta sección.',
] as const;

// ── Types ────────────────────────────────────────────────

type AiPhase = 'idle' | 'generating' | 'preview' | 'error';

interface AiAssistantPanelProps {
  readonly isOpen: boolean;
  readonly onClose: () => void;
  readonly onInsertContent: (html: string, mode: 'replace' | 'append') => void;
  readonly sectionTitle?: string;
}

// ── Component ────────────────────────────────────────────

export default function AiAssistantPanel({
  isOpen,
  onClose,
  onInsertContent,
  sectionTitle,
}: AiAssistantPanelProps) {
  const [prompt, setPrompt] = useState('');
  const [phase, setPhase] = useState<AiPhase>('idle');
  const [generatedHtml, setGeneratedHtml] = useState('');
  const [errorMsg, setErrorMsg] = useState('');

  const projectId = useEditorStore((s) => s.projectId);
  const activeSectionId = useEditorStore((s) => s.activeSectionId);
  const content = useEditorStore((s) => s.content);

  const handleGenerate = useCallback(async () => {
    if (!projectId || !activeSectionId || !prompt.trim()) return;

    setPhase('generating');
    setErrorMsg('');
    setGeneratedHtml('');

    try {
      const existingContent = content[activeSectionId] || '';
      const response = await aiService.generate(projectId, {
        sectionId: activeSectionId,
        prompt: prompt.trim(),
        context: existingContent || null,
      });

      setGeneratedHtml(response.generatedText);
      setPhase('preview');
    } catch (err) {
      setErrorMsg(
        err instanceof Error ? err.message : 'Error al generar contenido con IA.',
      );
      setPhase('error');
    }
  }, [projectId, activeSectionId, prompt, content]);

  const handleInsert = useCallback(
    (mode: 'replace' | 'append') => {
      if (!generatedHtml) return;
      onInsertContent(generatedHtml, mode);
      // Reset panel state
      setPhase('idle');
      setGeneratedHtml('');
      setPrompt('');
      onClose();
    },
    [generatedHtml, onInsertContent, onClose],
  );

  const handleRetry = useCallback(() => {
    setPhase('idle');
    setGeneratedHtml('');
    setErrorMsg('');
  }, []);

  const handleClose = useCallback(() => {
    setPhase('idle');
    setGeneratedHtml('');
    setPrompt('');
    setErrorMsg('');
    onClose();
  }, [onClose]);

  if (!isOpen) return null;

  return (
    <div className="w-96 border-l border-white/5 bg-dark-card/95 backdrop-blur-xl flex flex-col h-full shadow-2xl animate-in slide-in-from-right duration-200">
      {/* Header */}
      <div className="h-14 border-b border-white/5 flex items-center justify-between px-4">
        <div className="flex items-center gap-2">
          <div className="p-1.5 bg-brand-primary/15 rounded-lg">
            <Sparkles size={16} className="text-brand-primary" />
          </div>
          <span className="text-sm font-bold text-white">Asistente IA</span>
        </div>
        <button
          onClick={handleClose}
          className="p-1.5 hover:bg-white/10 rounded-lg transition-colors text-gray-400 hover:text-white"
          aria-label="Cerrar panel IA"
        >
          <X size={16} />
        </button>
      </div>

      {/* Body */}
      <div className="flex-1 overflow-y-auto p-4 space-y-4">
        {/* Section context */}
        {sectionTitle && (
          <div className="text-xs text-gray-500 bg-white/5 rounded-lg px-3 py-2 border border-white/5">
            <span className="font-semibold text-gray-400">Sección:</span>{' '}
            {sectionTitle}
          </div>
        )}

        {/* ── Phase: Idle ── */}
        {(phase === 'idle' || phase === 'error') && (
          <>
            {/* Prompt input */}
            <div className="space-y-2">
              <label htmlFor="ai-prompt" className="text-xs font-bold uppercase tracking-widest text-gray-500">
                Instrucciones
              </label>
              <textarea
                id="ai-prompt"
                value={prompt}
                onChange={(e) => setPrompt(e.target.value)}
                placeholder="Describe qué contenido quieres generar para esta sección..."
                rows={4}
                className="w-full bg-white/5 border border-white/10 rounded-lg px-3 py-2.5 text-sm text-white placeholder-gray-600 focus:border-brand-primary focus:ring-1 focus:ring-brand-primary/30 outline-none transition-colors resize-none"
              />
            </div>

            {/* Suggestions */}
            <div className="space-y-1.5">
              <span className="text-[10px] font-bold uppercase tracking-widest text-gray-600">
                Sugerencias rápidas
              </span>
              <div className="space-y-1">
                {PROMPT_SUGGESTIONS.map((suggestion) => (
                  <button
                    key={suggestion}
                    onClick={() => setPrompt(suggestion)}
                    className="w-full text-left text-xs text-gray-400 hover:text-white hover:bg-white/5 rounded-lg px-3 py-2 transition-colors border border-transparent hover:border-white/10"
                  >
                    {suggestion}
                  </button>
                ))}
              </div>
            </div>

            {/* Error message */}
            {phase === 'error' && errorMsg && (
              <div className="flex items-start gap-2 bg-red-500/10 border border-red-500/20 rounded-lg p-3">
                <AlertTriangle size={14} className="text-red-400 mt-0.5 shrink-0" />
                <p className="text-xs text-red-300">{errorMsg}</p>
              </div>
            )}
          </>
        )}

        {/* ── Phase: Generating ── */}
        {phase === 'generating' && (
          <div className="flex-1 flex flex-col items-center justify-center py-12 space-y-4">
            <div className="relative">
              <div className="absolute inset-0 bg-brand-primary/20 rounded-full blur-xl animate-pulse" />
              <div className="relative p-4 bg-brand-primary/10 rounded-full border border-brand-primary/20">
                <Sparkles size={28} className="text-brand-primary animate-pulse" />
              </div>
            </div>
            <div className="text-center space-y-1">
              <p className="text-sm font-semibold text-white">Generando contenido...</p>
              <p className="text-xs text-gray-500">Analizando normativa y contexto del proyecto</p>
            </div>
            <Loader2 size={16} className="animate-spin text-brand-primary" />
          </div>
        )}

        {/* ── Phase: Preview ── */}
        {phase === 'preview' && generatedHtml && (
          <div className="space-y-3">
            <div className="flex items-center justify-between">
              <span className="text-xs font-bold uppercase tracking-widest text-emerald-400">
                Vista previa
              </span>
              <span className="text-[10px] text-gray-600">Revisa antes de insertar</span>
            </div>
            <div
              className="prose prose-invert prose-sm max-w-none bg-white/5 border border-white/10 rounded-lg p-4 max-h-80 overflow-y-auto"
              dangerouslySetInnerHTML={{ __html: generatedHtml }}
            />
          </div>
        )}
      </div>

      {/* Footer Actions */}
      <div className="border-t border-white/5 p-4 space-y-2">
        {(phase === 'idle' || phase === 'error') && (
          <Button
            onClick={handleGenerate}
            disabled={!prompt.trim() || !activeSectionId}
            className="w-full h-10"
            size="sm"
          >
            <Send size={14} className="mr-2" />
            Generar con IA
          </Button>
        )}

        {phase === 'generating' && (
          <Button
            variant="outline"
            onClick={handleClose}
            className="w-full h-10"
            size="sm"
          >
            Cancelar
          </Button>
        )}

        {phase === 'preview' && (
          <>
            <Button
              onClick={() => handleInsert('replace')}
              className="w-full h-10"
              size="sm"
            >
              <ArrowDownToLine size={14} className="mr-2" />
              Reemplazar contenido
            </Button>
            <Button
              variant="outline"
              onClick={() => handleInsert('append')}
              className="w-full h-10"
              size="sm"
            >
              <ArrowDownToLine size={14} className="mr-2" />
              Añadir al final
            </Button>
            <Button
              variant="ghost"
              onClick={handleRetry}
              className="w-full h-10"
              size="sm"
            >
              <RotateCcw size={14} className="mr-2" />
              Regenerar
            </Button>
          </>
        )}
      </div>
    </div>
  );
}
