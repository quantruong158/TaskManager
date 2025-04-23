import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { CreateCommentRequest, Comment } from '../models/comment.model';

@Injectable({
  providedIn: 'root',
})
export class CommentService {
  private apiUrl = `${environment.apiUrl}/comments`;

  constructor(private http: HttpClient) {}

  getComments(taskId?: number): Observable<Comment[]> {
    const url = taskId ? `${this.apiUrl}?taskId=${taskId}` : this.apiUrl;
    return this.http.get<Comment[]>(url);
  }

  getCommentById(id: number): Observable<Comment> {
    return this.http.get<Comment>(`${this.apiUrl}/${id}`);
  }

  createComment(comment: CreateCommentRequest): Observable<number> {
    return this.http.post<number>(this.apiUrl, comment);
  }

  updateComment(id: number, comment: CreateCommentRequest): Observable<void> {
    return this.http.put<void>(`${this.apiUrl}/${id}`, comment);
  }

  deleteComment(id: number): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`);
  }
}
