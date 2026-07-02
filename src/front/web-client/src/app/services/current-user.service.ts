import { Injectable } from '@angular/core';
import { BehaviorSubject } from 'rxjs';

@Injectable({
  providedIn: 'root',
})
export class CurrentUserService {
  public currentUserName$ = new BehaviorSubject<string>('');

  changeCurrentUserName(userName: string): void {
    this.currentUserName$.next(userName);
  }
}
