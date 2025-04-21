import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { catchError } from 'rxjs/operators';

import { EndpointBase } from './endpoint-base.service';
import { ConfigurationService } from './configuration.service';
import {Bonuses, BonusesResponse} from '../models/bonuses.model';

@Injectable({
  providedIn: 'root'
})
export class BonusesEndpoint extends EndpointBase {
  private http = inject(HttpClient);
  private configurations = inject(ConfigurationService);
  get bonusesUrl() { return this.configurations.baseUrl + '/api/bonuses'; }
  get giveBonusUrl() { return this.configurations.baseUrl + '/api/bonuses/{0}/giveBonus'; }
  get setSettingToAllUrl() { return this.configurations.baseUrl + '/api/bonuses/setSettingToAll'; }

  getBonuses(offset: number, limit: number, search: string): Observable<BonusesResponse> {

    const url = `${this.bonusesUrl}?offset=${offset}&pageSize=${limit}&search=${search}`;

    return this.http.get<BonusesResponse>(url, this.requestHeaders).pipe(
      catchError(error => {
        return this.handleError<BonusesResponse>(error, () => this.getBonuses(offset, limit, search));
      }));
  }

  saveNewBonuses(bonuses: Bonuses): Observable<Bonuses[]> {

    const url = this.bonusesUrl;
    const content = JSON.stringify(bonuses);

    return this.http.post<Bonuses[]>(url, content, this.requestHeaders).pipe(
      catchError(error => {
        return this.handleError<Bonuses[]>(error, () => this.saveNewBonuses(bonuses));
      }));
  }

  deleteBonuses(bonuses: Bonuses): Observable<void> {
    const url = `${this.bonusesUrl}/${bonuses.id}`;

    return this.http.delete<void>(url, this.requestHeaders).pipe(
      catchError(error => {
        return this.handleError<void>(error, () => this.deleteBonuses(bonuses));
      }));
  }

  updateBonuses(bonuses: Bonuses): Observable<void> {
    const url = this.bonusesUrl;
    const content = JSON.stringify(bonuses);

    return this.http.patch<void>(url, content , this.requestHeaders).pipe(
      catchError(error => {
        return this.handleError<void>(error, () => this.updateBonuses(bonuses));
      }));
  }

  giveBonus(id: number): Observable<void> {
    const url = this.giveBonusUrl.replace("{0}", id.toString());

    return this.http.post<void>(url, null, this.requestHeaders).pipe(
      catchError(error => {
        return this.handleError<void>(error, () => this.giveBonus(id));
      }));
  }

  setSettingToAll(setting: number): Observable<void> {
    const url = this.setSettingToAllUrl;

    const content = JSON.stringify({'setting': setting});
    return this.http.post<void>(url, content, this.requestHeaders).pipe(
      catchError(error => {
        return this.handleError<void>(error, () => this.setSettingToAll(setting));
      }));
  }
}
