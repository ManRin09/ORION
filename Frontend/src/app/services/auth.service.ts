import { HttpClient, HttpParams } from '@angular/common/http';
import { computed, inject, Injectable, signal } from '@angular/core';
import { catchError, finalize, Observable, of, tap } from 'rxjs';
import { environment } from '../../environments/environment';
import {
  AuthResponse,
  ChangePasswordRequest,
  LoginCredentials,
  RegisterCharacterPayload,
} from '../models/auth.models';
import {
  PagedResponse,
  RegisterUserRequest,
  UpdateUserProfileRequest,
  UserResponse,
} from '../models/user.models';

interface StoredAuthSession {
  readonly accessToken: string;
  readonly expiresAtUtc: string | null;
  readonly rememberTerminal: boolean;
  readonly user: UserResponse | null;
}

const AUTH_STORAGE_KEY = 'orion.auth.session';

@Injectable({
  providedIn: 'root',
})
export class AuthService {
  private readonly http = inject(HttpClient);
  private readonly authUrl = `${environment.apiUrl}/auth`;
  private readonly usersUrl = `${environment.apiUrl}/users`;

  private readonly _accessToken = signal<string | null>(null);
  private readonly _expiresAtUtc = signal<string | null>(null);
  private readonly _currentUser = signal<UserResponse | null>(null);

  readonly accessToken = this._accessToken.asReadonly();
  readonly expiresAtUtc = this._expiresAtUtc.asReadonly();
  readonly currentUser = this._currentUser.asReadonly();
  readonly isAuthenticated = computed(
    () => Boolean(this._accessToken()) && !this.isSessionExpired(this._expiresAtUtc()),
  );

  constructor() {
    this.hydrateFromStorage();
  }

  // ===========================================================================
  // Authentication & Session Endpoints (/api/auth)
  // ===========================================================================

  /**
   * Authenticates an adventurer via POST /api/auth/login and stores the JWT session.
   */
  login(credentials: LoginCredentials): Observable<AuthResponse> {
    return this.http.post<AuthResponse>(`${this.authUrl}/login`, credentials).pipe(
      tap((response) => {
        this.persistSession({
          accessToken: response.accessToken,
          expiresAtUtc: response.expiresAtUtc,
          rememberTerminal: response.rememberTerminal,
          user: response.user,
        });
      }),
    );
  }

  /**
   * Fetches the authenticated adventurer profile via GET /api/auth/me.
   */
  getCurrentUser(): Observable<UserResponse> {
    return this.http.get<UserResponse>(`${this.authUrl}/me`).pipe(
      tap((user) => {
        this._currentUser.set(user);
        this.syncStoredUser(user);
      }),
    );
  }

  /**
   * Updates the authenticated adventurer password via PUT /api/auth/password.
   */
  changePassword(request: ChangePasswordRequest): Observable<void> {
    return this.http.put<void>(`${this.authUrl}/password`, request);
  }

  /**
   * Ends the current session via DELETE /api/auth/session and clears stored credentials.
   */
  logout(): Observable<void> {
    if (!this.getToken()) {
      this.clearSession();
      return of(void 0);
    }

    return this.http.delete<void>(`${this.authUrl}/session`).pipe(
      catchError(() => of(void 0)),
      finalize(() => this.clearSession()),
    );
  }

  /**
   * Returns the backend OAuth entry endpoint for GitHub login.
   */
  getGitHubLoginUrl(): string {
    return `${this.authUrl}/github/login`;
  }

  /**
   * Completes an external OAuth callback (e.g. GitHub redirect with ?token=...)
   * by storing the token and hydrating the adventurer profile via GET /api/auth/me.
   */
  completeExternalLogin(token: string, rememberTerminal: boolean = true): Observable<UserResponse> {
    const trimmed = token.trim();
    this.persistSession({
      accessToken: trimmed,
      expiresAtUtc: null,
      rememberTerminal,
      user: null,
    });

    return this.getCurrentUser();
  }

  // ===========================================================================
  // Adventurer / User CRUD Endpoints (/api/users)
  // ===========================================================================

  /**
   * Registers a new adventurer ("Spawn into Orion") via POST /api/users/register.
   */
  register(request: RegisterUserRequest): Observable<UserResponse> {
    return this.http.post<UserResponse>(`${this.usersUrl}/register`, request);
  }

  /**
   * Adapter helper that maps RegisterCharacterPayload to backend RegisterUserRequest.
   */
  registerFromCharacterPayload(payload: RegisterCharacterPayload): Observable<UserResponse> {
    const request: RegisterUserRequest = {
      firstName: payload.firstName,
      middleName: payload.middleTitle ? payload.middleTitle : null,
      email: payload.email,
      username: null,
      password: payload.password,
      confirmPassword: payload.password,
      acceptTerms: payload.acceptedTerms,
    };

    return this.register(request);
  }

