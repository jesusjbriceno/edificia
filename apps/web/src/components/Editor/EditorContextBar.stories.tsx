import type { Meta, StoryObj } from '@storybook/react';
import { EditorContextBar } from '@/components/Editor/EditorContextBar';
import type { EditorContextBarProps } from '@/components/Editor/EditorContextBar';

const ACTIVE_PATH = ['Proyecto de Ejecución', '1. Memoria', '1.1 Memoria Descriptiva'];

const meta: Meta<typeof EditorContextBar> = {
  title: 'Editor/EditorContextBar',
  component: EditorContextBar,
  tags: ['autodocs'],
  parameters: {
    docs: {
      description: {
        component: `
Barra contextual del editor que muestra las migas de pan, el estado de sincronización
y las acciones principales: **Enviar a Revisión**, **IA** y **Exportar**.

Este componente es completamente presentacional (sin acoplamiento al store) lo que facilita
su uso en tests y Storybook. Las acciones se delegan mediante callbacks.

**Accesibilidad (WCAG AA):**
- Botón IA con \`aria-pressed\` (toggle accesible)
- Estados de carga con \`aria-busy\` y texto \`sr-only\` para lectores de pantalla
- Migas de pan con \`aria-current="page"\` en la sección activa
- Toolbar de acciones con \`role="toolbar"\` y \`aria-label\`
- \`SyncBadge\` con \`role="status"\` / \`role="alert"\` según el estado`,
      },
    },
    backgrounds: { default: 'dark' },
    layout: 'fullscreen',
  },
  argTypes: {
    syncStatus: {
      control: 'select',
      options: ['idle', 'modified', 'syncing', 'synced', 'offline', 'error'],
    },
    pendingCount: { control: { type: 'number', min: 0, max: 20 } },
    isReadonly: { control: 'boolean' },
    canSubmitForReview: { control: 'boolean' },
    submittingReview: { control: 'boolean' },
    aiPanelOpen: { control: 'boolean' },
    exporting: { control: 'boolean' },
  },
};

export default meta;
type Story = StoryObj<typeof EditorContextBar>;

const defaultArgs: EditorContextBarProps = {
  activePath: ACTIVE_PATH,
  syncStatus: 'synced',
  pendingCount: 0,
  isReadonly: false,
  canSubmitForReview: false,
  submittingReview: false,
  aiPanelOpen: false,
  exporting: false,
  projectId: 'proj-123',
  onSubmitForReview: () => {},
  onToggleAi: () => {},
  onExport: () => {},
};

export const Default: Story = {
  name: 'Por defecto (Sincronizado)',
  args: defaultArgs,
};

export const WithSubmitButton: Story = {
  name: 'Con botón "Enviar a Revisión" (Draft/InProgress)',
  args: { ...defaultArgs, canSubmitForReview: true, syncStatus: 'modified', pendingCount: 2 },
};

export const SubmittingReview: Story = {
  name: 'Enviando a revisión (cargando)',
  args: { ...defaultArgs, canSubmitForReview: true, submittingReview: true },
};

export const ReadOnly: Story = {
  name: 'Solo lectura (PendingReview / Completed)',
  args: {
    ...defaultArgs,
    isReadonly: true,
    canSubmitForReview: false,
    syncStatus: 'synced',
  },
};

export const AiPanelOpen: Story = {
  name: 'Panel IA abierto (aria-pressed=true)',
  args: { ...defaultArgs, aiPanelOpen: true },
};

export const Exporting: Story = {
  name: 'Exportando DOCX (cargando)',
  args: { ...defaultArgs, exporting: true },
};

export const OfflineMode: Story = {
  name: 'Modo offline',
  args: {
    ...defaultArgs,
    syncStatus: 'offline',
    canSubmitForReview: true,
  },
};

export const SyncError: Story = {
  name: 'Error de sincronización',
  args: { ...defaultArgs, syncStatus: 'error' },
};

export const NoProject: Story = {
  name: 'Sin proyecto guardado (Exportar deshabilitado)',
  args: { ...defaultArgs, projectId: null },
};
