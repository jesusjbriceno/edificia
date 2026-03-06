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

  it('bloquea subida si la extensión no es .dotx/.docx', async () => {
    render(<TemplateUploadForm />);

    fireEvent.change(screen.getByPlaceholderText(/plantilla memoria v1/i), {
      target: { value: 'Plantilla invalida' },
    });

    const fileInput = screen.getByLabelText(/archivo \.dotx\/.docx/i);
    const invalidFile = new File(['dummy'], 'plantilla.pdf', {
      type: 'application/pdf',
    });

    fireEvent.change(fileInput, {
      target: { files: [invalidFile] },
    });

    const submitButton = screen.getByRole('button', { name: /subir plantilla/i });
    fireEvent.submit(submitButton.closest('form') as HTMLFormElement);

    expect(await screen.findByText(/formato no válido/i)).toBeInTheDocument();
    expect(templateService.create).not.toHaveBeenCalled();
  });

  it('muestra mensaje de validación cuando backend devuelve Template.InvalidFormat', async () => {
    vi.mocked(templateService.create).mockRejectedValueOnce(
      new ApiError(
        400,
        'Formato de plantilla inválido: no se pudo procesar el documento Word OpenXML.',
        'Validation.Template.InvalidFormat',
      ),
    );

    render(<TemplateUploadForm />);

    fireEvent.change(screen.getByPlaceholderText(/plantilla memoria v1/i), {
      target: { value: 'Plantilla test' },
    });

    const fileInput = screen.getByLabelText(/archivo \.dotx\/.docx/i);
    const validFile = new File(['dummy'], 'plantilla.dotx', {
      type: 'application/vnd.openxmlformats-officedocument.wordprocessingml.template',
    });

    fireEvent.change(fileInput, {
      target: { files: [validFile] },
    });

    const submitButton = screen.getByRole('button', { name: /subir plantilla/i });
    fireEvent.submit(submitButton.closest('form') as HTMLFormElement);

    const errorBox = await screen.findByText(/la plantilla no cumple el formato requerido/i);
    expect(errorBox).toBeInTheDocument();
    expect(addToastMock).toHaveBeenCalledWith(expect.stringMatching(/no se pudo procesar el documento word openxml/i), 'error');
  });

  it('envía plantilla con drag & drop', async () => {
    vi.mocked(templateService.create).mockResolvedValueOnce('template-id');

    render(<TemplateUploadForm />);

    fireEvent.change(screen.getByPlaceholderText(/plantilla memoria v1/i), {
      target: { value: 'Plantilla drag' },
    });

    const dropZone = screen.getByRole('button', { name: /arrastrar archivo de plantilla o pulsar para seleccionar/i });
    const droppedFile = new File(['dummy'], 'drag-template.docx', {
      type: 'application/vnd.openxmlformats-officedocument.wordprocessingml.document',
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
