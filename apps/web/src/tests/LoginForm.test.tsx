import { render, screen, fireEvent, waitFor } from '@testing-library/react';
import { describe, it, expect, vi, beforeEach } from 'vitest';
import LoginForm from '@/components/LoginForm'; // Asumiendo ruta relativa, ajustar si usas alias @
import { useAuthStore } from '@/store/useAuthStore';

// Mock de los componentes UI para aislar la lógica del formulario
vi.mock('@/components/ui/Input', () => ({
  Input: (props: any) => (
    <input 
      data-testid={props.id} 
      onChange={props.onChange} 
      value={props.value} 
      type={props.type}
      disabled={props.disabled}
    />
  )
}));

vi.mock('../components/ui/Button', () => ({
  Button: (props: any) => (
    <button onClick={props.onClick} type={props.type} disabled={props.disabled}>
      {props.children}
    </button>
  )
}));

describe('LoginForm', () => {
  // Mock de fetch global
  const fetchMock = vi.fn();
  global.fetch = fetchMock;

  // Mock de window.location
  Object.defineProperty(window, 'location', {
    value: { href: '' },
    writable: true,
  });

  beforeEach(() => {
    vi.clearAllMocks();
    useAuthStore.setState({ user: null, isAuthenticated: false });
    window.location.href = '';
  });

  it('envía los datos correctamente al endpoint proxy de Astro', async () => {
    // 1. Configurar respuesta exitosa del mock
    fetchMock.mockResolvedValueOnce({
      ok: true,
      json: async () => ({
        success: true,
        user: { id: '1', name: 'Admin', email: 'admin@test.com', role: 'Admin' }
      }),
    });

    render(<LoginForm />);

    // 2. Simular input
    fireEvent.change(screen.getByTestId('email'), { target: { value: 'admin@test.com' } });
    fireEvent.change(screen.getByTestId('password'), { target: { value: 'password123' } });

    // 3. Submit
    const submitBtn = screen.getByText('Ingresar');
    fireEvent.click(submitBtn);

    // 4. Verificar que se llamó a fetch con la URL correcta (Proxy de Astro)
    await waitFor(() => {
      expect(fetchMock).toHaveBeenCalledWith('/api/auth/login', expect.objectContaining({
        method: 'POST',
        body: JSON.stringify({ email: 'admin@test.com', password: 'password123' }),
      }));
    });

    // 5. Verificar redirección
    await waitFor(() => {
      expect(window.location.href).toBe('/dashboard');
    });

    // 6. Verificar actualización del store
    expect(useAuthStore.getState().user?.email).toBe('admin@test.com');
  });

  it('muestra error si el login falla', async () => {
    // 1. Configurar respuesta de error
    fetchMock.mockResolvedValueOnce({
      ok: false,
      json: async () => ({ error: 'Credenciales inválidas' }),
    });

    render(<LoginForm />);

    fireEvent.click(screen.getByText('Ingresar'));

    // 2. Esperar mensaje de error
    await waitFor(() => {
      expect(screen.getByText('Credenciales inválidas')).toBeDefined();
    });
  });
});