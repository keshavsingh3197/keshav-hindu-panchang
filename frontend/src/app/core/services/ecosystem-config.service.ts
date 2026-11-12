import { Injectable, computed, inject, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { catchError, of, shareReplay, tap } from 'rxjs';
import { environment } from '../../../environments/environment';
import { AppConfigEnvelope, EcosystemWebsite } from '../models/panchang.models';

const FALLBACK_WEBSITES: EcosystemWebsite[] = [
  { key: 'studio', name: 'Studio AI', url: 'https://studio.keshavsingh.in', icon: '🎬', sortOrder: 10, showInEcosystemBar: true },
  { key: 'social', name: 'Social Analytics', url: 'https://social.keshavsingh.in', icon: '📊', sortOrder: 20, showInEcosystemBar: true },
  { key: 'panchang', name: 'Hindu Panchang', url: 'https://panchang.keshavsingh.in', icon: '🕉️', sortOrder: 30, showInEcosystemBar: true },
  { key: 'ghar-ledger', name: 'Ghar Ledger', url: 'https://ghar.keshavsingh.in', icon: '💰', sortOrder: 40, showInEcosystemBar: true },
  { key: 'pratiyogita', name: 'Pratiyogita', url: 'https://pratiyogita.keshavsingh.in', icon: '🏆', sortOrder: 50, showInEcosystemBar: true },
  { key: 'blog', name: 'Content Blog', url: 'https://git.keshavsingh.in', icon: '✍️', sortOrder: 60, showInEcosystemBar: true },
  { key: 'admin', name: 'Admin & SSO', url: 'https://admin.keshavsingh.in', icon: '🛡️', sortOrder: 90, showInEcosystemBar: true }
];

@Injectable({ providedIn: 'root' })
export class EcosystemConfigService {
  private http = inject(HttpClient);
  private loadOnce$?: ReturnType<HttpClient['get']>;

  readonly websites = signal<EcosystemWebsite[]>(FALLBACK_WEBSITES);
  readonly values = signal<Record<string, string>>({});

  readonly ecosystemBarSites = computed(() =>
    this.websites()
      .filter(w => w.showInEcosystemBar !== false)
      .sort((a, b) => a.sortOrder - b.sortOrder)
  );

  readonly footerSites = computed(() =>
    this.websites()
      .filter(w => ['studio', 'social', 'admin', 'blog', 'portfolio'].includes(w.key))
      .sort((a, b) => a.sortOrder - b.sortOrder)
  );

  readonly creatorName = computed(() =>
    this.values()['ecosystem.creatorName'] || 'Keshav Singh'
  );

  readonly copyrightNotice = computed(() =>
    this.values()['ecosystem.copyrightNotice'] ||
    '© 2026 Keshav Singh. Part of the keshavsingh.in ecosystem. All rights reserved.'
  );

  readonly youtubeChannelName = computed(() =>
    this.values()['ecosystem.youtubeChannelName'] || 'Keshav Singh Official'
  );

  readonly youtubeChannelUrl = computed(() =>
    this.values()['ecosystem.youtubeChannelUrl'] || 'https://www.youtube.com/@KeshavSingh'
  );

  readonly dailyBroadcastNote = computed(() =>
    this.values()['panchang.dailyBroadcastNote'] || ''
  );

  readonly adminPortalUrl = computed(() =>
    this.websites().find(w => w.key === 'admin')?.url || 'https://admin.keshavsingh.in'
  );

  readonly studioPortalUrl = computed(() =>
    this.websites().find(w => w.key === 'studio')?.url || 'https://studio.keshavsingh.in'
  );

  load(): void {
    if (!this.loadOnce$) {
      this.loadOnce$ = this.http.get<AppConfigEnvelope>(`${environment.idpUrl}/config`).pipe(
        tap(env => {
          if (env.websites?.length) {
            this.websites.set([...env.websites].sort((a, b) => a.sortOrder - b.sortOrder));
          }
          if (env.values) {
            this.values.set(env.values);
          }
        }),
        catchError(() => of(null)),
        shareReplay(1)
      );
      this.loadOnce$.subscribe();
    }
  }
}
