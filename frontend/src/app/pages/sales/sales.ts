import { CurrencyPipe, DatePipe, DecimalPipe } from '@angular/common';
import { ChangeDetectionStrategy, Component, computed, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { LucideBadgeDollarSign, LucideCircleCheck, LucidePlus, LucideReceiptText, LucideTrash2, LucideX } from '@lucide/angular';
import { finalize } from 'rxjs';
import { ApiService } from '../../core/api.service';
import { CocoaState, CocoaVariety, InventoryItem, PaymentMethod, Sale } from '../../core/models';

interface SaleForm { customerName: string; customerTaxId: string; customerEmail: string; variety: CocoaVariety; state: CocoaState; quantityQuintals: number; unitPrice: number; paymentMethod: PaymentMethod; soldAtUtc: string; notes: string; }
const empty = (): SaleForm => ({ customerName: '', customerTaxId: '', customerEmail: '', variety: 'Nacional', state: 'Seco', quantityQuintals: 1, unitPrice: 350, paymentMethod: 'Transferencia', soldAtUtc: new Date().toISOString().slice(0, 16), notes: '' });

@Component({ selector: 'app-sales', imports: [CurrencyPipe, DatePipe, DecimalPipe, FormsModule, LucideBadgeDollarSign, LucideCircleCheck, LucidePlus, LucideReceiptText, LucideTrash2, LucideX], templateUrl: './sales.html', changeDetection: ChangeDetectionStrategy.OnPush })
export class Sales {
  private readonly api = inject(ApiService); readonly items = signal<Sale[]>([]); readonly inventory = signal<InventoryItem[]>([]); readonly panelOpen = signal(false); readonly saving = signal(false); readonly emailing = signal(false); readonly error = signal(''); readonly success = signal(''); readonly completed = signal<Sale | null>(null); receiptEmail = '';
  form = empty(); readonly total = computed(() => this.formSignal().quantityQuintals * this.formSignal().unitPrice); private readonly formSignal = signal(this.form);
  constructor() { this.load(); }
  touch() { this.formSignal.set({ ...this.form }); }
  load() { this.api.sales().subscribe(x => this.items.set(x)); this.api.inventory().subscribe(x => this.inventory.set(x)); }
  open() { this.form = empty(); this.touch(); this.panelOpen.set(true); }
  available() { return this.inventory().find(x => x.variety === this.form.variety && x.state === this.form.state)?.quantityQuintals ?? 0; }
  save() { this.saving.set(true); this.api.createSale({ ...this.form, soldAtUtc: new Date(this.form.soldAtUtc).toISOString() }).pipe(finalize(() => this.saving.set(false))).subscribe({ next: item => { this.panelOpen.set(false); this.success.set(`Venta ${item.code} registrada correctamente.`); this.receiptEmail = item.customerEmail ?? ''; this.completed.set(item); this.load(); }, error: e => this.error.set(e?.error?.title ?? 'No se pudo registrar la venta.') }); }
  void(item: Sale) { if (!confirm(`¿Anular la venta ${item.code}?`)) return; this.api.voidSale(item.id).subscribe({ next: () => this.load(), error: e => this.error.set(e?.error?.title ?? 'No se pudo anular.') }); }
  receipt(item: Sale) { this.api.saleReceipt(item.id).subscribe({ next: blob => { const url = URL.createObjectURL(blob); const anchor = document.createElement('a'); anchor.href = url; anchor.download = `comprobante-${item.code}.pdf`; anchor.click(); URL.revokeObjectURL(url); }, error: () => this.error.set('No se pudo generar el comprobante PDF.') }); }
  emailReceipt() { const item = this.completed(); if (!item) return; this.error.set(''); this.emailing.set(true); this.api.emailSaleReceipt(item.id, this.receiptEmail.trim() || undefined).pipe(finalize(() => this.emailing.set(false))).subscribe({ next: x => this.success.set(x.message), error: e => this.error.set(e?.error?.title ?? e?.error?.detail ?? 'No se pudo enviar el comprobante.') }); }
}
