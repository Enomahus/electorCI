import { setTestLanguage } from '@helpers/language-helper';
import { loginAsAdmin } from '@helpers/login-helper';
import { checkToast } from '@helpers/toast-helper';
import test, { expect, Locator, Page } from '@playwright/test';

function firstDataRow(page: Page): Locator {
  return page.locator('table tr').nth(1);
}

function toggleButton(row: Locator): Locator {
  return row.locator('[id^="btn-disable-"]');
}

// La grille n'est pas triée de façon stable côté backend (pas d'ORDER BY explicite) : une ligne
// modifiée peut changer de position, voire de page, après un rafraîchissement. On agrandit la
// pagination pour garder toutes les lignes visibles sur une seule page pendant les tests.
async function showAllRows(page: Page): Promise<void> {
  await page.getByLabel('Items per page:').click({ force: true });
  await page.getByRole('option', { name: '50' }).click();
  await expect(page.locator('.mat-mdc-paginator-range-label')).not.toContainText('1 – 10');
}

test.describe('Gestion des bureaux de vote', () => {
  setTestLanguage(test);

  test.beforeEach(async ({ page }) => {
    await loginAsAdmin(page);
    await page.goto('/admin/polling-stations');
    await expect(page.getByRole('heading', { level: 1 })).toHaveText('Les Bureaux de vote');
  });

  test("La liste des bureaux de vote s'affiche avec au moins un bureau", async ({ page }) => {
    await expect(firstDataRow(page)).toBeVisible();
    await expect(firstDataRow(page).locator('.btn-edit')).toBeVisible();
  });

  test("Modification d'un bureau de vote", async ({ page }) => {
    await showAllRows(page);
    await firstDataRow(page).locator('.btn-edit').click();
    await expect(page).toHaveURL(/\/admin\/polling-stations\/\d+\/edit/);
    const editUrl = page.url();

    const wordingInput = page.getByLabel('Description');
    await expect(wordingInput).not.toHaveValue('');
    const originalWording = await wordingInput.inputValue();
    const updatedWording = `${originalWording} E2E ${Date.now()}`;

    await wordingInput.fill(updatedWording);
    await page.getByRole('button', { name: 'Enregistrer' }).click();
    await checkToast(page, 'success', 'Bureau de vote modifier avec succès', { timeout: 15000 });
    await expect(page).toHaveURL(/\/admin\/polling-stations$/);

    // On revérifie directement sur la fiche plutôt que dans la liste : la grille n'étant pas
    // triée, la ligne modifiée peut ne plus être sur la page courante.
    await page.goto(editUrl);
    await expect(page.getByLabel('Description')).toHaveValue(updatedWording);

    // On restaure la valeur d'origine pour ne pas altérer les données seedées.
    await page.getByLabel('Description').fill(originalWording);
    await page.getByRole('button', { name: 'Enregistrer' }).click();
    await checkToast(page, 'success', 'Bureau de vote modifier avec succès', { timeout: 15000 });

    await page.goto(editUrl);
    await expect(page.getByLabel('Description')).toHaveValue(originalWording);
  });

  test("Activation et désactivation d'un bureau de vote", async ({ page }) => {
    await showAllRows(page);

    // On fige la référence sur l'id du bouton (lié au stationId) : la grille n'étant pas triée,
    // la position de la ligne peut changer après le rafraîchissement déclenché par le toggle.
    const btnId = await toggleButton(firstDataRow(page)).getAttribute('id');
    const toggleBtn = page.locator(`#${btnId}`);
    const initialTitle = await toggleBtn.getAttribute('title');

    await toggleBtn.click();
    await checkToast(page, 'success', 'Bureau de vote désactivé avec succès', { timeout: 15000 });
    await expect(toggleBtn).not.toHaveAttribute('title', initialTitle ?? '');

    await toggleBtn.click();
    await checkToast(page, 'success', 'Bureau de vote désactivé avec succès', { timeout: 15000 });
    await expect(toggleBtn).toHaveAttribute('title', initialTitle ?? '');
  });

  test('Validation du formulaire de modification (description requise)', async ({ page }) => {
    await firstDataRow(page).locator('.btn-edit').click();

    const wordingInput = page.getByLabel('Description');
    await expect(wordingInput).not.toHaveValue('');
    const originalWording = await wordingInput.inputValue();

    const saveBtn = page.getByRole('button', { name: 'Enregistrer' });
    await expect(saveBtn).toBeEnabled();

    await wordingInput.fill('');
    await expect(saveBtn).toBeDisabled();

    await wordingInput.fill(originalWording);
    await expect(saveBtn).toBeEnabled();
  });
});
