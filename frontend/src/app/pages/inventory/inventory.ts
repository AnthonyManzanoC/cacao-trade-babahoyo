import { CurrencyPipe, DecimalPipe } from '@angular/common';
import { ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import { LucideBoxes, LucidePackageCheck, LucideRefreshCw, LucideWarehouse } from '@lucide/angular';
import { ApiService } from '../../core/api.service';
import { forkJoin } from 'rxjs';
import { InventoryItem, InventoryLot } from '../../core/models';

@Component({ selector: 'app-inventory', imports: [CurrencyPipe, DecimalPipe, LucideBoxes, LucidePackageCheck, LucideRefreshCw, LucideWarehouse], templateUrl: './inventory.html', changeDetection: ChangeDetectionStrategy.OnPush })
export class Inventory { private readonly api = inject(ApiService); readonly items = signal<InventoryItem[]>([]); readonly lots = signal<InventoryLot[]>([]); readonly error = signal(''); constructor() { this.load(); } load() { forkJoin({ items: this.api.inventory(), lots: this.api.inventoryLots() }).subscribe({ next: x => { this.items.set(x.items); this.lots.set(x.lots); }, error: () => this.error.set('No se pudo cargar el inventario.') }); } totalQty() { return this.items().reduce((s, x) => s + x.quantityQuintals, 0); } totalValue() { return this.items().reduce((s, x) => s + x.estimatedValue, 0); } }
