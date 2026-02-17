import { Button } from '@/components/ui/Button';
import { User, Mail, Shield, Save } from 'lucide-react';

interface ProfileViewProps {
  user: {
    name: string;
    email: string;
    role: string;
  };
}

export function ProfileView({ user }: ProfileViewProps) {
  return (
    <div className="max-w-4xl mx-auto space-y-8">
      <div className="bg-dark-card/50 border border-white/5 rounded-2xl p-8 backdrop-blur-xl">
        <div className="flex items-center gap-6 mb-8">
          <div className="w-20 h-20 rounded-full bg-brand-primary/20 flex items-center justify-center text-brand-primary text-3xl font-bold border border-brand-primary/30">
            {user.name.charAt(0)}
          </div>
          <div>
            <h2 className="text-2xl font-bold text-white">{user.name}</h2>
            <p className="text-gray-400">Gestiona tu información personal y preferencias.</p>
          </div>
        </div>

        <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
          <div className="space-y-2">
            <label className="text-xs font-bold uppercase tracking-widest text-gray-500">Nombre Completo</label>
            <div className="relative">
              <User className="absolute left-3 top-1/2 -translate-y-1/2 text-gray-500 w-4 h-4" />
              <input 
                defaultValue={user.name}
                className="w-full bg-white/5 border border-white/10 rounded-lg pl-10 pr-4 py-3 text-white focus:border-brand-primary outline-none transition-colors"
              />
            </div>
          </div>

          <div className="space-y-2">
            <label className="text-xs font-bold uppercase tracking-widest text-gray-500">Email Profesional</label>
            <div className="relative">
              <Mail className="absolute left-3 top-1/2 -translate-y-1/2 text-gray-500 w-4 h-4" />
              <input 
                defaultValue={user.email}
                className="w-full bg-white/5 border border-white/10 rounded-lg pl-10 pr-4 py-3 text-white focus:border-brand-primary outline-none transition-colors opacity-70 cursor-not-allowed"
                readOnly
              />
            </div>
          </div>

          <div className="space-y-2">
            <label className="text-xs font-bold uppercase tracking-widest text-gray-500">Rol asignado</label>
            <div className="flex items-center gap-3 bg-white/5 border border-white/10 rounded-lg px-4 py-3">
              <Shield className="text-brand-primary w-4 h-4" />
              <span className="text-white font-medium">{user.role === 'Architect' ? 'Arquitecto' : user.role}</span>
            </div>
          </div>
        </div>

        <div className="mt-8 flex justify-end">
          <Button className="h-12 px-8">
            <Save className="w-4 h-4 mr-2" />
            Guardar Cambios
          </Button>
        </div>
      </div>
    </div>
  );
}
