export type UploadFileKind = 'local' | 'distant' | 'fake';

export interface UploadFileInfo {
  uid?: string;
  name: string;
  size?: number;
  kind: UploadFileKind;
}
