import { Injectable, computed, signal } from '@angular/core';
import { Router } from '@angular/router';
import { tap } from 'rxjs';
import { ApiService } from './api.service';

interface Session { token: string; expiresAtUtc: string; fullName: string; email: string; }

@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly storageKey = 'origen_cacao_session';
  private readonly session = signal<Session | null>(this.read());
  readonly user = computed(() => this.session());
  readonly isAuthenticated = computed(() => !!this.session() && new Date(this.session()!.expiresAtUtc) > new Date());

  constructor(private api: ApiService, private router: Router) {}
  login(email: string, password: string) { return this.api.login(email, password).pipe(tap(session => { localStorage.setItem(this.storageKey, JSON.stringify(session)); this.session.set(session); })); }
  token() { return this.session()?.token ?? null; }
  logout() { localStorage.removeItem(this.storageKey); this.session.set(null); void this.router.navigate(['/admin/login']); }
  private read(): Session | null { try { return JSON.parse(localStorage.getItem(this.storageKey) ?? 'null'); } catch { return null; } }
}
