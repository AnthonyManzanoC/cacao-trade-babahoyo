import { CurrencyPipe, DatePipe, DecimalPipe } from '@angular/common';
import { ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { LucideArrowUpRight, LucideBadgeDollarSign, LucideBoxes, LucideCircleDollarSign, LucidePlus, LucideReceiptText, LucideRefreshCw } from '@lucide/angular';
import { ApiService } from '../../core/api.service';
import { DashboardData } from '../../core/models';

@Component({ selector: 'app-dashboard', imports: [CurrencyPipe, DatePipe, DecimalPipe, RouterLink, LucideArrowUpRight, LucideBadgeDollarSign, LucideBoxes, LucideCircleDollarSign, LucidePlus, LucideReceiptText, LucideRefreshCw], templateUrl: './dashboard.html', changeDetection: ChangeDetectionStrategy.OnPush })
export class Dashboard {
  readonly Math = Math;
  private readonly api = inject(ApiService); readonly loading = signal(true); readonly error = signal('');
  readonly data = signal<DashboardData | null>(null);
  constructor() { this.load(); }
  load() { this.loading.set(true); this.api.dashboard().subscribe({ next: x => { this.data.set(x); this.loading.set(false); }, error: () => { this.error.set('No se pudo cargar el resumen.'); this.loading.set(false); } }); }
}
