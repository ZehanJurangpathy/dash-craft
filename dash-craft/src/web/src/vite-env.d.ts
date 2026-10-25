/// <reference types="vite/client" />

interface ImportMetaEnv {
  /** Optional API origin. Empty (the default) means same-origin via the dev or nginx proxy. */
  readonly VITE_API_BASE_URL?: string;
}

interface ImportMeta {
  readonly env: ImportMetaEnv;
}
