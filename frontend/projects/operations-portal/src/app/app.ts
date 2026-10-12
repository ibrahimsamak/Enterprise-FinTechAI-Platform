import { Component, inject } from '@angular/core';
import { Router, RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';
import { AuthService } from 'shared-ui';
import { FraudFeedService } from './fraud/fraud-feed.service';

@Component({
  selector: 'app-root',
  imports: [RouterOutlet, RouterLink, RouterLinkActive],
  template: `
    @if (auth.isSignedIn()) {
      <nav class="bar">
        <strong>FinAI Operations</strong>
        <a routerLink="/fraud" routerLinkActive="active">
          Fraud feed @if (feed.openCount()) { <span class="chip bad">{{ feed.openCount() }}</span> }
        </a>
        <a routerLink="/dashboard" routerLinkActive="active">Dashboard</a>
        <span class="spacer"></span>
        <small class="hint">feed: {{ feed.state() }}</small>
        <button (click)="signOut()">Sign out</button>
      </nav>
    }
    <router-outlet />
  `,
})
export class App {
  protected readonly auth = inject(AuthService);
  protected readonly feed = inject(FraudFeedService);
  private readonly router = inject(Router);

  async signOut(): Promise<void> {
    await this.feed.stop();
    this.auth.logout();
    await this.router.navigateByUrl('/login');
  }
}
