import { describe, it, expect, vi, beforeEach } from 'vitest';
import { render, screen, fireEvent, waitFor } from '@testing-library/react';
import AiAssistantPanel from '@/components/Editor/AiAssistantPanel';
import { useEditorStore } from '@/store/useEditorStore';

// Mock aiService
vi.mock('@/lib/services/aiService.js', () => ({
  aiService: {
    generate: vi.fn(),
  },
}));

import { aiService } from '@/lib/services/aiService.js';

const mockOnClose = vi.fn();
const mockOnInsertContent = vi.fn();

function renderPanel(isOpen = true) {
  return render(
    <AiAssistantPanel
      isOpen={isOpen}
      onClose={mockOnClose}
      onInsertContent={mockOnInsertContent}
      sectionTitle="1.1 Memoria Descriptiva"
    />,
  );
}

describe('AiAssistantPanel', () => {
  beforeEach(() => {
    vi.clearAllMocks();
    // Set up editor store with a project and active section
    useEditorStore.getState().initProject('proj-123', [], { 'sec-1': '<p>Existing</p>' });
    useEditorStore.getState().setActiveSection('sec-1');
  });

  it('should not render when closed', () => {
    renderPanel(false);
    expect(screen.queryByText('Asistente IA')).not.toBeInTheDocument();
  });

  it('should render panel header and section context when open', () => {
    renderPanel();
    expect(screen.getByText('Asistente IA')).toBeInTheDocument();
    expect(screen.getByText('1.1 Memoria Descriptiva')).toBeInTheDocument();
  });

  it('should render prompt input and suggestions', () => {
    renderPanel();
    expect(screen.getByLabelText('Instrucciones')).toBeInTheDocument();
    expect(screen.getByText('Redacta este apartado según la normativa CTE vigente.')).toBeInTheDocument();
  });

  it('should fill prompt when clicking a suggestion', () => {
    renderPanel();
    fireEvent.click(screen.getByText('Redacta este apartado según la normativa CTE vigente.'));
    const textarea = screen.getByLabelText('Instrucciones') as HTMLTextAreaElement;
    expect(textarea.value).toBe('Redacta este apartado según la normativa CTE vigente.');
  });

  it('should disable generate button when prompt is empty', () => {
    renderPanel();
    const btn = screen.getByRole('button', { name: /generar con ia/i });
    expect(btn).toBeDisabled();
  });

  it('should call aiService.generate and show preview on success', async () => {
    const mockGenerate = vi.mocked(aiService.generate);
    mockGenerate.mockResolvedValueOnce({
      projectId: 'proj-123',
      sectionId: 'sec-1',
      generatedText: '<p>Generated content from AI</p>',
    });

    renderPanel();

    // Type a prompt
    fireEvent.change(screen.getByLabelText('Instrucciones'), {
      target: { value: 'Genera contenido de prueba' },
    });

    // Click generate
    fireEvent.click(screen.getByRole('button', { name: /generar con ia/i }));

    // Wait for preview (generating state is transient with instant mock)
    await waitFor(() => {
      expect(screen.getByText('Vista previa')).toBeInTheDocument();
    });

    // Verify service was called correctly
    expect(mockGenerate).toHaveBeenCalledWith('proj-123', {
      sectionId: 'sec-1',
      prompt: 'Genera contenido de prueba',
      context: '<p>Existing</p>',
    });

    // Should show insert buttons
    expect(screen.getByRole('button', { name: /reemplazar contenido/i })).toBeInTheDocument();
    expect(screen.getByRole('button', { name: /añadir al final/i })).toBeInTheDocument();
  });

  it('should show error state when generation fails', async () => {
    const mockGenerate = vi.mocked(aiService.generate);
    mockGenerate.mockRejectedValueOnce(new Error('Network error'));

    renderPanel();

    fireEvent.change(screen.getByLabelText('Instrucciones'), {
      target: { value: 'Generate something' },
    });
    fireEvent.click(screen.getByRole('button', { name: /generar con ia/i }));

    await waitFor(() => {
      expect(screen.getByText('Network error')).toBeInTheDocument();
    });
  });

  it('should call onInsertContent with replace mode', async () => {
    const mockGenerate = vi.mocked(aiService.generate);
    mockGenerate.mockResolvedValueOnce({
      projectId: 'proj-123',
      sectionId: 'sec-1',
      generatedText: '<p>AI text</p>',
    });

    renderPanel();

    fireEvent.change(screen.getByLabelText('Instrucciones'), {
      target: { value: 'Go' },
    });
    fireEvent.click(screen.getByRole('button', { name: /generar con ia/i }));

    await waitFor(() => {
      expect(screen.getByText('Vista previa')).toBeInTheDocument();
    });

    fireEvent.click(screen.getByRole('button', { name: /reemplazar contenido/i }));

    expect(mockOnInsertContent).toHaveBeenCalledWith('<p>AI text</p>', 'replace');
    expect(mockOnClose).toHaveBeenCalled();
  });

  it('should call onInsertContent with append mode', async () => {
    const mockGenerate = vi.mocked(aiService.generate);
    mockGenerate.mockResolvedValueOnce({
      projectId: 'proj-123',
      sectionId: 'sec-1',
      generatedText: '<p>Appended text</p>',
    });

    renderPanel();

    fireEvent.change(screen.getByLabelText('Instrucciones'), {
      target: { value: 'Append this' },
    });
    fireEvent.click(screen.getByRole('button', { name: /generar con ia/i }));

    await waitFor(() => {
      expect(screen.getByText('Vista previa')).toBeInTheDocument();
    });

    fireEvent.click(screen.getByRole('button', { name: /añadir al final/i }));

    expect(mockOnInsertContent).toHaveBeenCalledWith('<p>Appended text</p>', 'append');
  });

  it('should close panel and reset state', () => {
    renderPanel();

    fireEvent.click(screen.getByLabelText('Cerrar panel IA'));
    expect(mockOnClose).toHaveBeenCalled();
  });

  it('should allow regeneration after preview', async () => {
    const mockGenerate = vi.mocked(aiService.generate);
    mockGenerate.mockResolvedValueOnce({
      projectId: 'proj-123',
      sectionId: 'sec-1',
      generatedText: '<p>First result</p>',
    });

    renderPanel();

    fireEvent.change(screen.getByLabelText('Instrucciones'), {
      target: { value: 'Test' },
    });
    fireEvent.click(screen.getByRole('button', { name: /generar con ia/i }));

    await waitFor(() => {
      expect(screen.getByText('Vista previa')).toBeInTheDocument();
    });

    // Click regenerate
    fireEvent.click(screen.getByRole('button', { name: /regenerar/i }));

    // Should go back to idle with prompt input visible
    expect(screen.getByLabelText('Instrucciones')).toBeInTheDocument();
  });
});
