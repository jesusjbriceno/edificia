import { describe, it, expect, vi, beforeEach } from 'vitest';
import { notificationService } from './notificationService';

describe('NotificationService (Clean State)', () => {
  beforeEach(() => {
    localStorage.clear();
    vi.clearAllMocks();
  });

  it('list() should return empty array initially', async () => {
    const result = await notificationService.list();
    expect(result).toEqual([]);
  });

  it('addTestNotification() should add a new notification and persist it', async () => {
    await notificationService.addTestNotification('Test Title', 'Test Message');
    const result = await notificationService.list();
    
    expect(result.length).toBe(1);
    expect(result[0].title).toBe('Test Title');
    expect(result[0].isRead).toBe(false);
  });

  it('markAsRead() should update state for a specific notification', async () => {
    await notificationService.addTestNotification('T1', 'M1');
    const notifications = await notificationService.list();
    const id = notifications[0].id;
    
    await notificationService.markAsRead(id);
    const updated = await notificationService.list();
    
    expect(updated.find(n => n.id === id)?.isRead).toBe(true);
  });

  it('markAllAsRead() should update all notifications in storage', async () => {
    await notificationService.addTestNotification('T1', 'M1');
    await notificationService.addTestNotification('T2', 'M2');
    
    await notificationService.markAllAsRead();
    const result = await notificationService.list();
    
    expect(result.length).toBe(2);
    expect(result.every(n => n.isRead)).toBe(true);
  });

  it('delete() should remove the specified notification', async () => {
    await notificationService.addTestNotification('To Delete', 'Msg');
    const notifications = await notificationService.list();
    const id = notifications[0].id;
    
    await notificationService.delete(id);
    const updated = await notificationService.list();
    
    expect(updated.length).toBe(0);
  });
  
  it('clearAll() should empty the storage', async () => {
    await notificationService.addTestNotification('T1', 'M1');
    await notificationService.clearAll();
    const result = await notificationService.list();
    expect(result).toEqual([]);
  });
});
