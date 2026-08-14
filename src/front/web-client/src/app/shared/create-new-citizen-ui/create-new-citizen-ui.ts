import { Component, input } from '@angular/core';
import { ReactiveFormsModule } from '@angular/forms';
import { TranslatePipe } from '@ngx-translate/core';
import { BasicCitizenForm } from '../../pages/registration-requests-home-ui/registration-request-ui/registration-request-form';
import { allGenders } from '../../pages/types/enumerations';
import { InputDatepickerUi } from '../input-datepicker-ui/input-datepicker-ui';

@Component({
  selector: 'app-create-new-citizen-ui',
  imports: [TranslatePipe, ReactiveFormsModule, InputDatepickerUi],
  templateUrl: './create-new-citizen-ui.html',
  styleUrl: './create-new-citizen-ui.scss',
})
export class CreateNewCitizenUi {
  form = input.required<BasicCitizenForm>();
  isEmbedded = input(false);

  allGenders = allGenders;
}
