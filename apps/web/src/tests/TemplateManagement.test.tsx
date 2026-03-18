import { describe, it, expect, vi, beforeEach } from 'vitest';
import { render, screen, waitFor, fireEvent } from '@testing-library/react';
import TemplateManagement from '@/components/Admin/TemplateManagement';

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
  },
}));

import { templateService } from '@/lib/services/templateService';

describe('TemplateManagement', () => {
  beforeEach(() => {
    vi.clearAllMocks();
    vi.mocked(templateService.list).mockResolvedValue([]);
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
      {
        id: 'tpl-1',
        name: 'Plantilla Oficial',
        description: null,
        templateType: 'MemoriaTecnica',
        version: 2,
        isActive: true,
        originalFileName: 'plantilla.dotx',
        mimeType: 'application/vnd.openxmlformats-officedocument.wordprocessingml.template',
        fileSizeBytes: 100,
        createdAt: new Date().toISOString(),
        updatedAt: null,
      },
    ]);

    render(<TemplateManagement />);

    expect(await screen.findByText(/Plantilla Oficial · v2/i)).toBeInTheDocument();
  });

  it('permite activar/desactivar plantillas desde el listado', async () => {
    vi.mocked(templateService.list).mockResolvedValueOnce([
      {
        id: 'tpl-1',
        name: 'Plantilla A',
        description: null,
        templateType: 'MemoriaTecnica',
        version: 1,
        isActive: false,
        originalFileName: 'a.dotx',
        mimeType: 'application/vnd.openxmlformats-officedocument.wordprocessingml.template',
        fileSizeBytes: 100,
        createdAt: new Date().toISOString(),
        updatedAt: null,
      },
    ]);

    render(<TemplateManagement />);

    const activateButton = await screen.findByRole('button', { name: /activar/i });
    fireEvent.click(activateButton);

    await waitFor(() => {
      expect(templateService.toggleStatus).toHaveBeenCalledWith('tpl-1', true);
    });
  });
});
