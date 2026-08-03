import { selectDistrictChain } from '@helpers/district-select-helper';
import { setTestLanguage } from '@helpers/language-helper';
import { loginAsAdmin } from '@helpers/login-helper';
import { selectOptionContainingText } from '@helpers/select-helper';
import { checkToast } from '@helpers/toast-helper';
import test, { expect, Page } from '@playwright/test';

interface NewUser {
  firstName: string;
  lastName: string;
  email: string;
  phone: string;
}

// La création/mise à jour d'un utilisateur (hachage du mot de passe, rôles, circonscription)
// est plus lente que les autres opérations : on laisse plus de marge au toast de confirmation.
const WRITE_TOAST_TIMEOUT = 15000;

function getUserRow(page: Page, text: string) {
  return page.locator('table tr', { hasText: text });
}

async function fillPhoneNumber(page: Page, value: string): Promise<void> {
  await page.locator('.input-block', { hasText: 'Téléphone' }).locator('input[type="tel"]').fill(value);
}

async function fillUserForm(page: Page, user: NewUser): Promise<void> {
  await page.getByLabel('Prénom').fill(user.firstName);
  await page.getByLabel('Nom', { exact: true }).fill(user.lastName);
  await fillPhoneNumber(page, user.phone);
  // getByLabel('Email') est ambigu : le libellé du bouton radio "Authentification par email"
  // porte aussi le texte "Email". On cible le rôle textbox pour ne viser que le champ texte.
  await page.getByRole('textbox', { name: 'Email' }).fill(user.email);
  await selectOptionContainingText(page.getByLabel('Activité'), 'Electeur');
  await selectDistrictChain(page);
  await page.getByLabel('Actif').check();
}

async function createUser(page: Page, user: NewUser): Promise<void> {
  await page.getByRole('link', { name: 'Créer un utilisateur' }).click();
  await expect(page).toHaveURL(/\/admin\/users\/new/);
  await expect(page.getByRole('heading', { level: 1 })).toHaveText('Créer un utilisateur');

  await fillUserForm(page, user);
  await page.getByLabel('Mot de passe', { exact: true }).fill('Secret12!');
  await page.getByLabel('Confirmation mot de passe').fill('Secret12!');

  await page.getByRole('button', { name: 'Enregistrer' }).click();
  await checkToast(page, 'success', 'Utilisateur créé avec succès', { timeout: WRITE_TOAST_TIMEOUT });
  await expect(page).toHaveURL(/\/admin\/users$/);
}

test.describe('Gestion des utilisateurs', () => {
  setTestLanguage(test);

  test.beforeEach(async ({ page }) => {
    await loginAsAdmin(page);
    await page.goto('/admin/users');
    await expect(page.getByRole('heading', { level: 1 })).toHaveText('Liste des Utilisateurs');
  });

  test("La liste des utilisateurs s'affiche avec au moins un utilisateur", async ({ page }) => {
    await expect(page.getByRole('link', { name: 'Créer un utilisateur' })).toBeVisible();
    await expect(page.getByLabel('Rechercher')).toBeVisible();
    await expect(page.locator('table tr').nth(1)).toBeVisible();
  });

  test("Création d'un utilisateur", async ({ page }) => {
    const unique = Date.now();
    const user: NewUser = {
      firstName: 'Jean',
      lastName: `E2E Nom ${unique}`,
      email: `e2e.user.${unique}@test.ci`,
      phone: '0102030405',
    };

    await createUser(page, user);

    await page.getByLabel('Rechercher').fill(user.email);
    await expect(getUserRow(page, user.email)).toBeVisible();
    await expect(getUserRow(page, user.email)).toContainText(user.lastName);
  });

  test("Modification d'un utilisateur", async ({ page }) => {
    const unique = Date.now();
    const user: NewUser = {
      firstName: 'Jean',
      lastName: `E2E Nom ${unique}`,
      email: `e2e.edit.${unique}@test.ci`,
      phone: '0102030405',
    };
    const updatedLastName = `${user.lastName} modifié`;

    await createUser(page, user);

    await page.getByLabel('Rechercher').fill(user.email);
    await getUserRow(page, user.email).locator('.btn-edit').click();

    await expect(page).toHaveURL(/\/admin\/users\/[^/]+\/edit/);
    await expect(page.getByRole('heading', { level: 1 })).toHaveText('Modifier un utilisateur');

    await page.getByLabel('Nom', { exact: true }).fill(updatedLastName);
    // Le formulaire d'édition ne pré-remplit ni "Actif" ni la méthode d'authentification
    // (l'utilisateur fraîchement créé n'a pas de authProvider persisté) : il faut les renseigner
    // explicitement pour que le formulaire devienne valide.
    await page.getByLabel('Actif').check();
    // getByLabel('Email') est ambigu avec le champ texte "Email" : on cible le rôle radio.
    await page.getByRole('radio', { name: 'Email' }).check();

    await page.getByRole('button', { name: 'Enregistrer' }).click();
    await checkToast(page, 'success', 'Utilisateur mis à jour avec succès', {
      timeout: WRITE_TOAST_TIMEOUT,
    });
    await expect(page).toHaveURL(/\/admin\/users$/);

    await page.getByLabel('Rechercher').fill(updatedLastName);
    await expect(getUserRow(page, updatedLastName)).toBeVisible();
  });

  test("Suppression d'un utilisateur", async ({ page }) => {
    const unique = Date.now();
    const user: NewUser = {
      firstName: 'Jean',
      lastName: `E2E Nom ${unique}`,
      email: `e2e.delete.${unique}@test.ci`,
      phone: '0102030405',
    };

    await createUser(page, user);

    await page.getByLabel('Rechercher').fill(user.email);
    await getUserRow(page, user.email).locator('.btn-del').click();

    await expect(page.getByRole('heading', { name: 'Confirmation' })).toBeVisible();
    await page.getByRole('button', { name: 'Supprimer' }).click();

    await checkToast(page, 'success', 'Utilisateur supprimé avec succès', {
      timeout: WRITE_TOAST_TIMEOUT,
    });
    await expect(getUserRow(page, user.email)).toBeHidden();
  });

  test('Validation du formulaire de création (champs requis)', async ({ page }) => {
    await page.getByRole('link', { name: 'Créer un utilisateur' }).click();

    const saveBtn = page.getByRole('button', { name: 'Enregistrer' });
    await expect(saveBtn).toBeDisabled();

    const unique = Date.now();
    await fillUserForm(page, {
      firstName: 'Jean',
      lastName: `E2E Nom ${unique}`,
      email: `e2e.valid.${unique}@test.ci`,
      phone: '0102030405',
    });

    await page.getByLabel('Mot de passe', { exact: true }).fill('Secret12!');
    await page.getByLabel('Confirmation mot de passe').fill('Different12!');
    await expect(saveBtn).toBeDisabled();

    await page.getByLabel('Confirmation mot de passe').fill('Secret12!');
    await expect(saveBtn).toBeEnabled();
  });
});
