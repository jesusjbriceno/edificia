import type { Meta, StoryObj } from '@storybook/react';
import { useState } from 'react';
import Modal from './Modal';
import { Button } from './Button';

const meta: Meta<typeof Modal> = {
  title: 'UI/Modal',
  component: Modal,
  tags: ['autodocs'],
  parameters: { layout: 'fullscreen' },
  argTypes: {
    title: { control: 'text' },
    isOpen: { control: 'boolean' },
  },
};

export default meta;
type Story = StoryObj<typeof Modal>;

/** Historia interactiva con botón trigger para abrir/cerrar el modal */
export const Interactive: Story = {
  name: 'Interactivo (con trigger)',
  render: () => {
    const [open, setOpen] = useState(false);
    return (
      <div className="flex items-center justify-center min-h-screen bg-dark-bg">
        <Button onClick={() => setOpen(true)}>Abrir modal</Button>
        <Modal isOpen={open} onClose={() => setOpen(false)} title="Confirmar acción">
          <p className="text-gray-400 text-sm">
            ¿Estás seguro de que deseas eliminar este proyecto? Esta acción no se puede deshacer.
          </p>
          <div className="flex justify-end gap-3 mt-6">
            <Button variant="outline" onClick={() => setOpen(false)}>Cancelar</Button>
            <Button variant="primary" onClick={() => setOpen(false)}>Confirmar</Button>
          </div>
        </Modal>
      </div>
    );
  },
};

export const Open: Story = {
  name: 'Abierto (estado fijo)',
  args: {
    isOpen: true,
    title: 'Detalles del proyecto',
    onClose: () => {},
    children: (
      <p className="text-gray-400 text-sm">
        Aquí iría el contenido del modal, como un formulario o información detallada del proyecto.
      </p>
    ),
  },
};
