import { CommonModule } from '@angular/common';
import { Component } from '@angular/core';
import { RouterLink, RouterLinkActive } from '@angular/router';
import { TranslatePipe } from '@ngx-translate/core';
import { HideIfAdminDirective } from '../../../../services/auth/hide-if-admin.directive';
import { PermissionDirective } from '../../../../services/auth/permission.directive';
import { LanguageSelectorUi } from '../../language-selector-ui/language-selector-ui';
import { BaseNavbar } from '../base-navbar';

@Component({
  selector: 'app-navbar-desktop-ui',
  imports: [
    CommonModule,
    TranslatePipe,
    RouterLink,
    RouterLinkActive,
    LanguageSelectorUi,
    PermissionDirective,
    HideIfAdminDirective,
  ],
  templateUrl: './navbar-desktop-ui.html',
  styleUrl: './navbar-desktop-ui.scss',
})
export class NavbarDesktopUi extends BaseNavbar {}
