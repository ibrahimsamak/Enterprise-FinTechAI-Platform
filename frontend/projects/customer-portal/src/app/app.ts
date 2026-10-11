import { Component, inject, signal } from '@angular/core';
import { Router, RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';
import { AuthService } from 'shared-ui';
import { SupportChatService } from './assistant/support-chat.service';

@Component({
  selector: 'app-root',
  imports: [RouterOutlet, RouterLink, RouterLinkActive],
  template: `
    @if (auth.isSignedIn()) {
      <nav class="bar">
        <strong>FinAI</strong>
        <a routerLink="/pay" routerLinkActive="active">Make a payment</a>
        <a routerLink="/assistant" routerLinkActive="active">Assistant</a>
        <span class="spacer"></span>
        <small class="hint">customer {{ auth.session()?.customerId?.slice(0, 8) }}...</small>
        <button (click)="signOut()">Sign out</button>
      </nav>
    }
    <router-outlet />
  `,
}) 

export class App {
  protected readonly auth = inject(AuthService);
  protected readonly router = inject(Router);
  private readonly chat = inject(SupportChatService)

  async signOut(): Promise<void> {
    await this.chat.stop();
    this.auth.logout();
    await this.router.navigateByUrl('/login');
  }
}
