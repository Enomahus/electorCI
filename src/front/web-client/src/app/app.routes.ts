import { Routes } from '@angular/router';
import { HomeUi } from './pages/home-ui/home-ui';
import { PageTemplateUi } from './shared/page-template-ui/page-template-ui';

export const routes: Routes = [
  {
    path: '',
    redirectTo: '/home',
    pathMatch: 'full',
  },
  {
    path: '',
    component: PageTemplateUi,
    children: [
      {
        path: 'home',
        component: HomeUi,
      },
    ],
  },
];
