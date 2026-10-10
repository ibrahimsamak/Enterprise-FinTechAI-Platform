import { HttpErrorResponse } from '@angular/common/http';

export interface ProblemDetails {
  title?: string;
  detail?: string;
  status?: number;
  errors?: Record<string, string[]>;
}

export function describeError(e: unknown, byStatus: Record<number, string> = {}): string {
  if (!(e instanceof HttpErrorResponse)) return e instanceof Error ? e.message : 'Unexpected error.';
  if (byStatus[e.status]) return byStatus[e.status];
  if (e.status === 0) return 'Cannot reach the API - is FinAI.Api running on :5286 and the dev proxy configured?';

  const p = e.error as ProblemDetails | null;
  const fieldErrors = p?.errors ? Object.values(p.errors).flat().join(' ') : '';
  return fieldErrors || p?.detail || p?.title || `${e.status} ${e.statusText}`;
}
