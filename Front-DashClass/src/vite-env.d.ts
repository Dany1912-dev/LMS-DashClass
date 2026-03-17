/// <reference types="vite/client" />

declare module "*.png";
declare module "*.jpg";
declare module "*.svg";
declare module "*.jpeg";

interface ImportMetaEnv {
  readonly VITE_API_URL: string;
  readonly VITE_GOOGLE_CLIENT_ID: string;
  readonly VITE_MICROSOFT_CLIENT_ID: string;
  readonly VITE_MICROSOFT_TENANT_ID: string;
}

interface ImportMeta {
  readonly env: ImportMetaEnv;
}
