import { UpperCasePipe } from '@angular/common';
import { ChangeDetectionStrategy, Component, computed, inject, signal } from '@angular/core';
import { DomSanitizer } from '@angular/platform-browser';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { LucideArrowRight, LucideMapPin, LucideScale, LucideShieldCheck, LucideSprout, LucideTruck } from '@lucide/angular';
import { forkJoin } from 'rxjs';
import { ApiService } from '../../core/api.service';
import { PublicContent, PublicContentSection, PublicPrice } from '../../core/models';

@Component({ selector: 'app-public-info', imports: [RouterLink, UpperCasePipe, LucideArrowRight, LucideMapPin, LucideScale, LucideShieldCheck, LucideSprout, LucideTruck], templateUrl: './public-info.html', changeDetection: ChangeDetectionStrategy.OnPush })
export class PublicInfo {
  private readonly api = inject(ApiService); private readonly route = inject(ActivatedRoute); private readonly sanitizer = inject(DomSanitizer);
  readonly section = this.route.snapshot.data['section'] as PublicContentSection;
  readonly blocks = signal<PublicContent[]>([]); readonly price = signal<PublicPrice | null>(null); readonly loading = signal(true);
  readonly mapUrl = computed(() => { const value = this.price()?.googleMapsEmbedUrl?.trim(); if (!value) return null; try { const url = new URL(value); const allowed = url.protocol === 'https:' && (url.hostname === 'www.google.com' || url.hostname.endsWith('.google.com') || url.hostname.startsWith('maps.google.')); return allowed && url.pathname.includes('/maps/') ? this.sanitizer.bypassSecurityTrustResourceUrl(value) : null; } catch { return null; } });
  constructor() { forkJoin({ blocks: this.api.publicContent(this.section), price: this.api.publicPrice() }).subscribe({ next: x => { this.blocks.set(x.blocks); this.price.set(x.price); this.loading.set(false); }, error: () => this.loading.set(false) }); }
  whatsapp() { const phone = this.price()?.contactWhatsApp.replace(/\D/g, '') ?? ''; return `https://wa.me/${phone}`; }
  sectionTitle() { return this.section === 'Servicio' ? 'Servicios para la cadena del cacao' : this.section === 'Nosotros' ? 'Una relación que comienza en el origen' : 'Hablemos de tu próxima cosecha'; }
}
