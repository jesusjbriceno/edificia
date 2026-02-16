import { describe, it, expect } from 'vitest';
import { render, screen, fireEvent, waitFor } from '@testing-library/react';
import { ProfileView } from './ProfileView';

const mockUser = {
  name: 'Alvaro Arquitecto',
  email: 'alvaro@edificia.es',
  role: 'Architect',
};

describe('ProfileView component', () => {
  it('should render user profile information', () => {
    render(<ProfileView user={mockUser} />);
    
    expect(screen.getByDisplayValue('Alvaro Arquitecto')).toBeInTheDocument();
    expect(screen.getByDisplayValue('alvaro@edificia.es')).toBeInTheDocument();
    expect(screen.getByText('Arquitecto')).toBeInTheDocument();
  });

  it('should allow editing the name', async () => {
    render(<ProfileView user={mockUser} />);
    const nameInput = screen.getByDisplayValue('Alvaro Arquitecto');
    
    fireEvent.change(nameInput, { target: { value: 'Alvaro Editado' } });
    expect(nameInput).toHaveValue('Alvaro Editado');
  });
});
