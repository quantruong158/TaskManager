import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { BehaviorSubject, Observable, throwError } from 'rxjs';
import { catchError, map, tap } from 'rxjs/operators';
import { JwtHelperService } from '@auth0/angular-jwt';
import { AuthResponse, DecodedToken, UserInfo } from '../models/auth.model';
import { environment } from '../../../environments/environment';

@Injectable({
  providedIn: 'root',
})
export class AuthService {
  private readonly jwtHelper = new JwtHelperService();
  private currentUserSubject = new BehaviorSubject<UserInfo | null>(null);
  public currentUser$ = this.currentUserSubject.asObservable();

  constructor(private http: HttpClient) {
    this.loadStoredUserInfo();
  }

  private loadStoredUserInfo(): void {
    const token = localStorage.getItem('access_token');
    if (token) {
      const decodedToken = this.jwtHelper.decodeToken(token) as DecodedToken;
      this.setCurrentUser(decodedToken);
    }
  }

  private setCurrentUser(decodedToken: DecodedToken): void {
    const userInfo: UserInfo = {
      id: decodedToken.nameid,
      email: decodedToken.email,
      name: decodedToken.unique_name,
      role: decodedToken.role,
    };
    this.currentUserSubject.next(userInfo);
  }

  public refreshToken(): Observable<boolean> {
    const refreshToken = localStorage.getItem('refresh_token');
    if (!refreshToken) {
      return throwError(() => new Error('No refresh token available'));
    }

    return this.http
      .post<AuthResponse>(`${environment.apiUrl}/auth/refresh-token`, {
        refreshToken,
      })
      .pipe(
        tap((response) => {
          this.storeTokens(response);
          const decodedToken = this.jwtHelper.decodeToken(
            response.accessToken
          ) as DecodedToken;
          this.setCurrentUser(decodedToken);
        }),
        map(() => true),
        catchError((error) => {
          this.logout();
          return throwError(() => error);
        })
      );
  }

  private storeTokens(response: AuthResponse): void {
    localStorage.setItem('access_token', response.accessToken);
    localStorage.setItem('refresh_token', response.refreshToken);
  }

  public isAuthenticated(): boolean {
    const token = localStorage.getItem('access_token');
    return token ? true : false;
  }

  public logout(): void {
    localStorage.removeItem('access_token');
    localStorage.removeItem('refresh_token');
    this.currentUserSubject.next(null);
  }

  public getRole(): string | null {
    const user = this.currentUserSubject.value;
    return user ? user.role : null;
  }

  public login(email: string, password: string): Observable<boolean> {
    return this.http
      .post<AuthResponse>(`${environment.apiUrl}/auth/login`, {
        email,
        password,
      })
      .pipe(
        tap((response) => {
          this.storeTokens(response);
          const decodedToken = this.jwtHelper.decodeToken(
            response.accessToken
          ) as DecodedToken;
          this.setCurrentUser(decodedToken);
        }),
        map(() => true),
        catchError((error) => throwError(() => error))
      );
  }
}
