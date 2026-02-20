export interface Notification {
  id: string;
  title: string;
  message: string;
  isRead: boolean;
  createdAt: string;
}

const STORAGE_KEY = 'edificia_notifications_v2';

const INITIAL_DATA: Notification[] = [
  {
    id: '1',
    title: 'Nuevo Proyecto',
    message: 'Se ha creado el proyecto "Residencial Alhambra"',
    isRead: false,
    createdAt: new Date(Date.now() - 5 * 60000).toISOString(),
  },
  {
    id: '2',
    title: 'Usuario Activo',
    message: 'El usuario "Carlos Pérez" ha iniciado sesión',
    isRead: false,
    createdAt: new Date(Date.now() - 60 * 60000).toISOString(),
  },
  {
    id: '3',
    title: 'Sistema Actualizado',
    message: 'La plataforma se ha actualizado a la versión v1.2',
    isRead: true,
    createdAt: new Date(Date.now() - 120 * 60000).toISOString(),
  },
];

class NotificationService {
  private getStorage(): Notification[] {
    if (typeof window === 'undefined') return INITIAL_DATA;
    const stored = localStorage.getItem(STORAGE_KEY);
    if (!stored) {
      localStorage.setItem(STORAGE_KEY, JSON.stringify(INITIAL_DATA));
      return INITIAL_DATA;
    }
    return JSON.parse(stored);
  }

  private saveStorage(notifications: Notification[]) {
    if (typeof window !== 'undefined') {
      localStorage.setItem(STORAGE_KEY, JSON.stringify(notifications));
      window.dispatchEvent(new CustomEvent('notifications-updated'));
    }
  }

  async list(): Promise<Notification[]> {
    // Simulamos latencia de red
    await new Promise(resolve => setTimeout(resolve, 300));
    return this.getStorage();
  }

  async markAsRead(id: string): Promise<void> {
    const notifications = this.getStorage();
    const updated = notifications.map(n => 
      n.id === id ? { ...n, isRead: true } : n
    );
    this.saveStorage(updated);
  }

  async markAllAsRead(): Promise<void> {
    const notifications = this.getStorage();
    const updated = notifications.map(n => ({ ...n, isRead: true }));
    this.saveStorage(updated);
  }

  async delete(id: string): Promise<void> {
    const notifications = this.getStorage();
    const updated = notifications.filter(n => n.id !== id);
    this.saveStorage(updated);
  }
  
  async clearAll(): Promise<void> {
    this.saveStorage([]);
  }
}

export const notificationService = new NotificationService();
