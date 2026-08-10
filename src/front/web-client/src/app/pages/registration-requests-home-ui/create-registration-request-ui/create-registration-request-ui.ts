import { Component, inject, signal } from '@angular/core';
import { Router } from '@angular/router';
import { TranslateService } from '@ngx-translate/core';
import { RegistrationRequestsApiService } from '../../../services/api/registration-requests.api.service';
import { FileParameter, RegistrationRequestModel } from '../../../services/nswag/api-nswag-client';
import { getFileParametersAsync } from '../../../shared/upload-multiple-ui/file-upload-helper';

@Component({
  selector: 'app-create-registration-request-ui',
  imports: [],
  templateUrl: './create-registration-request-ui.html',
  styleUrl: './create-registration-request-ui.scss',
})
export class CreateRegistrationRequestUi {
  private readonly registrationRequestService = inject(RegistrationRequestsApiService);
  private readonly translateService = inject(TranslateService);
  private readonly router = inject(Router);

  isSaving = signal(false);

  async saveCreatedRequest(event: {
    registrationRequest: RegistrationRequestModel;
    identityDocs: File[];
    residenceCertificates: File[];
    photos: File[];
  }): Promise<void> {
    this.isSaving.set(true);
    const { identityDocs, residenceCertificates, photos } = await this.mapAttachments(event);

    this.registrationRequestService
      .createRegistrationRequest(
        event.registrationRequest,
        identityDocs,
        residenceCertificates,
        photos,
        {
          successMessage: this.translateService.instant('registrationRequest.createSuccessMsg'),
          errorMessage: this.translateService.instant('registrationRequest.createErrorMsg'),
        },
      )
      .subscribe({
        next: () => {
          this.isSaving.set(false);
          this.router.navigate(['registration-requests']);
        },
        error: () => {
          this.isSaving.set(false);
        },
      });
  }

  private async mapAttachments(event: {
    registrationRequest: RegistrationRequestModel;
    identityDocs: File[];
    residenceCertificates: File[];
    photos: File[];
  }): Promise<{
    identityDocs: FileParameter[];
    residenceCertificates: FileParameter[];
    photos: FileParameter[];
  }> {
    const identityDocs = await getFileParametersAsync(event.identityDocs);
    const residenceCertificates = await getFileParametersAsync(event.residenceCertificates);
    const photos = await getFileParametersAsync(event.photos);

    return { identityDocs, residenceCertificates, photos };
  }
}
