import { useState } from 'react';
import { Button } from '@/components/ui/Button';
import { Mail, ArrowLeft } from 'lucide-react';

interface ForgotPasswordProps {
  onSubmit: (email: string) => void;
}

export function ForgotPassword({ onSubmit }: ForgotPasswordProps) {
  const [email, setEmail] = useState('');
  const [isSent, setIsSent] = useState(false);

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    onSubmit(email);
    setIsSent(true);
  };

  if (isSent) {
    return (
      <div className="text-center space-y-4">
        <div className="w-16 h-16 bg-brand-primary/20 rounded-full flex items-center justify-center mx-auto text-brand-primary">
          <Mail size={32} />
        </div>
        <h2 className="text-2xl font-bold text-white">¡Enviado!</h2>
        <p className="text-gray-400">Revisa tu correo para las instrucciones de recuperación.</p>
        <a href="/" className="inline-block text-brand-primary hover:underline text-sm font-medium">Volver al inicio de sesión</a>
      </div>
    );
  }

  return (
    <div className="space-y-6">
      <div className="space-y-2">
        <h2 className="text-2xl font-bold text-white">¿Olvidaste tu contraseña?</h2>
        <p className="text-gray-400 text-sm">Introduce tu email y te enviaremos los pasos para restablecerla.</p>
      </div>

      <form onSubmit={handleSubmit} className="space-y-4">
        <div className="space-y-2">
          <label htmlFor="email-recovery" className="text-xs font-bold uppercase tracking-widest text-gray-500">Email Profesional</label>
          <input
            id="email-recovery"
            type="email"
            value={email}
            onChange={(e) => setEmail(e.target.value)}
            className="w-full bg-white/5 border border-white/10 rounded-lg px-4 py-3 text-white focus:border-brand-primary outline-none transition-colors"
            placeholder="tu@email.com"
            required
          />
        </div>

        <Button type="submit" className="w-full h-12">
          Enviar instrucciones
        </Button>

        <a href="/" className="flex items-center justify-center gap-2 text-gray-400 hover:text-white text-sm transition-colors">
          <ArrowLeft size={14} />
          Volver atrás
        </a>
      </form>
    </div>
  );
}
