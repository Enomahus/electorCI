import {
  APIRequestContext,
  Page,
  PlaywrightTestArgs,
  PlaywrightTestOptions,
  PlaywrightWorkerArgs,
  PlaywrightWorkerOptions,
  TestType,
} from 'playwright/test';

export async function getCurrentTime(request: APIRequestContext) {
  const apiUrl = process.env.API_URL ?? 'http://localhost:40180';
  const res = await request.get(`${apiUrl}/info/date`);
  const dateStr: string = await res.json();
  return new Date(dateStr);
}

// This will be called when tests need to be run with the browser time set to the test value of 2024-01-01T10:00:00
export function setTestTime(
  test: TestType<PlaywrightTestArgs & PlaywrightTestOptions, PlaywrightWorkerArgs & PlaywrightWorkerOptions>
): void {
  test.beforeEach(async ({ page }) => {
    await setFixedTime(page);
  });
}

export async function setFixedTime(page: Page) {
  const currentTime = await getCurrentTime(page.request);
  await page.clock.install({ time: currentTime });
}

export async function getCurrentYearAndTrimester(page: Page): Promise<{ year: number; trimester: number }> {
  const now = new Date(await page.evaluate(() => Date.now()));
  return {
    year: now.getFullYear(),
    trimester: Math.ceil((now.getMonth() + 1) / 3),
  };
}

export function periodLabel(year: number, trimester: number): string {
  return `${year} - Trimestre ${trimester}`;
}

export function addTrimesters(
  year: number,
  trimester: number,
  delta: number
): { year: number; trimester: number } {
  let t = trimester + delta;
  let y = year;
  while (t > 4) {
    t -= 4;
    y++;
  }
  while (t < 1) {
    t += 4;
    y--;
  }
  return { year: y, trimester: t };
}
