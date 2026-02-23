import type { Meta, StoryObj } from '@storybook/react';
import { Button } from './Button';

const meta: Meta<typeof Button> = {
  title: 'UI/Button',
  component: Button,
  tags: ['autodocs'],
  argTypes: {
    variant: {
      control: 'select',
      options: ['primary', 'secondary', 'outline', 'ghost'],
      description: 'Estilo visual del botón',
    },
    size: {
      control: 'select',
      options: ['sm', 'md', 'lg', 'icon'],
      description: 'Tamaño del botón',
    },
    isLoading: {
      control: 'boolean',
      description: 'Muestra spinner y deshabilita el botón',
    },
    disabled: {
      control: 'boolean',
    },
    children: {
      control: 'text',
    },
  },
};

export default meta;
type Story = StoryObj<typeof Button>;

export const Primary: Story = {
  args: { variant: 'primary', children: 'Crear proyecto' },
};

export const Secondary: Story = {
  args: { variant: 'secondary', children: 'Cancelar' },
};

export const Outline: Story = {
  args: { variant: 'outline', children: 'Ver detalles' },
};

export const Ghost: Story = {
  args: { variant: 'ghost', children: 'Más opciones' },
};

export const Loading: Story = {
  args: { variant: 'primary', isLoading: true, children: 'Guardando...' },
};

export const Disabled: Story = {
  args: { variant: 'primary', disabled: true, children: 'No disponible' },
};

export const Small: Story = {
  args: { variant: 'primary', size: 'sm', children: 'Pequeño' },
};

export const Large: Story = {
  args: { variant: 'primary', size: 'lg', children: 'Botón Grande' },
};

export const AllVariants: Story = {
  name: 'Todas las variantes',
  render: () => (
    <div className="flex flex-wrap gap-3 p-4">
      <Button variant="primary">Primary</Button>
      <Button variant="secondary">Secondary</Button>
      <Button variant="outline">Outline</Button>
      <Button variant="ghost">Ghost</Button>
    </div>
  ),
};
