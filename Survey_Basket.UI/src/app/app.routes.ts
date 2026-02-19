import { Routes } from '@angular/router';
import { LoginComponent } from './pages/auth/login.component';
import { RegisterComponent } from './pages/auth/register.component';
import { ActivateCompanyComponent } from './pages/auth/activate-company.component';
import { DashboardComponent } from './pages/dashboard/dashboard.component';
import { CreatePollComponent } from './pages/polls/create-poll.component';
import { EditPollComponent } from './pages/polls/edit-poll.component';
import { VoteComponent } from './pages/polls/vote.component';
import { ResultsComponent } from './pages/polls/results.component';
import { UsersComponent } from './pages/admin/users.component';
import { CompaniesComponent } from './pages/admin/companies.component';
import { CompanyUsersComponent } from './pages/admin/company-users.component';
import { ProfileComponent } from './pages/profile/profile.component';
import { authGuard } from './core/guards/auth.guard';
import { AppShellComponent } from './layout/app-shell.component';
import { RolesComponent } from './pages/admin/roles.component';

export const routes: Routes = [
  { path: 'login', component: LoginComponent },
  { path: 'register', component: RegisterComponent },
  { path: 'activate-company/:companyAccountUserId', component: ActivateCompanyComponent },
  {
    path: '',
    component: AppShellComponent,
    canActivate: [authGuard],
    children: [
      { path: '', redirectTo: 'dashboard', pathMatch: 'full' },
      {
        path: 'dashboard',
        component: DashboardComponent
      },
      {
        path: 'polls/new',
        component: CreatePollComponent,
        data: { roles: ['Admin', 'SystemAdmin', 'PartnerCompany'] }
      },
      {
        path: 'polls/:id/edit',
        component: EditPollComponent,
        data: { roles: ['Admin', 'SystemAdmin', 'PartnerCompany'] }
      },
      {
        path: 'polls/:id/vote',
        component: VoteComponent,
        data: { roles: ['Member', 'PartnerCompany'] }
      },
      {
        path: 'polls/:id/results',
        component: ResultsComponent,
        data: { roles: ['Admin', 'SystemAdmin', 'PartnerCompany'] }
      },
      {
        path: 'admin/companies',
        component: CompaniesComponent,
        data: { roles: ['Admin', 'SystemAdmin'] }
      },
      {
        path: 'users',
        component: UsersComponent,
        data: { roles: ['Admin', 'SystemAdmin'] }
      },
      {
        path: 'admin/roles',
        component: RolesComponent,
        data: { roles: ['Admin', 'SystemAdmin'] }
      },
      {
        path: 'company/users',
        component: CompanyUsersComponent,
        data: { roles: ['PartnerCompany'] }
      },
      {
        path: 'profile',
        component: ProfileComponent
      }
    ]
  },
  { path: '**', redirectTo: 'dashboard' }
];
