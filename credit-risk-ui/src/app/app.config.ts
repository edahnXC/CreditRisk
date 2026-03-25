import { ApplicationConfig } from '@angular/core';
import { provideRouter, withInMemoryScrolling } from '@angular/router';
import { provideHttpClient } from '@angular/common/http';
import { routes } from './app.routes';

export const appConfig: ApplicationConfig = {
  providers: [
    provideRouter(
      routes, 
      // This is the correct way to tell Angular to scroll to the top on navigation
      withInMemoryScrolling({ scrollPositionRestoration: 'top' })
    ),
    provideHttpClient()
  ]
};