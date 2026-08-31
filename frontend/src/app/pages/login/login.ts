import { ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { LucideArrowLeft, LucideEye, LucideEyeOff, LucideLockKeyhole, LucideSprout } from '@lucide/angular';
import { finalize } from 'rxjs';
import { AuthService } from '../../core/auth.service';
import { ApiService } from '../../core/api.service';

@Component({ selector: 'app-login', imports: [FormsModule, RouterLink, LucideArrowLeft, LucideEye, LucideEyeOff, LucideLockKeyhole, LucideSprout], templateUrl: './login.html', changeDetection: ChangeDetectionStrategy.OnPush })
export class Login {
  private readonly auth = inject(AuthService); private readonly router = inject(Router); private readonly api = inject(ApiService);
  readonly businessName = signal('Origen Cacao');
  email = 'admin@cacao.local'; password = 'CacaoLocal2026!'; readonly show = signal(false); readonly loading = signal(false); readonly error = signal('');
  constructor() { this.api.publicPrice().subscribe({ next: x => this.businessName.set(x.businessName) }); }
  submit() { this.error.set(''); this.loading.set(true); this.auth.login(this.email, this.password).pipe(finalize(() => this.loading.set(false))).subscribe({ next: () => void this.router.navigate(['/admin/dashboard']), error: error => this.error.set(error?.error?.title ?? 'No pudimos iniciar sesión. Revisa que la API esté activa.') }); }
}
