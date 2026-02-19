import { render, screen, fireEvent, waitFor } from '@testing-library/react';
import { describe, it, expect, vi, beforeEach } from 'vitest';
import LoginForm from '../components/LoginForm';
import { useAuthStore } from '../store/useAuthStore';

// Mock de authService
vi.mock('../lib/services/authService', () => ({
  authService: {
    login: vi.fn(),
  },
}));

// Mock de api (para ApiError)
vi.mock('../lib/api', () => ({
  ApiError: class ApiError extends Error {
    status: number;
    constructor(status: number, message: string) {
      super(message);
      this.status = status;
    }
  },
  default: {},
}));

// Mock de los componentes UI para aislar la lógica del formulario
vi.mock('../components/ui/Input', () => ({
  Input: (props: any) => (
    <input
      data-testid={props.id}
      type={props.type}
      value={props.value}
      onChange={props.onChange}
      placeholder={props.placeholder}
      disabled={props.disabled}
    />
  )
}));

// Mock del Button para evitar problemas con iconos hijos en los tests
vi.mock('../components/ui/Button', () => ({
  Button: (props: any) => (
    <button onClick={props.onClick} type={props.type} disabled={props.disabled}>
      {props.children}
    </button>
  ),
}));

describe('LoginForm', () => {
  beforeEach(() => {
    useAuthStore.setState({
      user: null,
      accessToken: null,
      refreshToken: null,
      isAuthenticated: false,
      isHydrated: false,
    });
    vi.clearAllMocks();
  });

  it('renderiza correctamente los campos de email y contraseña', () => {
    render(<LoginForm />);

    expect(screen.getByTestId('email')).toBeDefined();
    expect(screen.getByTestId('password')).toBeDefined();
    expect(screen.getByRole('button', { name: /entrar en edificia/i })).toBeDefined();
  });

  it('envía las credenciales y redirige al dashboard al hacer login exitoso', async () => {
    const { authService } = await import('../lib/services/authService');

    const mockResponse = {
      accessToken: 'mock-access-token',
      refreshToken: 'mock-refresh-token',
      expiresInMinutes: 60,
      mustChangePassword: false,
      user: {
        id: '1',
        email: 'admin@test.com',
        fullName: 'Admin User',
        collegiateNumber: null,
        roles: ['Admin'],
      },
    };

    (authService.login as any).mockResolvedValueOnce(mockResponse);

    render(<LoginForm />);

    fireEvent.change(screen.getByTestId('email'), { target: { value: 'admin@test.com' } });
    fireEvent.change(screen.getByTestId('password'), { target: { value: 'password123' } });

    const submitBtn = screen.getByRole('button', { name: /entrar en edificia/i });
    fireEvent.click(submitBtn);

    await waitFor(() => {
      expect(authService.login).toHaveBeenCalledWith({
        email: 'admin@test.com',
        password: 'password123',
      });
    });

    await waitFor(() => {
      const state = useAuthStore.getState();
      expect(state.isAuthenticated).toBe(true);
      expect(state.accessToken).toBe('mock-access-token');
      expect(state.user?.fullName).toBe('Admin User');
    });
  });

  it('muestra un mensaje de error cuando las credenciales son inválidas', async () => {
    const { authService } = await import('../lib/services/authService');

    (authService.login as any).mockRejectedValueOnce(
      new Error('Email o contraseña incorrectos.')
    );

    render(<LoginForm />);

    const submitBtn = screen.getByRole('button', { name: /entrar en edificia/i });
    fireEvent.click(submitBtn);

    await waitFor(() => {
      expect(screen.getByText(/email o contraseña incorrectos/i)).toBeDefined();
    });
  });
});