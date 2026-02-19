import { Routes } from '@angular/router';
import { LoginComponent } from './pages/auth/login.component';
import { RegisterComponent } from './pages/auth/register.component';
import { DashboardComponent } from './pages/dashboard/dashboard.component';
import { CreatePollComponent } from './pages/polls/create-poll.component';
import { EditPollComponent } from './pages/polls/edit-poll.component';
import { VoteComponent } from './pages/polls/vote.component';
import { ResultsComponent } from './pages/polls/results.component';
import { UsersComponent } from './pages/admin/users.component';
import { CompaniesComponent } from './pages/admin/companies.component';
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
    canActivate: [authGuard],
    data: { roles: ['Admin'] }
  },
  { 
    path: 'polls/:id/edit', 
    component: EditPollComponent,
    canActivate: [authGuard],
    data: { roles: ['Admin'] }
  },
  { 
    path: 'polls/:id/vote', 
    component: VoteComponent,
    canActivate: [authGuard],
    data: { roles: ['Member'] }
  },
  { 
    path: 'polls/:id/results', 
    component: ResultsComponent,
    canActivate: [authGuard],
    data: { roles: ['Admin'] }
  },
  { 
    path: 'admin/companies',
    component: CompaniesComponent,
    canActivate: [authGuard],
    data: { roles: ['Admin', 'SystemAdmin'] }
  },
  { 
    path: 'users', 
    component: UsersComponent,
    canActivate: [authGuard],
    data: { roles: ['Admin'] }
  },
  { 
    path: 'profile', 
    component: ProfileComponent,
    canActivate: [authGuard]
  },
  { path: '**', redirectTo: 'dashboard' }
];
