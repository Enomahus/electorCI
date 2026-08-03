import { Locator } from '@playwright/test';

export async function selectOptionContainingText(select: Locator, text: string): Promise<void> {
  const option = select.locator('option', { hasText: text }).first();
  const value = await option.getAttribute('value');
  if (!value) {
    throw new Error(`Aucune option contenant "${text}" n'a été trouvée.`);
  }
  await select.selectOption(value);
}
