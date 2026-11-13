import { Injectable, computed, inject, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { catchError, of } from 'rxjs';
import { environment } from '../../../environments/environment';
import { AppConfigEnvelope, EcosystemWebsite } from '../models/panchang.models';
import { CentralChannelItem } from '../models/ecosystem.models';

interface ExtendedConfigEnvelope extends AppConfigEnvelope {
  siteTitle?: string;
  channels?: CentralChannelItem[];
}

@Injectable({ providedIn: 'root' })
export class EcosystemConfigService {
  private http = inject(HttpClient);
  private loaded = false;

  readonly siteTitle = signal<string>('');
  readonly values = signal<Record<string, string>>({});
  readonly websites = signal<EcosystemWebsite[]>([]);
  readonly channels = signal<CentralChannelItem[]>([]);

  readonly ecosystemBarSites = computed(() =>
    this.websites()
      .filter(w => w.showInEcosystemBar)
      .sort((a, b) => a.sortOrder - b.sortOrder)
  );

  readonly footerSites = computed(() =>
    this.websites()
      .filter(w => ['studio', 'social', 'panchang', 'admin', 'blog', 'pratiyogita'].includes(w.key))
      .sort((a, b) => a.sortOrder - b.sortOrder)
  );

  readonly enabledChannels = computed(() =>
    this.channels()
      .filter(c => c.isEnabled !== false)
      .sort((a, b) => (a.sortOrder ?? 0) - (b.sortOrder ?? 0))
  );

  readonly panchangChannels = computed(() => {
    const flagged = this.enabledChannels().filter(c => c.showInPanchang);
    if (flagged.length > 0) return flagged;
    return this.enabledChannels().filter(c => (c.platform || '').toLowerCase() === 'youtube');
  });

  readonly footerChannels = computed(() =>
    this.enabledChannels().filter(c => c.showInFooter)
  );

  readonly creatorName = computed(() =>
    this.values()['ecosystem.creatorName'] || this.siteTitle() || 'Panchang Network'
  );

  readonly copyrightNotice = computed(() =>
    this.values()['ecosystem.copyrightNotice'] || ''
  );

  readonly youtubeChannelName = computed(() =>
    this.panchangChannels()[0]?.name || this.values()['ecosystem.youtubeChannelName'] || ''
  );

  readonly youtubeChannelUrl = computed(() =>
    this.panchangChannels()[0]?.url || this.values()['ecosystem.youtubeChannelUrl'] || ''
  );

  readonly adminPortalUrl = computed(() =>
    this.values()['ecosystem.adminUrl'] ||
    this.websites().find(w => w.key === 'admin')?.url ||
    environment.loginUrl.replace(/\/login\/?$/, '')
  );

  readonly studioPortalUrl = computed(() =>
    this.websites().find(w => w.key === 'studio')?.url || ''
  );

  readonly socialPortalUrl = computed(() =>
    this.websites().find(w => w.key === 'social')?.url || ''
  );

  load(): void {
    if (this.loaded) return;
    this.loaded = true;
    this.fetchConfig();
  }

  refresh(): void {
    this.fetchConfig();
  }

  private fetchConfig(): void {
    this.http.get<ExtendedConfigEnvelope>(`${environment.idpUrl}/config`).pipe(
      catchError(() => of(null))
    ).subscribe(res => {
      if (!res) return;
      if (res.siteTitle) this.siteTitle.set(res.siteTitle);
      if (res.values) this.values.set(res.values);
      if (res.websites?.length) this.websites.set(res.websites);
      if (res.channels) this.channels.set(res.channels);
    });
  }
}
