import { Component, inject, input, OnInit, output } from '@angular/core';
import { ReactiveFormsModule } from '@angular/forms';
import { TranslatePipe } from '@ngx-translate/core';
import { DistrictTreeHelperService } from '../../../../services/district-tree-helper.service';
import {
  DistrictModel,
  ElectoralDistrictLevel,
  GetDistrictResponse,
} from '../../../../services/nswag/api-nswag-client';
import { allLocationLevel } from '../../../types/enumerations';
import { DistrictForm } from './district-form';

@Component({
  selector: 'app-district-ui',
  imports: [TranslatePipe, ReactiveFormsModule],
  templateUrl: './district-ui.html',
  styleUrl: './district-ui.scss',
})
export class DistrictUi implements OnInit {
  saveDistrict = output<DistrictModel>();
  goBack = output<void>();
  isSaving = input(false);
  form = input.required<DistrictForm>();
  isEditMode = input<boolean>(false);
  dsitrict = input<GetDistrictResponse | null>(null);

  private readonly store = inject(DistrictTreeHelperService);

  levels: ElectoralDistrictLevel[] = allLocationLevel;

  ngOnInit(): void {}
}
