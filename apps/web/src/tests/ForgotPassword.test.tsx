import { describe, it, expect, vi } from 'vitest';
import { render, screen, fireEvent, waitFor } from '@testing-library/react';
import { ForgotPassword } from '@/components/auth/ForgotPassword';

vi.mock('@/lib/services/authService', () => ({
  authService: {
    forgotPassword: vi.fn().mockResolvedValue(undefined),
  },
}));

import { authService } from '@/lib/services/authService';

describe('ForgotPassword component', () => {
  it('should render the recovery form', () => {
    render(<ForgotPassword />);
    expect(screen.getByPlaceholderText(/tu@email.com/i)).toBeInTheDocument();
  });

  it('should show success message after submission', async () => {
    render(<ForgotPassword />);

    fireEvent.change(screen.getByPlaceholderText(/tu@email.com/i), { target: { value: 'test@edificia.es' } });
    fireEvent.click(screen.getByRole('button', { name: /enviar instrucciones/i }));

    await waitFor(() => {
      expect(authService.forgotPassword).toHaveBeenCalledWith({ email: 'test@edificia.es' });
      expect(screen.getByText(/¡Enviado!/i)).toBeInTheDocument();
    });
  });

  it('should show error message when API call fails', async () => {
    vi.mocked(authService.forgotPassword).mockRejectedValueOnce(new Error('Network error'));

    render(<ForgotPassword />);

    fireEvent.change(screen.getByPlaceholderText(/tu@email.com/i), { target: { value: 'test@edificia.es' } });
    fireEvent.click(screen.getByRole('button', { name: /enviar instrucciones/i }));

    await waitFor(() => {
      expect(screen.getByText(/No se pudo procesar/i)).toBeInTheDocument();
    });
  });
});
