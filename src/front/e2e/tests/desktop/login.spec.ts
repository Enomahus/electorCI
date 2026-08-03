import { setTestLanguage } from '@helpers/language-helper';
import test, { expect } from '@playwright/test';

test.describe('Page Connexion', () => {
  setTestLanguage(test);

  test.beforeEach(async ({ page }) => {
    await page.goto('/home');
    await page.getByRole('button', { name: 'Connexion' }).click();
  });

  test('La page de connexion est accessible', async ({ page }) => {
    await page.getByRole('link', { name: 'Superviseur' }).click();

    await expect(page).toHaveURL(/\/login/);
    await expect(page.getByLabel('Identifiant')).toBeVisible();
  });

  test('La connexion fonctionne', async ({ page }) => {
    await page.getByRole('link', { name: 'Superviseur' }).click();
    await page.getByLabel('Identifiant').fill('user1');
    await page.getByLabel('Mot de passe').fill('Secret12');
    await page.getByRole('button', { name: 'Se connecter' }).click();

    await expect(page.locator('app-home-ui')).toBeInViewport();
  });

  test('Connexion refusée', async ({ page }) => {
    await page.goto('/login');

    const loginError = page.getByText('Mot de passe ou identifiant erroné');

    await expect(loginError).toBeHidden();
    await page.getByLabel('Identifiant').fill('user1');
    await page.getByLabel('Mot de passe').fill('WrongPass1');
    await page.getByRole('button', { name: 'Se connecter' }).click();
    await expect(loginError).toBeVisible();
  });

  test('Validation du formulaire de connexion', async ({ page }) => {
    await page.goto('/login');
    const loginBtn = page.getByRole('button', { name: 'Se connecter' });
    await expect(loginBtn).toBeDisabled();

    await page.getByLabel('Identifiant').fill('user1');
    await expect(loginBtn).toBeDisabled();

    await page.getByLabel('Mot de passe').fill('Secret12');
    await expect(loginBtn).toBeEnabled();

    await page.getByLabel('Identifiant').fill('');
    await expect(loginBtn).toBeDisabled();
  });
});
