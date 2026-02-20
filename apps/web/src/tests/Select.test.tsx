import { describe, it, expect, vi } from 'vitest';
import { render, screen, fireEvent } from '@testing-library/react';
import { Select } from '@/components/ui/Select';

describe('Select Component', () => {
  const options = [
    { value: '1', label: 'Opción 1' },
    { value: '2', label: 'Opción 2' },
  ];

  it('renders correctly with options', () => {
    render(<Select options={options} label="Mi Selector" />);
    
    expect(screen.getByText('Mi Selector')).toBeInTheDocument();
    expect(screen.getByText('Opción 1')).toBeInTheDocument();
    expect(screen.getByText('Opción 2')).toBeInTheDocument();
  });

  it('calls onChange when selection changes', () => {
    const handleChange = vi.fn();
    render(<Select options={options} onChange={handleChange} />);
    
    const select = screen.getByRole('combobox');
    fireEvent.change(select, { target: { value: '2' } });
    
    expect(handleChange).toHaveBeenCalled();
    expect((select as HTMLSelectElement).value).toBe('2');
  });

  it('displays error message correctly', () => {
    render(<Select options={options} error="Campo requerido" />);
    expect(screen.getByText('Campo requerido')).toBeInTheDocument();
  });

  it('applies custom background and text color to options', () => {
    render(<Select options={options} />);
    const option1 = screen.getByText('Opción 1');
    
    // Verificamos que se apliquen los estilos de legibilidad
    expect(option1).toHaveStyle({
      backgroundColor: 'rgb(22, 22, 24)',
      color: 'rgb(255, 255, 255)'
    });
  });
});
