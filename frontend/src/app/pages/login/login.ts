import { ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { LucideArrowLeft, LucideEye, LucideEyeOff, LucideLockKeyhole } from '@lucide/angular';
import { finalize } from 'rxjs';
import { AuthService } from '../../core/auth.service';
import { BrandService } from '../../core/brand.service';

@Component({ selector: 'app-login', imports: [FormsModule, RouterLink, LucideArrowLeft, LucideEye, LucideEyeOff, LucideLockKeyhole], templateUrl: './login.html', changeDetection: ChangeDetectionStrategy.OnPush })
export class Login {
  private readonly auth = inject(AuthService); private readonly router = inject(Router);
  readonly identity = inject(BrandService).identity;
  email = 'admin@cacao.local'; password = 'CacaoLocal2026!'; readonly show = signal(false); readonly loading = signal(false); readonly error = signal('');
  submit() { this.error.set(''); this.loading.set(true); this.auth.login(this.email, this.password).pipe(finalize(() => this.loading.set(false))).subscribe({ next: () => void this.router.navigate(['/admin/dashboard']), error: error => this.error.set(error?.error?.title ?? 'No pudimos iniciar sesión. Revisa que la API esté activa.') }); }
}
