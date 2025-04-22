import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { CreateUserDto, UpdateUserDto, User } from '../models/user.model';
import { environment } from '../../../environments/environment';
import { PaginatedResponse } from '../../shared/models/pagination.model';

@Injectable({
  providedIn: 'root',
})
export class UserService {
  private apiUrl = `${environment.apiUrl}/users`;

  constructor(private http: HttpClient) {}

  public getUsers(): Observable<User[]> {
    return this.http.get<User[]>(`${this.apiUrl}`);
  }

  public getUserById(id: number): Observable<User> {
    return this.http.get<User>(`${this.apiUrl}/${id}`);
  }

  public createUser(createUserDto: CreateUserDto): Observable<User> {
    return this.http.post<User>(this.apiUrl, createUserDto);
  }

  public updateUser(
    id: number,
    updateUserDto: UpdateUserDto
  ): Observable<User> {
    return this.http.put<User>(`${this.apiUrl}/${id}`, updateUserDto);
  }
}
