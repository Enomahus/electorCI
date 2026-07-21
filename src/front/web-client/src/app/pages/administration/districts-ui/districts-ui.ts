import { Component, inject } from '@angular/core';
import { Router } from '@angular/router';
import { TranslateService } from '@ngx-translate/core';
import { DistrictApiService } from '../../../services/api/district.api.service';

@Component({
  selector: 'app-districts-ui',
  imports: [],
  templateUrl: './districts-ui.html',
  styleUrl: './districts-ui.scss',
})
export class DistrictsUi {
  private readonly router = inject(Router);
  private readonly translateService = inject(TranslateService);
  private readonly districtService = inject(DistrictApiService);
}
