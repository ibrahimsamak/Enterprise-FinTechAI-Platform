import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { Resolution } from './fraud.models';


@Injectable({ providedIn: 'root' })
export class FraudCasesApi {
    private readonly http = inject(HttpClient)
    resolve(paymentId: string, resolution: Resolution, notes: string): Observable<void>{
        return this.http.post<void>(`/fraud-cases/${paymentId}/resolve`, {resolution, notes})
    }
}