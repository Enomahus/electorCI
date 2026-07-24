import { Routes } from '@angular/router';
import { DistrictsUi } from './pages/administration/districts-ui/districts-ui';
import { PollingStationsUi } from './pages/administration/polling-stations-ui/polling-stations-ui';
import { UserCreateUi } from './pages/administration/users-ui/user-create-ui/user-create-ui';
import { UserUpdateUi } from './pages/administration/users-ui/user-update-ui/user-update-ui';
import { UsersUi } from './pages/administration/users-ui/users-ui';
import { FaqUi } from './pages/faq-ui/faq-ui';
import { HomeUi } from './pages/home-ui/home-ui';
import { CreateAccountUi } from './pages/login-ui/create-account-ui/create-account-ui';
import { LoginUi } from './pages/login-ui/login-ui';
import { MyAccountUi } from './pages/my-account-ui/my-account-ui';
import { PermissionsGuard } from './services/auth/permission.guard';
import { AppPermission } from './services/nswag/api-nswag-client';
import { PageTemplateUi } from './shared/page-template-ui/page-template-ui';

export function perm(p: AppPermission): AppPermission {
  return p;
}

export const routes: Routes = [
  {
    path: '',
    redirectTo: '/home',
    pathMatch: 'full',
  },
  {
    path: 'login',
    component: LoginUi,
    title: 'login.title',
  },
  {
    path: '',
    component: PageTemplateUi,
    children: [
      {
        path: 'home',
        component: HomeUi,
        title: 'home.title',
      },
      {
        path: 'my-account',
        component: MyAccountUi,
        canActivate: [PermissionsGuard],
        title: 'register.title',
      },
      {
        path: 'register',
        component: CreateAccountUi,
        title: 'register.title',
      },
      {
        path: 'faq',
        component: FaqUi,
        title: 'faq.title',
      },
      {
        path: 'admin',
        children: [
          {
            path: 'districts',
            component: DistrictsUi,
            canActivate: [PermissionsGuard],
            data: {
              permission: perm('accessDistrictsAdminPage'),
            },
            title: 'districts.title',
          },
          {
            path: 'polling-stations',
            component: PollingStationsUi,
            canActivate: [PermissionsGuard],
            data: {
              permission: perm('accessPollingStationsAdminPage'),
            },
            title: 'pollingStation.title',
          },
          {
            path: 'users',
            component: UsersUi,
            canActivate: [PermissionsGuard],
            title: 'users.title',
            data: {
              permission: perm('accessUsersAdminPage'),
            },
          },
          {
            path: 'users/new',
            component: UserCreateUi,
            canActivate: [PermissionsGuard],
            title: 'users.titleNewUser',
            data: {
              permission: perm('createUser'),
            },
          },
          {
            path: 'users/:id/edit',
            component: UserUpdateUi,
            title: 'users.titleEditUser',
            data: {
              permission: perm('updateUser'),
            },
          },
        ],
      },
    ],
  },
  {
    path: '**',
    redirectTo: '/home',
  },
];
