import { Locator, Page } from '@playwright/test';

export function getDistrictTreeItem(page: Page, wording: string): Locator {
  return page
    .locator('li[role="treeitem"]')
    .filter({ has: page.locator('.tree-item-label', { hasText: wording }) });
}

export async function openDistrictActions(page: Page, wording: string): Promise<void> {
  await getDistrictTreeItem(page, wording)
    .getByRole('button', { name: 'Actions', exact: true })
    .click();
}
