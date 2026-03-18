import { useEffect, useMemo, useState } from 'react';
import { AlertCircle, FilePlus2, FileText, Loader2 } from 'lucide-react';
import { Button } from '@/components/ui/Button';
import { templateService } from '@/lib/services/templateService';
import type { TemplateResponse } from '@/lib/types';
import { ApiError } from '@/lib/api';
import { useToastStore } from '@/store/useToastStore';

export default function TemplateManagement() {
  const [templates, setTemplates] = useState<TemplateResponse[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  const { addToast } = useToastStore();

  const activeTemplatesByType = useMemo(() => {
    return new Map(
      templates
        .filter((t) => t.isActive)
        .map((t) => [t.templateType, t.id]),
    );
  }, [templates]);

  const activeTemplateByType = useMemo(() => {
    return new Map(
      templates
        .filter((template) => template.isActive)
        .map((template) => [template.templateType, template]),
    );
  }, [templates]);

  const activeMemoriaTemplate = activeTemplateByType.get('MemoriaTecnica');
  let activeMemoriaTemplateSummary = 'No hay plantilla activa (se usará el exportador estándar).';
  if (isLoading) {
    activeMemoriaTemplateSummary = 'Cargando…';
  } else if (activeMemoriaTemplate) {
    activeMemoriaTemplateSummary = `${activeMemoriaTemplate.name} · v${activeMemoriaTemplate.version}`;
  }

  useEffect(() => {
    loadTemplates();
  }, []);

  async function loadTemplates() {
    setIsLoading(true);
    setError(null);
    try {
      const data = await templateService.list();
      setTemplates(data);
    } catch (err) {
      if (err instanceof ApiError) {
        setError(err.message);
      } else {
        setError('No se pudieron cargar las plantillas.');
      }
    } finally {
      setIsLoading(false);
    }
  }

  async function handleToggleStatus(template: TemplateResponse) {
    try {
      await templateService.toggleStatus(template.id, !template.isActive);
      addToast(
        template.isActive ? 'Plantilla desactivada' : 'Plantilla activada',
        'success',
      );
      await loadTemplates();
    } catch (err) {
      if (err instanceof ApiError) {
        addToast(err.message, 'error');
      } else {
        addToast('No se pudo actualizar el estado de la plantilla', 'error');
      }
    }
  }

  return (
    <div className="space-y-8">
      <div>
        <h1 className="text-3xl font-bold text-white tracking-tight">Plantillas .dotx</h1>
        <p className="text-gray-400 mt-1">Sube y gestiona plantillas para la exportación de documentos.</p>
        <div className="mt-4 flex flex-wrap items-center gap-2">
          <a href="/admin/templates/new" className="inline-flex">
            <Button type="button" className="h-10">
              <FilePlus2 size={16} className="mr-2" />
              Nueva plantilla
            </Button>
          </a>
          <a
            href="/ayuda/guia-definicion-plantillas-dotx"
            className="text-sm text-brand-primary hover:text-brand-primary/80 underline underline-offset-2"
          >
            Guía de definición .dotx
          </a>
        </div>
      </div>

      <div className="bg-dark-card border border-white/5 rounded-2xl p-6">
        <h2 className="text-base font-semibold text-white">Estado rápido</h2>
        <div className="mt-3 grid gap-3 md:grid-cols-3">
          <div className="rounded-xl border border-white/10 bg-white/5 px-4 py-3">
            <p className="text-xs uppercase tracking-wider text-gray-400">Plantillas cargadas</p>
            <p className="mt-1 text-2xl font-semibold text-white">{isLoading ? '…' : templates.length}</p>
          </div>
          <div className="rounded-xl border border-white/10 bg-white/5 px-4 py-3 md:col-span-2">
            <p className="text-xs uppercase tracking-wider text-gray-400">Activa para exportación (`MemoriaTecnica`)</p>
            <p className="mt-1 text-sm text-white">{activeMemoriaTemplateSummary}</p>
          </div>
        </div>

        <div className="mt-4 rounded-xl border border-brand-primary/25 bg-brand-primary/10 px-4 py-3 text-sm text-gray-200">
          <p className="font-medium text-white">Cómo se asignan las plantillas</p>
          <p className="mt-1">
            La asignación es <strong>global por tipo</strong>: al exportar, el backend usa automáticamente la plantilla <strong>activa</strong> de{' '}
            <strong>MemoriaTecnica</strong>. No se asigna plantilla por proyecto de forma manual.
          </p>
        </div>
      </div>

      <div className="bg-dark-card border border-white/5 rounded-2xl p-6">
        <h2 className="text-base font-semibold text-white mb-4">Plantillas cargadas</h2>
        {isLoading && (
          <div className="flex items-center justify-center py-10">
            <Loader2 className="w-6 h-6 text-brand-primary animate-spin" />
            <span className="ml-3 text-gray-400">Cargando plantillas…</span>
          </div>
        )}

        {!isLoading && error && (
          <div className="flex items-center gap-2 text-red-400 py-2">
            <AlertCircle size={18} />
            <span>{error}</span>
          </div>
        )}

        {!isLoading && !error && templates.length === 0 && (
          <div className="flex flex-col items-center justify-center py-10 text-gray-400">
            <FileText size={28} className="mb-2" />
            <span>No hay plantillas disponibles.</span>
          </div>
        )}

        {!isLoading && !error && templates.length > 0 && (
          <div className="space-y-3">
            {templates.map((template) => {
              const hasAnotherActive =
                activeTemplatesByType.has(template.templateType) &&
                activeTemplatesByType.get(template.templateType) !== template.id;

              return (
                <div
                  key={template.id}
                  className="flex flex-col md:flex-row md:items-center justify-between gap-3 rounded-xl border border-white/5 bg-white/5 px-4 py-3"
                >
                  <div>
                    <p className="text-white font-medium">{template.name}</p>
                    <p className="text-xs text-gray-400">
                      {template.templateType} · v{template.version} · {template.originalFileName}
                    </p>
                    {template.description && (
                      <p className="text-xs text-gray-500 mt-1">{template.description}</p>
                    )}
                  </div>

                  <div className="flex items-center gap-2">
                    <span
                      className={template.isActive
                        ? 'text-xs px-2 py-1 rounded-full bg-green-500/20 text-green-300'
                        : 'text-xs px-2 py-1 rounded-full bg-gray-500/20 text-gray-300'}
                    >
                      {template.isActive ? 'Activa' : 'Inactiva'}
                    </span>
                    <Button
                      size="sm"
                      variant={template.isActive ? 'outline' : 'primary'}
                      onClick={() => handleToggleStatus(template)}
                      disabled={!template.isActive && hasAnotherActive}
                      title={!template.isActive && hasAnotherActive ? 'Ya existe otra plantilla activa del mismo tipo' : undefined}
                    >
                      {template.isActive ? 'Desactivar' : 'Activar'}
                    </Button>
                  </div>
                </div>
              );
            })}
          </div>
        )}
      </div>
    </div>
  );
}
