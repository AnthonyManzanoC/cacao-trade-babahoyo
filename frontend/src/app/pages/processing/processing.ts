import { CurrencyPipe, DatePipe, DecimalPipe } from '@angular/common';
import { ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { LucideCircleCheck, LucidePlus, LucideRefreshCw, LucideSun, LucideX } from '@lucide/angular';
import { finalize, forkJoin } from 'rxjs';
import { ApiService } from '../../core/api.service';
import { CocoaVariety, InventoryLot, ProcessingBatch } from '../../core/models';

@Component({ selector: 'app-processing', imports: [CurrencyPipe, DatePipe, DecimalPipe, FormsModule, LucideCircleCheck, LucidePlus, LucideRefreshCw, LucideSun, LucideX], templateUrl: './processing.html', changeDetection: ChangeDetectionStrategy.OnPush })
export class Processing {
  private readonly api = inject(ApiService); readonly batches = signal<ProcessingBatch[]>([]); readonly wetLots = signal<InventoryLot[]>([]);
  readonly panelOpen = signal(false); readonly saving = signal(false); readonly error = signal(''); readonly success = signal('');
  form: { variety: CocoaVariety; inputWetQuintals: number; expectedDryYieldPercent: number; startedAtUtc: string; notes: string } = this.empty();
  completing = signal<ProcessingBatch | null>(null); outputDryQuintals = 0; completionNotes = '';
  constructor() { this.load(); }
  private empty() { return { variety: 'Nacional' as CocoaVariety, inputWetQuintals: 1, expectedDryYieldPercent: 40, startedAtUtc: new Date().toISOString().slice(0, 16), notes: '' }; }
  load() { forkJoin({ batches: this.api.processing(), lots: this.api.inventoryLots('Baba') }).subscribe({ next: x => { this.batches.set(x.batches); this.wetLots.set(x.lots); }, error: () => this.error.set('No se pudo cargar el módulo de secado.') }); }
  available(variety: CocoaVariety) { return this.wetLots().filter(x => x.variety === variety && x.status === 'Disponible').reduce((sum, x) => sum + x.availableQuantityQuintals, 0); }
  open() { this.form = this.empty(); this.panelOpen.set(true); this.error.set(''); }
  save() { this.saving.set(true); this.api.createProcessing({ ...this.form, startedAtUtc: new Date(this.form.startedAtUtc).toISOString() }).pipe(finalize(() => this.saving.set(false))).subscribe({ next: x => { this.panelOpen.set(false); this.success.set(`Proceso ${x.code} iniciado.`); this.load(); }, error: e => this.error.set(e?.error?.title ?? 'No se pudo iniciar el secado.') }); }
  openComplete(item: ProcessingBatch) { this.completing.set(item); this.outputDryQuintals = Number((item.inputWetQuintals * item.expectedDryYieldPercent / 100).toFixed(4)); this.completionNotes = ''; }
  complete() { const item = this.completing(); if (!item) return; this.saving.set(true); this.api.completeProcessing(item.id, { outputDryQuintals: this.outputDryQuintals, completedAtUtc: new Date().toISOString(), notes: this.completionNotes }).pipe(finalize(() => this.saving.set(false))).subscribe({ next: x => { this.completing.set(null); this.success.set(`${x.code} completado: el lote seco ya está en inventario.`); this.load(); }, error: e => this.error.set(e?.error?.title ?? 'No se pudo completar el secado.') }); }
  cancel(item: ProcessingBatch) { if (!confirm(`¿Cancelar ${item.code} y devolver el cacao en baba al inventario?`)) return; this.api.cancelProcessing(item.id).subscribe({ next: () => { this.success.set('Proceso cancelado e inventario restaurado.'); this.load(); }, error: e => this.error.set(e?.error?.title ?? 'No se pudo cancelar.') }); }
}
