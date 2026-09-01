import { ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { LucideCircleCheck, LucidePlus, LucideSave, LucideSettings, LucideTrash2, LucideX } from '@lucide/angular';
import { finalize } from 'rxjs';
import { ApiService } from '../../core/api.service';
import { PublicContent, PublicContentSection } from '../../core/models';

type ContentForm = Omit<PublicContent, 'updatedAtUtc'>;
const empty = (): ContentForm => ({ id: '', contentKey: '', section: 'Servicio', eyebrow: '', title: '', subtitle: '', body: '',
  primaryCtaLabel: '', primaryCtaUrl: '', secondaryCtaLabel: '', secondaryCtaUrl: '', icon: '', imageUrl: '', displayOrder: 0, isPublished: true });

@Component({ selector: 'app-site-content', imports: [FormsModule, LucideCircleCheck, LucidePlus, LucideSave, LucideSettings, LucideTrash2, LucideX], templateUrl: './site-content.html', changeDetection: ChangeDetectionStrategy.OnPush })
export class SiteContent {
  private readonly api = inject(ApiService);
  readonly items = signal<PublicContent[]>([]); readonly panelOpen = signal(false); readonly saving = signal(false);
  readonly error = signal(''); readonly success = signal(''); readonly sections: PublicContentSection[] = [
    'Hero', 'Nosotros', 'CarruselNosotros', 'Servicio', 'Beneficio', 'Proceso', 'Impacto', 'Testimonio', 'Galeria', 'Contacto', 'Footer'
  ];
  form = empty();
  constructor() { this.load(); }
  load() { this.api.adminPublicContent().subscribe({ next: x => this.items.set(x), error: () => this.error.set('No se pudo cargar el contenido público.') }); }
  add(section: PublicContentSection = 'Servicio') { this.form = { ...empty(), section }; this.error.set(''); this.panelOpen.set(true); }
  edit(item: PublicContent) { this.form = { ...item }; this.error.set(''); this.panelOpen.set(true); }
  save() {
    if (!this.form.contentKey.trim() || !this.form.title.trim()) { this.error.set('La clave y el título son obligatorios.'); return; }
    const { id, ...body } = this.form; this.saving.set(true);
    const request = id ? this.api.updatePublicContent(id, body) : this.api.createPublicContent(body);
    request.pipe(finalize(() => this.saving.set(false))).subscribe({ next: () => { this.panelOpen.set(false); this.success.set('Contenido publicado correctamente.'); this.load(); }, error: e => this.error.set(e?.error?.title ?? 'No se pudo guardar el contenido.') });
  }
  remove(item: Pick<PublicContent, 'id' | 'title'>) { if (!confirm(`¿Eliminar el bloque “${item.title}”?`)) return; this.api.deletePublicContent(item.id).subscribe({ next: () => { this.panelOpen.set(false); this.load(); }, error: e => this.error.set(e?.error?.title ?? 'No se pudo eliminar.') }); }
  sectionItems(section: PublicContentSection) { return this.items().filter(x => x.section === section); }
  sectionLabel(section: PublicContentSection) { return section === 'CarruselNosotros' ? 'Carrusel Nosotros' : section; }
  sectionHelp(section: PublicContentSection) {
    if (section === 'CarruselNosotros') return 'Solo cambia la fotografía de Nosotros; usa el orden para organizar las imágenes.';
    if (section === 'Servicio') return 'Alimenta las tarjetas y el encabezado de /servicios. Usa “intro” para el encabezado.';
    if (section === 'Contacto') return 'Alimenta la portada y /contacto. Mapa, teléfono, correo y WhatsApp se editan en Configuración.';
    return '';
  }
}
