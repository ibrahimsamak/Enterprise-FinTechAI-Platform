import { Component, inject, input, signal } from '@angular/core';
import { NonNullableFormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { AuthService, Role } from './auth.service';
import { describeError } from './problem';

const GUID = /^[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}$/i;

@Component({
  selector: 'finai-dev-login',
  imports: [ReactiveFormsModule],
  template: `
    <main class="page" style="max-width: 28rem">
      <h1>{{ heading() }}</h1>
      <p class="hint warn">
        Development sign-in: <code>POST /dev/token</code> mints any role.
      </p>
      <form class="card" [formGroup]="form" (ngSubmit)="submit()">
        <label>User id <input formControlName="userId" /></label>
        @if (role() === 'Customer') {
          <label>Customer id <input formControlName="customerId" /></label>
          <p class="hint">
            Payments are published with this id and the fraud tools read history by it - reuse the id
            of a customer created with <code>POST /customers</code> to see real history.
          </p>
        }
        <button type="submit" [disabled]="form.invalid || busy()">
          {{ busy() ? 'Signing in...' : 'Sign in as ' + role() }}
        </button>
      </form>
      @if (error()) { <p class="error">{{ error() }}</p> }
    </main>
  `,
})
export class DevLogin {
  readonly role = input.required<Role>();
  readonly heading = input('FinAI');

  private readonly auth = inject(AuthService);
  private readonly router = inject(Router);
  private readonly route = inject(ActivatedRoute);

  readonly busy = signal(false);
  readonly error = signal<string | null>(null);

  readonly form = inject(NonNullableFormBuilder).group({
    userId: [crypto.randomUUID(), [Validators.required, Validators.pattern(GUID)]],
    customerId: [crypto.randomUUID(), [Validators.pattern(GUID)]],
  });

  submit(): void {
    const { userId, customerId } = this.form.getRawValue();
    if (this.role() === 'Customer' && !customerId) {
      this.error.set('A Customer token needs a customer id.');
      return;
    }

    this.busy.set(true);
    this.error.set(null);
    this.auth.login(this.role(), userId, this.role() === 'Customer' ? customerId : null).subscribe({
      next: () => void this.router.navigateByUrl(this.route.snapshot.queryParamMap.get('returnUrl') ?? '/'),
      error: e => { this.busy.set(false); this.error.set(describeError(e)); },
    });
  }
}
