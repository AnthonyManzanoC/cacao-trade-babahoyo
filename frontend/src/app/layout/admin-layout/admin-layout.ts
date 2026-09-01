import { UpperCasePipe } from '@angular/common';
import { ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import { RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';
import { LucideBadgeDollarSign, LucideBoxes, LucideLayoutDashboard, LucideLogOut, LucideMenu, LucideReceiptText, LucideSettings, LucideSun, LucideUsersRound, LucideX } from '@lucide/angular';
import { AuthService } from '../../core/auth.service';
import { BrandService } from '../../core/brand.service';

@Component({ selector: 'app-admin-layout', imports: [UpperCasePipe, RouterOutlet, RouterLink, RouterLinkActive, LucideBadgeDollarSign, LucideBoxes, LucideLayoutDashboard, LucideLogOut, LucideMenu, LucideReceiptText, LucideSettings, LucideSun, LucideUsersRound, LucideX], templateUrl: './admin-layout.html', changeDetection: ChangeDetectionStrategy.OnPush })
export class AdminLayout {
  readonly identity = inject(BrandService).identity;
  readonly menuOpen = signal(false);
  readonly links = [
    { path: '/admin/dashboard', label: 'Resumen', icon: 'dashboard' }, { path: '/admin/compras', label: 'Compras', icon: 'purchases' },
    { path: '/admin/productores', label: 'Productores', icon: 'producers' }, { path: '/admin/inventario', label: 'Inventario', icon: 'inventory' },
    { path: '/admin/secado', label: 'Secado', icon: 'processing' }, { path: '/admin/ventas', label: 'Ventas', icon: 'sales' },
    { path: '/admin/caja', label: 'Caja', icon: 'cash' }, { path: '/admin/sitio-web', label: 'Sitio Web', icon: 'site' },
    { path: '/admin/configuracion', label: 'Configuración', icon: 'settings' },
  ];
  constructor(readonly auth: AuthService) {}
}
