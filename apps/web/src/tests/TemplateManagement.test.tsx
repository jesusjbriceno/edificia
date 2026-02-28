import { describe, it, expect, vi, beforeEach } from 'vitest';
import { render, screen, waitFor, fireEvent } from '@testing-library/react';
import TemplateManagement from '@/components/Admin/TemplateManagement';
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

describe('TemplateManagement', () => {
  beforeEach(() => {
    vi.clearAllMocks();
    vi.mocked(templateService.list).mockResolvedValue([]);
  });

  it('muestra tags faltantes cuando backend devuelve Template.InvalidFormat', async () => {
    vi.mocked(templateService.create).mockRejectedValueOnce(
      new ApiError(
        400,
        'Formato de plantilla inválido: faltan Tag(s) obligatorios para MemoriaTecnica: ProjectTitle, MD.01, MC.01.',
        'Validation.Template.InvalidFormat',
      ),
    );

    render(<TemplateManagement />);

    await waitFor(() => {
      expect(templateService.list).toHaveBeenCalled();
    });

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
    expect(errorBox.textContent).toContain('ProjectTitle');
    expect(addToastMock).toHaveBeenCalledWith(expect.stringMatching(/faltan estos tags obligatorios/i), 'error');
  });

  it('bloquea subida si la extensión no es .dotx', async () => {
    render(<TemplateManagement />);

    await waitFor(() => {
      expect(templateService.list).toHaveBeenCalled();
    });

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

  it('bloquea subida si el archivo supera 10MB', async () => {
    render(<TemplateManagement />);

    await waitFor(() => {
      expect(templateService.list).toHaveBeenCalled();
    });

    fireEvent.change(screen.getByPlaceholderText(/plantilla memoria v1/i), {
      target: { value: 'Plantilla pesada' },
    });

    const fileInput = screen.getByLabelText(/archivo \.dotx/i);
    const largeFile = new File(['dummy'], 'plantilla.dotx', {
      type: 'application/vnd.openxmlformats-officedocument.wordprocessingml.template',
    });

    Object.defineProperty(largeFile, 'size', { value: 10 * 1024 * 1024 + 1 });

    fireEvent.change(fileInput, {
      target: { files: [largeFile] },
    });

    const submitButton = screen.getByRole('button', { name: /subir plantilla/i });
    fireEvent.submit(submitButton.closest('form') as HTMLFormElement);

    expect(await screen.findByText(/supera el tamaño máximo de 10 MB/i)).toBeInTheDocument();
    expect(templateService.create).not.toHaveBeenCalled();
  });

  it('bloquea subida si el MIME no está permitido', async () => {
    render(<TemplateManagement />);

    await waitFor(() => {
      expect(templateService.list).toHaveBeenCalled();
    });

    fireEvent.change(screen.getByPlaceholderText(/plantilla memoria v1/i), {
      target: { value: 'Plantilla mime inválido' },
    });

    const fileInput = screen.getByLabelText(/archivo \.dotx/i);
    const wrongMimeFile = new File(['dummy'], 'plantilla.dotx', {
      type: 'application/pdf',
    });

    fireEvent.change(fileInput, {
      target: { files: [wrongMimeFile] },
    });

    const submitButton = screen.getByRole('button', { name: /subir plantilla/i });
    fireEvent.submit(submitButton.closest('form') as HTMLFormElement);

    expect(await screen.findByText(/tipo MIME no permitido/i)).toBeInTheDocument();
    expect(templateService.create).not.toHaveBeenCalled();
  });

  it('muestra el nombre del archivo cuando se suelta por drag & drop', async () => {
    render(<TemplateManagement />);

    await waitFor(() => {
      expect(templateService.list).toHaveBeenCalled();
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

    expect(await screen.findByText(/drag-template\.dotx/i)).toBeInTheDocument();
  });

  it('envía plantilla seleccionada por drag & drop', async () => {
    vi.mocked(templateService.create).mockResolvedValueOnce('template-id');

    render(<TemplateManagement />);

    await waitFor(() => {
      expect(templateService.list).toHaveBeenCalled();
    });

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
