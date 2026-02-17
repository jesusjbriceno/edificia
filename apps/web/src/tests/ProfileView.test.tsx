import { describe, it, expect, beforeEach } from 'vitest';
import { render, screen, fireEvent } from '@testing-library/react';
import { ProfileView } from '@/components/Profile/ProfileView';
import { useAuthStore } from '@/store/useAuthStore';

describe('ProfileView component', () => {
  beforeEach(() => {
    // Seed the auth store so ProfileView has user data
    useAuthStore.setState({
      user: {
        id: '1',
        email: 'alvaro@edificia.es',
        fullName: 'Alvaro Arquitecto',
        collegiateNumber: null,
        roles: ['Architect'],
      },
      accessToken: 'tok',
      refreshToken: 'rt',
      mustChangePassword: false,
      isAuthenticated: true,
    });
  });

  it('should render user profile information', () => {
    render(<ProfileView />);

    expect(screen.getByDisplayValue('Alvaro Arquitecto')).toBeInTheDocument();
    expect(screen.getByDisplayValue('alvaro@edificia.es')).toBeInTheDocument();
    expect(screen.getByText('Arquitecto')).toBeInTheDocument();
  });

  it('should allow editing the name', async () => {
    render(<ProfileView />);
    const nameInput = screen.getByDisplayValue('Alvaro Arquitecto');

    fireEvent.change(nameInput, { target: { value: 'Alvaro Editado' } });
    expect(nameInput).toHaveValue('Alvaro Editado');
  });
});
