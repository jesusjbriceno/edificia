import React, { useState } from 'react';
import { useAuthStore } from '@/store/useAuthStore';
import { authService } from '@/lib/services/authService';
import { ApiError } from '@/lib/api';
import { Button } from './ui/Button';
import { Input } from './ui/Input';
import { LogIn, AlertCircle } from 'lucide-react';

export default function LoginForm() {
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [error, setError] = useState('');
  const [isLoading, setIsLoading] = useState(false);
  const login = useAuthStore((state) => state.login);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError('');
    setIsLoading(true);

    try {
      // Llamada directa al backend .NET vía authService (Axios)
      const data = await authService.login({ email, password });

      // Guardar tokens y usuario en el store (persiste en localStorage)
      login({
        user: data.user,
        accessToken: data.accessToken,
        refreshToken: data.refreshToken ?? '',
      });

      // Redirigir al dashboard
      window.location.href = '/dashboard';
      
    } catch (err: any) {
      console.error('Login error:', err);
      if (err instanceof ApiError) {
        setError(err.message || 'Credenciales inválidas');
      } else {
        setError(err.message || 'Ocurrió un error inesperado');
      }
      setIsLoading(false);
    }
  };

  return (
    <div className="w-full max-w-md space-y-8 p-8 md:p-12 animate-in fade-in slide-in-from-bottom-4 duration-700">
      <div className="space-y-3">
      <div className="flex justify-center mb-4">
        <a href="/">
          <img 
            src="/logo-completo.webp" 
            alt="EDIFICIA" 
            className="h-20 w-auto object-contain"
          />
        </a>
      </div>
      <div className="space-y-3">
        <p className="text-gray-400 text-sm font-medium text-center">
          Plataforma avanzada de gestión y redacción de memorias técnicas.
        </p>
      </div>
      </div>

      {error && (
        <div className="flex items-center gap-3 bg-red-500/10 border border-red-500/20 rounded-xl px-4 py-3 text-red-400 text-sm animate-in zoom-in-95 duration-200">
          <AlertCircle className="w-5 h-5 shrink-0" />
          <span>{error}</span>
        </div>
      )}

      <form onSubmit={handleSubmit} className="space-y-6">
        <div className="space-y-5">
          <Input
            id="email"
            label="Email"
            type="email"
            placeholder="arquitecto@edificia.es"
            value={email}
            onChange={(e) => setEmail(e.target.value)}
            required
            disabled={isLoading}
            className="bg-white/5 border-white/10 text-white placeholder:text-gray-600 focus:border-brand-primary/50"
          />
          <Input
            id="password"
            label="Contraseña"
            type="password"
            placeholder="••••••••"
            value={password}
            onChange={(e) => setPassword(e.target.value)}
            required
            disabled={isLoading}
            className="bg-white/5 border-white/10 text-white placeholder:text-gray-600 focus:border-brand-primary/50"
          />
        </div>

        <div className="flex items-center justify-between">
          <div className="flex items-center space-x-2 opacity-70 hover:opacity-100 transition-opacity">
            <input
              type="checkbox"
              id="remember"
              className="w-4 h-4 rounded border-white/10 bg-white/5 text-brand-primary focus:ring-brand-primary"
            />
            <label htmlFor="remember" className="text-xs text-gray-400 cursor-pointer">
              Recordar acceso
            </label>
          </div>
          <a href="/forgot-password" className="text-xs text-brand-primary hover:text-blue-400 font-medium transition-colors">
            ¿Has olvidado la contraseña?
          </a>
        </div>

        <Button 
          type="submit" 
          className="w-full h-12 text-base font-semibold shadow-lg shadow-brand-primary/20" 
          isLoading={isLoading}
          disabled={isLoading}
        >
          {!isLoading && <LogIn className="w-4 h-4 mr-2" />}
          {isLoading ? 'Iniciando sesión...' : 'Entrar en EdificIA'}
        </Button>
      </form>

      <div className="pt-6 border-t border-white/5">
        <p className="text-xs text-center text-gray-500">
          ¿No tienes cuenta?{' '}
          <button type="button" className="text-brand-primary hover:text-blue-400 font-medium transition-colors underline-offset-4 hover:underline">
            Contacta con tu administrador
          </button>
        </p>
      </div>
    </div>
  );
}