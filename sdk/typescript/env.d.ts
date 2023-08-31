/* eslint-disable @typescript-eslint/consistent-type-definitions */
/// <reference types="vite/client" />

interface ImportMetaEnv {
  readonly VITE_SDK_VERSION: string
}

interface ImportMeta {
  readonly env: ImportMetaEnv
}
