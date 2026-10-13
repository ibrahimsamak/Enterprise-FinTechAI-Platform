import { CurrencyPipe, DatePipe, DecimalPipe } from '@angular/common';
import { Component, inject, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { catchError, EMPTY, switchMap, timer } from 'rxjs';
import { describeError } from 'shared-ui';
import { FraudFeedService } from '../fraud/fraud-feed.service';
import { DashboardApi, DashboardData } from './dashboard.api';

@Component({
  selector: 'app-dashboard',
  imports: [CurrencyPipe, DatePipe, DecimalPipe],
  template: `
    <main class="page">
      <h1>Dashboard</h1>
      @if (data(); as d) {
        <section class="tiles">
          <div class="card"><span class="hint">Customers</span><strong>{{ d.customerCount | number }}</strong></div>
          <div class="card"><span class="hint">Payment volume today</span>
            @for (v of d.paymentVolumeToday; track v.currency) {
              <strong>{{ v.amount | currency: v.currency : 'symbol' : '1.0-0' }}</strong>
              <span class="hint">{{ v.count | number }} payments</span>
            } @empty { <strong>-</strong> }
          </div>
          <div class="card"><span class="hint">Payments on hold</span><strong>{{ d.paymentsOnHold | number }}</strong></div>
          <div class="card"><span class="hint">Open fraud cases (pending approval)</span><strong>{{ d.openFraudCases | number }}</strong></div>
          <div class="card"><span class="hint">Cases resolved today</span><strong>{{ d.fraudCasesResolvedToday | number }}</strong></div>
          <div class="card"><span class="hint">Open alerts (live, SignalR)</span><strong>{{ feed.openCount() }}</strong></div>
        </section>
        <p class="hint">Updated {{ updatedAt() | date: 'HH:mm:ss' }} · refreshes every 30 s</p>
      }
      @if (error()) { <p class="error">{{ error() }}</p> }
    </main>
  `,
  styles: [`
    .tiles { display: grid; grid-template-columns: repeat(auto-fill, minmax(12rem, 1fr)); gap: 1rem; }
    .tiles strong { font-size: 1.75rem; }
  `],
})
export class Dashboard {
  protected readonly feed = inject(FraudFeedService);
  private readonly api = inject(DashboardApi);

  readonly data = signal<DashboardData | null>(null);
  readonly updatedAt = signal<Date | null>(null);
  readonly error = signal<string | null>(null);

  constructor() {
    timer(0, 30_000)
      .pipe(
        switchMap(() => this.api.load().pipe(catchError(e => {
          this.error.set(describeError(e, {
            403: 'Your token has no FraudOfficer role.',
          }));
          return EMPTY;
        }))),
        takeUntilDestroyed(),
      )
      .subscribe(d => { this.data.set(d); this.updatedAt.set(new Date()); this.error.set(null); });
  }
}
