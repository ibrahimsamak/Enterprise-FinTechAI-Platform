import { CurrencyPipe, DatePipe, DecimalPipe } from '@angular/common';
import { Component, computed, inject, signal } from '@angular/core';
import { FraudFeedService } from './fraud-feed.service';
import { ReviewCard } from './review-card';


@Component({
  selector: 'app-fraud-feed',
  imports: [CurrencyPipe, DatePipe, DecimalPipe, ReviewCard],
  template: `
    <main class="page">
      <h1>Fraud feed <small class="chip" [class.ok]="feed.state() === 'live'">{{ feed.state() }}</small></h1>
      @if (feed.error()) {
        <p class="error">{{ feed.error() }} <button (click)="feed.start()">Reconnect</button></p>
      }

      <div class="split">
        <section class="list">
          @for (c of feed.cases(); track c.alert.paymentId) {
            <button class="card item" [class.selected]="c.alert.paymentId === selectedId()"
                    (click)="selectedId.set(c.alert.paymentId)">
              <strong>{{ c.alert.amount | currency: c.alert.currency }} → {{ c.alert.country }}</strong>
              <span>score {{ c.alert.score | number: '1.2-2' }} · {{ c.receivedAt | date: 'HH:mm:ss' }}</span>
              <span class="chip" [class.bad]="!c.resolution" [class.ok]="c.resolution">{{ c.resolution ?? 'Open' }}</span>
            </button>
          } @empty {
            <p class="hint">Waiting for alerts... make a suspicious payment ($9,500 → DE) in the customer portal.</p>
          }
        </section>

        @if (selected(); as c) {
          <app-review-card [fraudCase]="c" (resolved)="feed.markResolved(c.alert.paymentId, $event)" />
        }
      </div>
    </main>
  `,
    styles: [`
        .split { display: grid; grid-template-columns: minmax(16rem, 1fr) 2fr; gap: 1rem; align-items: start; }
        @media (max-width: 48rem) { .split { grid-template-columns: 1fr; } }
        .list { display: grid; gap: .5rem; }
        .item { text-align: left; cursor: pointer; background: none; color: inherit; }
        .selected { outline: 2px solid currentColor; }
  `],
})
export class FraudFeed {
    protected readonly feed = inject(FraudFeedService);
    readonly selectedId = signal<string | null>(null);
    readonly selected = computed(()=> this.feed.cases().find(c=> c.alert.paymentId === this.selectedId()) ?? null)

    constructor(){
        void this.feed.start();
    }
}