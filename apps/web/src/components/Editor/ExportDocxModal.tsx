import { useEffect, useMemo, useState } from 'react';
import { Loader2 } from 'lucide-react';
import Modal from '@/components/ui/Modal';
import { Button } from '@/components/ui/Button';
import { Input } from '@/components/ui/Input';
import { Select } from '@/components/ui/Select';
import { templateService } from '@/lib/services/templateService';
import { ApiError } from '@/lib/api';
import type { TemplateResponse } from '@/lib/types';

export interface ExportDocxOptions {
  preferredTemplateId?: string;
  fileName?: string;
}

interface ExportDocxModalProps {
  isOpen: boolean;
  onClose: () => void;
  onConfirm: (options: ExportDocxOptions) => void;
  exporting: boolean;
}

const DEFAULT_TEMPLATE_OPTION = 'default-exporter';

function normalizeFileName(rawValue: string): string {
  const trimmed = rawValue.trim();
  if (!trimmed) return '';

  const withoutExtension = trimmed.replace(/\.docx$/i, '');
  const sanitized = withoutExtension
    .replaceAll(/[\\/:*?"<>|]/g, '-')
    .replaceAll(/\s+/g, ' ')
    .trim();

  return sanitized ? `${sanitized}.docx` : '';
}

export function ExportDocxModal({ isOpen, onClose, onConfirm, exporting }: Readonly<ExportDocxModalProps>) {
  const [templates, setTemplates] = useState<TemplateResponse[]>([]);
  const [loadingTemplates, setLoadingTemplates] = useState(false);
  const [templateError, setTemplateError] = useState<string | null>(null);
  const [selectedTemplateId, setSelectedTemplateId] = useState(DEFAULT_TEMPLATE_OPTION);
  const [fileName, setFileName] = useState('');

  useEffect(() => {
    if (!isOpen) return;

    setFileName('');
    setTemplateError(null);

    let mounted = true;
    async function loadActiveTemplates() {
      setLoadingTemplates(true);
      try {
        const data = await templateService.list({ templateType: 'MemoriaTecnica', isAvailable: true });
        if (!mounted) return;
        setTemplates(data);
        const defaultTemplate = data.find((template) => template.isDefault);
        setSelectedTemplateId(defaultTemplate?.id ?? data[0]?.id ?? DEFAULT_TEMPLATE_OPTION);
      } catch (error) {
        if (!mounted) return;
        if (error instanceof ApiError) {
          setTemplateError(error.message || 'No se pudieron cargar las plantillas disponibles.');
        } else {
          setTemplateError('No se pudieron cargar las plantillas disponibles.');
        }
        setTemplates([]);
        setSelectedTemplateId(DEFAULT_TEMPLATE_OPTION);
      } finally {
        if (mounted) {
          setLoadingTemplates(false);
        }
      }
    }

    loadActiveTemplates();

    return () => {
      mounted = false;
    };
  }, [isOpen]);

  const templateOptions = useMemo(() => {
    const options = [
      { value: DEFAULT_TEMPLATE_OPTION, label: 'Exportador estándar (fallback)' },
    ];

    for (const template of templates) {
      options.push({
        value: template.id,
        label: template.isDefault
          ? `${template.name} · v${template.version} · Predeterminada`
          : `${template.name} · v${template.version}`,
      });
    }

    return options;
  }, [templates]);

  function handleConfirm() {
    const normalized = normalizeFileName(fileName);
    const preferredTemplateId =
      selectedTemplateId === DEFAULT_TEMPLATE_OPTION ? undefined : selectedTemplateId;

    onConfirm({
      preferredTemplateId,
      fileName: normalized || undefined,
    });
  }

  return (
    <Modal isOpen={isOpen} onClose={onClose} title="Exportar memoria a DOCX" className="max-w-xl">
      <div className="space-y-5">
        <p className="text-sm text-gray-300">
          Selecciona la plantilla a aplicar y define el nombre del archivo.
          Estos valores se envían al backend para la exportación real.
        </p>

        <div className="space-y-2">
          <label htmlFor="export-template" className="text-xs font-medium text-gray-400 uppercase tracking-wider ml-1">
            Plantilla
          </label>
          {loadingTemplates ? (
            <div className="h-11 rounded-lg border border-white/10 bg-white/5 px-3 flex items-center text-sm text-gray-400">
              <Loader2 size={14} className="animate-spin mr-2" />
              Cargando plantillas disponibles...
            </div>
          ) : (
            <Select
              id="export-template"
              options={templateOptions}
              value={selectedTemplateId}
              onChange={(event) => setSelectedTemplateId(event.target.value)}
            />
          )}
          {templateError && (
            <p className="text-xs text-amber-300">{templateError}</p>
          )}
        </div>

        <Input
          label="Nombre del archivo (opcional)"
          value={fileName}
          onChange={(event) => setFileName(event.target.value)}
          placeholder="memoria-proyecto.docx"
        />

        <div className="rounded-lg border border-white/10 bg-white/5 px-3 py-2 text-xs text-gray-400">
          Si no indicas nombre, se usará el que devuelva el servidor.
        </div>

        <div className="flex items-center justify-end gap-2 pt-1">
          <Button type="button" variant="outline" onClick={onClose} disabled={exporting}>
            Cancelar
          </Button>
          <Button type="button" onClick={handleConfirm} isLoading={exporting}>
            Exportar DOCX
          </Button>
        </div>
      </div>
    </Modal>
  );
}
