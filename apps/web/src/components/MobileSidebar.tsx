import { useState, useEffect, useRef } from 'react';
import { LayoutDashboard, FolderKanban, Users, Settings, X, Menu } from 'lucide-react';
import SidebarLogout from '@/components/SidebarLogout';

const NAV_ITEMS = [
  { href: '/dashboard', label: 'Inicio', Icon: LayoutDashboard },
  { href: '/admin/projects', label: 'Proyectos', Icon: FolderKanban },
  { href: '/admin/users', label: 'Usuarios', Icon: Users },
] as const;

export function MobileSidebarTrigger({ onClick }: { onClick: () => void }) {
  return (
    <button
      type="button"
      onClick={onClick}
      className="md:hidden p-2 text-gray-400 hover:text-white transition-colors"
      aria-label="Abrir menú"
    >
      <Menu size={22} />
    </button>
  );
}

export default function MobileSidebar() {
  const [open, setOpen] = useState(false);
  const sidebarRef = useRef<HTMLDivElement>(null);

  // Close on ESC
  useEffect(() => {
    const handler = (e: KeyboardEvent) => {
      if (e.key === 'Escape') setOpen(false);
    };
    if (open) document.addEventListener('keydown', handler);
    return () => document.removeEventListener('keydown', handler);
  }, [open]);

  // Prevent body scroll when open
  useEffect(() => {
    document.body.style.overflow = open ? 'hidden' : '';
    return () => { document.body.style.overflow = ''; };
  }, [open]);

  const currentPath = globalThis.location?.pathname ?? '';

  const isActive = (href: string) =>
    currentPath === href || currentPath.startsWith(href + '/');

  return (
    <>
      {/* Trigger button — visible only on mobile */}
      <button
        type="button"
        onClick={() => setOpen(true)}
        className="md:hidden p-2 text-gray-400 hover:text-white transition-colors"
        aria-label="Abrir menú"
      >
        <Menu size={22} />
      </button>

      {/* Overlay + Sidebar */}
      {open && (
        <div className="fixed inset-0 z-50 md:hidden">
          {/* Backdrop */}
          <div
            className="absolute inset-0 bg-black/60 backdrop-blur-sm"
            onClick={() => setOpen(false)}
          />

          {/* Sidebar panel */}
          <aside
            ref={sidebarRef}
            className="absolute left-0 top-0 h-full w-72 bg-dark-bg border-r border-white/5 flex flex-col animate-in slide-in-from-left duration-200"
          >
            {/* Header */}
            <div className="flex items-center justify-between p-6">
              <a href="/dashboard">
                <h2 className="text-2xl font-bold tracking-tight text-white">
                  <span className="text-brand-primary">E</span>DIFICIA
                </h2>
              </a>
              <button
                type="button"
                onClick={() => setOpen(false)}
                className="p-1 text-gray-400 hover:text-white transition-colors"
                aria-label="Cerrar menú"
              >
                <X size={20} />
              </button>
            </div>

            {/* Navigation */}
            <nav className="flex-1 px-4 space-y-2 mt-2">
              {NAV_ITEMS.map(({ href, label, Icon }) => (
                <a
                  key={href}
                  href={href}
                  className={
                    isActive(href)
                      ? 'flex items-center gap-3 px-4 py-3 text-sm font-medium text-white bg-brand-primary/10 border border-brand-primary/20 rounded-lg transition-all'
                      : 'flex items-center gap-3 px-4 py-3 text-sm font-medium text-gray-400 hover:text-white hover:bg-white/5 rounded-lg transition-all'
                  }
                >
                  <Icon size={18} className={isActive(href) ? 'text-brand-primary' : ''} />
                  {label}
                </a>
              ))}
            </nav>

            {/* Footer */}
            <div className="p-4 border-t border-white/5 space-y-2">
              <span className="flex items-center gap-3 px-4 py-3 text-sm font-medium text-gray-500 cursor-not-allowed rounded-lg opacity-50">
                <Settings size={18} />
                Ajustes
              </span>
              <SidebarLogout />
            </div>
          </aside>
        </div>
      )}
    </>
  );
}
