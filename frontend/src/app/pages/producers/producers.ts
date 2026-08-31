import { CurrencyPipe, DecimalPipe } from '@angular/common';
import { ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { LucideMapPin, LucidePencil, LucidePhone, LucidePlus, LucideSearch, LucideTrash2, LucideUserRound, LucideX } from '@lucide/angular';
import { finalize } from 'rxjs';
import { ApiService } from '../../core/api.service';
import { PaymentMethod, Producer } from '../../core/models';

type ProducerForm = { fullName: string; documentNumber: string; farmLocation: string; phone: string; email: string; preferredPaymentMethod: PaymentMethod; notes: string; isActive: boolean };
const emptyForm = (): ProducerForm => ({ fullName: '', documentNumber: '', farmLocation: '', phone: '', email: '', preferredPaymentMethod: 'Efectivo', notes: '', isActive: true });

@Component({ selector: 'app-producers', imports: [CurrencyPipe, DecimalPipe, FormsModule, LucideMapPin, LucidePencil, LucidePhone, LucidePlus, LucideSearch, LucideTrash2, LucideUserRound, LucideX], templateUrl: './producers.html', changeDetection: ChangeDetectionStrategy.OnPush })
export class Producers {
  private readonly api = inject(ApiService); readonly items = signal<Producer[]>([]); readonly loading = signal(true); readonly saving = signal(false); readonly panelOpen = signal(false); readonly error = signal('');
  search = ''; form = emptyForm(); editingId: string | null = null;
  constructor() { this.load(); }
  load() { this.loading.set(true); this.api.producers(this.search).subscribe({ next: x => { this.items.set(x); this.loading.set(false); }, error: () => { this.error.set('No se pudieron cargar los productores.'); this.loading.set(false); } }); }
  open(item?: Producer) { this.editingId = item?.id ?? null; this.form = item ? { fullName: item.fullName, documentNumber: item.documentNumber, farmLocation: item.farmLocation, phone: item.phone, email: item.email ?? '', preferredPaymentMethod: item.preferredPaymentMethod, notes: item.notes ?? '', isActive: item.isActive } : emptyForm(); this.panelOpen.set(true); }
  close() { this.panelOpen.set(false); this.error.set(''); }
  save() { if (!this.form.fullName.trim() || !this.form.documentNumber.trim()) { this.error.set('Nombre y cédula/RUC son obligatorios.'); return; } this.saving.set(true); const request = this.editingId ? this.api.updateProducer(this.editingId, this.form) : this.api.createProducer(this.form); request.pipe(finalize(() => this.saving.set(false))).subscribe({ next: () => { this.close(); this.load(); }, error: e => this.error.set(e?.error?.title ?? 'No se pudo guardar.') }); }
  remove(item: Producer) { if (!confirm(`¿Eliminar o desactivar a ${item.fullName}?`)) return; this.api.deleteProducer(item.id).subscribe({ next: () => this.load(), error: e => this.error.set(e?.error?.title ?? 'No se pudo eliminar.') }); }
}
