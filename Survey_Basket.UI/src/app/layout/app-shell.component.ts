import { ChangeDetectionStrategy, Component } from '@angular/core';
import { RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';

@Component({
  selector: 'app-shell',
  standalone: true,
  imports: [RouterOutlet, RouterLink, RouterLinkActive],
  templateUrl: './app-shell.component.html',
  styleUrls: ['./app-shell.component.scss'],
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class AppShellComponent {
  protected readonly navItems = [
    { label: 'Dashboard', route: '/dashboard' },
    { label: 'Polls', route: '/polls' },
    { label: 'Users', route: '/users' },
    { label: 'Profile', route: '/profile' }
  ];
}
