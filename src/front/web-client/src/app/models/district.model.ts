import { ElectoralDistrictLevel } from '../services/nswag/api-nswag-client';

export interface DistrictNode {
  id: number;
  code: string;
  wording: string;
  parentId: number;
  active: boolean;
  level: ElectoralDistrictLevel;
  children?: DistrictNode[];
}
