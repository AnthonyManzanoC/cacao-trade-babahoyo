import { CurrencyPipe, DatePipe } from '@angular/common';
import { ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { LucideBadgeDollarSign, LucideCircleCheck, LucidePlus, LucideRefreshCw } from '@lucide/angular';
import { finalize } from 'rxjs';
import { ApiService } from '../../core/api.service';
import { CashMovementCategory, CashMovementDirection, CashRegister, PaymentMethod } from '../../core/models';

@Component({ selector: 'app-cash-register', imports: [CurrencyPipe, DatePipe, FormsModule, LucideBadgeDollarSign, LucideCircleCheck, LucidePlus, LucideRefreshCw], templateUrl: './cash-register.html', changeDetection: ChangeDetectionStrategy.OnPush })
export class CashRegisterPage {
  private readonly api = inject(ApiService); readonly current = signal<CashRegister | null>(null); readonly history = signal<CashRegister[]>([]);
  readonly saving = signal(false); readonly error = signal(''); readonly success = signal('');
  opening = { businessDate: new Date().toISOString().slice(0, 10), openingBalance: 0, notes: '' };
  movement: { direction: CashMovementDirection; category: CashMovementCategory; amount: number; description: string; paymentMethod: PaymentMethod } = { direction: 'Egreso', category: 'GastoOperativo', amount: 0, description: '', paymentMethod: 'Efectivo' };
  countedClosingBalance = 0; closeNotes = '';
  constructor() { this.load(); }
  load() { this.api.currentCashRegister().subscribe({ next: x => { this.current.set(x); if (x) this.countedClosingBalance = x.expectedBalance; }, error: () => this.current.set(null) }); this.api.cashRegisters().subscribe({ next: x => this.history.set(x), error: () => this.error.set('No se pudo cargar el historial de caja.') }); }
  open() { this.saving.set(true); this.error.set(''); this.api.openCashRegister(this.opening).pipe(finalize(() => this.saving.set(false))).subscribe({ next: x => { this.current.set(x); this.success.set('Caja abierta. Las compras y ventas en efectivo se registrarán automáticamente.'); this.load(); }, error: e => this.error.set(e?.error?.title ?? 'No se pudo abrir la caja.') }); }
  addMovement() { const current = this.current(); if (!current) return; this.saving.set(true); this.error.set(''); this.api.addCashMovement(current.id, this.movement).pipe(finalize(() => this.saving.set(false))).subscribe({ next: x => { this.current.set(x); this.movement.amount = 0; this.movement.description = ''; this.success.set('Movimiento registrado.'); this.load(); }, error: e => this.error.set(e?.error?.title ?? 'No se pudo registrar el movimiento.') }); }
  close() { const current = this.current(); if (!current || !confirm('¿Cerrar la caja? Después no se podrán agregar movimientos.')) return; this.saving.set(true); this.api.closeCashRegister(current.id, { countedClosingBalance: this.countedClosingBalance, notes: this.closeNotes }).pipe(finalize(() => this.saving.set(false))).subscribe({ next: () => { this.current.set(null); this.success.set('Caja cerrada y conciliada.'); this.load(); }, error: e => this.error.set(e?.error?.title ?? 'No se pudo cerrar la caja.') }); }
}
