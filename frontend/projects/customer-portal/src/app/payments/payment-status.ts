import { CurrencyPipe } from "@angular/common";
import { Payment, PaymentStatusValue } from "./payment.models";
import { RouterLink } from "@angular/router";
import { Component, computed, inject, input, signal } from "@angular/core";
import { PaymentsApi } from "./payments.api";
import { catchError, EMPTY, finalize, switchMap, take, takeWhile, timer } from "rxjs";
import { takeUntilDestroyed, toObservable } from "@angular/core/rxjs-interop";
import { describeError } from "shared-ui";

const TERMINAL = new Set<PaymentStatusValue>(['Completed', 'Rejected']);
const POLL_MS = 2000;
const MAX_POLLS = 60;
const EXPLAIN: Record<PaymentStatusValue, string> = {
    Pending: 'Fraud screening in progress',
    OnHold: 'Held for review by a fraud officer. This page updates if they approve it.',
    Completed: 'Payment completed.',
    Rejected: 'Payment rejected after review.',
};

@Component({
  selector: 'app-payment-status',
  imports: [CurrencyPipe, RouterLink],
  template: `
    <main class="page" style="max-width: 32rem">
      <h1>Payment status</h1>
      @if (payment(); as p) {
        <section class="card">
          <div>{{ p.amount | currency: p.currency }} → {{ p.country }}</div>
          <div class="status" [class.ok]="p.status === 'Completed'" [class.bad]="p.status === 'Rejected'"
               [class.warn]="p.status === 'OnHold'">{{ p.status }}</div>
          <p class="hint">{{ explain() }}</p>
          <code class="hint">{{ p.id }}</code>
        </section>
      } @else if (!error()) {
        <p class="hint">Loading...</p>
      }
      @if (error()) { <p class="error">{{ error() }}</p> }
      @if (stopped()) { <button (click)="refresh()">Refresh</button> }
      <p><a routerLink="/pay">Make another payment</a></p>
    </main>
  `,
  styles: [`.status { font-size: 1.5rem; font-weight: 600; }`],
})

export class PaymentStatus {
    readonly id = input.required<string>();
    private readonly api = inject(PaymentsApi);
    private readonly reload = signal(0);

    readonly payment = signal<Payment | null>(null);
    readonly error = signal<string | null>(null);
    readonly stopped = signal(false);
    readonly explain = computed(() => { const p = this.payment(); return p ? EXPLAIN[p.status] : ''; });

    constructor() {
        toObservable(computed(() => ({id: this.id(), n: this.reload()})))
        .pipe(
            switchMap(({ id }) => {
                this.stopped.set(false)
                this.error.set(null)
                return timer(0, POLL_MS).pipe(
                    take(MAX_POLLS),
                    switchMap(()=>this.api.get(id)),
                    takeWhile(p => !TERMINAL.has(p.status), true),
                    catchError(e=>{
                        this.error.set(describeError(e, {
                            404: 'Payment not found.'
                        }));
                    return EMPTY;
                    }),
                finalize(() => this.stopped.set(!TERMINAL.has(this.payment()?.status ?? 'Pending'))),
                );
            }),
            takeUntilDestroyed(),
        ).subscribe(p=>this.payment.set(p))
    }

    refresh(): void {
        this.reload.update(m => m + 1);
    }
}