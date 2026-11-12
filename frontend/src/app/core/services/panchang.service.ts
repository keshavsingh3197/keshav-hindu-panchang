import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, catchError, of } from 'rxjs';
import { environment } from '../../../environments/environment';
import {
  DailyKatha,
  DailySutra,
  DevotionalKitItem,
  FestivalItem,
  HistoricalIncident,
  PanchangAdminStatus,
  PanchangCity,
  PanchangData,
  PanchangOverride,
  PanchangSiteConfig
} from '../models/panchang.models';
import { SpiritualVideo } from '../models/video.models';

@Injectable({ providedIn: 'root' })
export class PanchangService {
  private http = inject(HttpClient);
  private api = environment.apiUrl;

  getDailyPanchang(date?: string, location: string = 'New Delhi, India'): Observable<PanchangData> {
    const query = date
      ? `?date=${encodeURIComponent(date)}&location=${encodeURIComponent(location)}`
      : `?location=${encodeURIComponent(location)}`;
    return this.http.get<PanchangData>(`${this.api}/panchang/daily${query}`);
  }

  getCities(): Observable<PanchangCity[]> {
    return this.http.get<PanchangCity[]>(`${this.api}/panchang/cities`).pipe(
      catchError(() => of([
        { id: 'city-del', name: 'New Delhi, India', labelHindi: 'नई दिल्ली (New Delhi)', country: 'India', latitude: 28.6139, longitude: 77.209, timezoneOffsetHours: 5.5, isActive: true },
        { id: 'city-vns', name: 'Varanasi, India', labelHindi: 'काशी / वाराणसी (Varanasi)', country: 'India', latitude: 25.3176, longitude: 82.9739, timezoneOffsetHours: 5.5, isActive: true },
        { id: 'city-bom', name: 'Mumbai, India', labelHindi: 'मुंबई (Mumbai)', country: 'India', latitude: 19.076, longitude: 72.8777, timezoneOffsetHours: 5.5, isActive: true }
      ]))
    );
  }

  getSiteConfig(): Observable<PanchangSiteConfig> {
    return this.http.get<PanchangSiteConfig>(`${this.api}/panchang/config`).pipe(
      catchError(() => of({
        siteNameHindi: 'पंचांगम् & संस्कृति',
        siteSubtitle: 'Vedic Panchang & Aaj Ka Itihaas',
        defaultCityName: 'New Delhi, India',
        broadcastChannelName: 'Keshav Singh Official',
        broadcastChannelUrl: 'https://www.youtube.com/@KeshavSingh',
        dailyBroadcastBanner: 'Watch Daily Rashifal, Ekadashi Katha & Cultural Documentaries on our YouTube Channel.',
        videoCategories: ['Daily News & Rashifal', 'Pauranik Katha', 'Mantra & Meditation', 'Temple Darshan']
      }))
    );
  }

  getAajKaItihaas(date?: string): Observable<HistoricalIncident[]> {
    const query = date ? `?date=${encodeURIComponent(date)}` : '';
    return this.http.get<HistoricalIncident[]>(`${this.api}/culturalhistory/aaj-ka-itihaas${query}`).pipe(
      catchError(() => of([]))
    );
  }

  getDailyKatha(date?: string): Observable<DailyKatha | null> {
    const query = date ? `?date=${encodeURIComponent(date)}` : '';
    return this.http.get<DailyKatha>(`${this.api}/culturalhistory/daily-katha${query}`).pipe(
      catchError(() => of(null))
    );
  }

  getUpcomingFestivals(): Observable<FestivalItem[]> {
    return this.http.get<FestivalItem[]>(`${this.api}/festivals/upcoming`).pipe(
      catchError(() => of([]))
    );
  }

  getDevotionalKits(): Observable<DevotionalKitItem[]> {
    return this.http.get<DevotionalKitItem[]>(`${this.api}/festivals/kits`).pipe(
      catchError(() => of([]))
    );
  }

  getVideos(category?: string): Observable<SpiritualVideo[]> {
    const query = category && category !== 'All' ? `?category=${encodeURIComponent(category)}` : '';
    return this.http.get<SpiritualVideo[]>(`${this.api}/videohub/videos${query}`).pipe(
      catchError(() => of([]))
    );
  }

  getFeaturedVideo(): Observable<SpiritualVideo | null> {
    return this.http.get<SpiritualVideo>(`${this.api}/videohub/featured`).pipe(
      catchError(() => of(null))
    );
  }

  // ---- Admin-Only APIs (Require JWT with Admin role) ----

  getAdminStatus(): Observable<PanchangAdminStatus> {
    return this.http.get<PanchangAdminStatus>(`${this.api}/admin/status`);
  }

  getAdminVideos(): Observable<SpiritualVideo[]> {
    return this.http.get<SpiritualVideo[]>(`${this.api}/admin/videos`);
  }

  saveAdminVideo(video: SpiritualVideo): Observable<SpiritualVideo> {
    return video.id
      ? this.http.put<SpiritualVideo>(`${this.api}/admin/videos/${encodeURIComponent(video.id)}`, video)
      : this.http.post<SpiritualVideo>(`${this.api}/admin/videos`, video);
  }

