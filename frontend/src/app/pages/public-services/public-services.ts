import { UpperCasePipe } from '@angular/common';
import { ChangeDetectionStrategy, Component, computed, inject, signal } from '@angular/core';
import { Title } from '@angular/platform-browser';
import { RouterLink } from '@angular/router';
import {
  LucideArrowRight,
  LucideCheck,
  LucideLeaf,
  LucideMapPin,
  LucideMessageCircle,
  LucideScale,
  LucideShieldCheck,
  LucideSprout,
  LucideTruck,
} from '@lucide/angular';
import { catchError, forkJoin, of } from 'rxjs';
import { ApiService } from '../../core/api.service';
import { PublicContent, PublicPrice } from '../../core/models';

const content = (
  contentKey: string,
  section: PublicContent['section'],
  title: string,
  values: Partial<PublicContent> = {},
): PublicContent => ({
  id: contentKey,
  contentKey,
  section,
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

const serviceIntroFallback = content('servicio-intro', 'Servicio', 'Un aliado para hacer crecer cada cosecha.', {
  eyebrow: 'Servicios para productores',
  subtitle: 'Compra directa, manejo responsable y logística cercana para que cada lote conserve su valor.',
  body: 'Acompañamos el cacao desde la recepción hasta su preparación comercial con procesos claros, trazables y humanos.',
  imageUrl: '/cacao-pesaje-transparente.png',
  icon: 'intro',
});

const serviceFallbacks = [
  content('servicio-compra', 'Servicio', 'Recepción de cacao en baba y seco', { eyebrow: 'Compra directa', subtitle: 'Pesaje transparente y comprobante detallado.', body: 'Evaluamos humedad y merma, aplicamos el precio vigente y entregamos respaldo por cada compra.', icon: 'scale' }),
  content('servicio-secado', 'Servicio', 'Secado y manejo por lotes', { eyebrow: 'Valor agregado', subtitle: 'Control de rendimiento desde cacao en baba hasta cacao seco.', body: 'Conservamos la trazabilidad y el costo real de cada lote durante todo el proceso.', icon: 'shield', displayOrder: 1 }),
  content('servicio-recoleccion', 'Servicio', 'Coordinación de recolección', { eyebrow: 'Logística', subtitle: 'Consulta disponibilidad de retiro en finca.', body: 'Ayudamos a coordinar transporte cuando la zona y el volumen lo permiten.', icon: 'truck', displayOrder: 2 }),
];

const benefitIntroFallback = content('beneficio-intro', 'Beneficio', 'Claridad desde que llegas hasta que recibes tu pago', {
  eyebrow: 'Lo que puedes esperar',
  subtitle: 'Una compra bien hecha se nota en cada paso.',
  icon: 'intro',
});

const benefitFallbacks = [
  content('beneficio-precio', 'Beneficio', 'Sabes cuánto vale antes de vender', { eyebrow: 'Precio visible', subtitle: 'El marcador y la calculadora muestran el valor vigente.', body: 'Actualizamos la referencia y explicamos el cálculo aplicado.', icon: 'price' }),
  content('beneficio-peso', 'Beneficio', 'Pesaje y medición frente a ti', { eyebrow: 'Proceso abierto', subtitle: 'Sin pasos ocultos ni números difíciles de comprobar.', body: 'Revisamos peso, tara, humedad y merma contigo.', icon: 'scale', displayOrder: 1 }),
  content('beneficio-pago', 'Beneficio', 'Comprobante y pago acordado', { eyebrow: 'Cierre responsable', subtitle: 'Cada recepción queda respaldada y trazable.', body: 'Confirmamos el cálculo final antes de cerrar.', icon: 'shield', displayOrder: 2 }),
];

const processIntroFallback = content('proceso-intro', 'Proceso', 'De tu mensaje al pago, sin complicaciones', {
  eyebrow: 'Así trabajamos',
  subtitle: 'Un proceso corto, humano y verificable.',
  icon: 'intro',
});

const processFallbacks = [
  content('proceso-coordina', 'Proceso', 'Coordina tu entrega', { eyebrow: '01', subtitle: 'Cuéntanos tipo, estado y cantidad aproximada.', body: 'Confirmamos horario y opción de recolección.', icon: 'message' }),
  content('proceso-mide', 'Proceso', 'Medimos contigo', { eyebrow: '02', subtitle: 'Pesamos y comprobamos las condiciones.', body: 'Revisamos tara, humedad y merma de forma visible.', icon: 'scale', displayOrder: 1 }),
  content('proceso-recibe', 'Proceso', 'Recibe tu pago', { eyebrow: '03', subtitle: 'Te mostramos el cálculo y emitimos el comprobante.', body: 'El pago se realiza según el método acordado.', icon: 'check', displayOrder: 2 }),
];

const contactFallback = content('contacto-principal', 'Contacto', 'Trae tu cacao o coordina una visita', {
  eyebrow: 'Conversemos',
  subtitle: 'Atención directa para productores, exportadoras y chocolaterías.',
  body: 'Escríbenos para conocer el precio del día, horarios de recepción y opciones de transporte.',
  primaryCtaLabel: 'Abrir WhatsApp',
  primaryCtaUrl: '#whatsapp',
});

const footerFallback = content('footer-principal', 'Footer', 'Compra justa de cacao en Ecuador', {
  eyebrow: 'Grupo Álvarez',
  subtitle: 'Precio claro, peso exacto y relaciones que perduran.',
  body: 'Trabajamos junto a productores para convertir cada cosecha en una oportunidad de crecimiento compartido.',
});

@Component({
  selector: 'app-public-services',
  imports: [
    RouterLink,
    UpperCasePipe,
    LucideArrowRight,
    LucideCheck,
    LucideLeaf,
    LucideMapPin,
    LucideMessageCircle,
    LucideScale,
    LucideShieldCheck,
    LucideSprout,
    LucideTruck,
  ],
  templateUrl: './public-services.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class PublicServices {
  private readonly api = inject(ApiService);
  private readonly title = inject(Title);

  readonly loading = signal(true);
  readonly content = signal<PublicContent[]>([]);
  readonly price = signal<PublicPrice>(priceFallback);
  readonly currentYear = new Date().getFullYear();

  readonly serviceIntro = computed(() => this.intro('Servicio', serviceIntroFallback));
  readonly services = computed(() => this.cards('Servicio', serviceFallbacks));
  readonly benefitIntro = computed(() => this.intro('Beneficio', benefitIntroFallback));
  readonly benefits = computed(() => this.cards('Beneficio', benefitFallbacks));
  readonly processIntro = computed(() => this.intro('Proceso', processIntroFallback));
  readonly processSteps = computed(() => this.cards('Proceso', processFallbacks));
  readonly contact = computed(() => this.section('Contacto', [contactFallback])[0] ?? contactFallback);
  readonly footer = computed(() => this.section('Footer', [footerFallback])[0] ?? footerFallback);
  readonly heroImage = computed(() => this.serviceIntro().imageUrl?.trim() || this.services().find(item => item.imageUrl?.trim())?.imageUrl || '/cacao-pesaje-transparente.png');

  constructor() {
    forkJoin({
      content: this.api.publicContent().pipe(catchError(() => of([] as PublicContent[]))),
      price: this.api.publicPrice().pipe(catchError(() => of(priceFallback))),
    }).subscribe(({ content: blocks, price }) => {
      this.content.set(blocks);
      this.price.set(price);
      this.title.setTitle(`Servicios | ${price.businessName || priceFallback.businessName}`);
      this.loading.set(false);
    });
  }

  whatsappLink() {
    const phone = this.price().contactWhatsApp.replace(/\D/g, '');
    const message = 'Hola, quiero conocer los servicios para vender o manejar mi cacao.';
    return `https://wa.me/${phone}?text=${encodeURIComponent(message)}`;
  }

  resolveLink(url?: string) {
    if (!url || url === '#whatsapp') return this.whatsappLink();
    return url.startsWith('#') ? `/${url}` : url;
  }

  private section(section: PublicContent['section'], fallback: PublicContent[]) {
    const items = this.content().filter(item => item.section === section);
    return items.length ? items : fallback;
  }

  private cards(section: PublicContent['section'], fallback: PublicContent[]) {
    const items = this.content().filter(item => item.section === section && item.icon !== 'intro');
    return items.length ? items : fallback;
  }

  private intro(section: PublicContent['section'], fallback: PublicContent) {
    return this.content().find(item => item.section === section && item.icon === 'intro') ?? fallback;
  }
}
