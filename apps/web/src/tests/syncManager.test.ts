import { describe, it, expect, vi, beforeEach, afterEach } from 'vitest';

// ── Mocks ────────────────────────────────────────────────

vi.mock('idb-keyval', () => ({
  set: vi.fn().mockResolvedValue(undefined),
  get: vi.fn().mockResolvedValue(undefined),
  keys: vi.fn().mockResolvedValue([]),
  getMany: vi.fn().mockResolvedValue([]),
}));

vi.mock('@/lib/services', () => ({
  projectService: {
    updateSection: vi.fn().mockResolvedValue(undefined),
  },
}));

import { SyncManager, loadLocalContent } from '@/lib/syncManager';
import { set as idbSet, keys as idbKeys, getMany as idbGetMany } from 'idb-keyval';
import { projectService } from '@/lib/services';

// ── loadLocalContent ─────────────────────────────────────

describe('loadLocalContent', () => {
  beforeEach(() => vi.clearAllMocks());

  it('should return sections matching the project prefix', async () => {
    vi.mocked(idbKeys).mockResolvedValue([
      'project-p1-section-MD.01',
      'project-p1-section-MC.02',
      'project-p2-section-CN.01',
    ]);
    vi.mocked(idbGetMany).mockResolvedValue(['<p>A</p>', '<p>B</p>']);

    const result = await loadLocalContent('p1');

    expect(result).toEqual({ 'MD.01': '<p>A</p>', 'MC.02': '<p>B</p>' });
    expect(idbGetMany).toHaveBeenCalledWith([
      'project-p1-section-MD.01',
      'project-p1-section-MC.02',
    ]);
  });

  it('should return empty object when no keys match', async () => {
    vi.mocked(idbKeys).mockResolvedValue([
      'project-other-section-s1',
    ]);

    const result = await loadLocalContent('p1');
    expect(result).toEqual({});
  });

  it('should return empty object when IndexedDB is empty', async () => {
    vi.mocked(idbKeys).mockResolvedValue([]);

    const result = await loadLocalContent('p1');
    expect(result).toEqual({});
  });

  it('should skip null values from IndexedDB', async () => {
    vi.mocked(idbKeys).mockResolvedValue([
      'project-p1-section-s1',
      'project-p1-section-s2',
    ]);
    vi.mocked(idbGetMany).mockResolvedValue(['<p>A</p>', null]);

    const result = await loadLocalContent('p1');
    expect(result).toEqual({ s1: '<p>A</p>' });
  });
});

// ── SyncManager ──────────────────────────────────────────

