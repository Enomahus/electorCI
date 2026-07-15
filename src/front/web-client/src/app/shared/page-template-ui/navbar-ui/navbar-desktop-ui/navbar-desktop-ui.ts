import { CommonModule } from '@angular/common';
import { Component } from '@angular/core';
import { RouterLink, RouterLinkActive } from '@angular/router';
import { TranslatePipe } from '@ngx-translate/core';
import { LanguageSelectorUi } from '../../language-selector-ui/language-selector-ui';
import { BaseNavbar } from '../base-navbar';

@Component({
  selector: 'app-navbar-desktop-ui',
  imports: [RouterLink, TranslatePipe, RouterLinkActive, CommonModule, LanguageSelectorUi],
  templateUrl: './navbar-desktop-ui.html',
  styleUrl: './navbar-desktop-ui.scss',
})
export class NavbarDesktopUi extends BaseNavbar {}
