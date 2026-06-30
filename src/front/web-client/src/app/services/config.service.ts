import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root',
})
export class ConfigService {
  private static AppConfig?: Config;

  static async loadConfigFile(): Promise<Config> {
    try {
      const cacheBuster = new Date().getTime();
      const response = await fetch(`assets/config.json?v=${cacheBuster}`);

      if (!response.ok) {
        throw new Error(`HTTP error! status: ${response.status}`);
      }

      //const response = await fetch(`assets/config.json?v=${packageJson.version}`);
      const data: Config = await response.json();
      ConfigService.AppConfig = data;
      return data;
    } catch (error) {
      console.error('Failed to load application configuration:', error);
      throw error;
    }
  }

  getConfig(): Config {
    if (!ConfigService.AppConfig) {
      const errMsg = 'Config file not loaded!';
      console.error(errMsg);
      throw Error(errMsg);
    }

    return ConfigService.AppConfig;
  }
}

interface Config {
  apiUrl: string;
}
