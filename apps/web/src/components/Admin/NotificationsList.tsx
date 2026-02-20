import React, { useState, useEffect, useCallback } from 'react';
import { Bell, Trash2, CheckCircle2, Search, Filter } from 'lucide-react';
import { cn } from '@/lib/utils';
import { notificationService, type Notification } from '@/lib/services/notificationService';
import { Button } from '@/components/ui/Button';

export default function NotificationsList() {
  const [notifications, setNotifications] = useState<Notification[]>([]);
  const [loading, setLoading] = useState(true);
  const [filter, setFilter] = useState<'all' | 'unread'>('all');

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
  }, [fetchNotifications]);

  const filteredNotifications = notifications.filter(n => 
    filter === 'all' ? true : !n.isRead
  );

  const handleMarkAsRead = async (id: string) => {
    await notificationService.markAsRead(id);
    fetchNotifications();
  };

  const handleDelete = async (id: string) => {
    await notificationService.delete(id);
    fetchNotifications();
  };

  const handleClearAll = async () => {
    if (confirm('¿Estás seguro de que quieres eliminar todas las notificaciones?')) {
      await notificationService.clearAll();
      fetchNotifications();
    }
  };

  const handleMarkAllRead = async () => {
    await notificationService.markAllAsRead();
    fetchNotifications();
  };

  return (
    <div className="max-w-4xl mx-auto">
      <div className="flex flex-col sm:flex-row sm:items-center justify-between gap-4 mb-8">
        <div>
          <h1 className="text-2xl font-bold text-white mb-2">Centro de Notificaciones</h1>
          <p className="text-gray-400 text-sm">Gestiona todas tus alertas y avisos del sistema.</p>
        </div>
        <div className="flex items-center gap-3">
          <Button 
            variant="ghost" 
            size="sm" 
            className="text-gray-400 hover:text-white"
            onClick={handleMarkAllRead}
            disabled={!notifications.some(n => !n.isRead)}
          >
            <CheckCircle2 size={16} className="mr-2" />
            Marcar todo como leído
          </Button>
          <Button 
            variant="ghost" 
            size="sm" 
            className="text-red-400 hover:text-red-300 hover:bg-red-500/10"
            onClick={handleClearAll}
            disabled={notifications.length === 0}
          >
            <Trash2 size={16} className="mr-2" />
            Limpiar todo
          </Button>
        </div>
      </div>

      <div className="bg-dark-card/30 backdrop-blur-xl border border-white/5 rounded-2xl overflow-hidden shadow-xl">
        {/* Filters */}
        <div className="px-6 py-4 border-b border-white/5 flex items-center gap-4 bg-white/5">
          <button 
            onClick={() => setFilter('all')}
            className={cn(
              "text-sm font-medium transition-colors relative py-1",
              filter === 'all' ? "text-brand-primary" : "text-gray-400 hover:text-gray-200"
            )}
          >
            Todas
            {filter === 'all' && <span className="absolute bottom-0 left-0 w-full h-0.5 bg-brand-primary rounded-full"></span>}
          </button>
          <button 
            onClick={() => setFilter('unread')}
            className={cn(
              "text-sm font-medium transition-colors relative py-1",
              filter === 'unread' ? "text-brand-primary" : "text-gray-400 hover:text-gray-200"
            )}
          >
            No leídas
            {filter === 'unread' && <span className="absolute bottom-0 left-0 w-full h-0.5 bg-brand-primary rounded-full"></span>}
          </button>
        </div>

        {/* List */}
        <div className="divide-y divide-white/5">
          {loading ? (
            <div className="p-12 text-center text-gray-500">Cargando notificaciones...</div>
          ) : filteredNotifications.length > 0 ? (
            filteredNotifications.map((notification) => (
              <div 
                key={notification.id}
                className={cn(
                  "p-6 flex items-start gap-4 transition-all hover:bg-white/5 group",
                  !notification.isRead && "bg-brand-primary/5"
                )}
              >
                <div className={cn(
                  "w-10 h-10 rounded-full flex items-center justify-center shrink-0 shadow-sm",
                  notification.isRead ? "bg-white/5 text-gray-400" : "bg-brand-primary/20 text-brand-primary border border-brand-primary/20"
                )}>
                  <Bell size={20} />
                </div>
                <div className="flex-1 min-w-0">
                  <div className="flex justify-between items-start mb-1">
                    <h3 className={cn(
                      "font-semibold truncate",
                      notification.isRead ? "text-gray-300" : "text-white"
                    )}>
                      {notification.title}
                    </h3>
                    <span className="text-xs text-gray-500 italic shrink-0 ml-2">
                       {new Date(notification.createdAt).toLocaleString('es-ES', { 
                         day: '2-digit', month: 'short', hour: '2-digit', minute: '2-digit' 
                       })}
                    </span>
                  </div>
                  <p className={cn(
                    "text-sm leading-relaxed mb-3",
                    notification.isRead ? "text-gray-500" : "text-gray-300"
                  )}>
                    {notification.message}
                  </p>
                  <div className="flex items-center gap-4 opacity-0 group-hover:opacity-100 transition-opacity">
                    {!notification.isRead && (
                      <button 
                        onClick={() => handleMarkAsRead(notification.id)}
                        className="text-xs text-brand-primary hover:underline flex items-center gap-1"
                      >
                        <CheckCircle2 size={12} />
                        Marcar como leída
                      </button>
                    )}
                    <button 
                      onClick={() => handleDelete(notification.id)}
                      className="text-xs text-red-500/70 hover:text-red-400 hover:underline flex items-center gap-1"
                    >
                      <Trash2 size={12} />
                      Eliminar
                    </button>
                  </div>
                </div>
              </div>
            ))
          ) : (
            <div className="p-20 text-center">
              <div className="w-16 h-16 rounded-full bg-white/5 flex items-center justify-center mx-auto mb-6 text-gray-600">
                <Bell size={32} />
              </div>
              <p className="text-gray-400 font-medium">No se encontraron notificaciones</p>
              <p className="text-gray-600 text-sm mt-1">
                {filter === 'unread' ? 'No tienes alertas pendientes de lectura.' : 'Tu bandeja de entrada está limpia.'}
              </p>
            </div>
          )}
        </div>
      </div>
    </div>
  );
}
