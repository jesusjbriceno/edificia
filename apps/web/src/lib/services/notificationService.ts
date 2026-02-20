export interface Notification {
  id: string;
  title: string;
  message: string;
  isRead: boolean;
  createdAt: string;
}

const STORAGE_KEY = 'edificia_notifications_v2';

class NotificationService {
  private getStorage(): Notification[] {
    if (typeof window === 'undefined') return [];
    const stored = localStorage.getItem(STORAGE_KEY);
    return stored ? JSON.parse(stored) : [];
  }

  private saveStorage(notifications: Notification[]) {
    if (typeof window !== 'undefined') {
      localStorage.setItem(STORAGE_KEY, JSON.stringify(notifications));
      window.dispatchEvent(new CustomEvent('notifications-updated'));
    }
  }

  async list(): Promise<Notification[]> {
    // Simulamos latencia de red mínima para feedback visual
    await new Promise(resolve => setTimeout(resolve, 150));
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

  /**
   * Método útil para desarrollo/pruebas manuales sin hardcodeo inicial.
   */
  async addTestNotification(title: string, message: string): Promise<void> {
    const notifications = this.getStorage();
    const newNotification: Notification = {
      id: crypto.randomUUID(),
      title,
      message,
      isRead: false,
      createdAt: new Date().toISOString()
    };
    this.saveStorage([newNotification, ...notifications]);
  }
}

export const notificationService = new NotificationService();
