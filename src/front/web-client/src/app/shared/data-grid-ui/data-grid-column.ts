import { TemplateRef } from '@angular/core';

/**
 * Définition d'une colonne pour le composant réutilisable {@link DataGridUi}.
 *
 * Le champ `field` doit correspondre au nom de propriété attendu par le backend
 * (utilisé tel quel pour le tri et le filtrage côté serveur, chemins imbriqués supportés
 * ex: "district.wording").
 */
// eslint-disable-next-line @typescript-eslint/no-explicit-any
export interface DataGridColumn<T = any> {
  /** Nom du champ (propriété) — sert d'identifiant de colonne et de champ de tri/filtre backend. */
  field: string;
  /** Clé i18n de l'en-tête de colonne. */
  header: string;
  /** Colonne triable côté serveur. Défaut: true. */
  sortable?: boolean;
  /** Colonne exposant un filtre texte côté serveur. Défaut: true. */
  filterable?: boolean;
  /** Accesseur optionnel pour la valeur affichée. Défaut: row[field]. */
  value?: (row: T) => unknown;
  /** Gabarit de cellule personnalisé optionnel. Reçoit la ligne via `$implicit`. */
  cellTemplate?: TemplateRef<{ $implicit: T }>;
}
