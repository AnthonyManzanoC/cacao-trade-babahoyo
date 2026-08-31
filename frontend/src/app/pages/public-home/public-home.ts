import { CurrencyPipe, DatePipe, UpperCasePipe } from '@angular/common';
import { ChangeDetectionStrategy, Component, DestroyRef, computed, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { LucideArrowRight, LucideCalculator, LucideCheck, LucideMapPin, LucideScale, LucideShieldCheck, LucideSprout, LucideTruck } from '@lucide/angular';
import { timer } from 'rxjs';
import { switchMap } from 'rxjs/operators';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { ApiService } from '../../core/api.service';
import { PublicContent, PublicPrice } from '../../core/models';

const heroFallback: PublicContent = { id: '', contentKey: 'hero', section: 'Hero', eyebrow: 'El valor nace en el origen',
  title: 'Tu cacao vale más cuando el trato es claro.', subtitle: 'Pesamos frente a ti, explicamos cada descuento y pagamos con el precio publicado.',
  body: 'Sin letras pequeñas.', primaryCtaLabel: 'Calcular mi venta', primaryCtaUrl: '#precio', secondaryCtaLabel: 'Hablar por WhatsApp',
  displayOrder: 0, isPublished: true, updatedAtUtc: new Date().toISOString() };
const aboutFallback: PublicContent = { ...heroFallback, contentKey: 'about', section: 'Nosotros', eyebrow: 'Cómo trabajamos', title: 'De tu finca a un trato que sí se entiende.', subtitle: '', body: 'Coordinamos la entrega, medimos contigo y pagamos con criterios transparentes.' };

@Component({
  selector: 'app-public-home',
  imports: [CurrencyPipe, DatePipe, UpperCasePipe, FormsModule, RouterLink, LucideArrowRight, LucideCalculator,
    LucideCheck, LucideMapPin, LucideScale, LucideShieldCheck, LucideSprout, LucideTruck],
  templateUrl: './public-home.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class PublicHome {
  private readonly api = inject(ApiService);
  private readonly destroyRef = inject(DestroyRef);
  readonly loading = signal(true);
  readonly online = signal(false);
  readonly content = signal<PublicContent[]>([]);
  readonly price = signal<PublicPrice>({ businessName: 'Origen Cacao', dryPricePerQuintal: 300, wetPricePerQuintal: 120,
    marketPricePerMetricTon: 0, updatedAtUtc: new Date().toISOString(), source: 'Conectando con el centro de acopio', isManual: true,
    contactWhatsApp: '+593 99 000 0000', contactAddress: '', contactPhone: '', contactEmail: '', googleMapsEmbedUrl: '', location: 'Ecuador', pickupEnabled: true, nextAutomaticRefreshAtUtc: new Date().toISOString() });
  readonly amount = signal(1);
  readonly selectedState = signal<'Seco' | 'Baba'>('Seco');
  readonly selectedPrice = computed(() => this.selectedState() === 'Seco' ? this.price().dryPricePerQuintal : this.price().wetPricePerQuintal);
  readonly estimate = computed(() => Math.max(0, this.amount() || 0) * (this.selectedState() === 'Baba' ? this.selectedPrice() / 100 : this.selectedPrice()));
  readonly hero = computed(() => this.content().find(x => x.section === 'Hero') ?? heroFallback);
  readonly services = computed(() => this.content().filter(x => x.section === 'Servicio').slice(0, 6));
  readonly about = computed(() => this.content().find(x => x.section === 'Nosotros') ?? aboutFallback);
  readonly contact = computed(() => this.content().find(x => x.section === 'Contacto'));

  constructor() {
    timer(0, 10 * 60 * 1000).pipe(switchMap(() => this.api.publicPrice()), takeUntilDestroyed(this.destroyRef)).subscribe({
      next: value => { this.price.set(value); this.loading.set(false); this.online.set(true); },
      error: () => { this.loading.set(false); this.online.set(false); },
    });
    this.api.publicContent().subscribe({ next: value => this.content.set(value), error: () => this.content.set([]) });
  }

  selectState(state: 'Seco' | 'Baba') { this.selectedState.set(state); this.amount.set(state === 'Baba' ? 100 : 1); }
  updateAmount(value: number) { this.amount.set(Number(value)); }
  whatsappLink() {
    const phone = this.price().contactWhatsApp.replace(/\D/g, '');
    const unit = this.selectedState() === 'Baba' ? 'libras' : 'quintales';
    return `https://wa.me/${phone}?text=${encodeURIComponent(`Hola, tengo ${this.amount()} ${unit} de cacao ${this.selectedState().toLowerCase()} para vender.`)}`;
  }
}
