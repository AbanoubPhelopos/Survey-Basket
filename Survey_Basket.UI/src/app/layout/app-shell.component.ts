import { ChangeDetectionStrategy, Component, computed, inject } from '@angular/core';
import { RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';
import { AuthService } from '../core/services/auth.service';
import { APP_NAV_ITEMS } from '../core/models/navigation';
import { ThemeService } from '../core/services/theme.service';

@Component({
  selector: 'app-shell',
  standalone: true,
  imports: [RouterOutlet, RouterLink, RouterLinkActive],
  templateUrl: './app-shell.component.html',
  styleUrls: ['./app-shell.component.scss'],
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class AppShellComponent {
  private readonly authService = inject(AuthService);
  private readonly themeService = inject(ThemeService);

  protected readonly currentTheme = this.themeService.mode;

  protected readonly navItems = computed(() =>
    APP_NAV_ITEMS.filter((item) => {
      if (item.roles?.length && !this.authService.hasAnyRole(item.roles)) {
        return false;
      }
      if (item.permissions?.length && !this.authService.hasAnyPermission(item.permissions)) {
        return false;
      }
      return true;
    })
  );

  protected toggleTheme(): void {
    this.themeService.toggleTheme();
  }

  protected logout(): void {
    this.authService.logout();
  }
}
