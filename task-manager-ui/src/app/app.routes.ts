import { Routes } from '@angular/router';
import { LoginComponent } from './pages/login/login.component';
import { AuthorizedComponent } from './pages/authorized/authorized.component';
import { UnauthorizedComponent } from './pages/unauthorized/unauthorized.component';
import { authGuard } from './core/guards/auth.guard';
import { roleGuard } from './core/guards/role.guard';

export const routes: Routes = [
  { path: 'login', component: LoginComponent },
  { path: 'unauthorized', component: UnauthorizedComponent },
  {
    path: '',
    component: AuthorizedComponent,
    canActivate: [authGuard],
    canActivateChild: [authGuard],
    children: [
      {
        path: 'admin',
        canActivateChild: [roleGuard],
        data: { role: 'Admin' },
        children: [
          {
            path: 'dashboard',
            loadComponent: () =>
              import(
                './pages/admin/admin-dashboard/admin-dashboard.component'
              ).then((m) => m.AdminDashboardComponent),
          },
          {
            path: 'users',
            loadComponent: () =>
              import(
                './pages/admin/user-management/user-management.component'
              ).then((m) => m.UserManagementComponent),
          },
          {
            path: 'status',
            loadComponent: () =>
              import(
                './pages/admin/status-management/status-management.component'
              ).then((m) => m.StatusManagementComponent),
          },
          {
            path: 'login-log',
            loadComponent: () =>
              import('./pages/admin/login-log/login-log.component').then(
                (m) => m.LoginLogComponent
              ),
          },
          {
            path: 'activity-log',
            loadComponent: () =>
              import('./pages/admin/activity-log/activity-log.component').then(
                (m) => m.ActivityLogComponent
              ),
          },
        ],
      },
      {
        path: '',
        canActivateChild: [roleGuard],
        data: { role: 'User' },
        children: [
          {
            path: 'dashboard',
            loadComponent: () =>
              import('./pages/user-dashboard/user-dashboard.component').then(
                (m) => m.UserDashboardComponent
              ),
          },
          {
            path: 'task',
            loadComponent: () =>
              import('./pages/task/task.component').then(
                (m) => m.TaskComponent
              ),
          },
          {
            path: 'status-log',
            loadComponent: () =>
              import('./pages/status-log/status-log.component').then(
                (m) => m.StatusLogComponent
              ),
          },
          {
            path: 'profile',
            loadComponent: () =>
              import('./pages/profile/profile.component').then(
                (m) => m.ProfileComponent
              ),
          },
        ],
      },
    ],
  },
  { path: '**', redirectTo: '/login' },
];
