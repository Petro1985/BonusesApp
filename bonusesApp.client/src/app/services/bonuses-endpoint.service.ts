import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { catchError } from 'rxjs/operators';

import { EndpointBase } from './endpoint-base.service';
import { ConfigurationService } from './configuration.service';
import {Bonuses} from '../models/bonuses.model';

@Injectable({
  providedIn: 'root'
})
export class BonusesEndpoint extends EndpointBase {
  private http = inject(HttpClient);
  private configurations = inject(ConfigurationService);
  get bonusesUrl() { return this.configurations.baseUrl + '/api/bonuses'; }

  getBonuses(offset: number, limit: number, search: string): Observable<Bonuses[]> {

    const url = `${this.bonusesUrl}?offset=${offset}&limit=${limit}&search=${search}`;

    return this.http.get<Bonuses[]>(url, this.requestHeaders).pipe(
      catchError(error => {
        return this.handleError<Bonuses[]>(error, () => this.getBonuses(offset, limit, search));
      }));
  }

  saveBonuses(bonuses: Bonuses): Observable<Bonuses[]> {

    const url = this.bonusesUrl;
    const content = JSON.stringify(bonuses);

    return this.http.post<Bonuses[]>(url, content, this.requestHeaders).pipe(
      catchError(error => {
        return this.handleError<Bonuses[]>(error, () => this.saveBonuses(bonuses));
      }));
  }
}
