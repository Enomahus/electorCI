import { AfterViewInit, Component, OnDestroy, signal, ViewChild } from '@angular/core';
import { MatPaginator } from '@angular/material/paginator';
import { MatSort, SortDirection } from '@angular/material/sort';
import {
  BehaviorSubject,
  catchError,
  EMPTY,
  map,
  merge,
  Observable,
  of,
  startWith,
  Subject,
  switchMap,
  takeUntil,
} from 'rxjs';
import { GridDataQuery, GridSort, GridSortDirection } from '../../services/nswag/api-nswag-client';

/**
 * Base réutilisable pour les tables paginées/triées **côté serveur** (Angular Material).
 *
 * La classe construit une {@link GridDataQuery} (pagination `skip`/`take` + tri) à partir
 * du `MatPaginator` et du `MatSort`, puis la transmet à `getData`. Les sous-classes n'ont
 * qu'à implémenter `getData` en appelant leur service API ; elles peuvent enrichir la requête
 * reçue (recherche globale, filtres par colonne) avant l'appel.
 *
 * Le format de réponse `{ data, total }` correspond au `GridDataResponse<T>` du backend.
 */
@Component({
  selector: 'app-base-table',
  template: '',
})
export abstract class BaseTable<TResponse> implements AfterViewInit, OnDestroy {
  data = signal<TResponse[]>([]);
  resultsLength = signal(0);
  isLoadingResults = signal(true);

  protected destroy$ = new Subject<void>();
  protected refresh$ = new BehaviorSubject<void>(undefined); // Permet de déclencher un rafraîchissement manuel des données

  @ViewChild(MatPaginator) paginator?: MatPaginator;
  @ViewChild(MatSort) sort?: MatSort;

  /**
   * Charge les données pour la requête de grille fournie. La requête est déjà renseignée
   * avec la pagination et le tri courants ; les sous-classes peuvent y ajouter la recherche
   * ou des filtres avant d'appeler le service.
   */
  abstract getData(query: GridDataQuery): Observable<{ data: TResponse[]; total: number }>;

  ngAfterViewInit(): void {
    if (!this.sort || !this.paginator) {
      return;
    }

    const sort = this.sort;
    const paginator = this.paginator;

    // Si l'utilisateur trie, on revient à la première page
    sort.sortChange.pipe(takeUntil(this.destroy$)).subscribe(() => (paginator.pageIndex = 0));

    merge(sort.sortChange, paginator.page, this.refresh$)
      .pipe(
        startWith({}),
        switchMap(() => {
          this.isLoadingResults.set(true);
          return this.getData(this.buildQuery()).pipe(
            catchError((err) => {
              if (err.name === 'CanceledError' || err.status === 0) {
                return EMPTY;
              }
              console.log('Error loading data', err);
              return of({ data: [], total: 0 });
            }),
          );
        }),
        map((response) => {
          this.isLoadingResults.set(false);

          if (!response) return this.data();

          this.resultsLength.set(response.total);
          return response.data;
        }),
        takeUntil(this.destroy$),
      )
      .subscribe((res) => this.data.set(res));
  }

  /** Construit la requête de grille (pagination + tri) à partir du paginator et du tri courants. */
  protected buildQuery(): GridDataQuery {
    const pageIndex = this.paginator?.pageIndex ?? 0;
    const pageSize = this.paginator?.pageSize ?? 10;

    const sorts: GridSort[] = [];
    const direction = this.toSortDirection(this.sort?.direction);
    if (this.sort?.active && direction) {
      sorts.push({ field: this.sort.active, direction });
    }

    return {
      skip: pageIndex * pageSize,
      take: pageSize,
      sorts,
    };
  }

  private toSortDirection(direction: SortDirection | undefined): GridSortDirection | null {
    if (direction === 'asc') return 'ascending';
    if (direction === 'desc') return 'descending';
    return null;
  }

  refreshData(): void {
    this.refresh$.next();
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }
}
