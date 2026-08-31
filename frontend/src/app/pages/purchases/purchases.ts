import { CurrencyPipe, DatePipe, DecimalPipe } from '@angular/common';
import { ChangeDetectionStrategy, Component, computed, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { LucideCalculator, LucideCircleCheck, LucidePlus, LucideReceiptText, LucideTrash2, LucideX } from '@lucide/angular';
import { finalize } from 'rxjs';
import { ApiService } from '../../core/api.service';
import { CocoaState, CocoaVariety, PaymentMethod, Producer, Purchase } from '../../core/models';

interface PurchaseForm { producerId: string; variety: CocoaVariety; state: CocoaState; grossWeightLbs: number; tareLbs: number; humidityPercent: number; shrinkagePercent: number; unitPrice: number; paymentMethod: PaymentMethod; purchasedAtUtc: string; notes: string; }
const empty = (): PurchaseForm => ({ producerId: '', variety: 'Nacional', state: 'Seco', grossWeightLbs: 100, tareLbs: 0, humidityPercent: 0, shrinkagePercent: 0, unitPrice: 300, paymentMethod: 'Efectivo', purchasedAtUtc: new Date().toISOString().slice(0, 16), notes: '' });

@Component({ selector: 'app-purchases', imports: [CurrencyPipe, DatePipe, DecimalPipe, FormsModule, RouterLink, LucideCalculator, LucideCircleCheck, LucidePlus, LucideReceiptText, LucideTrash2, LucideX], templateUrl: './purchases.html', changeDetection: ChangeDetectionStrategy.OnPush })
export class Purchases {
  private readonly api = inject(ApiService); readonly items = signal<Purchase[]>([]); readonly producers = signal<Producer[]>([]); readonly panelOpen = signal(false); readonly saving = signal(false); readonly emailing = signal(false); readonly error = signal(''); readonly success = signal(''); readonly completed = signal<Purchase | null>(null); receiptEmail = '';
  form = empty(); readonly calculation = computed(() => { const f = this.formSignal(); const net = Math.max(0, Number(f.grossWeightLbs) - Number(f.tareLbs)); const payable = net * Math.max(0, 1 - (Number(f.humidityPercent) + Number(f.shrinkagePercent)) / 100); const quintals = payable / 100; return { net, payable, quintals, total: quintals * Number(f.unitPrice) }; }); private readonly formSignal = signal(this.form);
  constructor() { this.load(); this.api.producers().subscribe(x => this.producers.set(x.filter(p => p.isActive))); }
  touch() { this.formSignal.set({ ...this.form }); }
  load() { this.api.purchases().subscribe({ next: x => this.items.set(x), error: () => this.error.set('No se pudieron cargar las compras.') }); }
  open() { this.form = empty(); this.api.publicPrice().subscribe(p => { this.form.unitPrice = p.dryPricePerQuintal; this.touch(); }); this.touch(); this.panelOpen.set(true); }
  stateChanged() { this.api.publicPrice().subscribe(p => { this.form.unitPrice = this.form.state === 'Seco' ? p.dryPricePerQuintal : p.wetPricePerQuintal; this.touch(); }); }
  save() { if (!this.form.producerId) { this.error.set('Selecciona un productor.'); return; } this.saving.set(true); this.api.createPurchase({ ...this.form, purchasedAtUtc: new Date(this.form.purchasedAtUtc).toISOString() }).pipe(finalize(() => this.saving.set(false))).subscribe({ next: item => { this.panelOpen.set(false); this.success.set(`Compra ${item.code} registrada por ${item.totalPaid.toFixed(2)} USD.`); this.receiptEmail = item.producerEmail ?? ''; this.completed.set(item); this.load(); }, error: e => this.error.set(e?.error?.title ?? 'No se pudo registrar la compra.') }); }
  void(item: Purchase) { if (!confirm(`¿Anular la compra ${item.code}? Se revertirá el inventario.`)) return; this.api.voidPurchase(item.id).subscribe({ next: () => this.load(), error: e => this.error.set(e?.error?.title ?? 'No se pudo anular.') }); }
  receipt(item: Purchase) { this.api.purchaseReceipt(item.id).subscribe({ next: blob => { const url = URL.createObjectURL(blob); const anchor = document.createElement('a'); anchor.href = url; anchor.download = `comprobante-${item.code}.pdf`; anchor.click(); URL.revokeObjectURL(url); }, error: () => this.error.set('No se pudo generar el comprobante PDF.') }); }
  emailReceipt() { const item = this.completed(); if (!item) return; this.error.set(''); this.emailing.set(true); this.api.emailPurchaseReceipt(item.id, this.receiptEmail.trim() || undefined).pipe(finalize(() => this.emailing.set(false))).subscribe({ next: x => this.success.set(x.message), error: e => this.error.set(e?.error?.title ?? e?.error?.detail ?? 'No se pudo enviar el comprobante.') }); }
}
