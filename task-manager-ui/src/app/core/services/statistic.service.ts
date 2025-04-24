import { Injectable } from '@angular/core';
import { environment } from '../../../environments/environment';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { ChartDataResponse } from '../models/chart.model';

@Injectable({
  providedIn: 'root',
})
export class StatisticService {
  private apiUrl = `${environment.apiUrl}/statistics`;

  constructor(private http: HttpClient) {}

  getTaskCount(groupBy: string): Observable<ChartDataResponse> {
    return this.http.get<ChartDataResponse>(
      `${this.apiUrl}/tasks/count?groupBy=${groupBy}`
    );
  }
}
