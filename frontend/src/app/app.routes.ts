import { Routes } from '@angular/router';
import { authGuard } from './core/auth.guard';

export const routes: Routes = [
  { path: '', loadComponent: () => import('./pages/public-home/public-home').then(m => m.PublicHome) },
  { path: 'precios', loadComponent: () => import('./pages/public-home/public-home').then(m => m.PublicHome) },
  { path: 'servicios', data: { section: 'Servicio' }, loadComponent: () => import('./pages/public-info/public-info').then(m => m.PublicInfo) },
  { path: 'nosotros', data: { section: 'Nosotros' }, loadComponent: () => import('./pages/public-info/public-info').then(m => m.PublicInfo) },
  { path: 'contacto', data: { section: 'Contacto' }, loadComponent: () => import('./pages/public-info/public-info').then(m => m.PublicInfo) },
  { path: 'admin/login', loadComponent: () => import('./pages/login/login').then(m => m.Login) },
  {
    path: 'admin',
    canActivate: [authGuard],
    loadComponent: () => import('./layout/admin-layout/admin-layout').then(m => m.AdminLayout),
    children: [
      { path: '', pathMatch: 'full', redirectTo: 'dashboard' },
      { path: 'dashboard', loadComponent: () => import('./pages/dashboard/dashboard').then(m => m.Dashboard) },
      { path: 'compras', loadComponent: () => import('./pages/purchases/purchases').then(m => m.Purchases) },
      { path: 'productores', loadComponent: () => import('./pages/producers/producers').then(m => m.Producers) },
      { path: 'inventario', loadComponent: () => import('./pages/inventory/inventory').then(m => m.Inventory) },
      { path: 'secado', loadComponent: () => import('./pages/processing/processing').then(m => m.Processing) },
      { path: 'ventas', loadComponent: () => import('./pages/sales/sales').then(m => m.Sales) },
      { path: 'caja', loadComponent: () => import('./pages/cash-register/cash-register').then(m => m.CashRegisterPage) },
      { path: 'sitio-web', loadComponent: () => import('./pages/site-content/site-content').then(m => m.SiteContent) },
      { path: 'configuracion', loadComponent: () => import('./pages/settings/settings').then(m => m.Settings) },
    ],
  },
  { path: '**', redirectTo: '' },
];
