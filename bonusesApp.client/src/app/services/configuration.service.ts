// ---------------------------------------
// Email: quickapp@ebenmonney.com
// Templates: www.ebenmonney.com/templates
// (c) 2024 www.ebenmonney.com/mit-license
// ---------------------------------------

import { Injectable, inject } from '@angular/core';
import { Subject } from 'rxjs';

import { AppTranslationService } from './app-translation.service';
import { ThemeManager } from './theme-manager';
import { LocalStoreManager } from './local-store-manager.service';
import { DBkeys } from './db-keys';
import { Utilities } from './utilities';
import { environment } from '../../environments/environment';

interface UserConfiguration {
  language: string | null;
  themeId: number | null;
  defaultSetting: number | null;
}

@Injectable({
  providedIn: 'root'
})
export class ConfigurationService {
  private localStorage = inject(LocalStoreManager);
  private translationService = inject(AppTranslationService);
  private themeManager = inject(ThemeManager);

  constructor() {
    this.loadLocalChanges();
  }

  set language(value: string | null) {
    this._language = value;
    this.saveToLocalStore(value, DBkeys.LANGUAGE);
    this.translationService.changeLanguage(value);
  }
  get language(): string {
    return this._language ?? ConfigurationService.defaultLanguage;
  }

  set themeId(value: number) {
    value = +value;
    this._themeId = value;
    this.saveToLocalStore(value, DBkeys.THEME_ID);
    this.themeManager.installTheme(this.themeManager.getThemeByID(value));
  }
  get themeId() {
    return this._themeId ?? ConfigurationService.defaultThemeId;
  }

  set defaultSetting(value: number) {
    value = +value;
    this._defaultSetting = value;
    this.saveToLocalStore(value, DBkeys.DEFAULT_SETTING);
  }
  get defaultSetting() {
    return this._defaultSetting ?? ConfigurationService.defaultSetting;
  }

  public static readonly appVersion = '9.19.0';

  // ***Specify default configurations here***
  public static readonly defaultLanguage = 'en';
  public static readonly defaultHomeUrl = '/';
  public static readonly defaultThemeId = 1;
  public static readonly defaultSetting = 5;

  // ***End of defaults***

  public baseUrl = environment.baseUrl ?? Utilities.baseUrl();
  public fallbackBaseUrl = environment.fallbackBaseUrl;

  private _language: string | null = null;
  private _themeId: number | null = null;
  private _defaultSetting: number | null = null;
  private onConfigurationImported: Subject<void> = new Subject<void>();

  configurationImported$ = this.onConfigurationImported.asObservable();

  private loadLocalChanges() {
    if (this.localStorage.exists(DBkeys.LANGUAGE)) {
      this._language = this.localStorage.getDataObject<string>(DBkeys.LANGUAGE);
      this.translationService.changeLanguage(this._language);
    } else {
      this.resetLanguage();
    }

    if (this.localStorage.exists(DBkeys.THEME_ID)) {
      this._themeId = this.localStorage.getDataObject<number>(DBkeys.THEME_ID);
      this.themeManager.installTheme(this.themeManager.getThemeByID(this._themeId as number));
    } else {
      this.resetTheme();
    }

    if (this.localStorage.exists(DBkeys.DEFAULT_SETTING)) {
      this._defaultSetting = this.localStorage.getDataObject<number>(DBkeys.DEFAULT_SETTING);
    }

  }

  private saveToLocalStore(data: unknown, key: string) {
    setTimeout(() => this.localStorage.savePermanentData(data, key));
  }

  public import(jsonValue: string | null) {
    this.clearLocalChanges();

    if (jsonValue) {
      const importValue: UserConfiguration = Utilities.JsonTryParse(jsonValue);

      if (importValue.language != null) {
        this.language = importValue.language;
      }

      if (importValue.themeId != null) {
        this.themeId = importValue.themeId;
      }

      if (importValue.defaultSetting != null) {
        this.defaultSetting = importValue.defaultSetting;
      }
    }

    this.onConfigurationImported.next();
  }

  public export(changesOnly = true): string {
    const exportValue: UserConfiguration = {
      language: changesOnly ? this._language : this.language,
      themeId: changesOnly ? this._themeId : this.themeId,
      defaultSetting: changesOnly ? this.defaultSetting : this.defaultSetting,
    };

    return JSON.stringify(exportValue);
  }

  public clearLocalChanges() {
    this._language = null;
    this._themeId = null;

    this.localStorage.deleteData(DBkeys.LANGUAGE);
    this.localStorage.deleteData(DBkeys.THEME_ID);
    this.localStorage.deleteData(DBkeys.DEFAULT_SETTING);

    this.resetLanguage();
    this.resetTheme();
    this.clearUserConfigKeys();
  }

  private resetLanguage() {
    const language = this.translationService.useBrowserLanguage();

    if (language) {
      this._language = language;
    } else {
      this._language = this.translationService.useDefaultLanguage();
    }
  }

  private resetTheme() {
    this.themeManager.installTheme();
    this._themeId = null;
  }


  private addKeyToUserConfigKeys(configKey: string) {
    const configKeys = this.localStorage.getDataObject<string[]>(DBkeys.USER_CONFIG_KEYS) ?? [];

    if (!configKeys.includes(configKey)) {
      configKeys.push(configKey);
      this.localStorage.savePermanentData(configKeys, DBkeys.USER_CONFIG_KEYS);
    }
  }

  private clearUserConfigKeys() {
    const configKeys = this.localStorage.getDataObject<string[]>(DBkeys.USER_CONFIG_KEYS);

    if (configKeys != null && configKeys.length > 0) {
      for (const key of configKeys) {
        this.localStorage.deleteData(key);
      }

      this.localStorage.deleteData(DBkeys.USER_CONFIG_KEYS);
    }
  }

  public saveConfiguration(data: unknown, configKey: string) {
    this.addKeyToUserConfigKeys(configKey);
    this.localStorage.savePermanentData(data, configKey);
  }

  public getConfiguration<T>(configKey: string, isDateType = false) {
    return this.localStorage.getDataObject<T>(configKey, isDateType);
  }
}
