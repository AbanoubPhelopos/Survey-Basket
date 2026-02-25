import { CommonModule } from '@angular/common';
import { ChangeDetectionStrategy, Component, inject } from '@angular/core';
import { UiFeedbackService } from '../../core/services/ui-feedback.service';

@Component({
  selector: 'app-ui-feedback',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="sb-toast-stack" aria-live="polite" aria-atomic="true">
      @for (toast of uiFeedback.toasts(); track toast.id) {
        <article class="sb-toast" [class]="'sb-toast sb-toast--' + toast.type" role="status">
          <div>
            <p class="sb-toast__title">{{ toast.title }}</p>
            <p class="sb-toast__message">{{ toast.message }}</p>
          </div>
          <button type="button" class="sb-toast__close" (click)="uiFeedback.dismissToast(toast.id)">x</button>
        </article>
      }
    </div>

    @if (uiFeedback.confirmState(); as confirm) {
      <div class="sb-confirm__backdrop" (click)="uiFeedback.resolveConfirm(false)"></div>
      <section class="sb-confirm" role="alertdialog" aria-modal="true">
        <h2 class="sb-confirm__title">{{ confirm.title }}</h2>
        <p class="sb-confirm__message">{{ confirm.message }}</p>

        <div class="sb-confirm__actions">
          <button type="button" class="sb-btn-secondary" (click)="uiFeedback.resolveConfirm(false)">
            {{ confirm.cancelText }}
          </button>
          <button
            type="button"
            class="sb-btn-primary"
            [class.sb-btn-danger]="confirm.danger"
            (click)="uiFeedback.resolveConfirm(true)">
            {{ confirm.confirmText }}
          </button>
        </div>
      </section>
    }
  `,
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class UiFeedbackComponent {
  protected readonly uiFeedback = inject(UiFeedbackService);
}
