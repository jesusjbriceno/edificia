import { useState } from 'react';
import { Button } from '@/components/ui/Button';
import { Lock, Eye, EyeOff, CheckCircle2 } from 'lucide-react';

export function ChangePassword() {
  const [showPass, setShowPass] = useState(false);
  const [isSuccess, setIsSuccess] = useState(false);

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    setIsSuccess(true);
  };

  if (isSuccess) {
    return (
      <div className="bg-emerald-500/10 border border-emerald-500/20 rounded-xl p-6 text-emerald-500 flex items-center gap-4">
        <CheckCircle2 size={24} />
        <div>
          <p className="font-bold">Contraseña actualizada</p>
          <p className="text-sm opacity-80">Tu nueva contraseña ha sido guardada correctamente.</p>
        </div>
      </div>
    );
  }

  return (
    <div className="bg-dark-card/50 border border-white/5 rounded-2xl p-8 backdrop-blur-xl">
      <h3 className="text-xl font-bold text-white mb-6">Cambiar Contraseña</h3>
      
      <form onSubmit={handleSubmit} className="space-y-6 max-w-md">
        <div className="space-y-2">
          <label className="text-xs font-bold uppercase tracking-widest text-gray-500">Contraseña Actual</label>
          <div className="relative">
            <Lock className="absolute left-3 top-1/2 -translate-y-1/2 text-gray-500 w-4 h-4" />
            <input 
              type={showPass ? "text" : "password"}
              className="w-full bg-white/5 border border-white/10 rounded-lg pl-10 pr-10 py-3 text-white focus:border-brand-primary outline-none transition-colors"
            />
          </div>
        </div>

        <div className="space-y-2">
          <label className="text-xs font-bold uppercase tracking-widest text-gray-500">Nueva Contraseña</label>
          <div className="relative">
            <Lock className="absolute left-3 top-1/2 -translate-y-1/2 text-gray-500 w-4 h-4" />
            <input 
              type={showPass ? "text" : "password"}
              className="w-full bg-white/5 border border-white/10 rounded-lg pl-10 pr-10 py-3 text-white focus:border-brand-primary outline-none transition-colors"
            />
            <button 
              type="button"
              onClick={() => setShowPass(!showPass)}
              className="absolute right-3 top-1/2 -translate-y-1/2 text-gray-500 hover:text-white transition-colors"
            >
              {showPass ? <EyeOff size={16} /> : <Eye size={16} />}
            </button>
          </div>
        </div>

        <Button type="submit" className="h-12 px-8">
          Actualizar Seguridad
        </Button>
      </form>
    </div>
  );
}
