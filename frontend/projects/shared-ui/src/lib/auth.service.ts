import { HttpClient } from '@angular/common/http';
import { Injectable, Signal, computed, inject, signal } from '@angular/core';
import { MsalService } from '@azure/msal-angular';
import { AccountInfo, InteractionRequiredAuthError } from '@azure/msal-browser';
import { Observable, map, tap } from 'rxjs';

export type Role = 'Customer' | 'FraudOfficer' | 'ComplianceOfficer' | 'Admin';

interface IssuedToken { accessToken: string; expiresAtUtc: string; }

export interface Session {
  accessToken: string;
  expiresAtUtc: string;
  userId: string;
  role: Role;
  customerId: string | null;
}

export abstract class AuthService {
  abstract readonly session: Signal<Session | null>;
  abstract readonly isSignedIn: Signal<boolean>;
  abstract hasRole(role: Role): boolean;
  abstract getAccessToken(): Promise<string | null>;
  abstract login(role: Role, userId: string, customerId: string | null): Observable<Session>;
  abstract logout(): void;
}

const STORAGE_KEY = 'finai.session';

@Injectable()
export class DevAuthService extends AuthService {
  private readonly http = inject(HttpClient);
  private readonly _session = signal<Session | null>(readStoredSession());

  readonly session = this._session.asReadonly();
  readonly isSignedIn = computed(() => this._session() !== null);

  hasRole(role: Role): boolean {
    const s = this._session();
    return !!s && s.role === role && !isExpired(s);
  }

  getAccessToken(): Promise<string | null> {
    const s = this._session();
    return Promise.resolve(s && !isExpired(s) ? s.accessToken : null);
  }

  login(role: Role, userId: string, customerId: string | null): Observable<Session> {
    return this.http.post<IssuedToken>('/dev/token', { userId, role, customerId }).pipe(
      map(t => ({ ...t, userId, role, customerId })),
      tap(s => {
        sessionStorage.setItem(STORAGE_KEY, JSON.stringify(s));
        this._session.set(s);
      }),
    );
  }

  logout(): void {
    sessionStorage.removeItem(STORAGE_KEY);
    this._session.set(null);
  }
}

@Injectable()
export class EntraAuthService extends AuthService {
  private readonly msal = inject(MsalService);
  private readonly _account = signal<AccountInfo | null>(this.msal.instance.getActiveAccount() ?? firstAccount(this.msal));

  readonly session = computed<Session | null>(() => {
    const a = this._account();
    if (!a) return null;
    const claims = (a.idTokenClaims ?? {}) as { oid?: string; roles?: string[]; exp?: number };
    return {
      accessToken: '',
      expiresAtUtc: claims.exp ? new Date(claims.exp * 1000).toISOString() : '',
      userId: claims.oid ?? a.localAccountId,
      role: (claims.roles?.[0] as Role) ?? 'Customer',
      customerId: null,
    };
  });
  readonly isSignedIn = computed(() => this._account() !== null);

  hasRole(role: Role): boolean {
    const roles = (this._account()?.idTokenClaims as { roles?: string[] } | undefined)?.roles ?? [];
    return roles.includes(role);
  }

  async getAccessToken(): Promise<string | null> {
    const account = this._account();
    if (!account) return null;
    const scopes = [apiScope()];
    try {
      const result = await this.msal.instance.acquireTokenSilent({ account, scopes });
      return result.accessToken;
    } catch (e) {
      if (e instanceof InteractionRequiredAuthError) this.msal.instance.acquireTokenRedirect({ scopes });
      return null;
    }
  }

  login(): Observable<Session> {
    this.msal.instance.loginRedirect({ scopes: [apiScope()] });
    return new Observable<Session>();
  }

  logout(): void {
    this.msal.instance.logoutRedirect();
  }
}

function apiScope(): string {
  return (globalThis as { __finaiApiScope?: string }).__finaiApiScope ?? '';
}

function firstAccount(msal: MsalService): AccountInfo | null {
  const accounts = msal.instance.getAllAccounts();
  return accounts.length ? accounts[0] : null;
}

function isExpired(s: Session): boolean {
  return Date.parse(s.expiresAtUtc) <= Date.now();
}

function readStoredSession(): Session | null {
  try {
    const raw = sessionStorage.getItem(STORAGE_KEY);
    const s = raw ? (JSON.parse(raw) as Session) : null;
    return s && !isExpired(s) ? s : null;
  } catch {
    return null;
  }
}
