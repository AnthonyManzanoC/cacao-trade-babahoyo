import { CurrencyPipe, DatePipe, UpperCasePipe } from '@angular/common';
import { ChangeDetectionStrategy, Component, DestroyRef, computed, inject, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import {
  LucideArrowRight,
  LucideBadgeDollarSign,
  LucideCalculator,
  LucideCheck,
  LucideChevronLeft,
  LucideChevronRight,
  LucideClock3,
  LucideLeaf,
  LucideMapPin,
  LucideMenu,
  LucideMessageCircle,
  LucideQuote,
  LucideScale,
  LucideShieldCheck,
  LucideSprout,
  LucideTruck,
  LucideX,
} from '@lucide/angular';
import { timer } from 'rxjs';
import { switchMap } from 'rxjs/operators';
import { ApiService } from '../../core/api.service';
import { BrandService } from '../../core/brand.service';
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

const heroFallbacks = [
  content('hero-principal', 'Hero', 'Tu cacao vale más cuando el trato es claro.', {
    eyebrow: 'El valor nace en el origen',
    subtitle: 'Pesamos frente a ti, explicamos cada descuento y pagamos con el precio publicado.',
    body: 'Sin letras pequeñas.',
    primaryCtaLabel: 'Calcular mi venta',
    primaryCtaUrl: '#precio',
    secondaryCtaLabel: 'Hablar por WhatsApp',
    secondaryCtaUrl: '#whatsapp',
    imageUrl: '/cacao-hero.png',
  }),
  content('hero-pesaje', 'Hero', 'Cada libra se pesa contigo, cada valor se explica.', {
    eyebrow: 'Peso transparente',
    subtitle: 'Recibimos cacao en baba y seco con criterios visibles, comprobante y atención directa.',
    body: 'Tú ves el proceso completo.',
    primaryCtaLabel: 'Ver precio de hoy',
    primaryCtaUrl: '#precio',
    secondaryCtaLabel: 'Cómo trabajamos',
    secondaryCtaUrl: '#proceso',
    imageUrl: '/cacao-pesaje-transparente.png',
  }),
  content('hero-alianza', 'Hero', 'Una alianza que reconoce el trabajo detrás de cada cosecha.', {
    eyebrow: 'Crecemos desde el campo',
    subtitle: 'Compra responsable, logística cercana y relaciones construidas lote a lote.',
    body: 'Grupo Álvarez conecta origen, confianza y futuro.',
    primaryCtaLabel: 'Conócenos',
    primaryCtaUrl: '#nosotros',
    secondaryCtaLabel: 'Coordinar entrega',
    secondaryCtaUrl: '#whatsapp',
    imageUrl: '/cacao-productores-alianza.png',
  }),
];

const aboutFallback = content('nosotros-historia', 'Nosotros', 'Del campo ecuatoriano a relaciones que perduran', {
  eyebrow: 'Nuestra razón de ser',
  subtitle: 'Somos una empresa familiar que compra cacao con cercanía, respeto y visión de futuro.',
  body: 'Trabajamos junto a productores de la costa ecuatoriana con pesaje transparente, trazabilidad por lote y pago responsable. Nuestro crecimiento empieza cuando la cosecha de cada familia recibe un trato justo.',
  imageUrl: '/cacao-productores-alianza.png',
  primaryCtaLabel: 'Conoce nuestra forma de trabajar',
  primaryCtaUrl: '#proceso',
  secondaryCtaLabel: 'Crecemos cuando el productor también crece.',
});

const serviceFallbacks = [
  content('servicio-compra', 'Servicio', 'Recepción de cacao en baba y seco', { eyebrow: 'Compra directa', subtitle: 'Pesaje transparente y comprobante detallado.', body: 'Evaluamos humedad y merma, aplicamos el precio vigente y entregamos respaldo por cada compra.', icon: 'scale' }),
  content('servicio-secado', 'Servicio', 'Secado y manejo por lotes', { eyebrow: 'Valor agregado', subtitle: 'Control de rendimiento de baba a seco.', body: 'Conservamos la trazabilidad y el costo real de cada lote durante el proceso.', icon: 'sun', displayOrder: 1 }),
  content('servicio-recoleccion', 'Servicio', 'Coordinación de recolección', { eyebrow: 'Logística', subtitle: 'Consulta disponibilidad de retiro en finca.', body: 'Ayudamos a coordinar transporte cuando la zona y el volumen lo permiten.', icon: 'truck', displayOrder: 2 }),
];

const benefitFallbacks = [
  content('beneficio-precio', 'Beneficio', 'Sabes cuánto vale antes de vender', { eyebrow: 'Precio visible', subtitle: 'El marcador y la calculadora muestran el valor vigente.', body: 'Actualizamos la referencia y explicamos el cálculo aplicado.', icon: 'price' }),
  content('beneficio-peso', 'Beneficio', 'Pesaje y medición frente a ti', { eyebrow: 'Proceso abierto', subtitle: 'Sin pasos ocultos ni números difíciles de comprobar.', body: 'Revisamos peso, tara, humedad y merma contigo.', icon: 'scale', displayOrder: 1 }),
  content('beneficio-pago', 'Beneficio', 'Comprobante y pago acordado', { eyebrow: 'Cierre responsable', subtitle: 'Cada recepción queda respaldada y trazable.', body: 'Confirmamos el cálculo final antes de cerrar.', icon: 'shield', displayOrder: 2 }),
];

const processFallbacks = [
  content('proceso-coordina', 'Proceso', 'Coordina tu entrega', { eyebrow: '01', subtitle: 'Cuéntanos tipo, estado y cantidad aproximada.', body: 'Confirmamos horario y opción de recolección.', icon: 'message' }),
  content('proceso-mide', 'Proceso', 'Medimos contigo', { eyebrow: '02', subtitle: 'Pesamos y comprobamos las condiciones.', body: 'Revisamos tara, humedad y merma de forma visible.', icon: 'scale', displayOrder: 1 }),
  content('proceso-recibe', 'Proceso', 'Recibe tu pago', { eyebrow: '03', subtitle: 'Te mostramos el cálculo y emitimos el comprobante.', body: 'El pago se realiza según el método acordado.', icon: 'check', displayOrder: 2 }),
];

const impactFallbacks = [
  content('impacto-productores', 'Impacto', 'Productores aliados', { eyebrow: '+120', subtitle: 'Relaciones directas y cercanas.', icon: 'sprout' }),
  content('impacto-trazabilidad', 'Impacto', 'Compras trazables', { eyebrow: '100%', subtitle: 'Cada lote conserva su historia.', icon: 'shield', displayOrder: 1 }),
  content('impacto-pago', 'Impacto', 'Pago responsable', { eyebrow: 'Mismo día', subtitle: 'Cierre claro y comprobante.', icon: 'check', displayOrder: 2 }),
];

const testimonialFallbacks = [
  content('testimonio-ana', 'Testimonio', 'Ana M. · Los Ríos', { eyebrow: 'Productora aliada', subtitle: 'Ahora conozco el cálculo antes de entregar.', body: 'Me explicaron el peso y la humedad con calma. Salí con mi comprobante y el pago acordado.' }),
  content('testimonio-carlos', 'Testimonio', 'Carlos V. · Babahoyo', { eyebrow: 'Productor aliado', subtitle: 'La transparencia hace que uno vuelva.', body: 'El precio estaba publicado y todo se revisó frente a mí. Así se construye confianza.', displayOrder: 1 }),
  content('testimonio-luisa', 'Testimonio', 'Luisa P. · Vinces', { eyebrow: 'Familia productora', subtitle: 'Nos atendieron como socios del proceso.', body: 'Coordinamos la entrega por WhatsApp y al llegar ya sabían qué lote recibiríamos.', displayOrder: 2 }),
];

const galleryFallbacks = [
  content('galeria-origen', 'Galeria', 'Cacao que nace en nuestra tierra', { eyebrow: 'Origen', subtitle: 'Selección de mazorcas frescas.', imageUrl: '/cacao-hero.png' }),
  content('galeria-pesaje', 'Galeria', 'Peso que se comprueba', { eyebrow: 'Transparencia', subtitle: 'La medición se realiza contigo.', imageUrl: '/cacao-pesaje-transparente.png', displayOrder: 1 }),
  content('galeria-alianza', 'Galeria', 'Cosechas que crean futuro', { eyebrow: 'Comunidad', subtitle: 'Relaciones construidas en el campo.', imageUrl: '/cacao-productores-alianza.png', displayOrder: 2 }),
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
  selector: 'app-public-home',
  imports: [
    CurrencyPipe,
    DatePipe,
    UpperCasePipe,
    FormsModule,
    RouterLink,
    LucideArrowRight,
    LucideBadgeDollarSign,
    LucideCalculator,
    LucideCheck,
    LucideChevronLeft,
    LucideChevronRight,
    LucideClock3,
    LucideLeaf,
    LucideMapPin,
    LucideMenu,
    LucideMessageCircle,
    LucideQuote,
    LucideScale,
    LucideShieldCheck,
    LucideSprout,
    LucideTruck,
    LucideX,
  ],
  templateUrl: './public-home.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class PublicHome {
  private readonly api = inject(ApiService);
  private readonly brand = inject(BrandService);
  private readonly destroyRef = inject(DestroyRef);

  readonly loading = signal(true);
  readonly online = signal(false);
  readonly menuOpen = signal(false);
  readonly content = signal<PublicContent[]>([]);
  readonly now = signal(new Date());
  readonly activeHero = signal(0);
  readonly activeTestimonial = signal(0);
  readonly activeGallery = signal(0);
  readonly currentYear = new Date().getFullYear();
  readonly price = signal<PublicPrice>({
    businessName: 'Grupo Álvarez',
    logoUrl: '/grupo-alvarez-cacao-logo.png',
    priceClockLabel: 'Hora Ecuador',
    timeZone: 'America/Guayaquil',
    dryPricePerQuintal: 300,
    wetPricePerQuintal: 120,
    marketPricePerMetricTon: 0,
    updatedAtUtc: new Date().toISOString(),
    source: 'Conectando con el centro de acopio',
    isManual: true,
    contactWhatsApp: '+593 99 000 0000',
    contactAddress: '',
    contactPhone: '',
    contactEmail: '',
    googleMapsEmbedUrl: '',
    location: 'Ecuador',
    pickupEnabled: true,
    nextAutomaticRefreshAtUtc: new Date().toISOString(),
  });

  readonly amount = signal(1);
  readonly selectedState = signal<'Seco' | 'Baba'>('Seco');
  readonly selectedPrice = computed(() => this.selectedState() === 'Seco' ? this.price().dryPricePerQuintal : this.price().wetPricePerQuintal);
  readonly estimate = computed(() => Math.max(0, this.amount() || 0) * (this.selectedState() === 'Baba' ? this.selectedPrice() / 100 : this.selectedPrice()));

  readonly heroSlides = computed(() => this.section('Hero', heroFallbacks));
  readonly hero = computed(() => this.heroSlides()[this.activeHero() % this.heroSlides().length] ?? heroFallbacks[0]);
  readonly about = computed(() => this.section('Nosotros', [aboutFallback])[0] ?? aboutFallback);
  readonly serviceIntro = computed(() => this.intro('Servicio', content('servicio-intro', 'Servicio', 'Un aliado para hacer crecer cada cosecha.', { eyebrow: 'Más que comprar cacao', subtitle: 'Acompañamos a pequeños productores con procesos claros, logística cercana y una relación que se construye lote a lote.' })));
  readonly services = computed(() => this.cards('Servicio', serviceFallbacks).slice(0, 6));
  readonly benefitIntro = computed(() => this.intro('Beneficio', content('beneficio-intro', 'Beneficio', 'Claridad desde que llegas hasta que recibes tu pago', { eyebrow: 'Lo que puedes esperar', subtitle: 'Una compra bien hecha se nota en cada paso.' })));
  readonly benefits = computed(() => this.cards('Beneficio', benefitFallbacks));
  readonly processIntro = computed(() => this.intro('Proceso', content('proceso-intro', 'Proceso', 'De tu mensaje al pago, sin complicaciones', { eyebrow: 'Así trabajamos', subtitle: 'Un proceso corto, humano y verificable.' })));
  readonly processSteps = computed(() => this.cards('Proceso', processFallbacks));
  readonly impactIntro = computed(() => this.intro('Impacto', content('impacto-intro', 'Impacto', 'Crecer con el campo, cuidar cada relación', { eyebrow: 'Impacto con propósito', subtitle: 'Metas que reflejan el avance real de nuestra operación.' })));
  readonly impactStats = computed(() => this.cards('Impacto', impactFallbacks));
  readonly trustItems = computed(() => [...this.benefits().slice(0, 3), this.services().find(item => item.icon === 'truck') ?? serviceFallbacks[2]]);
  readonly testimonialIntro = computed(() => this.intro('Testimonio', content('testimonio-intro', 'Testimonio', 'La confianza se cultiva en cada compra.', { eyebrow: 'Voces del campo', subtitle: 'Historias que muestran cómo se vive nuestro proceso.' })));
  readonly testimonials = computed(() => this.cards('Testimonio', testimonialFallbacks));
  readonly testimonial = computed(() => this.testimonials()[this.activeTestimonial() % this.testimonials().length] ?? testimonialFallbacks[0]);
  readonly galleryIntro = computed(() => this.intro('Galeria', content('galeria-intro', 'Galeria', 'Historias del origen', { eyebrow: 'Nuestro cacao, nuestra gente', subtitle: 'Una mirada al trabajo y las relaciones detrás de cada lote.' })));
  readonly gallery = computed(() => this.cards('Galeria', galleryFallbacks));
  readonly visibleGallery = computed(() => {
    const items = this.gallery();
    return items.map((_, index) => items[(this.activeGallery() + index) % items.length]).slice(0, 3);
  });
  readonly contact = computed(() => this.section('Contacto', [contactFallback])[0] ?? contactFallback);
  readonly footerContent = computed(() => this.section('Footer', [footerFallback])[0] ?? footerFallback);
  readonly clockTime = computed(() => this.formatClock({ hour: '2-digit', minute: '2-digit', second: '2-digit' }));
  readonly clockDate = computed(() => this.formatClock({ weekday: 'short', day: '2-digit', month: 'short' }));

  constructor() {
    timer(0, 10 * 60 * 1000).pipe(
      switchMap(() => this.api.publicPrice()),
      takeUntilDestroyed(this.destroyRef),
    ).subscribe({
      next: value => {
        this.price.set(value);
        this.brand.set({ businessName: value.businessName, logoUrl: value.logoUrl });
        this.loading.set(false);
        this.online.set(true);
      },
      error: () => { this.loading.set(false); this.online.set(false); },
    });
    this.api.publicContent().pipe(takeUntilDestroyed(this.destroyRef)).subscribe({
      next: value => this.content.set(value),
      error: () => this.content.set([]),
    });
    timer(0, 1000).pipe(takeUntilDestroyed(this.destroyRef)).subscribe(() => this.now.set(new Date()));
    timer(8000, 8000).pipe(takeUntilDestroyed(this.destroyRef)).subscribe(() => this.moveHero(1));
  }

  selectState(state: 'Seco' | 'Baba') {
    this.selectedState.set(state);
    this.amount.set(state === 'Baba' ? 100 : 1);
  }

  updateAmount(value: number) { this.amount.set(Number(value)); }
  moveHero(direction: number) { this.activeHero.update(value => this.wrap(value + direction, this.heroSlides().length)); }
  selectHero(index: number) { this.activeHero.set(index); }
  moveTestimonial(direction: number) { this.activeTestimonial.update(value => this.wrap(value + direction, this.testimonials().length)); }
  moveGallery(direction: number) { this.activeGallery.update(value => this.wrap(value + direction, this.gallery().length)); }

  whatsappLink() {
    const phone = this.price().contactWhatsApp.replace(/\D/g, '');
    const unit = this.selectedState() === 'Baba' ? 'libras' : 'quintales';
    return `https://wa.me/${phone}?text=${encodeURIComponent(`Hola, tengo ${this.amount()} ${unit} de cacao ${this.selectedState().toLowerCase()} para vender.`)}`;
  }

  resolveLink(url?: string) {
    return !url || url === '#whatsapp' ? this.whatsappLink() : url;
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

  private wrap(value: number, length: number) {
    return length ? (value + length) % length : 0;
  }

  private formatClock(options: Intl.DateTimeFormatOptions) {
    try {
      return new Intl.DateTimeFormat('es-EC', { ...options, timeZone: this.price().timeZone || 'America/Guayaquil' }).format(this.now());
    } catch {
      return new Intl.DateTimeFormat('es-EC', { ...options, timeZone: 'America/Guayaquil' }).format(this.now());
    }
  }
}
