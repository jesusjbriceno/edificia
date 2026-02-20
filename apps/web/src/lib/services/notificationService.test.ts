import { describe, it, expect, vi, beforeEach } from 'vitest';
import { notificationService } from './notificationService';
import apiClient from '@/lib/api';

vi.mock('@/lib/api', () => ({
  default: {
    get: vi.fn(),
    post: vi.fn(),
  }
}));

describe('NotificationService', () => {
  const mockNotifications = [
    { id: '1', title: 'Test', message: 'Msg', isRead: false, createdAt: new Date().toISOString() }
  ];

  beforeEach(() => {
    vi.clearAllMocks();
  });

  it('list() should fetch notifications from API', async () => {
    (apiClient.get as any).mockResolvedValue({ data: mockNotifications });
    
    const result = await notificationService.list();
    
    expect(apiClient.get).toHaveBeenCalledWith('/notifications');
    expect(result).toEqual(mockNotifications);
  });

  it('markAsRead() should call post endpoint', async () => {
    (apiClient.post as any).mockResolvedValue({});
    
    await notificationService.markAsRead('123');
    
    expect(apiClient.post).toHaveBeenCalledWith('/notifications/123/read');
  });

  it('markAllAsRead() should call post endpoint', async () => {
    (apiClient.post as any).mockResolvedValue({});
    
    await notificationService.markAllAsRead();
    
    expect(apiClient.post).toHaveBeenCalledWith('/notifications/mark-all-read');
  });
});
