import { describe, it, expect, vi, beforeEach } from 'vitest';
import { render, screen, waitFor, fireEvent } from '@testing-library/react';
import TemplateUploadForm from '@/components/Admin/TemplateUploadForm';
import { ApiError } from '@/lib/api';

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

describe('TemplateUploadForm', () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  it('bloquea subida si la extensión no es .dotx', async () => {
    render(<TemplateUploadForm />);

    fireEvent.change(screen.getByPlaceholderText(/plantilla memoria v1/i), {
      target: { value: 'Plantilla invalida' },
    });

    const fileInput = screen.getByLabelText(/archivo \.dotx/i);
    const invalidFile = new File(['dummy'], 'plantilla.docx', {
      type: 'application/vnd.openxmlformats-officedocument.wordprocessingml.document',
    });

    fireEvent.change(fileInput, {
      target: { files: [invalidFile] },
    });

    const submitButton = screen.getByRole('button', { name: /subir plantilla/i });
    fireEvent.submit(submitButton.closest('form') as HTMLFormElement);

    expect(await screen.findByText(/formato no válido/i)).toBeInTheDocument();
    expect(templateService.create).not.toHaveBeenCalled();
  });

  it('muestra tags faltantes cuando backend devuelve Template.InvalidFormat', async () => {
    vi.mocked(templateService.create).mockRejectedValueOnce(
      new ApiError(
        400,
        'Formato de plantilla inválido: faltan Tag(s) obligatorios para MemoriaTecnica: ProjectTitle, MD.01, MC.01.',
        'Validation.Template.InvalidFormat',
      ),
    );

    render(<TemplateUploadForm />);

    fireEvent.change(screen.getByPlaceholderText(/plantilla memoria v1/i), {
      target: { value: 'Plantilla test' },
    });

    const fileInput = screen.getByLabelText(/archivo \.dotx/i);
    const validFile = new File(['dummy'], 'plantilla.dotx', {
      type: 'application/vnd.openxmlformats-officedocument.wordprocessingml.template',
    });

    fireEvent.change(fileInput, {
      target: { files: [validFile] },
    });

    const submitButton = screen.getByRole('button', { name: /subir plantilla/i });
    fireEvent.submit(submitButton.closest('form') as HTMLFormElement);

    const errorBox = await screen.findByText(/la plantilla no cumple el contrato de tags/i);
    expect(errorBox).toBeInTheDocument();
    expect(addToastMock).toHaveBeenCalledWith(expect.stringMatching(/faltan estos tags obligatorios/i), 'error');
  });

  it('envía plantilla con drag & drop', async () => {
    vi.mocked(templateService.create).mockResolvedValueOnce('template-id');

    render(<TemplateUploadForm />);

    fireEvent.change(screen.getByPlaceholderText(/plantilla memoria v1/i), {
      target: { value: 'Plantilla drag' },
    });

    const dropZone = screen.getByRole('button', { name: /arrastrar archivo de plantilla o pulsar para seleccionar/i });
    const droppedFile = new File(['dummy'], 'drag-template.dotx', {
      type: 'application/vnd.openxmlformats-officedocument.wordprocessingml.template',
    });

    fireEvent.drop(dropZone, {
      dataTransfer: {
        files: [droppedFile],
      },
    });

    const submitButton = screen.getByRole('button', { name: /subir plantilla/i });
    fireEvent.submit(submitButton.closest('form') as HTMLFormElement);

    await waitFor(() => {
      expect(templateService.create).toHaveBeenCalledWith(
        expect.objectContaining({
          name: 'Plantilla drag',
          templateType: 'MemoriaTecnica',
          file: expect.any(File),
        }),
      );
    });
  });
});
