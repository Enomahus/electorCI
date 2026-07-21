import { Component } from '@angular/core';
import { TranslatePipe } from '@ngx-translate/core';

@Component({
  selector: 'app-footer-ui',
  imports: [TranslatePipe],
  templateUrl: './footer-ui.html',
  styleUrl: './footer-ui.scss',
})
export class FooterUi {}
