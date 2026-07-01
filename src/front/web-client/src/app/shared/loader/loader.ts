import { Component, input } from '@angular/core';

@Component({
  selector: 'app-loader-ui',
  imports: [],
  templateUrl: './loader.html',
  styleUrl: './loader.scss',
})
export class LoaderUi {
  message = input<string | null>(null);
  size = input<number>(10);
}
