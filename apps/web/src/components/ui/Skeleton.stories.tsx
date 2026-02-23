import type { Meta, StoryObj } from '@storybook/react';
import { Skeleton, ProjectCardSkeleton, ProjectGridSkeleton } from './Skeleton';

const meta: Meta<typeof Skeleton> = {
  title: 'UI/Skeleton',
  component: Skeleton,
  tags: ['autodocs'],
  parameters: { layout: 'padded' },
  argTypes: {
    className: { control: 'text', description: 'Clases Tailwind adicionales para dimensionar el placeholder' },
  },
};

export default meta;
type Story = StoryObj<typeof Skeleton>;

export const Default: Story = {
  args: { className: 'h-8 w-64' },
};

export const TextLines: Story = {
  name: 'Líneas de texto',
  render: () => (
    <div className="space-y-2 w-72">
      <Skeleton className="h-4 w-full" />
      <Skeleton className="h-4 w-5/6" />
      <Skeleton className="h-4 w-4/6" />
    </div>
  ),
};

export const ProjectCard: Story = {
  name: 'Tarjeta de proyecto',
  render: () => (
    <div className="w-80">
      <ProjectCardSkeleton />
    </div>
  ),
};

export const ProjectGrid: Story = {
  name: 'Rejilla de proyectos',
  parameters: { layout: 'fullscreen' },
  render: () => (
    <div className="p-8 min-h-screen bg-dark-bg">
      <ProjectGridSkeleton count={6} />
    </div>
  ),
};
