import { CurrencyPipe, DatePipe } from '@angular/common';
import { ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { LucideCircleCheck, LucideCloudCog, LucideRefreshCw, LucideSave, LucideShieldCheck, LucideTriangleAlert } from '@lucide/angular';
import { finalize } from 'rxjs';
import { ApiService } from '../../core/api.service';
import { BrandService } from '../../core/brand.service';
import { SettingsData } from '../../core/models';

type SettingsForm = SettingsData & { smtpPassword: string };

@Component({ selector: 'app-settings', imports: [CurrencyPipe, DatePipe, FormsModule, LucideCircleCheck, LucideCloudCog, LucideRefreshCw, LucideSave, LucideShieldCheck, LucideTriangleAlert], templateUrl: './settings.html', changeDetection: ChangeDetectionStrategy.OnPush })
export class Settings {
  private readonly api = inject(ApiService);
  private readonly brand = inject(BrandService);
  readonly data = signal<SettingsData | null>(null);
  readonly form = signal<SettingsForm | null>(null);
  readonly saving = signal(false);
  readonly refreshing = signal(false);
  readonly message = signal('');
  readonly error = signal('');

  constructor() { this.load(); }

  load() {
    this.api.settings().subscribe({
      next: x => { this.data.set(x); this.form.set({ ...x, smtpPassword: '' }); },
      error: () => this.error.set('No se pudo cargar la configuración.')
    });
  }

  save() {
    const form = this.form();
    if (!form) return;
    this.message.set(''); this.error.set(''); this.saving.set(true);
    this.api.updateSettings(form).pipe(finalize(() => this.saving.set(false))).subscribe({
      next: x => { this.data.set(x); this.form.set({ ...x, smtpPassword: '' }); this.brand.set({ businessName: x.businessName, logoUrl: x.logoUrl }); this.message.set('Configuración guardada. El portal público ya usa estos datos.'); },
      error: e => this.error.set(e?.error?.title ?? e?.error?.detail ?? 'No se pudo guardar.')
    });
  }

  refresh() {
    this.message.set(''); this.error.set(''); this.refreshing.set(true);
    this.api.refreshPrice().pipe(finalize(() => this.refreshing.set(false))).subscribe({
      next: x => { this.message.set(x.message); this.load(); },
      error: e => this.error.set(e?.error?.title ?? e?.error?.detail ?? 'No se pudo consultar el mercado.')
    });
  }

  projectedDry() { const f = this.form(); if (!f) return 0; return f.useManualPrice ? Number(f.manualDryPricePerQuintal ?? 0) : Math.max(0, f.currentMarketPricePerMetricTon / 22.046 - Number(f.marginPerQuintal)); }
  projectedWet() { return this.projectedDry() * Number(this.form()?.wetPriceFactor ?? 0); }
}
