import { FileParameter } from '../../services/nswag/api-nswag-client';

export async function getFileParametersAsync(values: File[]): Promise<FileParameter[]> {
  if (!values) return [];

  const attachments = await Promise.all(
    values
      .filter((f) => f instanceof File)
      .map((f) => f as File)
      .map(async (d) => await getFileParameterAsync(d)),
  );

  return attachments;
}

export async function getFileParameterAsync(d: File): Promise<FileParameter> {
  return {
    fileName: d.name,
    data: new Blob([await d.arrayBuffer()], { type: d.type }),
  } as FileParameter;
}
