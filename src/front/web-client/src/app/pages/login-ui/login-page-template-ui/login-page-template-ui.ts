import { Component } from '@angular/core';
import { FooterUi } from '../../../shared/page-template-ui/footer-ui/footer-ui';
import { LanguageSelectorUi } from '../../../shared/page-template-ui/language-selector-ui/language-selector-ui';

@Component({
  selector: 'app-login-page-template-ui',
  imports: [FooterUi, LanguageSelectorUi],
  templateUrl: './login-page-template-ui.html',
  styleUrl: './login-page-template-ui.scss',
})
export class LoginPageTemplateUi {}
