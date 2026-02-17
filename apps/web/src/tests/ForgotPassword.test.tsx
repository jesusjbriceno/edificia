import { describe, it, expect, vi } from 'vitest';
import { render, screen, fireEvent, waitFor } from '@testing-library/react';
import { ForgotPassword } from '@/components/auth/ForgotPassword';

describe('ForgotPassword component', () => {
  const mockOnSubmit = vi.fn();

  it('should render the recovery form', () => {
    render(<ForgotPassword onSubmit={mockOnSubmit} />);
    expect(screen.getByPlaceholderText(/tu@email.com/i)).toBeInTheDocument();
  });

  it('should show success message after submission', async () => {
    render(<ForgotPassword onSubmit={mockOnSubmit} />);
    
    fireEvent.change(screen.getByPlaceholderText(/tu@email.com/i), { target: { value: 'test@edificia.es' } });
    fireEvent.click(screen.getByRole('button', { name: /enviar instrucciones/i }));

    await waitFor(() => {
      expect(mockOnSubmit).toHaveBeenCalledWith('test@edificia.es');
    });
  });
});
