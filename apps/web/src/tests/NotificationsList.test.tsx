import { describe, it, expect, vi, beforeEach } from 'vitest';
import { render, screen, fireEvent, waitFor } from '@testing-library/react';
import NotificationsList from '@/components/Admin/NotificationsList';
import { notificationService } from '@/lib/services/notificationService';

vi.mock('@/lib/services/notificationService', () => ({
  notificationService: {
    list: vi.fn(),
    markAsRead: vi.fn(),
    markAllAsRead: vi.fn(),
    delete: vi.fn(),
    clearAll: vi.fn(),
  }
}));

describe('NotificationsList Component', () => {
  const mockNotifications = [
    { id: '1', title: 'Unread', message: 'Msg 1', isRead: false, createdAt: new Date().toISOString() },
    { id: '2', title: 'Read', message: 'Msg 2', isRead: true, createdAt: new Date().toISOString() },
  ];

  beforeEach(() => {
    vi.clearAllMocks();
    (notificationService.list as any).mockResolvedValue(mockNotifications);
  });

  it('renders correctly', async () => {
    render(<NotificationsList />);
    expect(await screen.findByText('Unread')).toBeInTheDocument();
    expect(await screen.findByText('Read')).toBeInTheDocument();
  });

  it('filters unread', async () => {
    render(<NotificationsList />);
    const filterBtn = await screen.findByText('No leídas');
    fireEvent.click(filterBtn);
    
    expect(screen.getByText('Unread')).toBeInTheDocument();
    expect(screen.queryByText('Read')).not.toBeInTheDocument();
  });
});
