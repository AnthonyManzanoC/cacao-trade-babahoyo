import { Injectable, inject, signal } from '@angular/core';
import { ApiService } from './api.service';

export interface BrandIdentity {
  businessName: string;
  logoUrl: string;
}

@Injectable({ providedIn: 'root' })
export class BrandService {
  private readonly api = inject(ApiService);
  readonly identity = signal<BrandIdentity>({
    businessName: 'Grupo Álvarez',
    logoUrl: '/grupo-alvarez-cacao-logo.png',
  });

  constructor() {
    this.refresh();
  }

  refresh() {
    this.api.publicPrice().subscribe({
      next: value => this.set({ businessName: value.businessName, logoUrl: value.logoUrl }),
      error: () => undefined,
    });
  }

  set(value: BrandIdentity) {
    const identity = {
      businessName: value.businessName?.trim() || 'Grupo Álvarez',
      logoUrl: value.logoUrl?.trim() || '/grupo-alvarez-cacao-logo.png',
    };
    this.identity.set(identity);
    document.title = `${identity.businessName} | Compra justa de cacao en Ecuador`;
    const icon = document.querySelector<HTMLLinkElement>('link[rel="icon"]');
    if (icon) icon.href = identity.logoUrl;
  }
}
