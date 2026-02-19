import { bootstrapApplication } from '@angular/platform-browser';
import { appConfig } from './app/app.config';
import { App } from './app/app';

const storedMode = localStorage.getItem('sb_theme_mode');
const initialMode = storedMode === 'dark' || storedMode === 'light'
  ? storedMode
  : (window.matchMedia?.('(prefers-color-scheme: dark)').matches ? 'dark' : 'light');

document.documentElement.setAttribute('data-theme', initialMode);

bootstrapApplication(App, appConfig)
  .catch((err) => console.error(err));
