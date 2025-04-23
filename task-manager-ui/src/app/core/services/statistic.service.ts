import { Injectable } from '@angular/core';
import { environment } from '../../../environments/environment';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { TaskCountResponse } from '../models/task.model';

@Injectable({
  providedIn: 'root',
})
export class StatisticService {
  private apiUrl = `${environment.apiUrl}/statistics`;

  constructor(private http: HttpClient) {}

  getTaskCount(): Observable<TaskCountResponse> {
    return this.http.get<TaskCountResponse>(`${this.apiUrl}/tasks/count`);
  }
}
