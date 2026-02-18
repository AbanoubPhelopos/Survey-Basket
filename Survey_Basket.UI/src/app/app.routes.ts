import { Routes } from '@angular/router';
import { LoginComponent } from './pages/auth/login.component';
import { RegisterComponent } from './pages/auth/register.component';
import { DashboardComponent } from './pages/dashboard/dashboard.component';
import { CreatePollComponent } from './pages/polls/create-poll.component';
import { VoteComponent } from './pages/polls/vote.component';
import { ResultsComponent } from './pages/polls/results.component';
import { UsersComponent } from './pages/admin/users.component';
import { ProfileComponent } from './pages/profile/profile.component';
import { authGuard } from './core/guards/auth.guard';

export const routes: Routes = [
  { path: '', redirectTo: 'dashboard', pathMatch: 'full' },
  { path: 'login', component: LoginComponent },
  { path: 'register', component: RegisterComponent },
  { 
    path: 'dashboard', 
    component: DashboardComponent,
    canActivate: [authGuard]
  },
  { 
    path: 'polls/new', 
    component: CreatePollComponent,
    canActivate: [authGuard]
  },
  { 
    path: 'polls/:id/vote', 
    component: VoteComponent,
    canActivate: [authGuard]
  },
  { 
    path: 'polls/:id/results', 
    component: ResultsComponent,
    canActivate: [authGuard]
  },
  { 
    path: 'users', 
    component: UsersComponent,
    canActivate: [authGuard]
  },
  { 
    path: 'profile', 
    component: ProfileComponent,
    canActivate: [authGuard]
  },
  { path: '**', redirectTo: 'dashboard' }
];
