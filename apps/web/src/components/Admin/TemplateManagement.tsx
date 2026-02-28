import { useEffect, useMemo, useRef, useState } from 'react';
import { AlertCircle, FileText, Loader2, Upload, X } from 'lucide-react';
import { Button } from '@/components/ui/Button';
import { Input } from '@/components/ui/Input';
import { Select } from '@/components/ui/Select';
import { templateService } from '@/lib/services/templateService';
import type { TemplateResponse } from '@/lib/types';
import { ApiError } from '@/lib/api';
import { useToastStore } from '@/store/useToastStore';

const TEMPLATE_TYPES = [
  { value: 'MemoriaTecnica', label: 'Memoria Técnica' },
] as const;

const ALLOWED_TEMPLATE_MIME_TYPES = new Set([
  'application/vnd.openxmlformats-officedocument.wordprocessingml.template',
  'application/octet-stream',
  'application/zip',
  'application/x-zip-compressed',
  '',
]);

function extractMissingTags(detail: string): string[] {
  const regex = /faltan Tag\(s\) obligatorios[^:]*:\s*([^.]+).?/i;
  const match = regex.exec(detail);
  if (!match?.[1]) return [];

  return match[1]
    .split(',')
    .map((tag) => tag.trim())
    .filter(Boolean);
}

function getTemplateUploadErrorMessage(error: ApiError): string {
  const code = error.code ?? '';
  const detail = error.message ?? '';

  if (code.includes('Template.InvalidFormat')) {
    const missingTags = extractMissingTags(detail);
    if (missingTags.length > 0) {
      return [
        'La plantilla no cumple el contrato de tags para MemoriaTecnica.',
        `Faltan estos tags obligatorios: ${missingTags.join(', ')}.`,
        'Abre la plantilla en Word, añade los Content Controls y configura exactamente esos Tag.',
      ].join('\n');
    }

    return `La plantilla no cumple el formato requerido. ${detail}`;
  }

  if (code.includes('Validation.')) {
    if (error.validationErrors && error.validationErrors.length > 0) {
      const lines = error.validationErrors
        .map((validationError) => `• ${validationError.message}`)
        .join('\n');
      return `Revisa los datos del formulario:\n${lines}`;
    }

    return detail || 'Hay errores de validación en la subida de la plantilla.';
  }

  if (error.status === 413) {
    return 'El archivo supera el tamaño máximo permitido (10 MB).';
  }

  if (error.status === 415) {
    return 'El tipo de archivo no es válido. Sube una plantilla .dotx.';
  }

  return detail || 'Error al subir la plantilla.';
}

