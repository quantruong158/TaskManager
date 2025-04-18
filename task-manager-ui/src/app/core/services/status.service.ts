import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import {
  Status,
  CreateStatusRequest,
  UpdateStatusRequest,
} from '../models/status.model';
import { environment } from '../../../environments/environment';

@Injectable({
  providedIn: 'root',
})
export class StatusService {
  private apiUrl = `${environment.apiUrl}/status`;

  constructor(private http: HttpClient) {}

  public getAllStatuses(): Observable<Status[]> {
    return this.http.get<Status[]>(this.apiUrl);
  }

  public getStatusById(id: number): Observable<Status> {
    return this.http.get<Status>(`${this.apiUrl}/${id}`);
  }

  public createStatus(request: CreateStatusRequest): Observable<number> {
    return this.http.post<number>(this.apiUrl, request);
  }

  public updateStatus(
    id: number,
    request: UpdateStatusRequest
  ): Observable<void> {
    return this.http.put<void>(`${this.apiUrl}/${id}`, request);
  }

  public deleteStatus(id: number): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`);
  }
}
