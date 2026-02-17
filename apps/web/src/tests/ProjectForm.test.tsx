import { describe, it, expect, vi } from 'vitest';
import { render, screen, waitFor } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { ProjectForm } from '@/components/Admin/ProjectForm';

describe('ProjectForm component', () => {
  const mockOnSubmit = vi.fn();

  beforeEach(() => {
    mockOnSubmit.mockClear();
  });

  it('should render project fields correctly', () => {
    render(<ProjectForm onSubmit={mockOnSubmit} />);
    
    expect(screen.getByLabelText(/nombre del proyecto/i)).toBeInTheDocument();
    expect(screen.getByLabelText(/descripción técnica/i)).toBeInTheDocument();
    expect(screen.getByLabelText(/estado inicial/i)).toBeInTheDocument();
  });

  it('should show validation errors for invalid data', async () => {
    const user = userEvent.setup();
    render(<ProjectForm onSubmit={mockOnSubmit} />);
    
    await user.click(screen.getByRole('button', { name: /crear proyecto/i }));

    expect(await screen.findByText(/el título del proyecto es obligatorio/i)).toBeInTheDocument();
    expect(await screen.findByText(/la descripción debe tener al menos 10 caracteres/i)).toBeInTheDocument();
  });

  it('should call onSubmit with project data when valid', async () => {
    const user = userEvent.setup();
    render(<ProjectForm onSubmit={mockOnSubmit} />);
    
    const titleInput = screen.getByLabelText(/nombre del proyecto/i);
    const descInput = screen.getByLabelText(/descripción técnica/i);
    const statusSelect = screen.getByLabelText(/estado inicial/i);

    await user.clear(titleInput);
    await user.type(titleInput, 'Proyecto de Prueba');

    await user.clear(descInput);
    await user.type(descInput, 'Esta es una descripción técnica válida con más de diez caracteres.');

    await user.selectOptions(statusSelect, 'Active');

    await user.click(screen.getByRole('button', { name: /crear proyecto/i }));

    await waitFor(() => {
      expect(mockOnSubmit).toHaveBeenCalledWith(
        expect.objectContaining({
          title: 'Proyecto de Prueba',
          status: 'Active',
        }),
        expect.anything()
      );
    });
  });
});
