import { CommonModule } from '@angular/common';
import { Component, forwardRef, inject, input, output, signal } from '@angular/core';
import { ControlValueAccessor, FormsModule, NG_VALUE_ACCESSOR } from '@angular/forms';
import { TranslatePipe } from '@ngx-translate/core';
import { DocumentApiService } from '../../services/api/document.api.service';
import { saveBlobAsFile } from '../helpers/document.helper';
import { UploadFileInfo } from './upload-file-info';
import { UploadMultipleFormValue } from './upload-multiple-form-value';

@Component({
  selector: 'app-upload-multiple-ui',
  imports: [CommonModule, FormsModule, TranslatePipe],
  templateUrl: './upload-multiple-ui.html',
  styleUrl: './upload-multiple-ui.scss',
  providers: [
    {
      provide: NG_VALUE_ACCESSOR,
      useExisting: forwardRef(() => UploadMultipleUi),
      multi: true,
    },
  ],
})
export class UploadMultipleUi implements ControlValueAccessor {
  private readonly documentService = inject(DocumentApiService);

  accept = input('');
  allowedExtensions = input<string[]>([]);
  downloadFakeFile = output<string>();

  filesInfo = signal<UploadFileInfo[]>([]);
  value = signal<UploadMultipleFormValue | undefined>(undefined);
  isDisabled = signal(false);

  private onChange?: (formValue: UploadMultipleFormValue | undefined) => void;
  private onTouched?: () => void;

  onNativeFileChange(event: Event): void {
    const input = event.target as HTMLInputElement;
    this.onFileChange(Array.from(input.files ?? []));

    input.value = '';
  }

  onFileChange(event: File[]): void {

    const validLocalFiles = event
      .filter((e) => e instanceof File)
      .filter((f) => this.isValideFile(f));

    if (!validLocalFiles.length) return;

    this.value.update((current) => ({
      distantFileIds: current?.distantFileIds,
      fakeFiles: current?.fakeFiles,
      localFiles: validLocalFiles,
    }));

    this.filesInfo.update((currentFiles) => [
      ...currentFiles.filter((f) => f.kind !== 'local'),
      ...validLocalFiles.map((localFile): UploadFileInfo => ({
        name: localFile.name,
        size: localFile.size,
        kind: 'local',
      })),
    ]);

    this.notifyChanges();
  }

  onRemoveFile(fileToRemove: UploadFileInfo, event: Event): void {
    event.stopPropagation();
    if (this.isDisabled()) return;

    // Mise à jour de la valeur du formulaire
    this.value.update((current) => {
      if (!current) return current;
      return {
        localFiles:
          fileToRemove.kind === 'local'
            ? current.localFiles?.filter((f) => f.name !== fileToRemove.name)
            : current.localFiles,
        distantFileIds:
          fileToRemove.kind === 'distant'
            ? current.distantFileIds?.filter((id) => id !== fileToRemove.uid)
            : current.distantFileIds,
        fakeFiles:
          fileToRemove.kind === 'fake'
            ? current.fakeFiles?.filter((f) => f.id !== fileToRemove.uid)
            : current.fakeFiles,
      };
    });

    //Mise à jour de l'UI
    this.filesInfo.update((files) => files.filter((f) => f !== fileToRemove));
    this.notifyChanges();
  }

  onClick(event: Event): void {
    const el = event.target as Element;
    if (el.closest('.k-upload-button-wrap')) return;

    const fileEl = el.closest('.k-file');
    const id = fileEl?.getAttribute('data-uid');
    const fileName = fileEl?.querySelector('.k-file-name')?.innerHTML;

    if (id && this.value()?.distantFileIds?.includes(id)) {
      this.documentService.download(id).subscribe({
        next: (fileRes) => {
          saveBlobAsFile(fileRes.data, fileRes.fileName);
        },
      });
      return;
    }

    if (id && this.value()?.fakeFiles?.some((f) => f.id === id)) {
      this.downloadFakeFile.emit(id);
      return;
    }

    const localFile = this.value()?.localFiles?.find((file) => file.name === fileName);
    if (localFile) {
      saveBlobAsFile(localFile, localFile.name);
    }
  }

  writeValue(value: UploadMultipleFormValue | undefined): void {
    this.value.set(value);
    this.filesInfo.set([]);
    if (value?.fakeFiles?.length) {
      this.bindFakeFiles(value);
    }
    if (value?.distantFileIds && value?.distantFileIds?.length > 0) {
      this.bindDistantFiles(value);
    }

    if (value?.localFiles?.length) {
      this.bindLocalFiles(value);
    }
  }

  private bindLocalFiles(value: UploadMultipleFormValue): void {
    const localFiles = value.localFiles?.filter((l) => l instanceof File);
    this.filesInfo.set([
      ...this.filesInfo(),
      ...(localFiles ?? []).map((localFile): UploadFileInfo => ({
        name: localFile.name,
        size: localFile.size,
        kind: 'local',
      })),
    ]);
  }

  private bindDistantFiles(value: UploadMultipleFormValue): void {
    this.documentService.getDocumentsInfos(value.distantFileIds ?? []).subscribe({
      next: (docs) => {
        if (docs.documentsInfos) {
          this.filesInfo.set([
            ...this.filesInfo(),
            ...(docs.documentsInfos ?? []).map((doc): UploadFileInfo => ({
              uid: doc.id,
              name: doc.fileName,
              size: doc.size,
              kind: 'distant',
            })),
          ]);
        }
      },
    });
  }

  private bindFakeFiles(value: UploadMultipleFormValue): void {
    this.filesInfo.set([
      ...this.filesInfo(),
      ...(value.fakeFiles ?? []).map((fakeFile): UploadFileInfo => ({
        uid: fakeFile.id,
        name: fakeFile.name ?? '',
        kind: 'fake',
      })),
    ]);
  }

  registerOnChange(fn: (formValue: UploadMultipleFormValue | undefined) => void): void {
    this.onChange = fn;
  }

  registerOnTouched(fn: () => void): void {
    this.onTouched = fn;
  }

  setDisabledState?(isDisabled: boolean): void {
    this.isDisabled.set(isDisabled);
  }

  private isValideFile(file: File): boolean {
    const extensions = this.allowedExtensions();
    if (!extensions || extensions.length === 0) return true;

    const fileName = file.name.toLowerCase();
    return extensions.some((ext) => fileName.endsWith(ext.toLowerCase()));
  }

  private notifyChanges(): void {
    if (this.onChange) this.onChange(this.value());
    if (this.onTouched) this.onTouched();
  }
}
