import { describe, it, expect, vi, beforeEach } from 'vitest';
import { render, screen, waitFor, fireEvent } from '@testing-library/react';
import TemplateManagement from '@/components/Admin/TemplateManagement';
import type { TemplateParamResponse, TemplateResponse } from '@/lib/types';

const addToastMock = vi.fn();

vi.mock('@/store/useToastStore', () => ({
  useToastStore: () => ({
    addToast: addToastMock,
  }),
}));

vi.mock('@/lib/services/templateService', () => ({
  templateService: {
    list: vi.fn(),
    create: vi.fn(),
    toggleStatus: vi.fn(),
    updateMetadata: vi.fn(),
    setAvailability: vi.fn(),
    setDefault: vi.fn(),
    delete: vi.fn(),
  },
}));

vi.mock('@/lib/services/templateParamService', () => ({
  templateParamService: {
    list: vi.fn(),
    setActivation: vi.fn(),
  },
}));

import { templateService } from '@/lib/services/templateService';
import { templateParamService } from '@/lib/services/templateParamService';

function buildTemplate(overrides: Partial<TemplateResponse> = {}): TemplateResponse {
  return {
    id: 'tpl-1',
    name: 'Plantilla Base',
    description: null,
    templateType: 'MemoriaTecnica',
    version: 1,
    isAvailable: true,
    isDefault: false,
    isActive: false,
    originalFileName: 'plantilla.dotx',
    mimeType: 'application/vnd.openxmlformats-officedocument.wordprocessingml.template',
    fileSizeBytes: 100,
    createdAt: new Date().toISOString(),
    updatedAt: null,
    ...overrides,
  };
}

function buildTemplateParam(overrides: Partial<TemplateParamResponse> = {}): TemplateParamResponse {
  return {
    id: 'param-1',
    key: 'PROJECT_TITLE',
    displayName: 'Titulo del proyecto',
    sourceCode: 'PROJECT_TITLE',
    formatter: null,
    isActive: true,
    createdAt: new Date().toISOString(),
    updatedAt: null,
    ...overrides,
  };
}

describe('TemplateManagement', () => {
  beforeEach(() => {
    vi.clearAllMocks();
    vi.spyOn(globalThis, 'confirm').mockReturnValue(true);
    vi.mocked(templateService.list).mockResolvedValue([]);
    vi.mocked(templateParamService.list).mockResolvedValue([]);
  });

  it('muestra CTA para ir al formulario de nueva plantilla', async () => {
    render(<TemplateManagement />);

    await waitFor(() => {
      expect(templateService.list).toHaveBeenCalled();
    });

    const cta = screen.getByRole('link', { name: /nueva plantilla/i });
    expect(cta).toHaveAttribute('href', '/admin/templates/new');
  });

  it('renderiza el estado de plantilla activa cuando hay una activa', async () => {
    vi.mocked(templateService.list).mockResolvedValueOnce([
      buildTemplate({
        name: 'Plantilla Oficial',
        version: 2,
        isDefault: true,
        isActive: true,
      }),
    ]);

    render(<TemplateManagement />);

    expect(await screen.findByText(/Plantilla Oficial · v2/i)).toBeInTheDocument();
  });

  it('permite actualizar disponibilidad desde el listado', async () => {
    vi.mocked(templateService.list).mockResolvedValueOnce([
      buildTemplate({
        name: 'Plantilla A',
        isAvailable: false,
      }),
    ]);

    render(<TemplateManagement />);

    const button = await screen.findByRole('button', { name: /marcar disponible/i });
    fireEvent.click(button);

    await waitFor(() => {
      expect(templateService.setAvailability).toHaveBeenCalledWith('tpl-1', true);
    });
  });

  it('permite marcar plantilla predeterminada', async () => {
    vi.mocked(templateService.list).mockResolvedValueOnce([
      buildTemplate({
        id: 'tpl-2',
        name: 'Plantilla B',
      }),
    ]);

    render(<TemplateManagement />);

    const button = await screen.findByRole('button', { name: /marcar predeterminada/i });
    fireEvent.click(button);

    await waitFor(() => {
      expect(templateService.setDefault).toHaveBeenCalledWith('tpl-2', true);
    });
  });

  it('permite editar metadatos de plantilla', async () => {
    vi.mocked(templateService.list)
      .mockResolvedValueOnce([
        buildTemplate({
          id: 'tpl-3',
          name: 'Plantilla Original',
          description: 'Descripción inicial',
        }),
      ])
      .mockResolvedValueOnce([
        buildTemplate({
          id: 'tpl-3',
          name: 'Plantilla Editada',
          description: 'Nueva descripción',
        }),
      ]);

    render(<TemplateManagement />);

    fireEvent.click(await screen.findByRole('button', { name: /editar/i }));
    fireEvent.change(screen.getByLabelText(/nombre de plantilla/i), {
      target: { value: 'Plantilla Editada' },
    });
    fireEvent.change(screen.getByLabelText(/descripción de plantilla/i), {
      target: { value: 'Nueva descripción' },
    });
    fireEvent.click(screen.getByRole('button', { name: /guardar/i }));

    await waitFor(() => {
      expect(templateService.updateMetadata).toHaveBeenCalledWith('tpl-3', {
        name: 'Plantilla Editada',
        description: 'Nueva descripción',
      });
    });
  });

  it('permite eliminar plantilla', async () => {
    vi.mocked(templateService.list)
      .mockResolvedValueOnce([buildTemplate({ id: 'tpl-4', name: 'Plantilla Borrar' })])
      .mockResolvedValueOnce([]);

    render(<TemplateManagement />);

    fireEvent.click(await screen.findByRole('button', { name: /eliminar/i }));

    await waitFor(() => {
      expect(templateService.delete).toHaveBeenCalledWith('tpl-4');
      expect(globalThis.confirm).toHaveBeenCalled();
    });
  });

  it('permite activar o desactivar un parámetro global', async () => {
    vi.mocked(templateParamService.list)
      .mockResolvedValueOnce([
        buildTemplateParam({
          id: 'param-2',
          displayName: 'Tipo de intervencion',
          key: 'INTERVENTION_TYPE',
          sourceCode: 'INTERVENTION_TYPE',
          isActive: false,
        }),
      ])
      .mockResolvedValueOnce([
        buildTemplateParam({
          id: 'param-2',
          displayName: 'Tipo de intervencion',
          key: 'INTERVENTION_TYPE',
          sourceCode: 'INTERVENTION_TYPE',
          isActive: true,
        }),
      ]);

    render(<TemplateManagement />);

    const button = await screen.findByRole('button', { name: /activar/i });
    fireEvent.click(button);

    await waitFor(() => {
      expect(templateParamService.setActivation).toHaveBeenCalledWith('param-2', true);
    });
  });
});
