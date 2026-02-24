import type { Meta, StoryObj } from '@storybook/react';
import { SyncBadge } from '@/components/Editor/SyncBadge';

const meta: Meta<typeof SyncBadge> = {
  title: 'Editor/SyncBadge',
  component: SyncBadge,
  tags: ['autodocs'],
  parameters: {
    docs: {
      description: {
        component:
          'Indicador visual del estado de sincronización del editor. Refleja de forma accesible (role=status/alert, aria-label) si el contenido está sincronizado, pendiente, en modo offline o en error.',
      },
    },
    backgrounds: { default: 'dark' },
  },
  argTypes: {
    status: {
      control: 'select',
      options: ['idle', 'modified', 'syncing', 'synced', 'offline', 'error'],
      description: 'Estado de sincronización',
    },
    pendingCount: {
      control: { type: 'number', min: 0, max: 99 },
      description: 'Número de cambios pendientes (solo relevante en estado modified)',
    },
  },
};

export default meta;
type Story = StoryObj<typeof SyncBadge>;

export const Idle: Story = {
  name: 'Borrador local (idle)',
  args: { status: 'idle', pendingCount: 0 },
};

export const Syncing: Story = {
  name: 'Sincronizando',
  args: { status: 'syncing', pendingCount: 0 },
};

export const Synced: Story = {
  name: 'Sincronizado',
  args: { status: 'synced', pendingCount: 0 },
};

export const Modified: Story = {
  name: 'Con cambios pendientes',
  args: { status: 'modified', pendingCount: 4 },
};

export const ModifiedSingle: Story = {
  name: 'Con un cambio pendiente (singular)',
  args: { status: 'modified', pendingCount: 1 },
};

export const Offline: Story = {
  name: 'Modo local (offline)',
  args: { status: 'offline', pendingCount: 0 },
};

export const Error: Story = {
  name: 'Error de sincronización',
  args: { status: 'error', pendingCount: 0 },
};

export const AllStates: Story = {
  name: 'Todos los estados',
  render: () => (
    <div className="flex flex-wrap gap-3 p-6 bg-dark-bg">
      <SyncBadge status="idle" pendingCount={0} />
      <SyncBadge status="syncing" pendingCount={0} />
      <SyncBadge status="synced" pendingCount={0} />
      <SyncBadge status="modified" pendingCount={3} />
      <SyncBadge status="offline" pendingCount={0} />
      <SyncBadge status="error" pendingCount={0} />
    </div>
  ),
};
