export interface Country {
  name: string;
  iso2: string;
  dialCode: string;
  priority: number;
  areaCodes?: string[];
  htmlId: string;
  flagClass: string;
  placeHolder: string;
}

export interface CountryData {
  name: string;
  code: string;
  dial: string;
  flag: string;
}
