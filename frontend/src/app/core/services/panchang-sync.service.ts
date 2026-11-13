import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import {
  PanchangIntegrationSettingsView,
  PanchangSyncSummary,
  UpdatePanchangIntegrationsRequest
} from '../models/ecosystem.models';

@Injectable({ providedIn: 'root' })
export class PanchangSyncService {
  private http = inject(HttpClient);
  private readonly adminBase = `${environment.apiUrl}/admin`;

  syncEcosystem(): Observable<PanchangSyncSummary> {
    return this.http.post<PanchangSyncSummary>(`${this.adminBase}/sync/ecosystem`, {});
  }

  syncVideos(): Observable<PanchangSyncSummary> {
    return this.http.post<PanchangSyncSummary>(`${this.adminBase}/sync/videos`, {});
  }

  purgeLegacyVideos(): Observable<{ removed: number }> {
    return this.http.post<{ removed: number }>(`${this.adminBase}/purge-legacy-videos`, {});
  }

  getIntegrations(): Observable<PanchangIntegrationSettingsView> {
    return this.http.get<PanchangIntegrationSettingsView>(`${this.adminBase}/integrations`);
  }

  updateIntegrations(req: UpdatePanchangIntegrationsRequest): Observable<PanchangIntegrationSettingsView> {
    return this.http.put<PanchangIntegrationSettingsView>(`${this.adminBase}/integrations`, req);
  }

  exportBundle(): Observable<unknown> {
    return this.http.get(`${this.adminBase}/export`);
  }

  importBundle(bundle: unknown): Observable<{ imported: number }> {
    return this.http.post<{ imported: number }>(`${this.adminBase}/import`, bundle);
  }
}