  deleteAdminVideo(id: string): Observable<void> {
    return this.http.delete<void>(`${this.api}/admin/videos/${encodeURIComponent(id)}`);
  }

  getAdminFestivals(): Observable<FestivalItem[]> {
    return this.http.get<FestivalItem[]>(`${this.api}/admin/festivals`);
  }

  saveAdminFestival(festival: FestivalItem): Observable<FestivalItem> {
    return festival.id
      ? this.http.put<FestivalItem>(`${this.api}/admin/festivals/${encodeURIComponent(festival.id)}`, festival)
      : this.http.post<FestivalItem>(`${this.api}/admin/festivals`, festival);
  }

  deleteAdminFestival(id: string): Observable<void> {
    return this.http.delete<void>(`${this.api}/admin/festivals/${encodeURIComponent(id)}`);
  }

  getAdminKits(): Observable<DevotionalKitItem[]> {
    return this.http.get<DevotionalKitItem[]>(`${this.api}/admin/kits`);
  }

  saveAdminKit(kit: DevotionalKitItem): Observable<DevotionalKitItem> {
    return kit.id
      ? this.http.put<DevotionalKitItem>(`${this.api}/admin/kits/${encodeURIComponent(kit.id)}`, kit)
      : this.http.post<DevotionalKitItem>(`${this.api}/admin/kits`, kit);
  }

  deleteAdminKit(id: string): Observable<void> {
    return this.http.delete<void>(`${this.api}/admin/kits/${encodeURIComponent(id)}`);
  }

  getAdminIncidents(): Observable<HistoricalIncident[]> {
    return this.http.get<HistoricalIncident[]>(`${this.api}/admin/incidents`);
  }

  saveAdminIncident(inc: HistoricalIncident): Observable<HistoricalIncident> {
    return inc.id
      ? this.http.put<HistoricalIncident>(`${this.api}/admin/incidents/${encodeURIComponent(inc.id)}`, inc)
      : this.http.post<HistoricalIncident>(`${this.api}/admin/incidents`, inc);
  }

  deleteAdminIncident(id: string): Observable<void> {
    return this.http.delete<void>(`${this.api}/admin/incidents/${encodeURIComponent(id)}`);
  }

  getAdminKathas(): Observable<DailyKatha[]> {
    return this.http.get<DailyKatha[]>(`${this.api}/admin/kathas`);
  }

  saveAdminKatha(katha: DailyKatha): Observable<DailyKatha> {
    return katha.id
      ? this.http.put<DailyKatha>(`${this.api}/admin/kathas/${encodeURIComponent(katha.id)}`, katha)
      : this.http.post<DailyKatha>(`${this.api}/admin/kathas`, katha);
  }

  deleteAdminKatha(id: string): Observable<void> {
    return this.http.delete<void>(`${this.api}/admin/kathas/${encodeURIComponent(id)}`);
  }

  getAdminSutras(): Observable<DailySutra[]> {
    return this.http.get<DailySutra[]>(`${this.api}/admin/sutras`);
  }

  saveAdminSutra(sutra: DailySutra): Observable<DailySutra> {
    return sutra.id
      ? this.http.put<DailySutra>(`${this.api}/admin/sutras/${encodeURIComponent(sutra.id)}`, sutra)
      : this.http.post<DailySutra>(`${this.api}/admin/sutras`, sutra);
  }

  deleteAdminSutra(id: string): Observable<void> {
    return this.http.delete<void>(`${this.api}/admin/sutras/${encodeURIComponent(id)}`);
  }

  getAdminOverrides(): Observable<PanchangOverride[]> {
    return this.http.get<PanchangOverride[]>(`${this.api}/admin/overrides`);
  }

  saveAdminOverride(ov: PanchangOverride): Observable<PanchangOverride> {
    return this.http.post<PanchangOverride>(`${this.api}/admin/overrides`, ov);
  }

  deleteAdminOverride(dateKey: string): Observable<void> {
    return this.http.delete<void>(`${this.api}/admin/overrides/${encodeURIComponent(dateKey)}`);
  }

  getAdminCities(): Observable<PanchangCity[]> {
    return this.http.get<PanchangCity[]>(`${this.api}/admin/cities`);
  }

  saveAdminCity(city: PanchangCity): Observable<PanchangCity> {
    return city.id
      ? this.http.put<PanchangCity>(`${this.api}/admin/cities/${encodeURIComponent(city.id)}`, city)
      : this.http.post<PanchangCity>(`${this.api}/admin/cities`, city);
  }

  deleteAdminCity(id: string): Observable<void> {
    return this.http.delete<void>(`${this.api}/admin/cities/${encodeURIComponent(id)}`);
  }

  saveSiteConfig(cfg: PanchangSiteConfig): Observable<PanchangSiteConfig> {
    return this.http.put<PanchangSiteConfig>(`${this.api}/admin/site-config`, cfg);
  }
}
