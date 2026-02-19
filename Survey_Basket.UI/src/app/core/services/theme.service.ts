import { DOCUMENT } from '@angular/common';
import { Inject, Injectable, signal } from '@angular/core';

export type ThemeMode = 'light' | 'dark';

@Injectable({ providedIn: 'root' })
export class ThemeService {
  private readonly storageKey = 'sb_theme_mode';
  readonly mode = signal<ThemeMode>('light');

  constructor(@Inject(DOCUMENT) private readonly document: Document) {}

  initializeTheme(): ThemeMode {
    const stored = this.getStoredMode();
    const system = this.getSystemMode();
    const initial = stored ?? system;
    this.applyMode(initial);
    return initial;
  }

  toggleTheme(): ThemeMode {
    const next: ThemeMode = this.mode() === 'dark' ? 'light' : 'dark';
    this.setExplicitTheme(next);
    return next;
  }

  setExplicitTheme(mode: ThemeMode): void {
    localStorage.setItem(this.storageKey, mode);
    this.applyMode(mode);
  }

  private applyMode(mode: ThemeMode): void {
    this.mode.set(mode);
    this.document.documentElement.setAttribute('data-theme', mode);
  }

  private getStoredMode(): ThemeMode | null {
    const value = localStorage.getItem(this.storageKey);
    if (value === 'light' || value === 'dark') {
      return value;
    }
    return null;
  }

  private getSystemMode(): ThemeMode {
    return window.matchMedia?.('(prefers-color-scheme: dark)').matches ? 'dark' : 'light';
  }
}
