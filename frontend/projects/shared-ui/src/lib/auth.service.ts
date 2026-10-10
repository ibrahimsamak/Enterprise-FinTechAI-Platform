import { HttpClient } from '@angular/common/http';
import { Injectable, computed, inject, signal } from '@angular/core';
import { Observable, map, tap } from 'rxjs';

export type Role = 'Customer' | 'FraudOfficer';

interface IssuedToken { accessToken: string; expiresAtUtc: string; }

export interface Session {
  accessToken: string;
  expiresAtUtc: string;
  userId: string;
  role: Role;
  customerId: string | null;
}

const STORAGE_KEY = 'finai.session';

@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly http = inject(HttpClient);
  private readonly _session = signal<Session | null>(readStoredSession());

  readonly session = this._session.asReadonly();
  readonly isSignedIn = computed(() => this._session() !== null);

  get token(): string | null {
    const s = this._session();
    return s && !isExpired(s) ? s.accessToken : null;
  }

  hasRole(role: Role): boolean {
    const s = this._session();
    return !!s && s.role === role && !isExpired(s);
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
