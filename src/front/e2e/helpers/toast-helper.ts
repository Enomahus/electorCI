import { expect, Page } from '@playwright/test';

export async function checkToast(
  page: Page,
  toastType: 'success' | 'error' | 'warning',
  message?: string,
  options: {
    exact: boolean;
  } = { exact: true }
) {
  let elementToast = page.locator(`.toast-${toastType}`);
  if (message) {
    elementToast = elementToast.filter({ has: page.getByText(message, { exact: options.exact }) });
  }
  await expect(elementToast).toBeVisible();

  elementToast.click();
  await expect(elementToast).not.toBeVisible();
}
