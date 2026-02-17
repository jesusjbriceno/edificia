import { useState } from 'react';
import { ProjectRow } from './ProjectRow';
import { ProjectForm } from './ProjectForm';
import { Button } from '@/components/ui/Button';
import { Plus, Search, Filter } from 'lucide-react';

// Mock data inicial
const INITIAL_PROJECTS = [
  {
    id: '1',
    title: 'Reforma Integral Oficina Central',
    description: 'Proyecto de remodelación de la sede de Edificia en Madrid. Incluye nuevas salas de juntas y zona de relax.',
    status: 'Active' as const,
    createdAt: new Date().toISOString(),
    owner: 'Arq. Ana Martínez',
  },
  {
    id: '2',
    title: 'Complejo Residencial Marina',
    description: 'Estudio técnico para la cimentación y estructura de 45 viviendas unifamiliares en la costa.',
    status: 'OnHold' as const,
    createdAt: new Date(Date.now() - 86400000 * 5).toISOString(),
    owner: 'Arq. Juan Pérez',
  }
];

export default function ProjectManagement() {
  const [projects, setProjects] = useState(INITIAL_PROJECTS);
  const [isAdding, setIsAdding] = useState(false);
  const [searchTerm, setSearchTerm] = useState('');

  const handleAddProject = (data: any) => {
    const newProject = {
      id: Math.random().toString(36).substr(2, 9),
      ...data,
      createdAt: new Date().toISOString(),
      owner: 'Admin', // Por ahora
    };
    setProjects([newProject, ...projects]);
    setIsAdding(false);
  };

  const filteredProjects = projects.filter(p => 
    p.title.toLowerCase().includes(searchTerm.toLowerCase()) ||
    p.description.toLowerCase().includes(searchTerm.toLowerCase())
  );

  return (
    <div className="space-y-8">
      <div className="flex flex-col md:flex-row md:items-center justify-between gap-4">
        <div>
          <h1 className="text-3xl font-bold text-white tracking-tight">Gestión de Proyectos</h1>
          <p className="text-gray-400 mt-1">Supervise y gestione todas las obras y proyectos técnicos activos.</p>
        </div>
        
        <Button onClick={() => setIsAdding(!isAdding)} className="h-12 px-6">
          <Plus size={18} className="mr-2" />
          {isAdding ? 'Volver al Listado' : 'Nuevo Proyecto'}
        </Button>
      </div>

      {isAdding ? (
        <div className="animate-in fade-in slide-in-from-bottom-4 duration-500">
          <div className="bg-dark-card border border-white/5 p-8 rounded-3xl relative overflow-hidden">
            <div className="absolute top-0 right-0 p-12 opacity-5 pointer-events-none">
              <Plus size={120} />
            </div>
            <h2 className="text-xl font-bold text-white mb-6">Crear Nuevo Proyecto Técnico</h2>
            <ProjectForm onSubmit={handleAddProject} />
          </div>
        </div>
      ) : (
        <div className="space-y-6">
          {/* Barra de búsqueda y filtros */}
          <div className="flex gap-4 p-2 bg-white/5 rounded-2xl border border-white/5">
            <div className="flex-1 relative">
              <Search className="absolute left-4 top-1/2 -translate-y-1/2 text-gray-500" size={18} />
              <input 
                type="text" 
                placeholder="Buscar por título o descripción..." 
                className="w-full bg-transparent border-none focus:ring-0 text-white pl-12 py-3"
                value={searchTerm}
                onChange={(e) => setSearchTerm(e.target.value)}
              />
            </div>
            <Button variant="ghost" className="hidden sm:flex text-gray-400 hover:text-white">
              <Filter size={18} className="mr-2" />
              Filtrar
            </Button>
          </div>

          {/* Listado de proyectos */}
          <div className="grid gap-4">
            {filteredProjects.length > 0 ? (
              filteredProjects.map(project => (
                <ProjectRow 
                  key={project.id} 
                  project={project}
                  onView={(id) => console.log('Viewing project:', id)}
                  onEdit={(id) => console.log('Editing project:', id)}
                />
              ))
            ) : (
              <div className="py-20 text-center space-y-4 bg-white/2 rounded-3xl border border-dashed border-white/10">
                <p className="text-gray-500">No se han encontrado proyectos que coincidan con la búsqueda.</p>
                <Button variant="outline" onClick={() => setSearchTerm('')} size="sm">Cerrar filtros</Button>
              </div>
            )}
          </div>
        </div>
      )}
    </div>
  );
}
