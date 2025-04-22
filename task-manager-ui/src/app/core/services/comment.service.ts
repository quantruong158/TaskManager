import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { CommentRequest, CommentResponse } from '../models/comment.model';

@Injectable({
  providedIn: 'root',
})
export class CommentService {
  private apiUrl = `${environment.apiUrl}/comments`;

  constructor(private http: HttpClient) {}

  getComments(taskId?: number): Observable<CommentResponse[]> {
    const url = taskId ? `${this.apiUrl}?taskId=${taskId}` : this.apiUrl;
    return this.http.get<CommentResponse[]>(url);
  }

  getCommentById(id: number): Observable<CommentResponse> {
    return this.http.get<CommentResponse>(`${this.apiUrl}/${id}`);
  }

  createComment(comment: CommentRequest): Observable<number> {
    return this.http.post<number>(this.apiUrl, comment);
  }

  updateComment(id: number, comment: CommentRequest): Observable<void> {
    return this.http.put<void>(`${this.apiUrl}/${id}`, comment);
  }

  deleteComment(id: number): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`);
  }
}
