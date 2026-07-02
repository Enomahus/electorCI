import { inject, Injectable } from '@angular/core';
import { IndividualConfig, ToastrService } from 'ngx-toastr';

@Injectable({
  providedIn: 'root',
})
export class ToastService {
  private toastrService = inject(ToastrService);

  private defaultOptions: Partial<IndividualConfig<unknown>> = {
    positionClass: 'toast-bottom-right',
  };

  toastSuccess(
    message: string,
    title?: string,
    override?: Partial<IndividualConfig<unknown>>,
  ): void {
    const options = { ...this.defaultOptions, ...override };
    this.toastrService.success(message, title, options);
  }

  toastError(message: string, title?: string, override?: Partial<IndividualConfig<unknown>>): void {
    const options = { ...this.defaultOptions, ...override };
    this.toastrService.error(message, title, options);
  }

  toastWarning(
    message: string,
    title?: string,
    override?: Partial<IndividualConfig<unknown>>,
  ): void {
    const options = { ...this.defaultOptions, ...override };
    this.toastrService.warning(message, title, options);
  }

  toastInfo(message: string, title?: string, override?: Partial<IndividualConfig<unknown>>): void {
    const options = { ...this.defaultOptions, ...override };
    this.toastrService.info(message, title, options);
  }
}
