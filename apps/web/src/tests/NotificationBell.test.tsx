import { describe, it, expect, vi, beforeEach } from 'vitest';
import { render, screen, fireEvent, waitFor } from '@testing-library/react';
import NotificationBell from '@/components/Admin/NotificationBell';
import { notificationService } from '@/lib/services/notificationService';

// Mock del servicio
vi.mock('@/lib/services/notificationService', () => ({
  notificationService: {
    list: vi.fn(),
    markAsRead: vi.fn(),
    markAllAsRead: vi.fn(),
  }
}));

describe('NotificationBell Component', () => {
  const mockNotifications = [
    { id: '1', title: 'Test 1', message: 'Msg 1', isRead: false, createdAt: new Date().toISOString() },
    { id: '2', title: 'Test 2', message: 'Msg 2', isRead: true, createdAt: new Date().toISOString() },
  ];

  beforeEach(() => {
    vi.clearAllMocks();
    (notificationService.list as any).mockResolvedValue(mockNotifications);
  });

  it('renders the bell icon with unread indicator', async () => {
    render(<NotificationBell />);
    
    await waitFor(() => {
      expect(screen.getByLabelText(/notificaciones/i)).toBeInTheDocument();
    });

    const bellButton = screen.getByLabelText(/notificaciones/i);
    const indicator = bellButton.querySelector('span');
    expect(indicator).toBeInTheDocument();
  });

  it('opens and shows notifications', async () => {
    render(<NotificationBell />);
    
    const trigger = await screen.findByLabelText(/notificaciones/i);
    fireEvent.click(trigger);
    
    expect(screen.getByText('Test 1')).toBeInTheDocument();
  });
});
