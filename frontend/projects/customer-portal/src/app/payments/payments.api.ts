import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { CreatePayment, Payment } from './payment.models';

@Injectable({ providedIn: 'root' })
export class PaymentsApi {
    private readonly http = inject(HttpClient);

    create(body: CreatePayment, idempotencyKey: string): Observable<string> {
        return this.http.post<string>('/payments', body, {headers: {'Idempotency-Key': idempotencyKey}});
    }

    get(id: string): Observable<Payment>{
        return this.http.get<Payment>(`/payments/${id}`);
    }
}