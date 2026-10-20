import { ApplicationConfig, provideBrowserGlobalErrorListeners } from '@angular/core';
import { provideRouter, withComponentInputBinding } from '@angular/router';

import { routes } from './app.routes';
import { provideHttpClient, withFetch, withInterceptors, withInterceptorsFromDi } from '@angular/common/http';
import { authInterceptor, provideFinAiAuth } from 'shared-ui';
import { environment } from '../environments/environment';

const isDev = environment.auth.auth === 'dev';

export const appConfig: ApplicationConfig = {
  providers: [
    provideBrowserGlobalErrorListeners(),
    provideRouter(routes, withComponentInputBinding()),
    provideHttpClient(
      withFetch(),
      withInterceptors(isDev ? [authInterceptor] : []),
      withInterceptorsFromDi(),
    ),
    ...provideFinAiAuth(environment.auth),
  ]
};