export default function TemplateManagement() {
  const [templates, setTemplates] = useState<TemplateResponse[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [submitError, setSubmitError] = useState<string | null>(null);

  const [name, setName] = useState('');
  const [templateType, setTemplateType] = useState('MemoriaTecnica');
  const [description, setDescription] = useState('');
  const [file, setFile] = useState<File | null>(null);
  const [isDragOver, setIsDragOver] = useState(false);
  const fileInputRef = useRef<HTMLInputElement | null>(null);

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

  async function handleSubmit(e: React.FormEvent) {
    e.preventDefault();
    setSubmitError(null);

    if (!file) {
      const message = 'Debes seleccionar un archivo .dotx';
      setSubmitError(message);
      addToast(message, 'error');
      return;
    }

    if (!file.name.toLowerCase().endsWith('.dotx')) {
      const message = 'Formato no válido: selecciona un archivo con extensión .dotx.';
      setSubmitError(message);
      addToast(message, 'error');
      return;
    }

    if (file.size > 10 * 1024 * 1024) {
      const message = 'El archivo supera el tamaño máximo de 10 MB.';
      setSubmitError(message);
      addToast(message, 'error');
      return;
    }

    if (!ALLOWED_TEMPLATE_MIME_TYPES.has(file.type)) {
      const message = `Tipo MIME no permitido (${file.type || 'vacío'}). Sube una plantilla .dotx válida.`;
      setSubmitError(message);
      addToast(message, 'error');
      return;
    }

    setIsSubmitting(true);
    try {
      await templateService.create({
        name: name.trim(),
        templateType,
        description: description.trim() || undefined,
        file,
      });

      addToast('Plantilla subida correctamente', 'success');
      setName('');
      setDescription('');
      setFile(null);
      setSubmitError(null);
      await loadTemplates();
    } catch (err) {
      if (err instanceof ApiError) {
        const message = getTemplateUploadErrorMessage(err);
        setSubmitError(message);
        addToast(message, 'error');
      } else {
        const message = 'Error al subir la plantilla';
        setSubmitError(message);
        addToast(message, 'error');
      }
    } finally {
      setIsSubmitting(false);
    }
  }

  function handleFileSelect(selectedFile: File | null) {
    setSubmitError(null);
    setFile(selectedFile);
  }

  function handleClearFile() {
    setFile(null);
    setSubmitError(null);
    if (fileInputRef.current) {
      fileInputRef.current.value = '';
    }
  }

  function handleDragOver(event: React.DragEvent<HTMLButtonElement>) {
    event.preventDefault();
    event.stopPropagation();
    setIsDragOver(true);
  }

  function handleDragLeave(event: React.DragEvent<HTMLButtonElement>) {
    event.preventDefault();
    event.stopPropagation();
    setIsDragOver(false);
  }

  function handleDrop(event: React.DragEvent<HTMLButtonElement>) {
    event.preventDefault();
    event.stopPropagation();
    setIsDragOver(false);

    const droppedFile = event.dataTransfer.files?.[0] ?? null;
    handleFileSelect(droppedFile);
  }

  function handleDropZoneKeyDown(event: React.KeyboardEvent<HTMLButtonElement>) {
    if (event.key === 'Enter' || event.key === ' ') {
      event.preventDefault();
      fileInputRef.current?.click();
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
            <p className="mt-1 text-sm text-white">
              {isLoading
                ? 'Cargando…'
                : activeMemoriaTemplate
                  ? `${activeMemoriaTemplate.name} · v${activeMemoriaTemplate.version}`
                  : 'No hay plantilla activa (se usará el exportador estándar).'}
            </p>
          </div>
        </div>

        <div className="mt-4 rounded-xl border border-brand-primary/25 bg-brand-primary/10 px-4 py-3 text-sm text-gray-200">
          <p className="font-medium text-white">Cómo se asignan las plantillas</p>
          <p className="mt-1">
            La asignación es <strong>global por tipo</strong>: al exportar, el backend usa automáticamente la plantilla <strong>activa</strong> de
            <strong> MemoriaTecnica</strong>. No se asigna plantilla por proyecto de forma manual.
          </p>
        </div>
      </div>

      <div className="bg-dark-card border border-white/5 rounded-2xl p-6">
        <h2 className="text-base font-semibold text-white">Pautas y reglas de subida</h2>
        <ul className="mt-3 space-y-1.5 text-sm text-gray-300 list-disc pl-5">
          <li>Formato permitido: <strong>.dotx</strong> (plantilla Word OpenXML).</li>
          <li>Tamaño máximo: <strong>10 MB</strong>.</li>
          <li>La plantilla debe contener <strong>Content Controls</strong> con <strong>Tag</strong>.</li>
          <li>Para <strong>MemoriaTecnica</strong> son obligatorios: <strong>ProjectTitle</strong>, <strong>MD.01</strong>, <strong>MC.01</strong>.</li>
          <li>Si falla la validación, la subida se rechaza y no se guarda la plantilla.</li>
        </ul>
        <p className="mt-3 text-xs text-gray-400">
          Guía recomendada:{' '}
          <a
            href="/ayuda/guia-definicion-plantillas-dotx"
            className="text-brand-primary hover:text-brand-primary/80 underline underline-offset-2"
          >
            Ayuda · Guía de definición de plantillas .dotx
          </a>
        </p>
      </div>

      <div className="bg-dark-card border border-white/5 rounded-2xl p-6">
        <form className="grid gap-4 md:grid-cols-2" onSubmit={handleSubmit}>
          <Input
            label="Nombre"
            value={name}
            onChange={(e) => setName(e.target.value)}
            placeholder="Plantilla Memoria v1"
            required
          />

          <Select
            label="Tipo"
            options={TEMPLATE_TYPES.map((t) => ({ value: t.value, label: t.label }))}
            value={templateType}
            onChange={(e) => setTemplateType(e.target.value)}
          />

          <div className="md:col-span-2">
            <Input
              label="Descripción (opcional)"
              value={description}
              onChange={(e) => setDescription(e.target.value)}
              placeholder="Notas de versión o uso"
            />
          </div>

          <div className="md:col-span-2 space-y-1.5">
            <label htmlFor="template-file" className="text-xs font-medium text-gray-400 uppercase tracking-wider ml-1">Archivo .dotx</label>
            <button
              type="button"
              onDragOver={handleDragOver}
              onDragLeave={handleDragLeave}
              onDrop={handleDrop}
              onKeyDown={handleDropZoneKeyDown}
              onClick={() => fileInputRef.current?.click()}
              aria-label="Arrastrar archivo de plantilla o pulsar para seleccionar"
              className={[
                'w-full rounded-xl border-2 border-dashed px-6 py-8 text-center text-sm transition-colors',
                'flex min-h-[140px] flex-col items-center justify-center gap-2',
                isDragOver
                  ? 'border-brand-primary bg-brand-primary/10 text-brand-primary'
                  : 'border-white/15 bg-dark-bg/30 text-gray-300 hover:border-white/25 hover:bg-white/5',
              ].join(' ')}
            >
              <Upload size={24} className={isDragOver ? 'text-brand-primary' : 'text-gray-400'} />
              <span className="text-base font-medium text-white">Arrastra y suelta tu plantilla aquí</span>
              <span className="text-xs text-gray-400">Formato permitido: `.dotx` · tamaño máximo: 10 MB</span>
            </button>
            <input
              ref={fileInputRef}
              id="template-file"
              type="file"
              accept=".dotx,application/vnd.openxmlformats-officedocument.wordprocessingml.template"
              onChange={(e) => handleFileSelect(e.target.files?.[0] ?? null)}
              aria-label="Archivo .dotx"
              className="sr-only"
            />
            <div className="flex flex-wrap items-center justify-between gap-2 rounded-lg border border-white/10 bg-white/5 px-3 py-2">
              <p className="text-xs text-gray-400">
                También puedes seleccionar manualmente el archivo desde tu equipo.
              </p>
              <Button
                type="button"
                size="sm"
                variant="outline"
                onClick={() => fileInputRef.current?.click()}
              >
                Seleccionar archivo
              </Button>
            </div>
            {file && (
              <div className="flex items-center justify-between gap-3 rounded-lg border border-brand-primary/30 bg-brand-primary/10 px-3 py-2">
                <div className="flex items-center gap-2 text-sm text-white min-w-0">
                  <FileText size={16} className="text-brand-primary shrink-0" />
                  <span className="truncate">{file.name}</span>
                  <span className="text-xs text-gray-400 shrink-0">({(file.size / 1024).toFixed(1)} KB)</span>
                </div>
                <button
                  type="button"
                  onClick={handleClearFile}
                  className="text-gray-400 hover:text-white transition-colors"
                  aria-label="Quitar archivo seleccionado"
                >
                  <X size={16} />
                </button>
              </div>
            )}
          </div>

          <div className="md:col-span-2 flex justify-end">
            <Button type="submit" isLoading={isSubmitting} className="h-11">
              <Upload size={16} className="mr-2" />
              Subir plantilla
            </Button>
          </div>

          {submitError && (
            <div className="md:col-span-2 rounded-lg border border-red-500/30 bg-red-500/10 px-3 py-2 text-sm text-red-300 whitespace-pre-line">
              {submitError}
            </div>
          )}
        </form>
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
