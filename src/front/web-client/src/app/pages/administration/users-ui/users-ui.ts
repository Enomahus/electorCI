import { DatePipe, NgClass } from '@angular/common';
import { Component, inject, signal } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { MatDialog } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { MatPaginatorModule } from '@angular/material/paginator';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { MatSortModule } from '@angular/material/sort';
import { MatTableModule } from '@angular/material/table';
import { MatTooltipModule } from '@angular/material/tooltip';
import { TranslatePipe, TranslateService } from '@ngx-translate/core';
import {
  BehaviorSubject,
  catchError,
  debounceTime,
  distinctUntilChanged,
  finalize,
  map,
  Observable,
  of,
  takeUntil,
} from 'rxjs';
import { UserApiService } from '../../../services/api/user.api.service';
import { PermissionDirective } from '../../../services/auth/permission.directive';
import { GetUsersResponse, GridDataQuery } from '../../../services/nswag/api-nswag-client';
import { BaseTable } from '../../../shared/base-table/base-table';
import { ConfirmDialogUi } from '../../../shared/confirm-dialog/confirm-dialog';
import { RouterLink } from "@angular/router";

@Component({
  selector: 'app-users-ui',
  imports: [
    TranslatePipe,
    PermissionDirective,
    MatTableModule,
    MatSortModule,
    MatPaginatorModule,
    MatFormFieldModule,
    MatInputModule,
    MatIconModule,
    MatButtonModule,
    MatTooltipModule,
    MatProgressBarModule,
    NgClass,
    DatePipe,
    RouterLink
],
  templateUrl: './users-ui.html',
  styleUrl: './users-ui.scss',
})
export class UsersUi extends BaseTable<GetUsersResponse> {
  private readonly userService = inject(UserApiService);
  private readonly translateService = inject(TranslateService);
  private readonly dialog = inject(MatDialog);

  readonly displayedColumns = [
    'lastName',
    'firstName',
    'email',
    'phone',
    'district',
    'isActive',
    'isAdmin',
    'createdAt',
    'authProvider',
    'actions',
  ];

  isDeleting = signal(false);
  private currentSearch = signal('');
  private searchSubject = new BehaviorSubject<string>('');

  constructor() {
    super();

    this.searchSubject
      .pipe(debounceTime(300), distinctUntilChanged(), takeUntil(this.destroy$))
      .subscribe((value) => {
        this.currentSearch.set(value);
        if (this.paginator) {
          this.paginator.pageIndex = 0;
        }
        this.refreshData();
      });
  }

  override getData(query: GridDataQuery): Observable<{ data: GetUsersResponse[]; total: number }> {
    const search = this.currentSearch().trim();

    return this.userService
      .getUsers({
        ...query,
        search: search || undefined,
      })
      .pipe(
        map((result) => ({
          data: result.data ?? [],
          total: result.total ?? 0,
        })),
        catchError(() => {
          return of({ data: [], total: 0 });
        }),
      );
  }

  onSearchChange(value: string): void {
    this.searchSubject.next(value);
  }

   onToggleActiveUser(user: GetUsersResponse): void {
    // Implement toggle active user logic here
  }

  onDelete(user: GetUsersResponse): void {
    if (!user.canBeDeleted || this.isDeleting()) {
      return;
    }

    const dialogRef = this.dialog.open(ConfirmDialogUi, {
      width: '400',
      data: { name: `${user.firstName} ${user.lastName}` },
    });

    dialogRef.afterClosed().subscribe((res) => {
      if (res) {
        this.userService
          .deleteUser(user.id, {
            successMessage: this.translateService.instant('users.successDeleting'),
            errorMessage: this.translateService.instant('users.errorDeleting'),
          })
          .pipe(
            finalize(() => this.isDeleting.set(false)),
            takeUntil(this.destroy$),
          )
          .subscribe({
            next: () => this.refreshData(),
            error: () => undefined,
          });
      }
    });
  }
}
