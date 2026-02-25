import { Injectable, signal } from '@angular/core';

export type ToastType = 'success' | 'error' | 'info' | 'warning';

export interface ToastMessage {
  id: number;
  type: ToastType;
  title: string;
  message: string;
}

export interface ConfirmState {
  title: string;
  message: string;
  confirmText: string;
  cancelText: string;
  danger: boolean;
}

@Injectable({ providedIn: 'root' })
export class UiFeedbackService {
  readonly toasts = signal<ToastMessage[]>([]);
  readonly confirmState = signal<ConfirmState | null>(null);

  private toastSeed = 1;
  private confirmResolver: ((value: boolean) => void) | null = null;

  showToast(type: ToastType, title: string, message: string, durationMs = 3800): void {
    const id = this.toastSeed++;
    const toast: ToastMessage = { id, type, title, message };

    this.toasts.update((items) => [toast, ...items]);

    window.setTimeout(() => {
      this.dismissToast(id);
    }, durationMs);
  }

  success(title: string, message: string): void {
    this.showToast('success', title, message);
  }

  error(title: string, message: string): void {
    this.showToast('error', title, message, 5200);
  }

  info(title: string, message: string): void {
    this.showToast('info', title, message);
  }

  warning(title: string, message: string): void {
    this.showToast('warning', title, message);
  }

  dismissToast(id: number): void {
    this.toasts.update((items) => items.filter((item) => item.id !== id));
  }

  confirm(options: Partial<ConfirmState> & Pick<ConfirmState, 'title' | 'message'>): Promise<boolean> {
    const state: ConfirmState = {
      title: options.title,
      message: options.message,
      confirmText: options.confirmText ?? 'Confirm',
      cancelText: options.cancelText ?? 'Cancel',
      danger: options.danger ?? false
    };

    this.confirmState.set(state);

    return new Promise<boolean>((resolve) => {
      this.confirmResolver = resolve;
    });
  }

  resolveConfirm(result: boolean): void {
    if (this.confirmResolver) {
      this.confirmResolver(result);
    }

    this.confirmResolver = null;
    this.confirmState.set(null);
  }
}
