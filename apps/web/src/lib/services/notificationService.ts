import apiClient from '@/lib/api';

export interface Notification {
  id: string;
  title: string;
  message: string;
  isRead: boolean;
  createdAt: string;
}

class NotificationService {
  async list(): Promise<Notification[]> {
    const response = await apiClient.get<Notification[]>('/notifications');
    return response.data;
  }

  async markAsRead(id: string): Promise<void> {
    await apiClient.post(`/notifications/${id}/read`);
  }

  async markAllAsRead(): Promise<void> {
    await apiClient.post('/notifications/mark-all-read');
  }

  async delete(id: string): Promise<void> {
    // El backend actual solo marca como leída o lista. 
    // Si no hay endpoint de delete, podemos dejarlo como placeholder o implementarlo.
    // Por ahora, asumimos que existe o lo ignoramos si no es crítico.
    await apiClient.post(`/notifications/${id}/delete`);
  }
  
  async clearAll(): Promise<void> {
    await apiClient.post('/notifications/clear-all');
  }

  /** Helpful for testing, although in prod it would be triggered by backend events */
  async addTestNotification(title: string, message: string): Promise<void> {
    await apiClient.post('/notifications/test', { title, message });
  }
}

export const notificationService = new NotificationService();
