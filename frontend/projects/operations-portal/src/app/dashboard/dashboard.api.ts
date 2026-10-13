import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable, map } from 'rxjs';

export interface DashboardData {
  customerCount: number;
  paymentVolumeToday: VolumeData[];
  paymentsOnHold: number;
  openFraudCases: number;
  fraudCasesResolvedToday: number;
}

export interface VolumeData { currency: string; amount: number; count: number; }

interface GraphQlResponse<T> { data?: T | null; errors?: { message: string }[]; }
const DASHBOARD_QUERY = `
  query Dashboard {
    dashboard {
      customerCount
      paymentVolumeToday { currency amount count }
      paymentsOnHold
      openFraudCases
      fraudCasesResolvedToday
    }
  }`;

@Injectable({ providedIn: 'root' })
export class DashboardApi {
    private readonly http = inject(HttpClient);
    load(): Observable<DashboardData> {
        return this.http.post<GraphQlResponse<{dashboard: DashboardData }>>('/graphql', { query: DASHBOARD_QUERY }).pipe(
            map(r=> {
                if (r.errors?.length) throw new Error(r.errors.map(e => e.message).join('; '));
                if (!r.data?.dashboard) throw new Error('Empty GraphQL response.');
                return r.data.dashboard;
         })
        )
    }
}
