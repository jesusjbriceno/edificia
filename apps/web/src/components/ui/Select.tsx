import * as React from "react";
import { ChevronDown } from "lucide-react";
import { cn } from "@/lib/utils";

export interface SelectOption {
  value: string | number;
  label: string;
}

interface SelectProps extends React.SelectHTMLAttributes<HTMLSelectElement> {
  options: SelectOption[];
  label?: string;
  error?: string;
  containerClassName?: string;
}

/**
 * Componente Select premium para Edificia.
 * Resuelve problemas de legibilidad en modo oscuro al usar estilos personalizados
 * en lugar de los nativos del navegador donde sea posible, o aplicando reset de estilos CSS.
 */
export const Select = React.forwardRef<HTMLSelectElement, SelectProps>(
  ({ options, label, error, className, containerClassName, ...props }, ref) => {

    return (
      <div className={cn("space-y-2 w-full", containerClassName)}>
        {label && (
          <label 
            htmlFor={props.id}
            className="text-sm font-medium text-gray-300 block cursor-pointer"
          >
            {label}
          </label>
        )}
        <div className="relative">
          <select
            ref={ref}
            className={cn(
              "w-full bg-white/5 border border-white/10 rounded-lg px-4 py-2.5 text-white focus:border-brand-primary outline-none transition-colors appearance-none cursor-pointer",
              "focus:ring-1 focus:ring-brand-primary/50",
              error && "border-red-500/50 focus:border-red-500",
              className
            )}
            style={{
               /* Forzamos el color de fondo de las opciones en navegadores que lo permitan */
               backgroundColor: "#161618" 
            }}
            {...props}
          >
            {options.map((option) => (
              <option 
                key={option.value} 
                value={option.value}
                className="bg-dark-card text-white py-2"
                style={{ backgroundColor: "#161618", color: "#ffffff" }}
              >
                {option.label}
              </option>
            ))}
          </select>
          <div className="absolute right-3 top-1/2 -translate-y-1/2 pointer-events-none text-gray-500">
            <ChevronDown size={18} />
          </div>
        </div>
        {error && <p className="text-xs text-red-500 mt-1">{error}</p>}
      </div>
    );
  }
);

Select.displayName = "Select";
