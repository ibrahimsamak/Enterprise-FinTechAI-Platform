import { Component, DestroyRef, inject, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { NonNullableFormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { describeError } from 'shared-ui';
import { PaymentsApi } from './payments.api';

const ACCOUNT_KEY = 'finai.accountId';
const GUID = /^[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}$/i;

@Component({
  selector: 'app-make-payment',
  imports: [ReactiveFormsModule],
  template: `
    <main class="page" style="max-width: 32rem">
      <h1>Make a payment</h1>

      <div class="presets">
        <button type="button" (click)="preset(50, 'CAD', 'CA')">Normal: $50 → CA</button>
        <button type="button" (click)="preset(9500, 'CAD', 'DE')">Suspicious: $9,500 → DE</button>
      </div>

      <form class="card" [formGroup]="form" (ngSubmit)="submit()">
        <label>From account <input formControlName="accountId" /></label>
        <label>Amount <input type="number" min="0.01" step="0.01" formControlName="amount" /></label>
        <label>Currency
          <select formControlName="currency">
            @for (c of currencies; track c) { <option [value]="c">{{ c }}</option> }
          </select>
        </label>
        <label>Destination country (ISO-2) <input maxlength="2" formControlName="country" /></label>
        <button type="submit" [disabled]="form.invalid || busy()">{{ busy() ? 'Sending...' : 'Pay' }}</button>
        <p class="hint">A retry after a network error reuses the same Idempotency-Key, so it can't pay twice.</p>
      </form>

      @if (error()) { <p class="error">{{ error() }}</p> }
    </main>
  `,
styles: [`.presets { display: flex; gap: .5rem; margin-bottom: 1rem; }`],
})

export class MakePayment {
    private readonly api = inject(PaymentsApi);
    private readonly router = inject(Router)
    
    readonly currencies = ['CAD', 'USD', 'EUR'];
    readonly busy = signal(false);
    readonly error = signal<string | null>(null);

    readonly form = inject(NonNullableFormBuilder).group({
        accountId: [rememberedAccountId(), [Validators.required, Validators.pattern(GUID)]],
        amount: [50, [Validators.required, Validators.min(0.01)]],
        currency: ['CAD', Validators.required],
        country: ['CA', [Validators.required, Validators.pattern(/^[A-Za-z]{2}$/)]],
    });

    private idempotencyKey: string | null = null;

    constructor(){
        this.form.valueChanges
        .pipe(takeUntilDestroyed(inject(DestroyRef)))
        .subscribe(()=> (this.idempotencyKey = null))
    }

    preset(amount: number, currency: string, country: string): void {
        this.form.patchValue({amount, currency, country})
    }

    submit(): void {
        const v = this.form.getRawValue();
        sessionStorage.setItem(ACCOUNT_KEY, v.accountId)
        this.idempotencyKey ??= crypto.randomUUID();

        this.busy.set(true);
        this.error.set(null);
        this.api.create({...v, country: v.country.toUpperCase()}, this.idempotencyKey)
            .subscribe({
                next: id=> {
                    this.idempotencyKey = null;
                    void this.router.navigate(['/pay', id])
                },
                error: e=>{
                    this.busy.set(false)
                    this.error.set(describeError(e, {
                        409: 'The previous attempt is still being processed - wait a moment, then retry.',
                    }))
                }
            })
    }
}

function  rememberedAccountId(): string {
    const id = sessionStorage.getItem(ACCOUNT_KEY) ?? crypto.randomUUID();
    sessionStorage.setItem(ACCOUNT_KEY, id);
    return id;
}