import { describe, it, expect, vi, beforeEach } from 'vitest';
import { render, screen, waitFor } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { ProjectForm } from '@/components/Admin/ProjectForm';

// Mock the services and api modules
vi.mock('@/lib/services', () => ({
  projectService: {
    create: vi.fn().mockResolvedValue({ id: '1', title: 'Test' }),
  },
}));

vi.mock('@/lib/api', () => ({
  ApiError: class ApiError extends Error {
    status: number;
    constructor(message: string, status: number) {
      super(message);
      this.status = status;
    }
  },
}));

describe('ProjectForm component', () => {
  const mockOnSubmit = vi.fn();

  beforeEach(() => {
    mockOnSubmit.mockClear();
  });

  it('should render project fields correctly', () => {
    render(<ProjectForm onSubmit={mockOnSubmit} />);

    expect(screen.getByLabelText(/título del proyecto/i)).toBeInTheDocument();
    expect(screen.getByLabelText(/descripción/i)).toBeInTheDocument();
    expect(screen.getByLabelText(/tipo de intervención/i)).toBeInTheDocument();
    expect(screen.getByLabelText(/aplica loe/i)).toBeInTheDocument();
  });

  it('should show validation error for empty title', async () => {
    const user = userEvent.setup();
    render(<ProjectForm onSubmit={mockOnSubmit} />);

    await user.click(screen.getByRole('button', { name: /crear proyecto/i }));

    // The title field has a default empty string so it should trigger validation
    // But since it defaults to '' and min(1) is the check:
    // We need to clear the title first if it had a value
    await waitFor(() => {
      expect(screen.getByText(/el título del proyecto es obligatorio/i)).toBeInTheDocument();
    });
  });

  it('should have LOE checkbox checked by default', () => {
    render(<ProjectForm onSubmit={mockOnSubmit} />);

    const loeCheckbox = screen.getByLabelText(/aplica loe/i);
    expect(loeCheckbox).toBeChecked();
  });

  it('should have data-testid on the form', () => {
    render(<ProjectForm onSubmit={mockOnSubmit} />);

    expect(screen.getByTestId('project-form')).toBeInTheDocument();
  });

  it('should map API string interventionType correctly in edit mode', () => {
    render(
      <ProjectForm
        onSubmit={mockOnSubmit}
        project={{
          id: 'aed07b5e-e3e9-4cf6-9d24-ce7e0d8f6e29',
          title: 'PRD 05',
          description: 'ddd',
          address: 'dd',
          interventionType: 'NewConstruction',
          isLoeRequired: true,
          cadastralReference: '2134',
          localRegulations: null,
          status: 'Draft',
          createdAt: '2026-02-28T08:45:02.477368Z',
          updatedAt: '2026-02-28T08:48:00.684699Z',
        }}
      />,
    );

    const interventionSelect = screen.getByLabelText(/tipo de intervención/i) as HTMLSelectElement;
    expect(interventionSelect.value).toBe('0');
  });
});
