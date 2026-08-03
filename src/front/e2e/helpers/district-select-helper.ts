import { Page } from '@playwright/test';
import { selectOptionContainingText } from './select-helper';

// Chaîne de circonscriptions issue des données seedées par le backend (DistrictData.cs) :
// Bélier > Didiévi > Boli > Boli > EPP Allanikro
export const SEEDED_DISTRICT_CHAIN = {
  region: 'Bélier',
  department: 'Didiévi',
  subPrefecture: 'Boli',
  municipality: 'Boli',
  votingLocation: 'EPP Allanikro',
};

export async function selectDistrictChain(page: Page): Promise<void> {
  await selectOptionContainingText(page.locator('#regionId'), SEEDED_DISTRICT_CHAIN.region);
  await selectOptionContainingText(page.locator('#departmentId'), SEEDED_DISTRICT_CHAIN.department);
  await selectOptionContainingText(page.locator('#subPrefecture'), SEEDED_DISTRICT_CHAIN.subPrefecture);
  await selectOptionContainingText(page.locator('#municipalityId'), SEEDED_DISTRICT_CHAIN.municipality);
  await selectOptionContainingText(page.locator('#votingLocationId'), SEEDED_DISTRICT_CHAIN.votingLocation);
}
