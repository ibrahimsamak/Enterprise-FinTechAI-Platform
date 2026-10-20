import { HTTP_INTERCEPTORS } from '@angular/common/http';
import { EnvironmentProviders, Provider } from '@angular/core';
import {
  MSAL_GUARD_CONFIG, MSAL_INSTANCE, MSAL_INTERCEPTOR_CONFIG,
  MsalBroadcastService, MsalGuard, MsalInterceptor, MsalService,
} from '@azure/msal-angular';
import { InteractionType, PublicClientApplication } from '@azure/msal-browser';
import { AuthService, DevAuthService, EntraAuthService } from './auth.service';

export type AuthEnv =
  | { auth: 'dev' }
  | { auth: 'entra'; clientId: string; tenantId: string; apiScope: string };

export function provideFinAiAuth(env: AuthEnv): (Provider | EnvironmentProviders)[] {
  if (env.auth === 'dev') {
    return [{ provide: AuthService, useClass: DevAuthService }];
  }

  (globalThis as { __finaiApiScope?: string }).__finaiApiScope = env.apiScope;

  return [
    {
      provide: MSAL_INSTANCE,
      useFactory: () => new PublicClientApplication({
        auth: {
          clientId: env.clientId,
          authority: `https://login.microsoftonline.com/${env.tenantId}`,
          redirectUri: '/',
        },
        cache: { cacheLocation: 'sessionStorage' },
      }),
    },
    {
      provide: MSAL_GUARD_CONFIG,
      useValue: { interactionType: InteractionType.Redirect, authRequest: { scopes: [env.apiScope] } },
    },
    {
      provide: MSAL_INTERCEPTOR_CONFIG,
      useValue: {
        interactionType: InteractionType.Redirect,
        protectedResourceMap: new Map([['/', [env.apiScope]]]),
      },
    },
    MsalService, MsalGuard, MsalBroadcastService,
    { provide: HTTP_INTERCEPTORS, useClass: MsalInterceptor, multi: true },
    { provide: AuthService, useClass: EntraAuthService },
  ];
}
