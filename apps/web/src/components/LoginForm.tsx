import { useState } from "react";
import { Button } from "@/components/ui/Button";
import { Input } from "@/components/ui/Input";
import { LogIn } from "lucide-react";

export default function LoginForm() {
  const [isLoading, setIsLoading] = useState(false);

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    setIsLoading(true);
    // Simular login
    setTimeout(() => setIsLoading(false), 2000);
  };

  return (
    <div className="w-full max-w-md space-y-8 p-8 md:p-12">
      <div className="space-y-2">
        <h1 className="text-4xl font-bold tracking-tight text-white flex items-center gap-2">
          <span className="text-brand-primary">E</span>DIFICIA
        </h1>
        <p className="text-gray-400 text-sm">
          Plataforma avanzada de redacción de memorias.
        </p>
      </div>

      <form onSubmit={handleSubmit} className="space-y-6">
        <div className="space-y-4">
          <Input 
            label="Email" 
            type="email" 
            placeholder="arquitecto@edificia.es" 
            required 
          />
          <Input 
            label="Contraseña" 
            type="password" 
            placeholder="••••••••" 
            required 
          />
        </div>

        <div className="flex items-center justify-between">
          <div className="flex items-center space-x-2">
            <input 
              type="checkbox" 
              id="remember" 
              className="rounded border-white/10 bg-white/5 text-brand-primary focus:ring-brand-primary" 
            />
            <label htmlFor="remember" className="text-xs text-gray-400">
              Recordar acceso
            </label>
          </div>
          <a href="#" className="text-xs text-brand-primary hover:underline">
            ¿Has olvidado la contraseña?
          </a>
        </div>

        <Button type="submit" className="w-full h-12" isLoading={isLoading}>
          <LogIn className="w-4 h-4 mr-2" />
          Iniciar Sesión
        </Button>
      </form>

      <div className="pt-6 border-t border-white/5">
        <p className="text-xs text-center text-gray-500">
          ¿No tienes cuenta? <a href="#" className="text-brand-primary hover:underline">Contacta con tu administrador</a>
        </p>
      </div>
    </div>
  );
}
