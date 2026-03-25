import { ApplicationConfig } from '@angular/core';
import { provideRouter, withScrollPositionRestoration } from '@angular/router';
import { provideHttpClient } from '@angular/common/http';
import { routes } from './app.routes';

export const appConfig: ApplicationConfig = {
  providers: [
    provideRouter(routes, withScrollPositionRestoration('top')),
    provideHttpClient()
  ]
};