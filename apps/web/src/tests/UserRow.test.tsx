import { describe, it, expect, vi } from 'vitest';
import { render, screen } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { UserRow } from '@/components/Admin/UserRow';

const mockUser = {
  id: "1",
  name: "Alvaro",
  email: "alvaro@edificia.es",
  role: "Architect",
  status: "Active" as const,
  lastAccess: "Hoy",
};

const mockOnToggleStatus = vi.fn();
const mockOnEdit = vi.fn();

describe('UserRow component', () => {
  beforeEach(() => {
    mockOnToggleStatus.mockClear();
    mockOnEdit.mockClear();
  });

  it('should render user data correctly', () => {
    render(
      <table>
        <tbody>
          <UserRow
            user={mockUser}
            onToggleStatus={mockOnToggleStatus}
            onEdit={mockOnEdit}
          />
        </tbody>
      </table>
    );

    expect(screen.getByText('Alvaro')).toBeInTheDocument();
    expect(screen.getByText('alvaro@edificia.es')).toBeInTheDocument();
    expect(screen.getByText('Architect')).toBeInTheDocument();
    expect(screen.getByText('Activo')).toBeInTheDocument();
  });

  it('should show "Desactivado" for inactive users', () => {
    render(
      <table>
        <tbody>
          <UserRow
            user={{ ...mockUser, status: 'Inactive' }}
            onToggleStatus={mockOnToggleStatus}
            onEdit={mockOnEdit}
          />
        </tbody>
      </table>
    );
    expect(screen.getByText('Desactivado')).toBeInTheDocument();
  });

  it('should not render a delete button', () => {
    render(
      <table>
        <tbody>
          <UserRow
            user={mockUser}
            onToggleStatus={mockOnToggleStatus}
            onEdit={mockOnEdit}
          />
        </tbody>
      </table>
    );
    expect(screen.queryByText('Eliminar usuario')).not.toBeInTheDocument();
  });
});
