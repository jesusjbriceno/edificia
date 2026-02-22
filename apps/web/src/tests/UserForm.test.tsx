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

  it('should not include Supervisor as a role option', () => {
    render(<UserForm onSubmit={mockOnSubmit} />);
    
    const roleSelect = screen.getByLabelText(/rol del usuario/i);
    const options = Array.from(roleSelect.querySelectorAll('option'));
    const values = options.map((o) => o.value);

    expect(values).toContain('Admin');
    expect(values).toContain('Architect');
    expect(values).toContain('Collaborator');
    expect(values).not.toContain('Supervisor');
  });

  it('should show validation errors for empty fields', async () => {
    const user = userEvent.setup();
    render(<UserForm onSubmit={mockOnSubmit} />);
    
    await user.click(screen.getByRole('button', { name: /guardar usuario/i }));

    expect(await screen.findByText(/el nombre es obligatorio/i)).toBeInTheDocument();
    expect(await screen.findByText(/el email es obligatorio/i)).toBeInTheDocument();
  });

  it('should show CollegiateNumber field when role is Architect', () => {
    render(<UserForm onSubmit={mockOnSubmit} />);
    
    // Default role is Architect, so the field should be visible
    expect(screen.getByLabelText(/número de colegiado/i)).toBeInTheDocument();
  });

  it('should hide CollegiateNumber field when role is not Architect', async () => {
    const user = userEvent.setup();
    render(<UserForm onSubmit={mockOnSubmit} />);
    
    const roleSelect = screen.getByLabelText(/rol del usuario/i);
    await user.selectOptions(roleSelect, 'Admin');

    expect(screen.queryByLabelText(/número de colegiado/i)).not.toBeInTheDocument();
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

  it('should include collegiateNumber in form submission for Architect role', async () => {
    const user = userEvent.setup();
    render(<UserForm onSubmit={mockOnSubmit} />);
    
    const nameInput = screen.getByLabelText(/nombre completo/i);
    const emailInput = screen.getByLabelText(/correo electrónico/i);
    const collegiateInput = screen.getByLabelText(/número de colegiado/i);

    await user.clear(nameInput);
    await user.type(nameInput, 'Ana Martínez');

    await user.clear(emailInput);
    await user.type(emailInput, 'ana@edificia.es');

    await user.clear(collegiateInput);
    await user.type(collegiateInput, '12345-M');

    await user.click(screen.getByRole('button', { name: /guardar usuario/i }));

    await waitFor(() => {
      expect(mockOnSubmit).toHaveBeenCalledWith(
        expect.objectContaining({
          name: 'Ana Martínez',
          email: 'ana@edificia.es',
          role: 'Architect',
          collegiateNumber: '12345-M',
        }),
        expect.anything()
      );
    });
  });
});
