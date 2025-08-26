import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { BehaviorSubject, Observable, map } from 'rxjs';
import { Router } from '@angular/router';
import { environment } from '../../../environments/environment';

export interface User {
  id: string;
  username: string;
  email: string;
  fullName: string;
  role?: string;
  isSuperAdmin: boolean;
  permissions?: string;
}

export interface LoginRequest {
  username: string;
  password: string;
}

export interface AuthResponse {
  success: boolean;
  message: string;
  token?: string;
  refreshToken?: string;
  user?: User;
}

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private readonly TOKEN_KEY = 'auth_token';
  private readonly REFRESH_TOKEN_KEY = 'refresh_token';
  private readonly USER_KEY = 'current_user';

  private currentUserSubject = new BehaviorSubject<User | null>(null);
  public currentUser$ = this.currentUserSubject.asObservable();

  private isAuthenticatedSubject = new BehaviorSubject<boolean>(false);
  public isAuthenticated$ = this.isAuthenticatedSubject.asObservable();

  constructor(private http: HttpClient, private router: Router) {
    this.initializeAuthState();
  }

  private initializeAuthState(): void {
    const token = this.getToken();
    const user = this.getStoredUser();

    if (token && user) {
      this.currentUserSubject.next(user);
      this.isAuthenticatedSubject.next(true);
    }
  }

  login(credentials: LoginRequest): Observable<AuthResponse> {
    return this.http.post<AuthResponse>(`${environment.apiUrl}/api/Auth/login`, credentials).pipe(
      map(response => {
        if (response.success && response.token && response.user) {
          this.setSession(response.token, response.refreshToken, response.user);
        }
        return response;
      })
    );
  }

  logout(): Observable<any> {
    return this.http.post(`${environment.apiUrl}/api/Auth/logout`, {}).pipe(
      map(() => {
        this.clearSession();
        this.router.navigate(['/login']);
      })
    );
  }

  refreshToken(): Observable<AuthResponse> {
    const refreshToken = this.getRefreshToken();
    if (!refreshToken) {
      throw new Error('No refresh token available');
    }

    return this.http.post<AuthResponse>(`${environment.apiUrl}/api/Auth/refresh`, {
      token: this.getToken(),
      refreshToken: refreshToken
    }).pipe(
      map(response => {
        if (response.success && response.token) {
          this.updateToken(response.token, response.refreshToken);
        }
        return response;
      })
    );
  }

  getCurrentUser(): Observable<User | null> {
    const token = this.getToken();
    if (!token) {
      return new Observable(observer => {
        observer.next(null);
        observer.complete();
      });
    }

    return this.http.get<{ success: boolean; message: string; data: User }>(
      `${environment.apiUrl}/api/Auth/me`
    ).pipe(
      map(response => {
        if (response.success) {
          return response.data;
        }
        return null;
      })
    );
  }

  isLoggedIn(): boolean {
    const token = this.getToken();
    if (!token) return false;

    try {
      const payload = JSON.parse(atob(token.split('.')[1]));
      const exp = payload.exp * 1000; // Convert to milliseconds
      return Date.now() < exp;
    } catch {
      return false;
    }
  }

  getToken(): string | null {
    return localStorage.getItem(this.TOKEN_KEY);
  }

  getRefreshToken(): string | null {
    return localStorage.getItem(this.REFRESH_TOKEN_KEY);
  }

  getCurrentUserValue(): User | null {
    return this.currentUserSubject.value;
  }

  hasRole(role: string): boolean {
    const user = this.getCurrentUserValue();
    return user?.role === role || user?.isSuperAdmin === true;
  }

  hasPermission(permission: string): boolean {
    const user = this.getCurrentUserValue();
    if (user?.isSuperAdmin) return true;

    if (!user?.permissions) return false;

    try {
      const permissions = JSON.parse(user.permissions);
      return permissions.includes(permission);
    } catch {
      return false;
    }
  }

  private setSession(token: string, refreshToken?: string, user?: User): void {
    localStorage.setItem(this.TOKEN_KEY, token);
    if (refreshToken) {
      localStorage.setItem(this.REFRESH_TOKEN_KEY, refreshToken);
    }
    if (user) {
      localStorage.setItem(this.USER_KEY, JSON.stringify(user));
      this.currentUserSubject.next(user);
    }
    this.isAuthenticatedSubject.next(true);
  }

  private updateToken(token: string, refreshToken?: string): void {
    localStorage.setItem(this.TOKEN_KEY, token);
    if (refreshToken) {
      localStorage.setItem(this.REFRESH_TOKEN_KEY, refreshToken);
    }
  }

  private clearSession(): void {
    localStorage.removeItem(this.TOKEN_KEY);
    localStorage.removeItem(this.REFRESH_TOKEN_KEY);
    localStorage.removeItem(this.USER_KEY);
    this.currentUserSubject.next(null);
    this.isAuthenticatedSubject.next(false);
  }

  private getStoredUser(): User | null {
    const userJson = localStorage.getItem(this.USER_KEY);
    if (!userJson) return null;

    try {
      return JSON.parse(userJson);
    } catch {
      return null;
    }
  }

  // Auto-login check on app initialization
  checkAuthStatus(): Observable<boolean> {
    if (!this.isLoggedIn()) {
      this.clearSession();
      return new Observable(observer => {
        observer.next(false);
        observer.complete();
      });
    }

    return this.getCurrentUser().pipe(
      map(user => {
        if (user) {
          this.currentUserSubject.next(user);
          this.isAuthenticatedSubject.next(true);
          return true;
        } else {
          this.clearSession();
          return false;
        }
      })
    );
  }
}
