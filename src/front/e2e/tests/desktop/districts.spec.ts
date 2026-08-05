import { getDistrictTreeItem, openDistrictActions } from '@helpers/district-tree-helper';
import { setTestLanguage } from '@helpers/language-helper';
import { loginAsAdmin } from '@helpers/login-helper';
import { autoResetDatabase } from '@helpers/reset-database-helper';
import { checkToast } from '@helpers/toast-helper';
import test, { expect, Page } from '@playwright/test';

async function createRegionDistrict(page: Page, wording: string): Promise<void> {
  await page.getByRole('link', { name: 'Créer une circonscription' }).click();
  await expect(page).toHaveURL(/\/admin\/districts\/new/);
  await page.getByLabel('Libellé').fill(wording);
  await page.getByRole('button', { name: 'Enregistrer' }).click();
  await checkToast(page, 'success', 'Circonscription créée avec succès');
  await expect(page).toHaveURL(/\/admin\/districts$/);
}

test.describe('Gestion des circonscriptions', () => {
  setTestLanguage(test);

  test.beforeEach(async ({ page }) => {
    await loginAsAdmin(page);
    await page.goto('/admin/districts');
    await expect(page.getByRole('heading', { level: 1 })).toHaveText('Les Circonscriptions électorales');
  });
  autoResetDatabase(test);

  test("La liste des circonscriptions s'affiche avec au moins une région", async ({ page }) => {
    await expect(page.getByRole('link', { name: 'Créer une circonscription' })).toBeVisible();
    await expect(page.locator('li[role="treeitem"]').first()).toBeVisible();
  });

  test("Création d'une circonscription de type région", async ({ page }) => {
    const wording = `Region E2E ${Date.now()}`;

    await createRegionDistrict(page, wording);

    await expect(getDistrictTreeItem(page, wording)).toBeVisible();
  });

  test("Modification du libellé d'une circonscription", async ({ page }) => {
    const wording = `Region E2E ${Date.now()}`;
    const updatedWording = `${wording} modifiée`;

    await createRegionDistrict(page, wording);

    await openDistrictActions(page, wording);
    await page.getByRole('menuitem', { name: 'Modifier' }).click();

    await expect(page).toHaveURL(/\/admin\/districts\/\d+\/edit/);
    await expect(page.getByRole('heading', { level: 1 })).toHaveText('Modifier la circonscription');

    await page.getByLabel('Libellé').fill(updatedWording);
    await page.getByRole('button', { name: 'Enregistrer' }).click();

    await checkToast(page, 'success', 'Circonscription mise à jour avec succès');
    await expect(page).toHaveURL(/\/admin\/districts$/);
    await expect(getDistrictTreeItem(page, updatedWording)).toBeVisible();
  });

  test("Activation et désactivation d'une circonscription", async ({ page }) => {
    const wording = `Region E2E ${Date.now()}`;

    await createRegionDistrict(page, wording);

    await openDistrictActions(page, wording);
    await page.getByRole('menuitem', { name: 'Désactiver' }).click();
    await checkToast(page, 'success', 'Circonscription mise à jour avec succès');

    await openDistrictActions(page, wording);
    await expect(page.getByRole('menuitem', { name: 'Activer' })).toBeVisible();
    await page.getByRole('menuitem', { name: 'Activer' }).click();
    await checkToast(page, 'success', 'Circonscription mise à jour avec succès');

    await openDistrictActions(page, wording);
    await expect(page.getByRole('menuitem', { name: 'Désactiver' })).toBeVisible();
  });

  test("Suppression d'une circonscription", async ({ page }) => {
    const wording = `Region E2E ${Date.now()}`;

    await createRegionDistrict(page, wording);

    await openDistrictActions(page, wording);
    await page.getByRole('menuitem', { name: 'Supprimer' }).click();

    await expect(page.getByRole('heading', { name: 'Confirmation' })).toBeVisible();
    await page.getByRole('button', { name: 'Supprimer' }).click();

    await checkToast(page, 'success', 'Circonscription supprimée avec succès');
    await expect(getDistrictTreeItem(page, wording)).toBeHidden();
  });

  test('Validation du formulaire de création (libellé requis)', async ({ page }) => {
    await page.getByRole('link', { name: 'Créer une circonscription' }).click();

    const saveBtn = page.getByRole('button', { name: 'Enregistrer' });
    await expect(saveBtn).toBeDisabled();

    await page.getByLabel('Libellé').fill(`Region E2E ${Date.now()}`);
    await expect(saveBtn).toBeEnabled();

    await page.getByLabel('Libellé').fill('');
    await expect(saveBtn).toBeDisabled();
  });
});
