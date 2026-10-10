import { Injectable, inject } from '@angular/core';
import { HubConnection, HubConnectionBuilder, LogLevel } from '@microsoft/signalr';
import { AuthService } from './auth.service';

@Injectable({ providedIn: 'root' })
export class HubFactory {
  private readonly auth = inject(AuthService);

  create(path: string): HubConnection {
    return new HubConnectionBuilder()
      .withUrl(path, { accessTokenFactory: () => this.auth.token ?? '' })
      .withAutomaticReconnect()
      .configureLogging(LogLevel.Warning)
      .build();
  }
}
