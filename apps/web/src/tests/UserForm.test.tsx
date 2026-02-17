import { describe, it, expect, vi } from 'vitest';
import { render, screen, fireEvent, waitFor } from '@testing-library/react';
import { UserForm } from '@/components/Admin/UserForm';

describe('UserForm component', () => {
  const mockOnSubmit = vi.fn();

  it('should render form fields correctly', () => {
    render(<UserForm onSubmit={mockOnSubmit} />);
    
    expect(screen.getByLabelText(/nombre completo/i)).toBeInTheDocument();
    expect(screen.getByLabelText(/correo electrónico/i)).toBeInTheDocument();
    expect(screen.getByLabelText(/rol del usuario/i)).toBeInTheDocument();
  });

  it('should show validation errors for empty fields', async () => {
    render(<UserForm onSubmit={mockOnSubmit} />);
    
    const submitBtn = screen.getByRole('button', { name: /guardar usuario/i });
    fireEvent.click(submitBtn);

    expect(await screen.findByText(/el nombre es obligatorio/i)).toBeInTheDocument();
    expect(await screen.findByText(/el email es obligatorio/i)).toBeInTheDocument();
  });

  it('should call onSubmit with form data when valid', async () => {
    render(<UserForm onSubmit={mockOnSubmit} />);
    
    const nameInput = screen.getByLabelText(/nombre completo/i);
    const emailInput = screen.getByLabelText(/correo electrónico/i);
    const roleSelect = screen.getByRole('combobox');

    fireEvent.input(nameInput, { target: { value: 'Juan Pérez' } });
    fireEvent.input(emailInput, { target: { value: 'juan@edificia.es' } });
    fireEvent.change(roleSelect, { target: { value: 'Admin' } });
    
    const submitBtn = screen.getByRole('button', { name: /guardar usuario/i });
    fireEvent.click(submitBtn);

    await waitFor(() => {
      expect(mockOnSubmit).toHaveBeenCalledWith(expect.objectContaining({
        name: 'Juan Pérez',
        email: 'juan@edificia.es',
        role: 'Admin'
      }));
    }, { timeout: 6000 });
  });
});
