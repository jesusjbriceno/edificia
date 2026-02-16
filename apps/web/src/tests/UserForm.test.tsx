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
    
    fireEvent.click(screen.getByRole('button', { name: /guardar usuario/i }));

    await waitFor(() => {
      expect(screen.getByText(/el nombre es obligatorio/i)).toBeInTheDocument();
      expect(screen.getByText(/el email es obligatorio/i)).toBeInTheDocument();
    });
  });

  it('should call onSubmit with form data when valid', async () => {
    render(<UserForm onSubmit={mockOnSubmit} />);
    
    fireEvent.change(screen.getByLabelText(/nombre completo/i), { target: { value: 'Juan Pérez' } });
    fireEvent.change(screen.getByLabelText(/correo electrónico/i), { target: { value: 'juan@edificia.es' } });
    fireEvent.change(screen.getByLabelText(/rol del usuario/i), { target: { value: 'Admin' } });
    
    const form = screen.getByTestId('user-form');
    fireEvent.submit(form);

    await waitFor(() => {
      expect(mockOnSubmit).toHaveBeenCalledWith(expect.objectContaining({
        name: 'Juan Pérez',
        email: 'juan@edificia.es',
        role: 'Admin',
      }));
    }, { timeout: 4000 });
  });
});
