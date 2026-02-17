/// <reference path="../.astro/types.d.ts" />

interface ImportMetaEnv {
  /** Base URL of the EDIFICIA API (e.g. http://localhost:5000/api) */
  readonly PUBLIC_API_URL: string;
}

interface ImportMeta {
  readonly env: ImportMetaEnv;
}