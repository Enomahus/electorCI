import { CommonModule } from '@angular/common';
import { Component, DestroyRef, computed, inject, input, output, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { MatPaginatorModule, PageEvent } from '@angular/material/paginator';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { MatSortModule, Sort } from '@angular/material/sort';
import { MatTableModule } from '@angular/material/table';
import { TranslatePipe } from '@ngx-translate/core';
import { Subject, debounceTime } from 'rxjs';
import { GridDataQuery, GridSortDirection } from '../../services/nswag/api-nswag-client';
import { DataGridColumn } from './data-grid-column';

/**
 * Table de données réutilisable (Angular Material) : pagination, tri et filtrage
 * **côté serveur**. Le composant ne connaît pas la source de données : il émet une
 * `GridDataQuery` via `(queryChange)` à chaque interaction utilisateur, et le parent
 * se charge d'appeler l'API puis de fournir `data` / `total` / `loading`.
 *
 * Réutilisable pour n'importe quelle grille (districts, bureaux de vote, utilisateurs…) :
 * il suffit de fournir les `columns` et de brancher `(queryChange)`.
 */
@Component({
  selector: 'app-data-grid-ui',
  imports: [
    CommonModule,
    MatTableModule,
    MatSortModule,
    MatPaginatorModule,
    MatFormFieldModule,
    MatInputModule,
    MatIconModule,
    MatProgressBarModule,
    TranslatePipe,
  ],
  templateUrl: './data-grid-ui.html',
  styleUrl: './data-grid-ui.scss',
})
export class DataGridUi {
  // --- Entrées ---
  readonly columns = input.required<DataGridColumn[]>();
  readonly data = input<readonly unknown[]>([]);
  readonly total = input<number>(0);
  readonly loading = input<boolean>(false);
  readonly pageSizeOptions = input<number[]>([10, 25, 50]);
  /** Affiche la barre de recherche globale. */
  readonly showSearch = input<boolean>(true);
  /** Affiche la ligne de filtres par colonne. */
  readonly showColumnFilters = input<boolean>(true);

  // --- Sortie ---
  readonly queryChange = output<GridDataQuery>();

  // --- État interne (signaux) ---
  readonly pageIndex = signal(0);
  readonly pageSize = signal(10);
  private readonly sortField = signal<string | null>(null);
  private readonly sortDirection = signal<GridSortDirection | null>(null);
  readonly search = signal('');
  readonly columnFilters = signal<Record<string, string>>({});

  /** Identifiants de colonnes de la ligne d'en-tête (titres + tri). */
  readonly headerColumns = computed(() => this.columns().map((c) => c.field));
  /** Identifiants de colonnes de la ligne des filtres. */
  readonly filterColumns = computed(() => this.columns().map((c) => `${c.field}__filter`));

  private readonly filterDebounce$ = new Subject<void>();
  private readonly destroyRef = inject(DestroyRef);

  constructor() {
    // Les saisies de recherche/filtres sont débattues avant de déclencher un appel serveur.
    this.filterDebounce$
      .pipe(debounceTime(350), takeUntilDestroyed(this.destroyRef))
      .subscribe(() => {
        this.pageIndex.set(0);
        this.emit();
      });
  }

  cellValue(column: DataGridColumn, row: unknown): unknown {
    if (column.value) {
      return column.value(row);
    }
    return (row as Record<string, unknown>)[column.field];
  }

  filterColumnId(field: string): string {
    return `${field}__filter`;
  }

  onSortChange(sort: Sort): void {
    this.sortField.set(sort.direction ? sort.active : null);
    this.sortDirection.set(this.toSortDirection(sort.direction));
    this.pageIndex.set(0);
    this.emit();
  }

  onPageChange(event: PageEvent): void {
    this.pageIndex.set(event.pageIndex);
    this.pageSize.set(event.pageSize);
    this.emit();
  }

  onSearchChange(value: string): void {
    this.search.set(value);
    this.filterDebounce$.next();
  }

  onColumnFilterChange(field: string, value: string): void {
    this.columnFilters.update((filters) => ({ ...filters, [field]: value }));
    this.filterDebounce$.next();
  }

  private toSortDirection(direction: Sort['direction']): GridSortDirection | null {
    if (direction === 'asc') return 'ascending';
    if (direction === 'desc') return 'descending';
    return null;
  }

  private emit(): void {
    const filters = Object.entries(this.columnFilters())
      .filter(([, value]) => value?.trim())
      .map(([field, value]) => ({
        field,
        operator: 'contains' as const,
        value: value.trim(),
      }));

    const field = this.sortField();
    const direction = this.sortDirection();
    const sorts = field && direction ? [{ field, direction }] : [];

    const query: GridDataQuery = {
      skip: this.pageIndex() * this.pageSize(),
      take: this.pageSize(),
      search: this.search().trim() || undefined,
      filters,
      sorts,
    };

    this.queryChange.emit(query);
  }
}
