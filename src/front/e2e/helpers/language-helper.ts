import {
  Page,
  PlaywrightTestArgs,
  PlaywrightTestOptions,
  PlaywrightWorkerArgs,
  PlaywrightWorkerOptions,
  TestType,
} from '@playwright/test';

export function setTestLanguage(
  test: TestType<PlaywrightTestArgs & PlaywrightTestOptions, PlaywrightWorkerArgs & PlaywrightWorkerOptions>
): void {
  test.beforeEach(async ({ page }) => {
    setDefaultLanguage(page);
  });
}

export async function setDefaultLanguage(page: Page) {
  await page.addInitScript(() => {
    localStorage.setItem('chosenLanguage', 'fr');
  });
}
