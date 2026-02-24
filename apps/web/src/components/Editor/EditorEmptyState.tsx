import { FileText } from 'lucide-react';

/**
 * Estado vacío del editor cuando no hay ninguna sección activa seleccionada.
 * Se muestra mientras el usuario no ha elegido un capítulo en la barra lateral.
 */
export function EditorEmptyState() {
  return (
    <div
      role="status"
      aria-label="Ningún capítulo seleccionado"
      className="flex-1 flex flex-col items-center justify-center text-gray-500 space-y-6 bg-dark-bg/20 backdrop-blur-sm"
    >
      <div className="p-8 bg-white/5 rounded-full animate-pulse border border-white/5" aria-hidden="true">
        <FileText size={64} className="opacity-10" />
      </div>
      <div className="text-center space-y-2">
        <h3 className="text-white/80 font-bold text-lg">Editor de Memoria</h3>
        <p className="text-sm max-w-70">
          Selecciona un capítulo de la barra lateral para empezar el proceso de redacción técnica.
        </p>
      </div>
    </div>
  );
}
