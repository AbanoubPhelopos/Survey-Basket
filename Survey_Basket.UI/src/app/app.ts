import { Component, signal } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { ThemeService } from './core/services/theme.service';
import { UiFeedbackComponent } from './shared/ui-feedback/ui-feedback.component';

@Component({
  selector: 'app-root',
  imports: [RouterOutlet, UiFeedbackComponent],
  templateUrl: './app.html',
  styleUrls: ['./app.css']
})
export class App {
  protected readonly title = signal('Survey_Basket.UI');

  constructor(private readonly themeService: ThemeService) {
    this.themeService.initializeTheme();
  }
}
