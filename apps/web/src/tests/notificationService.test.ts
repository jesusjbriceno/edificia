import { describe, it, expect, vi, beforeEach } from 'vitest';
import { notificationService } from '@/lib/services/notificationService';
import apiClient from '@/lib/api';

vi.mock('@/lib/api', () => ({
  default: {
    get: vi.fn(),
    post: vi.fn(),
  },
}));

describe('NotificationService (API)', () => {
  const mockNotifications = [
    { id: '1', title: 'T1', message: 'M1', isRead: false, createdAt: new Date().toISOString() },
    { id: '2', title: 'T2', message: 'M2', isRead: true, createdAt: new Date().toISOString() },
  ];

  beforeEach(() => {
    vi.clearAllMocks();
  });

  it('list() should fetch from API', async () => {
    (apiClient.get as any).mockResolvedValue({ data: mockNotifications });
    
    const result = await notificationService.list();
    
    expect(apiClient.get).toHaveBeenCalledWith('/notifications');
    expect(result.length).toBe(2);
    expect(result[0].title).toBe('T1');
  });

  it('markAsRead() should call correctly the endpoint', async () => {
    (apiClient.post as any).mockResolvedValue({});
    
    await notificationService.markAsRead('123');
    
    expect(apiClient.post).toHaveBeenCalledWith('/notifications/123/read');
  });

  it('markAllAsRead() should call correctly the endpoint', async () => {
    (apiClient.post as any).mockResolvedValue({});
    
    await notificationService.markAllAsRead();
    
    expect(apiClient.post).toHaveBeenCalledWith('/notifications/mark-all-read');
  });

  it('delete() should call correctly the endpoint', async () => {
    (apiClient.post as any).mockResolvedValue({});
    
    await notificationService.delete('123');
    
    expect(apiClient.post).toHaveBeenCalledWith('/notifications/123/delete');
  });

  it('clearAll() should call correctly the endpoint', async () => {
    (apiClient.post as any).mockResolvedValue({});
    
    await notificationService.clearAll();
    
    expect(apiClient.post).toHaveBeenCalledWith('/notifications/clear-all');
  });
});
