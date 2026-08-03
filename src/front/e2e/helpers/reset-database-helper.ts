import {
  APIRequestContext,
  PlaywrightTestArgs,
  PlaywrightTestOptions,
  PlaywrightWorkerArgs,
  PlaywrightWorkerOptions,
  TestType,
} from '@playwright/test';

export async function executeDbReset(request: APIRequestContext) {
  const apiUrl = process.env.API_URL ?? 'http://localhost:44200';
  await request.get(`${apiUrl}/testing/reset-database`, { timeout: 0 });
}

export function autoResetDatabase(
  test: TestType<PlaywrightTestArgs & PlaywrightTestOptions, PlaywrightWorkerArgs & PlaywrightWorkerOptions>
): void {
  test.afterEach(async ({ request }) => {
    test.setTimeout(60 * 1000);
    if (!test.info().title.includes('(reset)')) return;
    await executeDbReset(request);
  });
}