  /**
   * Retrieves an adventurer by unique identifier (GUID) via GET /api/users/{id}.
   */
  getById(id: string): Observable<UserResponse> {
    return this.http.get<UserResponse>(`${this.usersUrl}/${id}`);
  }

  /**
   * Lists active adventurers with pagination via GET /api/users.
   */
  getPaged(pageNumber: number = 1, pageSize: number = 10): Observable<PagedResponse<UserResponse>> {
    const params = new HttpParams()
      .set('pageNumber', pageNumber.toString())
      .set('pageSize', pageSize.toString());

    return this.http.get<PagedResponse<UserResponse>>(this.usersUrl, { params });
  }

  /**
   * Updates editable profile fields via PUT /api/users/{id}/profile.
   */
  updateProfile(id: string, request: UpdateUserProfileRequest): Observable<UserResponse> {
    return this.http.put<UserResponse>(`${this.usersUrl}/${id}/profile`, request).pipe(
      tap((updated) => {
        if (this._currentUser()?.id === updated.id) {
          this._currentUser.set(updated);
          this.syncStoredUser(updated);
        }
      }),
    );
  }

  /**
   * Soft-deletes (deactivates) an adventurer account via DELETE /api/users/{id}.
   */
  softDelete(id: string): Observable<void> {
    return this.http.delete<void>(`${this.usersUrl}/${id}`).pipe(
      tap(() => {
        if (this._currentUser()?.id === id) {
          this.clearSession();
        }
      }),
    );
  }

  // ===========================================================================
  // Token & Session Helpers
  // ===========================================================================

  /**
   * Returns the current valid JWT access token or null if missing/expired.
   */
  getToken(): string | null {
    const token = this._accessToken();
    if (!token) {
      return null;
    }

    if (this.isSessionExpired(this._expiresAtUtc())) {
      this.clearSession();
      return null;
    }

    return token;
  }

  /**
   * Clears in-memory signals and both browser storage buckets.
   */
  clearSession(): void {
    this._accessToken.set(null);
    this._expiresAtUtc.set(null);
    this._currentUser.set(null);

    this.safeRemoveStorage(this.getStorage('local'));
    this.safeRemoveStorage(this.getStorage('session'));
  }

  private persistSession(session: StoredAuthSession): void {
    this._accessToken.set(session.accessToken);
    this._expiresAtUtc.set(session.expiresAtUtc);
    this._currentUser.set(session.user);

    const target = this.getStorage(session.rememberTerminal ? 'local' : 'session');
    const other = this.getStorage(session.rememberTerminal ? 'session' : 'local');

    this.safeRemoveStorage(other);
    if (target) {
      try {
        target.setItem(AUTH_STORAGE_KEY, JSON.stringify(session));
      } catch {
        // Ignore storage quota / privacy mode errors
      }
    }
  }

  private syncStoredUser(user: UserResponse): void {
    for (const kind of ['local', 'session'] as const) {
      const storage = this.getStorage(kind);
      if (!storage) {
        continue;
      }

      try {
        const raw = storage.getItem(AUTH_STORAGE_KEY);
        if (!raw) {
          continue;
        }

        const parsed = JSON.parse(raw) as StoredAuthSession;
        storage.setItem(AUTH_STORAGE_KEY, JSON.stringify({ ...parsed, user }));
      } catch {
        // Ignore malformed storage entries
      }
    }
  }

  private hydrateFromStorage(): void {
    const session = this.readStoredSession('local') ?? this.readStoredSession('session');
    if (!session || !session.accessToken || this.isSessionExpired(session.expiresAtUtc)) {
      this.clearSession();
      return;
    }

    this._accessToken.set(session.accessToken);
    this._expiresAtUtc.set(session.expiresAtUtc);
    this._currentUser.set(session.user);
  }

  private readStoredSession(kind: 'local' | 'session'): StoredAuthSession | null {
    const storage = this.getStorage(kind);
    if (!storage) {
      return null;
    }

    try {
      const raw = storage.getItem(AUTH_STORAGE_KEY);
      if (!raw) {
        return null;
      }

      return JSON.parse(raw) as StoredAuthSession;
    } catch {
      return null;
    }
  }

  private isSessionExpired(expiresAtUtc: string | null): boolean {
    if (!expiresAtUtc) {
      return false;
    }

    const expiryMs = Date.parse(expiresAtUtc);
    if (Number.isNaN(expiryMs)) {
      return false;
    }

    return expiryMs <= Date.now();
  }

  private getStorage(kind: 'local' | 'session'): Storage | null {
    if (typeof window === 'undefined') {
      return null;
    }

    try {
      return kind === 'local' ? window.localStorage : window.sessionStorage;
    } catch {
      return null;
    }
  }

  private safeRemoveStorage(storage: Storage | null): void {
    if (!storage) {
      return;
    }

    try {
      storage.removeItem(AUTH_STORAGE_KEY);
    } catch {
      // Ignore storage access errors
    }
  }
}
