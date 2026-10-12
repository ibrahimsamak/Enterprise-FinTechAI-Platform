import { CurrencyPipe, DecimalPipe } from '@angular/common';
import { Component, effect, inject, input, output, signal, untracked } from '@angular/core';
import { FormControl, ReactiveFormsModule, Validators } from '@angular/forms';
import { describeError } from 'shared-ui';
import { FraudCasesApi } from './fraud-cases.api';
import { FraudCaseView, Resolution } from './fraud.models';

@Component({
  selector: 'app-review-card',
  imports: [CurrencyPipe, DecimalPipe, ReactiveFormsModule],
  template: `
    @let a = fraudCase().alert;
    <section class="card">
      <h2>{{ a.amount | currency: a.currency }} → {{ a.country }}</h2>
      <div class="hint">payment <code>{{ a.paymentId }}</code> · customer <code>{{ a.customerId }}</code></div>
      <div class="hint">trace <code>{{ a.correlationId }}</code> (search it in Jaeger)</div>

      <div>Risk score <strong>{{ a.score | number: '1.2-2' }}</strong></div>
      <ul>@for (r of a.reasons; track r) { <li>{{ r }}</li> }</ul>

      <h3>AI investigation <small class="hint">advisory only - you decide</small></h3>
      <div class="investigation">{{ a.investigation }}</div>

      @if (fraudCase().resolution; as r) {
        <p class="ok">{{ r === 'ResolvedElsewhere' ? 'Already resolved by another officer.' : 'Resolved: ' + r }}</p>
      } @else {
        <label>Decision notes (required, recorded in the audit log)
          <textarea rows="3" maxlength="2000" [formControl]="notes"></textarea>
        </label>
        <div class="actions">
          <button (click)="decide('Approve')" [disabled]="notes.invalid || busy()">Approve</button>
          <button (click)="decide('Escalate')" [disabled]="notes.invalid || busy()">Escalate</button>
        </div>
      }
      @if (error()) { <p class="error">{{ error() }}</p> }
    </section>
  `,
  styles: [`
    .investigation { white-space: pre-wrap; padding: .75rem; border-left: 3px solid var(--line); }
    .actions { display: flex; gap: .5rem; }
  `],
})
export class ReviewCard {
    readonly fraudCase = input.required<FraudCaseView>();
    readonly resolved = output<FraudCaseView['resolution']>()
    private readonly api = inject(FraudCasesApi)
    readonly notes = new FormControl('', {nonNullable: true, validators: [Validators.required, Validators.maxLength(2000)]})
    readonly busy = signal(false);
    readonly error = signal<string | null>(null);

    constructor(){
        effect(() => {
            this.fraudCase().alert.paymentId;
            untracked(() => { this.notes.reset(); this.error.set(null); });
        });
    }

    decide(resolution: Resolution): void {
        this.busy.set(true);
        this.error.set(null);
        this.api.resolve(this.fraudCase().alert.paymentId, resolution, this.notes.value.trim()).subscribe({
            next: ()=> {
                this. busy.set(false);
                this.resolved.emit(resolution);
            },
            error: e=> {
                this.busy.set(false)
                if(e?.status === 409){
                    this.resolved.emit('ResolvedElsewhere');
                    return
                }
                this.error.set(describeError(e, {
                    404: 'No fraud case for this payment yet - the worker may still be saving it. Retry in a few seconds.',
                    403: 'Your token has no FraudOfficer role.'
                }))
            }
        })
    }
}