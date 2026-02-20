import { describe, it, expect, beforeEach, vi } from 'vitest';
import { notificationService } from '@/lib/services/notificationService';

describe('NotificationService', () => {
  beforeEach(() => {
    localStorage.clear();
    vi.clearAllMocks();
  });

  it('should list initial notifications if storage is empty', async () => {
    const notifications = await notificationService.list();
    expect(notifications.length).toBe(3);
    expect(notifications[0].title).toBe('Nuevo Proyecto');
  });

  it('should mark a notification as read', async () => {
    const notifications = await notificationService.list();
    const firstId = notifications[0].id;
    
    await notificationService.markAsRead(firstId);
    
    const updated = await notificationService.list();
    expect(updated.find(n => n.id === firstId)?.read).toBe(true);
  });

  it('should mark all notifications as read', async () => {
    await notificationService.markAllAsRead();
    const notifications = await notificationService.list();
    expect(notifications.every(n => n.read)).toBe(true);
  });

  it('should delete a notification', async () => {
    const initial = await notificationService.list();
    const idToDelete = initial[0].id;
    
    await notificationService.delete(idToDelete);
    
    const updated = await notificationService.list();
    expect(updated.find(n => n.id === idToDelete)).toBeUndefined();
    expect(updated.length).toBe(initial.length - 1);
  });

  it('should clear all notifications', async () => {
    await notificationService.clearAll();
    const notifications = await notificationService.list();
    expect(notifications.length).toBe(0);
  });
});
