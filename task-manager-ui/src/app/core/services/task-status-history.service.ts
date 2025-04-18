import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { environment } from '../../../environments/environment';
import { TaskStatusHistory } from '../models/task.model';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root',
})
export class TaskStatusHistoryService {
  private apiUrl = `${environment.apiUrl}`;

  constructor(private http: HttpClient) {}

  getTaskStatusHistory(): Observable<TaskStatusHistory[]> {
    return this.http.get<TaskStatusHistory[]>(
      `${this.apiUrl}/task-status-history`
    );
  }
}
