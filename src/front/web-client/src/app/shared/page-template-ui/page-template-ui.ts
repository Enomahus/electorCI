import { Component } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { BreadcrumbUi } from './breadcrumb-ui/breadcrumb-ui';
import { FooterUi } from './footer-ui/footer-ui';
import { NavbarUi } from './navbar-ui/navbar-ui';

@Component({
  selector: 'app-page-template-ui',
  imports: [NavbarUi, FooterUi, BreadcrumbUi, RouterOutlet],
  templateUrl: './page-template-ui.html',
  styleUrl: './page-template-ui.scss',
})
export class PageTemplateUi {}
