import { Page } from '@playwright/test';

export async function login(page: Page, userName: string, password: string): Promise<void> {
  await page.goto('/login');
  await page.getByLabel('Identifiant').fill(userName);
  await page.getByLabel('Mot de passe').fill(password);
  await page.getByRole('button', { name: 'Se connecter' }).click();
  await page.waitForURL('**/home');
}

export async function loginAsAdmin(page: Page): Promise<void> {
  await login(page, 'admin', 'Secret12');
}
