import { describe, it, expect, vi } from 'vitest';
import { render, screen, fireEvent } from '@testing-library/react';
import { Dropdown } from '@/components/ui/Dropdown';

describe('Dropdown Component', () => {
  it('renders trigger correctly', () => {
    render(
      <Dropdown trigger={<button>Open</button>}>
        <div>Content</div>
      </Dropdown>
    );
    expect(screen.getByText('Open')).toBeInTheDocument();
    expect(screen.queryByText('Content')).not.toBeInTheDocument();
  });

  it('opens and closes clicking the trigger', () => {
    render(
      <Dropdown trigger={<button>Open</button>}>
        <div>Content</div>
      </Dropdown>
    );
    
    const trigger = screen.getByText('Open');
    fireEvent.click(trigger);
    expect(screen.getByText('Content')).toBeInTheDocument();
    
    fireEvent.click(trigger);
    expect(screen.queryByText('Content')).not.toBeInTheDocument();
  });

  it('renders in document.body using Portal', () => {
    render(
      <Dropdown trigger={<button>Open</button>}>
        <div data-testid="dropdown-content">Content</div>
      </Dropdown>
    );
    
    fireEvent.click(screen.getByText('Open'));
    const content = screen.getByTestId('dropdown-content');
    
    // El contenido debería estar fuera del contenedor de renderizado (en el body)
    expect(document.body).toContainElement(content);
  });

  it('closes when clicking outside', () => {
    render(
      <div>
        <Dropdown trigger={<button>Open</button>}>
          <div>Dropdown Content</div>
        </Dropdown>
        <button data-testid="outside">Outside</button>
      </div>
    );
    
    fireEvent.click(screen.getByText('Open'));
    expect(screen.getByText('Dropdown Content')).toBeInTheDocument();
    
    fireEvent.mouseDown(screen.getByTestId('outside'));
    expect(screen.queryByText('Dropdown Content')).not.toBeInTheDocument();
  });
});
