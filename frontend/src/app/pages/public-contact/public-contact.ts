import { UpperCasePipe } from '@angular/common';
import { ChangeDetectionStrategy, Component, computed, inject, signal } from '@angular/core';
import { DomSanitizer, SafeResourceUrl, Title } from '@angular/platform-browser';
import { RouterLink } from '@angular/router';
import {
  LucideArrowRight,
  LucideCheck,
  LucideMail,
  LucideMapPin,
  LucideMessageCircle,
  LucidePhone,
  LucideShieldCheck,
  LucideTruck,
} from '@lucide/angular';
import { catchError, forkJoin, of } from 'rxjs';
import { ApiService } from '../../core/api.service';
import { PublicContent, PublicPrice } from '../../core/models';

const content = (contentKey: string, title: string, values: Partial<PublicContent> = {}): PublicContent => ({
  id: contentKey,
  contentKey,
  section: 'Contacto',
  eyebrow: '',
  title,
  subtitle: '',
  body: '',
  displayOrder: 0,
  isPublished: true,
  updatedAtUtc: new Date().toISOString(),
  ...values,
});

const priceFallback: PublicPrice = {
  businessName: 'Grupo Álvarez',
  logoUrl: '/grupo-alvarez-cacao-logo.png',
  priceClockLabel: 'Hora Ecuador',
  timeZone: 'America/Guayaquil',
  dryPricePerQuintal: 300,
  wetPricePerQuintal: 120,
  marketPricePerMetricTon: 0,
  updatedAtUtc: new Date().toISOString(),
  source: 'Precio de respaldo',
  isManual: true,
  contactWhatsApp: '+593 99 000 0000',
  contactAddress: '',
  contactPhone: '',
  contactEmail: '',
  googleMapsEmbedUrl: '',
  location: 'Babahoyo, Los Ríos, Ecuador',
  pickupEnabled: true,
  nextAutomaticRefreshAtUtc: new Date().toISOString(),
};

const contactFallbacks = [
  content('contacto-principal', 'Trae tu cacao o coordina una visita', {
    eyebrow: 'Conversemos',
    subtitle: 'Atención directa para productores, exportadoras y chocolaterías.',
    body: 'Escríbenos para conocer el precio del día, horarios de recepción y opciones de transporte.',
    primaryCtaLabel: 'Abrir WhatsApp',
    primaryCtaUrl: '#whatsapp',
    imageUrl: '/cacao-productores-alianza.png',
  }),
  content('contacto-productores', 'Atención para productores', {
    eyebrow: 'Compra de cacao',
    subtitle: 'Consulta precio, condiciones de recepción y disponibilidad de recolección.',
    body: 'Cuéntanos si tu cacao está en baba o seco y la cantidad aproximada para orientarte mejor.',
    primaryCtaLabel: 'Consultar por WhatsApp',
    primaryCtaUrl: '#whatsapp',
    icon: 'message',
    displayOrder: 1,
  }),
  content('contacto-comercial', 'Relaciones comerciales', {
    eyebrow: 'Empresas y aliados',
    subtitle: 'Atención para exportadoras, chocolaterías y compradores institucionales.',
    body: 'Conversemos sobre disponibilidad, trazabilidad, calidades y coordinación logística.',
    icon: 'shield',
    displayOrder: 2,
  }),
  content('contacto-visitas', 'Visitas y logística', {
    eyebrow: 'Centro de acopio',
    subtitle: 'Ubica el punto de atención y abre la ruta directamente en tu mapa.',
    body: 'Confirma el horario antes de trasladar tu cosecha para asegurar una recepción ágil.',
    icon: 'truck',
    displayOrder: 3,
  }),
];

const footerFallback: PublicContent = {
  ...content('footer-principal', 'Compra justa de cacao en Ecuador', {
    section: 'Footer',
    eyebrow: 'Grupo Álvarez',
    subtitle: 'Precio claro, peso exacto y relaciones que perduran.',
    body: 'Trabajamos junto a productores para convertir cada cosecha en una oportunidad de crecimiento compartido.',
  }),
};

@Component({
  selector: 'app-public-contact',
  imports: [
    RouterLink,
    UpperCasePipe,
    LucideArrowRight,
    LucideCheck,
    LucideMail,
    LucideMapPin,
    LucideMessageCircle,
    LucidePhone,
    LucideShieldCheck,
    LucideTruck,
  ],
  templateUrl: './public-contact.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class PublicContact {
  private readonly api = inject(ApiService);
  private readonly sanitizer = inject(DomSanitizer);
  private readonly title = inject(Title);

  readonly loading = signal(true);
  readonly content = signal<PublicContent[]>([]);
  readonly price = signal<PublicPrice>(priceFallback);
  readonly currentYear = new Date().getFullYear();

  readonly contactBlocks = computed(() => {
    const items = this.content().filter(item => item.section === 'Contacto');
    return items.length ? items : contactFallbacks;
  });
  readonly hero = computed(() => this.contactBlocks().find(item => item.contentKey === 'contacto-principal') ?? this.contactBlocks()[0] ?? contactFallbacks[0]);
  readonly cards = computed(() => {
    const hero = this.hero();
    const items = this.contactBlocks().filter(item => item.id !== hero.id && item.contentKey !== hero.contentKey);
    return items.length ? items : contactFallbacks.slice(1);
  });
  readonly footer = computed(() => this.content().find(item => item.section === 'Footer') ?? footerFallback);
  readonly address = computed(() => this.price().contactAddress?.trim() || this.price().location?.trim() || 'Babahoyo, Los Ríos, Ecuador');
  readonly displayPhone = computed(() => this.price().contactPhone?.trim() || this.price().contactWhatsApp?.trim());
  readonly mapUrl = computed<SafeResourceUrl>(() => {
    const configured = this.safeConfiguredMap(this.price().googleMapsEmbedUrl);
    if (configured) return this.sanitizer.bypassSecurityTrustResourceUrl(configured);
    const query = encodeURIComponent(this.address());
    return this.sanitizer.bypassSecurityTrustResourceUrl(`https://www.google.com/maps?q=${query}&output=embed`);
  });
  readonly directionsUrl = computed(() => `https://www.google.com/maps/search/?api=1&query=${encodeURIComponent(this.address())}`);

  constructor() {
    forkJoin({
      content: this.api.publicContent().pipe(catchError(() => of([] as PublicContent[]))),
      price: this.api.publicPrice().pipe(catchError(() => of(priceFallback))),
    }).subscribe(({ content: blocks, price }) => {
      this.content.set(blocks);
      this.price.set(price);
      this.title.setTitle(`Contacto | ${price.businessName || priceFallback.businessName}`);
      this.loading.set(false);
    });
  }

  whatsappLink() {
    const phone = this.price().contactWhatsApp.replace(/\D/g, '');
    const message = 'Hola, quiero coordinar la entrega o compra de cacao.';
    return `https://wa.me/${phone}?text=${encodeURIComponent(message)}`;
  }

  resolveLink(url?: string) {
    if (!url || url === '#whatsapp') return this.whatsappLink();
    return url.startsWith('#') ? `/${url}` : url;
  }

  private safeConfiguredMap(value: string | undefined) {
    const trimmed = value?.trim();
    if (!trimmed) return null;
    try {
      const url = new URL(trimmed);
      const googleHost = url.hostname === 'www.google.com' || url.hostname.endsWith('.google.com') || url.hostname.startsWith('maps.google.');
      return url.protocol === 'https:' && googleHost && (url.pathname === '/maps' || url.pathname.includes('/maps/')) ? trimmed : null;
    } catch {
      return null;
    }
  }
}
