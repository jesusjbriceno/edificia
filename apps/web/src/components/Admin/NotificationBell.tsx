import React, { useState, useEffect, useCallback } from 'react';
import { Bell, BellOff, CheckCircle2 } from 'lucide-react';
import { Dropdown } from '@/components/ui/Dropdown';
import { cn } from '@/lib/utils';
import { notificationService, type Notification } from '@/lib/services/notificationService';

export default function NotificationBell() {
  const [notifications, setNotifications] = useState<Notification[]>([]);
  const [loading, setLoading] = useState(true);

  const fetchNotifications = useCallback(async () => {
    try {
      const data = await notificationService.list();
      setNotifications(data);
    } catch (error) {
      console.error('Error fetching notifications:', error);
    } finally {
      setLoading(false);
    }
  }, []);

  useEffect(() => {
    fetchNotifications();

    // Escuchamos actualizaciones locales (si se marcan como leídas en la página completa)
    window.addEventListener('notifications-updated', fetchNotifications);
    return () => window.removeEventListener('notifications-updated', fetchNotifications);
  }, [fetchNotifications]);

  const hasUnread = notifications.some(n => !n.isRead);

  const markAllAsRead = async (e: React.MouseEvent) => {
    e.stopPropagation();
    await notificationService.markAllAsRead();
    fetchNotifications();
  };

  const markAsRead = async (id: string) => {
    await notificationService.markAsRead(id);
    fetchNotifications();
  };

  return (
    <Dropdown
      align="right"
      className="w-80"
      trigger={
        <button 
          className="p-2 text-gray-400 hover:text-white transition-colors relative"
          aria-label="Notificaciones"
        >
          <Bell size={20} />
          {hasUnread && (
            <span className="absolute top-2 right-2.5 w-2 h-2 bg-brand-primary rounded-full ring-2 ring-dark-bg"></span>
          )}
        </button>
      }
    >
      <div className="flex flex-col max-h-[400px]">
        {/* Header */}
        <div className="px-4 py-3 border-b border-white/10 flex items-center justify-between bg-white/5">
          <h3 className="text-sm font-semibold text-white">Notificaciones</h3>
          {hasUnread && !loading && (
            <button 
              onClick={markAllAsRead}
              className="text-[11px] text-brand-primary hover:text-blue-400 transition-colors flex items-center gap-1"
            >
              <CheckCircle2 size={12} />
              Marcar todo como leído
            </button>
          )}
        </div>

        {/* List */}
        <div className="overflow-y-auto custom-scrollbar">
          {loading ? (
            <div className="p-8 text-center text-gray-500 text-xs">Cargando...</div>
          ) : notifications.length > 0 ? (
            notifications.slice(0, 5).map((notification) => (
              <div 
                key={notification.id}
                onClick={() => markAsRead(notification.id)}
                className={cn(
                  "px-4 py-3 border-b border-white/5 hover:bg-white/5 transition-colors cursor-pointer group",
                  !notification.isRead && "bg-brand-primary/5"
                )}
              >
                <div className="flex justify-between items-start gap-2 mb-1">
                  <span className={cn(
                    "text-xs font-medium",
                    notification.isRead ? "text-gray-400" : "text-white"
                  )}>
                    {notification.title}
                  </span>
                  <span className="text-[10px] text-gray-500 whitespace-nowrap">
                    {new Date(notification.createdAt).toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' })}
                  </span>
                </div>
                <p className={cn(
                  "text-xs leading-relaxed",
                  notification.isRead ? "text-gray-500" : "text-gray-300"
                )}>
                  {notification.message}
                </p>
              </div>
            ))
          ) : (
            <div className="px-8 py-12 flex flex-col items-center justify-center text-center">
              <div className="w-12 h-12 rounded-full bg-white/5 flex items-center justify-center text-gray-500 mb-4">
                <BellOff size={24} />
              </div>
              <p className="text-sm text-gray-400">No tienes notificaciones</p>
            </div>
          )}
        </div>

        {/* Footer */}
        <div className="px-4 py-2 bg-white/5 text-center border-t border-white/10">
          <a 
            href="/admin/notifications"
            className="text-xs text-gray-400 hover:text-white transition-colors block py-1"
          >
            Ver todas las alertas
          </a>
        </div>
      </div>
    </Dropdown>
  );
}
