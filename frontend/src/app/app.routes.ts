import { Routes } from '@angular/router';
import { adminGuard } from './core/guards/auth.guard';

export const routes: Routes = [
  {
    path: '',
    redirectTo: 'panchang',
    pathMatch: 'full'
  },
  {
    path: 'panchang',
    loadComponent: () => import('./features/daily-panchang/daily-panchang.component').then(m => m.DailyPanchangComponent)
  },
  {
    path: 'videos',
    loadComponent: () => import('./features/video-hub/video-hub.component').then(m => m.VideoHubComponent)
  },
  {
    path: 'itihaas',
    loadComponent: () => import('./features/aaj-ka-itihaas/aaj-ka-itihaas.component').then(m => m.AajKaItihaasComponent)
  },
  {
    path: 'festivals',
    loadComponent: () => import('./features/festivals/festivals.component').then(m => m.FestivalsComponent)
  },
  {
    path: 'admin',
    canActivate: [adminGuard],
    loadComponent: () => import('./features/admin/panchang-admin.component').then(m => m.PanchangAdminComponent)
  },
  {
    path: 'manage',
    canActivate: [adminGuard],
    redirectTo: 'admin',
    pathMatch: 'full'
  },
  {
    path: '**',
    redirectTo: 'panchang'
  }
];
