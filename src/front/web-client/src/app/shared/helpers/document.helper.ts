export function saveBlobAsFile(blob: Blob, fileName?: string): void {
  const url = URL.createObjectURL(blob);

  const a = document.createElement('a');
  a.href = url;
  a.download = fileName ?? 'DOCUMENT_NAME_MISSING';

  document.body.appendChild(a);
  a.click();

  document.body.removeChild(a);
  URL.revokeObjectURL(url);
}
