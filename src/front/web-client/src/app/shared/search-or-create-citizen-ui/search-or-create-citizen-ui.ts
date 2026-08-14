import { Component, input } from '@angular/core';
import { ReactiveFormsModule } from '@angular/forms';
import { TranslatePipe } from '@ngx-translate/core';
import { SearchCreateCitizenForm } from '../../pages/registration-requests-home-ui/registration-request-ui/registration-request-form';
import { BasicCitizenModel, Gender } from '../../services/nswag/api-nswag-client';
import { CreateNewCitizenUi } from '../create-new-citizen-ui/create-new-citizen-ui';
import { SearchCitizenUi } from '../search-citizen-ui/search-citizen-ui';

@Component({
  selector: 'app-search-or-create-citizen-ui',
  imports: [TranslatePipe, ReactiveFormsModule, SearchCitizenUi, CreateNewCitizenUi],
  templateUrl: './search-or-create-citizen-ui.html',
  styleUrl: './search-or-create-citizen-ui.scss',
})
export class SearchOrCreateCitizenUi {
  form = input.required<SearchCreateCitizenForm>();
  gender = input.required<Gender>();
  canChangeCitizen = input<boolean>(false);
  citizenId = input<string | null>(null);
  readOnlyCitizen = input<BasicCitizenModel>();

  beginCitizenCreation(): void {
    this.form().controls.isNewCitizen.setValue(true);
    this.form().controls.citizenId.disable();
    this.form().controls.newCitizen.enable();
  }

  endCitizenCreation(): void {
    this.form().controls.isNewCitizen.setValue(false);
    this.form().controls.citizenId.enable();
    this.form().controls.newCitizen.disable();
  }
}
