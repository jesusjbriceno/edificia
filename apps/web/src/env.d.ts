/// <reference path="../.astro/types.d.ts" />

interface ImportMetaEnv {
  /** Base URL of the EDIFICIA API (e.g. http://localhost:5000/api) */
  readonly PUBLIC_API_URL: string;
}

interface ImportMeta {
  readonly env: ImportMetaEnv;
}

interface UserSession {
  id: string;
  email: string;
  role: string;
  name: string;
  token?: string; // Opcional, solo si se necesita en cliente (no recomendado por seguridad)
}

declare namespace App {
  interface Locals {
    user: UserSession | null;
    isAuthenticated: boolean;
  }
}

interface Window {
  __INITIAL_SESSION__?: UserSession | null;
}