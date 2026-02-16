import { describe, it, expect } from 'vitest';
import { render, screen } from '@testing-library/react';
import { UserRow } from '@/components/Admin/UserRow';

const mockUser = {
  id: "1",
  name: "Alvaro",
  email: "alvaro@edificia.es",
  role: "Architect",
  status: "Active" as const,
  lastAccess: "Hoy",
};

describe('UserRow component', () => {
  it('should render user data correctly', () => {
    render(
      <table>
        <tbody>
          <UserRow user={mockUser} />
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
          <UserRow user={{ ...mockUser, status: 'Inactive' }} />
        </tbody>
      </table>
    );
    expect(screen.getByText('Desactivado')).toBeInTheDocument();
  });
});
