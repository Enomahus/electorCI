import { Injectable } from '@angular/core';
import { BehaviorSubject } from 'rxjs';
import { Theme } from '../enums/theme.enum';

@Injectable({
  providedIn: 'root',
})
export class ThemeService {
  currentTheme$ = new BehaviorSubject<Theme>(Theme.light);
}
