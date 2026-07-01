import { Component } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { FooterUi } from './footer-ui/footer-ui';
import { NavbarUi } from './navbar-ui/navbar-ui';

@Component({
  selector: 'app-page-template-ui',
  imports: [NavbarUi, FooterUi, RouterOutlet],
  templateUrl: './page-template-ui.html',
  styleUrl: './page-template-ui.scss',
})
export class PageTemplateUi {}
