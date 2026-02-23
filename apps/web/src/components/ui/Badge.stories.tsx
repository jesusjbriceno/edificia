import type { Meta, StoryObj } from '@storybook/react';
import { Badge } from './Badge';

const meta: Meta<typeof Badge> = {
  title: 'UI/Badge',
  component: Badge,
  tags: ['autodocs'],
  argTypes: {
    variant: {
      control: 'select',
      options: ['default', 'success', 'warning', 'error', 'info'],
      description: 'Variante semántica del badge',
    },
    children: { control: 'text' },
  },
};

export default meta;
type Story = StoryObj<typeof Badge>;

export const Default: Story = {
  args: { variant: 'default', children: 'Borrador' },
};

export const Success: Story = {
  args: { variant: 'success', children: 'Completado' },
};

export const Warning: Story = {
  args: { variant: 'warning', children: 'En revisión' },
};

export const Error: Story = {
  args: { variant: 'error', children: 'Error' },
};

export const Info: Story = {
  args: { variant: 'info', children: 'Obra Nueva' },
};

export const AllVariants: Story = {
  name: 'Todas las variantes',
  render: () => (
    <div className="flex flex-wrap gap-3 p-4">
      <Badge variant="default">Borrador</Badge>
      <Badge variant="success">Completado</Badge>
      <Badge variant="warning">En revisión</Badge>
      <Badge variant="error">Error</Badge>
      <Badge variant="info">Obra Nueva</Badge>
    </div>
  ),
};
