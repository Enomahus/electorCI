import { FakeFile } from './fake-file';

export interface UploadMultipleFormValue {
  localFiles?: File[];
  distantFileIds?: string[];
  fakeFiles?: FakeFile[];
}
