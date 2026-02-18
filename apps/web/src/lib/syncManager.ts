import { set as idbSet, keys as idbKeys, getMany as idbGetMany } from 'idb-keyval';
import { projectService } from '@/lib/services';

// ─── Types ───────────────────────────────────────────────

export type SyncStatus = 'idle' | 'modified' | 'syncing' | 'synced' | 'offline' | 'error';

// ─── Helpers ─────────────────────────────────────────────

/**
 * Loads all locally-cached section content for a project from IndexedDB.
 * Returns a map of `sectionId → HTML content`.
 */
export async function loadLocalContent(
  projectId: string,
): Promise<Record<string, string>> {
  const allKeys = await idbKeys();
  const prefix = `project-${projectId}-section-`;
  const sectionKeys = allKeys.filter(
    (k): k is string => typeof k === 'string' && k.startsWith(prefix),
  );

  if (sectionKeys.length === 0) return {};

  const values = await idbGetMany<string>(sectionKeys);
  const result: Record<string, string> = {};

  sectionKeys.forEach((key, i) => {
    const sectionId = key.slice(prefix.length);
    if (values[i] != null) {
      result[sectionId] = values[i];
    }
  });

  return result;
}

// ─── SyncManager ─────────────────────────────────────────

interface SyncManagerOptions {
  projectId: string;
  /** Milliseconds to wait after last keystroke before writing to IndexedDB. Default 500. */
  debounceMs?: number;
  /** Milliseconds between API sync attempts. Default 5000. */
  syncIntervalMs?: number;
  /** Called whenever the sync status changes. */
  onStatusChange: (status: SyncStatus, pendingCount: number) => void;
}

/**
 * Offline-first persistence manager:
 *
 * 1. **Debounce** — Waits `debounceMs` after the last keystroke, then writes to IndexedDB.
 * 2. **Periodic sync** — Every `syncIntervalMs`, pushes dirty sections to the API via PATCH.
 * 3. **Online/offline** — Detects connectivity; queues changes when offline, auto-syncs on reconnect.
 */
export class SyncManager {
  private readonly projectId: string;
  private readonly debounceMs: number;
  private readonly syncIntervalMs: number;
  private readonly onStatusChange: (status: SyncStatus, pendingCount: number) => void;

  private readonly debounceTimers = new Map<string, ReturnType<typeof setTimeout>>();
  private readonly dirtyKeys = new Set<string>();
  private readonly latestContent = new Map<string, string>();
  private syncInterval: ReturnType<typeof setInterval> | null = null;
  private isSyncing = false;
  private _isOnline: boolean;

  constructor(opts: SyncManagerOptions) {
    this.projectId = opts.projectId;
    this.debounceMs = opts.debounceMs ?? 500;
    this.syncIntervalMs = opts.syncIntervalMs ?? 5_000;
    this.onStatusChange = opts.onStatusChange;
    this._isOnline =
      globalThis.navigator === undefined
        ? true
        : globalThis.navigator.onLine;
  }

  /** Start listening for online/offline events and periodic sync. */
  start(): void {
    globalThis.addEventListener?.('online', this.handleOnline);
    globalThis.addEventListener?.('offline', this.handleOffline);
    this.syncInterval = setInterval(() => void this.sync(), this.syncIntervalMs);
  }

  /** Stop all timers and listeners. Flushes pending debounce writes to IndexedDB. */
  stop(): void {
    globalThis.removeEventListener?.('online', this.handleOnline);
    globalThis.removeEventListener?.('offline', this.handleOffline);

    if (this.syncInterval) {
      clearInterval(this.syncInterval);
      this.syncInterval = null;
    }

    // Flush any pending debounce timers immediately to IndexedDB
    for (const [sectionId, timer] of this.debounceTimers) {
      clearTimeout(timer);
      const html = this.latestContent.get(sectionId);
      if (html != null) {
        const key = `project-${this.projectId}-section-${sectionId}`;
        void idbSet(key, html);
      }
    }
    this.debounceTimers.clear();
  }

  /**
   * Called on every editor content change.
   * Debounces the local IndexedDB save and marks the section as dirty for API sync.
   */
  notifyChange(sectionId: string, html: string): void {
    this.latestContent.set(sectionId, html);

    const existing = this.debounceTimers.get(sectionId);
    if (existing) clearTimeout(existing);

    const timer = setTimeout(() => {
      this.debounceTimers.delete(sectionId);
      void this.saveToLocal(sectionId, html);
    }, this.debounceMs);

    this.debounceTimers.set(sectionId, timer);
  }

  /** Force an immediate sync of all dirty sections to the API. */
  async sync(): Promise<void> {
    if (this.dirtyKeys.size === 0 || this.isSyncing) return;

    if (!this._isOnline) {
      this.onStatusChange('offline', this.dirtyKeys.size);
      return;
    }

    this.isSyncing = true;
    this.onStatusChange('syncing', this.dirtyKeys.size);

    // Snapshot content so edits during sync don't cause data loss
    const snapshot = new Map<string, string>();
    for (const sectionId of this.dirtyKeys) {
      const content = this.latestContent.get(sectionId);
      if (content != null) snapshot.set(sectionId, content);
    }

    try {
      await Promise.all(
        [...snapshot.entries()].map(async ([sectionId, content]) => {
          await projectService.updateSection(this.projectId, sectionId, { content });

          // Only mark clean if content hasn't changed since our snapshot
          if (this.latestContent.get(sectionId) === content) {
            this.dirtyKeys.delete(sectionId);
            this.latestContent.delete(sectionId);
          }
        }),
      );

      this.onStatusChange(
        this.dirtyKeys.size > 0 ? 'modified' : 'synced',
        this.dirtyKeys.size,
      );
    } catch {
      this.onStatusChange('error', this.dirtyKeys.size);
    } finally {
      this.isSyncing = false;
    }
  }

  get isOnline(): boolean {
    return this._isOnline;
  }

  get pendingCount(): number {
    return this.dirtyKeys.size;
  }

  // ── Private ─────────────────────────────────────────────

  private async saveToLocal(sectionId: string, html: string): Promise<void> {
    const key = `project-${this.projectId}-section-${sectionId}`;
    await idbSet(key, html);
    this.dirtyKeys.add(sectionId);
    this.emitStatus();
  }

  private emitStatus(): void {
    const status: SyncStatus = this._isOnline ? 'modified' : 'offline';
    this.onStatusChange(status, this.dirtyKeys.size);
  }

  private readonly handleOnline = (): void => {
    this._isOnline = true;
    void this.sync();
  };

  private readonly handleOffline = (): void => {
    this._isOnline = false;
    this.onStatusChange('offline', this.dirtyKeys.size);
  };
}
