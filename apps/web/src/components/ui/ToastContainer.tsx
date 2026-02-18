import { useToastStore, type ToastVariant } from '@/store/useToastStore';
import { X, CheckCircle2, AlertCircle, Info, AlertTriangle } from 'lucide-react';

const variantStyles: Record<ToastVariant, { bg: string; icon: typeof CheckCircle2; iconColor: string }> = {
  success: { bg: 'bg-emerald-500/15 border-emerald-500/25', icon: CheckCircle2, iconColor: 'text-emerald-400' },
  error: { bg: 'bg-red-500/15 border-red-500/25', icon: AlertCircle, iconColor: 'text-red-400' },
  warning: { bg: 'bg-amber-500/15 border-amber-500/25', icon: AlertTriangle, iconColor: 'text-amber-400' },
  info: { bg: 'bg-brand-primary/15 border-brand-primary/25', icon: Info, iconColor: 'text-brand-primary' },
};

/**
 * Fixed-position toast container — render once at the layout level.
 * Reads toasts from Zustand store and auto-dismisses.
 */
export default function ToastContainer() {
  const toasts = useToastStore((s) => s.toasts);
  const removeToast = useToastStore((s) => s.removeToast);

  if (toasts.length === 0) return null;

  return (
    <div className="fixed bottom-6 right-6 z-[9999] flex flex-col gap-3 max-w-sm" role="status" aria-live="polite">
      {toasts.map((t) => {
        const style = variantStyles[t.variant];
        const Icon = style.icon;
        return (
          <div
            key={t.id}
            className={`flex items-start gap-3 px-4 py-3 rounded-xl border backdrop-blur-xl shadow-2xl animate-slide-in ${style.bg}`}
          >
            <Icon size={18} className={`mt-0.5 shrink-0 ${style.iconColor}`} />
            <p className="text-sm text-white flex-1">{t.message}</p>
            <button
              onClick={() => removeToast(t.id)}
              className="text-gray-400 hover:text-white transition-colors shrink-0"
              aria-label="Cerrar notificación"
            >
              <X size={14} />
            </button>
          </div>
        );
      })}
    </div>
  );
}
