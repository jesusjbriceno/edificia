import { useEffect, useMemo, useState } from 'react';
import { AlertCircle, FilePlus2, FileText, Loader2, Pencil, Trash2 } from 'lucide-react';
import { Button } from '@/components/ui/Button';
import { templateService } from '@/lib/services/templateService';
import type { TemplateResponse } from '@/lib/types';
import { ApiError } from '@/lib/api';
import { useToastStore } from '@/store/useToastStore';

export default function TemplateManagement() {
  const [templates, setTemplates] = useState<TemplateResponse[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [editingTemplateId, setEditingTemplateId] = useState<string | null>(null);
  const [editName, setEditName] = useState('');
  const [editDescription, setEditDescription] = useState('');
  const [isSavingMetadata, setIsSavingMetadata] = useState(false);
  const [busyTemplateId, setBusyTemplateId] = useState<string | null>(null);

  const { addToast } = useToastStore();

  const defaultTemplateByType = useMemo(() => {
    return new Map(
      templates
        .filter((template) => template.isDefault)
        .map((template) => [template.templateType, template]),
    );
  }, [templates]);

  const availableTemplates = useMemo(
    () => templates.filter((template) => template.isAvailable),
    [templates],
  );

  const defaultMemoriaTemplate = defaultTemplateByType.get('MemoriaTecnica');
  let defaultMemoriaTemplateSummary = 'No hay plantilla predeterminada (se usará el exportador estándar).';
  if (isLoading) {
    defaultMemoriaTemplateSummary = 'Cargando…';
  } else if (defaultMemoriaTemplate) {
    defaultMemoriaTemplateSummary = `${defaultMemoriaTemplate.name} · v${defaultMemoriaTemplate.version}`;
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

  function beginEdit(template: TemplateResponse) {
    setEditingTemplateId(template.id);
    setEditName(template.name);
    setEditDescription(template.description ?? '');
  }

  function cancelEdit() {
    setEditingTemplateId(null);
    setEditName('');
    setEditDescription('');
  }

  async function handleUpdateMetadata() {
    if (!editingTemplateId) return;

    const name = editName.trim();
    if (!name) {
      addToast('El nombre de la plantilla es obligatorio', 'error');
      return;
    }

    setIsSavingMetadata(true);
    try {
      await templateService.updateMetadata(editingTemplateId, {
        name,
        description: editDescription.trim() || undefined,
      });
      addToast('Plantilla actualizada', 'success');
      cancelEdit();
      await loadTemplates();
    } catch (err) {
      if (err instanceof ApiError) {
        addToast(err.message, 'error');
      } else {
        addToast('No se pudo actualizar la plantilla', 'error');
      }
    } finally {
      setIsSavingMetadata(false);
    }
  }

  async function handleSetAvailability(template: TemplateResponse) {
    setBusyTemplateId(template.id);
    try {
      await templateService.setAvailability(template.id, !template.isAvailable);
      addToast(
        template.isAvailable ? 'Plantilla marcada como no disponible' : 'Plantilla marcada como disponible',
        'success',
      );
      await loadTemplates();
    } catch (err) {
      if (err instanceof ApiError) {
        addToast(err.message, 'error');
      } else {
        addToast('No se pudo actualizar la disponibilidad de la plantilla', 'error');
      }
    } finally {
      setBusyTemplateId(null);
    }
  }

  async function handleSetDefault(template: TemplateResponse) {
    setBusyTemplateId(template.id);
    try {
      await templateService.setDefault(template.id, true);
      addToast('Plantilla marcada como predeterminada', 'success');
      await loadTemplates();
    } catch (err) {
      if (err instanceof ApiError) {
        addToast(err.message, 'error');
      } else {
        addToast('No se pudo marcar la plantilla como predeterminada', 'error');
      }
    } finally {
      setBusyTemplateId(null);
    }
  }

  async function handleDelete(template: TemplateResponse) {
    const shouldDelete = globalThis.confirm(
      `¿Eliminar la plantilla "${template.name}"? Esta acción no se puede deshacer.`,
    );

    if (!shouldDelete) {
      return;
    }

    setBusyTemplateId(template.id);
    try {
      await templateService.delete(template.id);
      addToast('Plantilla eliminada', 'success');
      if (editingTemplateId === template.id) {
        cancelEdit();
      }
      await loadTemplates();
    } catch (err) {
      if (err instanceof ApiError) {
        addToast(err.message, 'error');
      } else {
        addToast('No se pudo eliminar la plantilla', 'error');
      }
    } finally {
      setBusyTemplateId(null);
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
          <div className="rounded-xl border border-white/10 bg-white/5 px-4 py-3">
            <p className="text-xs uppercase tracking-wider text-gray-400">Plantillas disponibles</p>
            <p className="mt-1 text-2xl font-semibold text-white">{isLoading ? '…' : availableTemplates.length}</p>
          </div>
          <div className="rounded-xl border border-white/10 bg-white/5 px-4 py-3">
            <p className="text-xs uppercase tracking-wider text-gray-400">Predeterminada MemoriaTecnica</p>
            <p className="mt-1 text-sm text-white">{defaultMemoriaTemplateSummary}</p>
          </div>
        </div>

        <div className="mt-4 rounded-xl border border-brand-primary/25 bg-brand-primary/10 px-4 py-3 text-sm text-gray-200">
          <p className="font-medium text-white">Cómo se asignan las plantillas</p>
          <p className="mt-1">
            Marca las plantillas como <strong>disponibles</strong> para poder seleccionarlas en exportación y define una plantilla{' '}
            <strong>predeterminada</strong> por tipo para cubrir los casos en los que no se indica una selección explícita.
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
              const isEditing = editingTemplateId === template.id;
              const isBusy = busyTemplateId === template.id;
              const defaultButtonTitle = template.isAvailable
                ? undefined
                : 'Solo las plantillas disponibles pueden marcarse como predeterminadas';

              return (
                <div
                  key={template.id}
                  className="rounded-xl border border-white/5 bg-white/5 px-4 py-3"
                >
                  <div className="flex flex-col gap-4 md:flex-row md:items-start md:justify-between">
                    <div>
                      <p className="text-white font-medium">{template.name}</p>
                      <p className="text-xs text-gray-400">
                        {template.templateType} · v{template.version} · {template.originalFileName}
                      </p>
                      {template.description && (
                        <p className="text-xs text-gray-500 mt-1">{template.description}</p>
                      )}
                    </div>

                    <div className="flex flex-wrap items-center gap-2">
                      <span
                        className={template.isAvailable
                          ? 'text-xs px-2 py-1 rounded-full bg-green-500/20 text-green-300'
                          : 'text-xs px-2 py-1 rounded-full bg-gray-500/20 text-gray-300'}
                      >
                        {template.isAvailable ? 'Disponible' : 'No disponible'}
                      </span>
                      <span
                        className={template.isDefault
                          ? 'text-xs px-2 py-1 rounded-full bg-brand-primary/20 text-brand-primary'
                          : 'text-xs px-2 py-1 rounded-full bg-white/10 text-gray-300'}
                      >
                        {template.isDefault ? 'Predeterminada' : 'No predeterminada'}
                      </span>
                    </div>
                  </div>

                  {isEditing ? (
                    <div className="mt-4 rounded-lg border border-white/10 bg-black/20 p-3 space-y-3">
                      <div className="space-y-1">
                        <label htmlFor={`template-name-${template.id}`} className="text-xs text-gray-400 uppercase tracking-wider">
                          Nombre
                        </label>
                        <input
                          id={`template-name-${template.id}`}
                          aria-label="Nombre de plantilla"
                          type="text"
                          value={editName}
                          onChange={(event) => setEditName(event.target.value)}
                          className="w-full rounded-lg border border-white/10 bg-white/5 px-3 py-2 text-sm text-white outline-none focus:border-brand-primary"
                        />
                      </div>

                      <div className="space-y-1">
                        <label htmlFor={`template-description-${template.id}`} className="text-xs text-gray-400 uppercase tracking-wider">
                          Descripción
                        </label>
                        <textarea
                          id={`template-description-${template.id}`}
                          aria-label="Descripción de plantilla"
                          value={editDescription}
                          onChange={(event) => setEditDescription(event.target.value)}
                          rows={3}
                          className="w-full rounded-lg border border-white/10 bg-white/5 px-3 py-2 text-sm text-white outline-none focus:border-brand-primary"
                        />
                      </div>

                      <div className="flex flex-wrap items-center gap-2">
                        <Button size="sm" onClick={handleUpdateMetadata} isLoading={isSavingMetadata}>
                          Guardar
                        </Button>
                        <Button
                          size="sm"
                          variant="outline"
                          onClick={cancelEdit}
                          disabled={isSavingMetadata}
                        >
                          Cancelar
                        </Button>
                      </div>
                    </div>
                  ) : null}

                  <div className="mt-4 flex flex-wrap items-center gap-2">
                    <Button
                      size="sm"
                      variant="outline"
                      onClick={() => beginEdit(template)}
                      disabled={isBusy || Boolean(editingTemplateId)}
                    >
                      <Pencil size={14} className="mr-1" />
                      Editar
                    </Button>

                    <Button
                      size="sm"
                      variant="outline"
                      onClick={() => handleDelete(template)}
                      disabled={isBusy || isSavingMetadata}
                    >
                      <Trash2 size={14} className="mr-1" />
                      Eliminar
                    </Button>

                    <Button
                      size="sm"
                      variant={template.isAvailable ? 'outline' : 'primary'}
                      onClick={() => handleSetAvailability(template)}
                      disabled={isBusy || isSavingMetadata}
                    >
                      {template.isAvailable ? 'Marcar no disponible' : 'Marcar disponible'}
                    </Button>

                    <Button
                      size="sm"
                      variant={template.isDefault ? 'primary' : 'outline'}
                      onClick={() => handleSetDefault(template)}
                      disabled={template.isDefault || !template.isAvailable || isBusy || isSavingMetadata}
                      title={defaultButtonTitle}
                    >
                      {template.isDefault ? 'Predeterminada' : 'Marcar predeterminada'}
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
