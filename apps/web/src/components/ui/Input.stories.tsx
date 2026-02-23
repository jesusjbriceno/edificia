import type { Meta, StoryObj } from '@storybook/react';
import { Input } from './Input';

const meta: Meta<typeof Input> = {
  title: 'UI/Input',
  component: Input,
  tags: ['autodocs'],
  argTypes: {
    label: { control: 'text', description: 'Etiqueta sobre el campo' },
    error: { control: 'text', description: 'Mensaje de error (activa estilo rojo)' },
    placeholder: { control: 'text' },
    disabled: { control: 'boolean' },
    type: {
      control: 'select',
      options: ['text', 'email', 'password', 'number'],
    },
  },
};

export default meta;
type Story = StoryObj<typeof Input>;

export const Default: Story = {
  args: { placeholder: 'Introduce un valor...' },
};

export const WithLabel: Story = {
  name: 'Con etiqueta',
  args: { label: 'Nombre del proyecto', placeholder: 'Ej: Vivienda Unifamiliar Calle Mayor' },
};

export const WithError: Story = {
  name: 'Con error',
  args: {
    label: 'Email',
    type: 'email',
    placeholder: 'usuario@ejemplo.com',
    error: 'El email no tiene un formato válido',
  },
};

export const Password: Story = {
  args: { label: 'Contraseña', type: 'password', placeholder: '••••••••' },
};

export const Disabled: Story = {
  args: { label: 'Campo deshabilitado', value: 'Valor de solo lectura', disabled: true },
};
