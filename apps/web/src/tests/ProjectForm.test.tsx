import { describe, it, expect, vi } from 'vitest';
import { render, screen, fireEvent, waitFor } from '@testing-library/react';
import { ProjectForm } from '@/components/Admin/ProjectForm';

describe('ProjectForm component', () => {
  const mockOnSubmit = vi.fn();

  it('should render project fields correctly', () => {
    render(<ProjectForm onSubmit={mockOnSubmit} />);
    
    expect(screen.getByLabelText(/nombre del proyecto/i)).toBeInTheDocument();
    expect(screen.getByLabelText(/descripción técnica/i)).toBeInTheDocument();
    expect(screen.getByLabelText(/estado inicial/i)).toBeInTheDocument();
  });

  it('should show validation errors for invalid data', async () => {
    render(<ProjectForm onSubmit={mockOnSubmit} />);
    
    fireEvent.click(screen.getByRole('button', { name: /crear proyecto/i }));

    expect(await screen.findByText(/el título del proyecto es obligatorio/i)).toBeInTheDocument();
    expect(await screen.findByText(/la descripción debe tener al menos 10 caracteres/i)).toBeInTheDocument();
  });

  it('should call onSubmit with project data when valid', async () => {
    render(<ProjectForm onSubmit={mockOnSubmit} />);
    
    fireEvent.change(screen.getByLabelText(/nombre del proyecto/i), { target: { value: 'Proyecto de Prueba' } });
    fireEvent.change(screen.getByLabelText(/descripción técnica/i), { target: { value: 'Esta es una descripción técnica válida de más de 10 caracteres.' } });
    fireEvent.change(screen.getByLabelText(/estado inicial/i), { target: { value: 'Active' } });
    
    // Verificación de valores
    expect(screen.getByLabelText(/nombre del proyecto/i)).toHaveValue('Proyecto de Prueba');

    const form = screen.getByRole('button', { name: /crear proyecto/i });
    fireEvent.click(form);

    await waitFor(() => {
      expect(mockOnSubmit).toHaveBeenCalledWith(expect.objectContaining({
        title: 'Proyecto de Prueba',
        status: 'Active'
      }));
    }, { timeout: 4000 });
  });
});
