import { describe, it, expect, vi, beforeEach } from 'vitest';
import { render, screen, waitFor } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { UserForm } from '@/components/Admin/UserForm';

describe('UserForm component', () => {
  const mockOnSubmit = vi.fn();

  beforeEach(() => {
    mockOnSubmit.mockClear();
  });

  it('should render form fields correctly', () => {
    render(<UserForm onSubmit={mockOnSubmit} />);
    
    expect(screen.getByLabelText(/nombre completo/i)).toBeInTheDocument();
    expect(screen.getByLabelText(/correo electrónico/i)).toBeInTheDocument();
    expect(screen.getByLabelText(/rol del usuario/i)).toBeInTheDocument();
  });

  it('should show validation errors for empty fields', async () => {
    const user = userEvent.setup();
    render(<UserForm onSubmit={mockOnSubmit} />);
    
    await user.click(screen.getByRole('button', { name: /guardar usuario/i }));

    expect(await screen.findByText(/el nombre es obligatorio/i)).toBeInTheDocument();
    expect(await screen.findByText(/el email es obligatorio/i)).toBeInTheDocument();
  });

  it('should call onSubmit with form data when valid', async () => {
    const user = userEvent.setup();
    render(<UserForm onSubmit={mockOnSubmit} />);
    
    const nameInput = screen.getByLabelText(/nombre completo/i);
    const emailInput = screen.getByLabelText(/correo electrónico/i);
    const roleSelect = screen.getByLabelText(/rol del usuario/i);

    await user.clear(nameInput);
    await user.type(nameInput, 'Juan Pérez');

    await user.clear(emailInput);
    await user.type(emailInput, 'juan@edificia.es');

    await user.selectOptions(roleSelect, 'Admin');

    await user.click(screen.getByRole('button', { name: /guardar usuario/i }));

    await waitFor(() => {
      expect(mockOnSubmit).toHaveBeenCalledWith(
        expect.objectContaining({
          name: 'Juan Pérez',
          email: 'juan@edificia.es',
          role: 'Admin',
        }),
        expect.anything()
      );
    });
  });
});
