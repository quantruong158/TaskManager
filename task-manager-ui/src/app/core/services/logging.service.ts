import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { environment } from '../../../environments/environment';
import { LoginLog, TaskStatusLog } from '../models/logging.model';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root',
})
export class LoggingService {
  private apiUrl = `${environment.apiUrl}/logs`;

  constructor(private http: HttpClient) {}

  getTaskStatusLog(): Observable<TaskStatusLog[]> {
    return this.http.get<TaskStatusLog[]>(`${this.apiUrl}/task-status`);
  }

  getLoginLog(): Observable<LoginLog[]> {
    return this.http.get<LoginLog[]>(`${this.apiUrl}/login`);
  }
}
