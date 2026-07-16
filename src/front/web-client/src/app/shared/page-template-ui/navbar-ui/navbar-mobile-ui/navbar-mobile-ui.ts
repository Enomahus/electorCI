import { Component } from '@angular/core';
import { RouterLink, RouterLinkActive } from '@angular/router';
import { TranslatePipe } from '@ngx-translate/core';
import { HideIfAdminDirective } from '../../../../services/auth/hide-if-admin.directive';
import { PermissionDirective } from '../../../../services/auth/permission.directive';
import { LanguageSelectorUi } from '../../language-selector-ui/language-selector-ui';
import { BaseNavbar } from '../base-navbar';

@Component({
  selector: 'app-navbar-mobile-ui',
  imports: [
    RouterLink,
    RouterLinkActive,
    TranslatePipe,
    LanguageSelectorUi,
    PermissionDirective,
    HideIfAdminDirective,
  ],
  templateUrl: './navbar-mobile-ui.html',
  styleUrl: './navbar-mobile-ui.scss',
})
export class NavbarMobileUi extends BaseNavbar {}
