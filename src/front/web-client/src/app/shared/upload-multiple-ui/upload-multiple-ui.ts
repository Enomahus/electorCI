import { CommonModule } from '@angular/common';
import { Component, forwardRef, inject, input, output, signal } from '@angular/core';
import { ControlValueAccessor, FormsModule, NG_VALUE_ACCESSOR } from '@angular/forms';
import { DocumentApiService } from '../../services/api/document.api.service';
import { saveBlobAsFile } from '../helpers/document.helper';
import { UploadMultipleFormValue } from './upload-multiple-form-value';

@Component({
  selector: 'app-upload-multiple-ui',
  imports: [CommonModule, FormsModule],
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

  filesInfo = signal<File[]>([]);
  value = signal<UploadMultipleFormValue | undefined>(undefined);
  isDisabled = signal(false);

  private onChange?: (formValue: UploadMultipleFormValue | undefined) => void;
  private onTouched?: () => void;

  onFileChange(event: File[]): void {
    this.value.set({
      //distantFileIds: this.value()?.distantFileIds?.filter((f) => event?.some((ef) => ef.uid === f)),
      localFiles: event?.filter((e) => e instanceof File),
      fakeFiles: this.value()?.fakeFiles,
    });

    if (this.onChange) this.onChange(this.value());
    if (this.onTouched) this.onTouched();
  }

  onClick(event: Event): void {
    const el = event.target as Element;
    if (el.closest('.k-upload-button-wrap')) return;

    const fileEl = el.closest('.k-file');
    const id = fileEl?.getAttribute('data-uid');
    const fileName = fileEl?.querySelector('.k-file-name')?.innerHTML;

    const localFile = this.value()?.localFiles?.find((file) => file.name === fileName);

    if (id && this.value()?.distantFileIds?.includes(id)) {
      this.documentService.download(id).subscribe({
        next: (fileRes) => {
          saveBlobAsFile(fileRes.data, fileRes.fileName);
        },
      });
    } else if (localFile) {
      saveBlobAsFile(localFile as File, localFile.name);
    } else if (id && this.value()?.fakeFiles?.find((f) => f.id === id)?.id !== null) {
      this.downloadFakeFile.emit(id);
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
      ...(this.filesInfo() ?? []),
      ...(localFiles ?? []).map(
        (localFile) =>
          ({
            name: localFile.name,
            size: localFile.size,
          }) as File,
      ),
    ]);
  }

  private bindDistantFiles(value: UploadMultipleFormValue): void {
    this.documentService.getDocumentsInfos(value.distantFileIds ?? []).subscribe({
      next: (docs) => {
        if (docs.documentsInfos) {
          this.filesInfo.set([
            ...(this.filesInfo() ?? []),
            ...(docs.documentsInfos ?? []).map(
              (doc) =>
                ({
                  name: doc.fileName,
                  size: doc.size,
                  //uid: doc.id
                }) as File,
            ),
          ]);
        }
      },
    });
  }

  private bindFakeFiles(value: UploadMultipleFormValue): void {
    this.filesInfo.set([
      ...(this.filesInfo() ?? []),
      ...(value.fakeFiles ?? []).map(
        (fakeFile) =>
          ({
            name: fakeFile.name,
            //uid: fakeFile.id,
          }) as File,
      ),
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
}