describe('SyncManager', () => {
  let manager: SyncManager;
  let statusChanges: Array<{ status: string; pending: number }>;

  beforeEach(() => {
    vi.useFakeTimers();
    vi.clearAllMocks();
    statusChanges = [];
    manager = new SyncManager({
      projectId: 'test-project',
      debounceMs: 500,
      syncIntervalMs: 5_000,
      onStatusChange: (status, pending) => {
        statusChanges.push({ status, pending });
      },
    });
  });

  afterEach(() => {
    manager.stop();
    vi.useRealTimers();
  });

  // ── Debounce ───────────────────────────────────────────

  it('should debounce local saves to IndexedDB', async () => {
    manager.start();

    // Rapid-fire changes
    manager.notifyChange('s1', '<p>A</p>');
    manager.notifyChange('s1', '<p>AB</p>');
    manager.notifyChange('s1', '<p>ABC</p>');

    // No writes yet
    expect(idbSet).not.toHaveBeenCalled();

    // Advance past debounce
    await vi.advanceTimersByTimeAsync(500);

    // Only the last value should be saved
    expect(idbSet).toHaveBeenCalledTimes(1);
    expect(idbSet).toHaveBeenCalledWith(
      'project-test-project-section-s1',
      '<p>ABC</p>',
    );
  });

  it('should save different sections independently', async () => {
    manager.start();

    manager.notifyChange('s1', '<p>First</p>');
    manager.notifyChange('s2', '<p>Second</p>');

    await vi.advanceTimersByTimeAsync(500);

    expect(idbSet).toHaveBeenCalledTimes(2);
    expect(idbSet).toHaveBeenCalledWith(
      'project-test-project-section-s1',
      '<p>First</p>',
    );
    expect(idbSet).toHaveBeenCalledWith(
      'project-test-project-section-s2',
      '<p>Second</p>',
    );
  });

  // ── API Sync ───────────────────────────────────────────

  it('should sync dirty sections to API on interval', async () => {
    manager.start();

    manager.notifyChange('s1', '<p>Hello</p>');
    await vi.advanceTimersByTimeAsync(500); // debounce fires

    await vi.advanceTimersByTimeAsync(5_000); // sync interval fires

    expect(projectService.updateSection).toHaveBeenCalledWith(
      'test-project',
      's1',
      { content: '<p>Hello</p>' },
    );
  });

  it('should report synced status after successful sync', async () => {
    manager.start();

    manager.notifyChange('s1', '<p>Hello</p>');
    await vi.advanceTimersByTimeAsync(500);
    await vi.advanceTimersByTimeAsync(5_000);

    const last = statusChanges[statusChanges.length - 1];
    expect(last.status).toBe('synced');
    expect(last.pending).toBe(0);
  });

  it('should report error status when API call fails', async () => {
    vi.mocked(projectService.updateSection).mockRejectedValueOnce(
      new Error('Network error'),
    );
    manager.start();

    manager.notifyChange('s1', '<p>Hello</p>');
    await vi.advanceTimersByTimeAsync(500);
    await vi.advanceTimersByTimeAsync(5_000);

    const last = statusChanges[statusChanges.length - 1];
    expect(last.status).toBe('error');
  });

  it('should not sync when no dirty sections exist', async () => {
    manager.start();

    await vi.advanceTimersByTimeAsync(5_000);

    expect(projectService.updateSection).not.toHaveBeenCalled();
  });

  it('should report modified status after local save', async () => {
    manager.start();

    manager.notifyChange('s1', '<p>Draft</p>');
    await vi.advanceTimersByTimeAsync(500);

    const modified = statusChanges.find((s) => s.status === 'modified');
    expect(modified).toBeDefined();
    expect(modified!.pending).toBe(1);
  });

  // ── Online/Offline ─────────────────────────────────────

  it('should report offline status when network is lost', async () => {
    manager.start();

    manager.notifyChange('s1', '<p>Hello</p>');
    await vi.advanceTimersByTimeAsync(500);

    // Simulate going offline
    globalThis.dispatchEvent(new Event('offline'));

    const offline = statusChanges.find((s) => s.status === 'offline');
    expect(offline).toBeDefined();
  });

  it('should auto-sync on reconnect', async () => {
    manager.start();

    manager.notifyChange('s1', '<p>Hello</p>');
    await vi.advanceTimersByTimeAsync(500);

    // Go offline
    globalThis.dispatchEvent(new Event('offline'));

    // Try sync while offline — should skip API call
    await manager.sync();
    expect(projectService.updateSection).not.toHaveBeenCalled();

    // Come back online — should trigger sync
    globalThis.dispatchEvent(new Event('online'));
    // Wait for the auto-sync triggered by handleOnline
    await vi.advanceTimersByTimeAsync(0);

    expect(projectService.updateSection).toHaveBeenCalledWith(
      'test-project',
      's1',
      { content: '<p>Hello</p>' },
    );
  });

  // ── Lifecycle ──────────────────────────────────────────

  it('should flush debounce timers to IndexedDB on stop', () => {
    manager.start();

    // Notify but don't wait for debounce
    manager.notifyChange('s1', '<p>Unsaved</p>');

    // Stop should flush
    manager.stop();

    expect(idbSet).toHaveBeenCalledWith(
      'project-test-project-section-s1',
      '<p>Unsaved</p>',
    );
  });

  it('should report pending count correctly', async () => {
    manager.start();

    expect(manager.pendingCount).toBe(0);

    manager.notifyChange('s1', '<p>A</p>');
    manager.notifyChange('s2', '<p>B</p>');
    await vi.advanceTimersByTimeAsync(500);

    expect(manager.pendingCount).toBe(2);

    // Sync clears pending
    await vi.advanceTimersByTimeAsync(5_000);

    expect(manager.pendingCount).toBe(0);
  });
});
