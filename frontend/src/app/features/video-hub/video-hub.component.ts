import { Component, OnInit, computed, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { DomSanitizer, SafeResourceUrl } from '@angular/platform-browser';
import { finalize } from 'rxjs';
import { PanchangService } from '../../core/services/panchang.service';
import { EcosystemConfigService } from '../../core/services/ecosystem-config.service';
import { LocaleService } from '../../core/services/locale.service';
import { AuthService } from '../../core/services/auth.service';
import { SpiritualVideo } from '../../core/models/video.models';
import { CentralChannelItem } from '../../core/models/ecosystem.models';
import { PanchangSiteConfig } from '../../core/models/panchang.models';
import { LoadingStateComponent } from '../shared/loading/loading-state.component';

@Component({
  selector: 'app-video-hub',
  standalone: true,
  imports: [CommonModule, RouterModule, LoadingStateComponent],
  template: `
    <div class="video-hub-page">
      <div class="header-box">
        <span class="hub-tag">🕉️ दैनिक वीडियो एवं समाचार प्रसारण</span>
        <h1>{{ locale.t('panchang.videoHub.title') }}</h1>
        <p>
          {{ siteConfig()?.dailyBroadcastBanner || 'प्रतिदिन नवीनतम पंचांग विश्लेषण, पौराणिक कथाएं, मंत्र जाप और आध्यात्मिक वीडियो देखें।' }}
        </p>

        @if (connectedChannels().length > 0) {
          <div class="channel-bar">
            <span class="ch-bar-label">Connected Channels:</span>
            <button
              type="button"
              class="ch-chip"
              [class.active]="selectedChannel() === 'All'"
              (click)="selectedChannel.set('All')">
              All Channels ({{ allVideos().length }})
            </button>
            @for (ch of connectedChannels(); track ch.id || ch.key) {
              <button
                type="button"
                class="ch-chip"
                [class.active]="selectedChannel() === ch.name"
                (click)="selectedChannel.set(ch.name)">
                📺 {{ ch.name }}
              </button>
            }
          </div>
        }
      </div>

      <!-- Featured Main Video Player -->
      @if (featuredVideo(); as featured) {
        <section class="featured-video-player" [class.theater-mode]="theaterMode()">
          <div class="player-wrapper">
            <iframe
              [src]="safeEmbedUrl()"
              title="Featured Video"
              frameborder="0"
              allow="accelerometer; autoplay; clipboard-write; encrypted-media; gyroscope; picture-in-picture"
              allowfullscreen>
            </iframe>
          </div>

          <div class="featured-meta">
            <div class="meta-top">
              <div class="badges-row">
                <span class="category-badge">{{ featured.category }}</span>
                @if (featured.format === 'Short') {
                  <span class="short-badge">⚡ Short</span>
                }
                @if (featured.dateKey) {
                  <span class="date-badge">📅 {{ featured.dateKey }}</span>
                }
                @if (featured.isAutoSynced) {
                  <span class="sync-badge">📡 Auto-Synced</span>
                }
              </div>
              @if (featured.viewCount > 0) {
                <span class="view-count">👁️ {{ featured.viewCount | number }} Views</span>
              }
            </div>
            <h2>{{ featured.title }}</h2>
            <p class="featured-desc">{{ featured.description }}</p>

            <div class="channel-attribution">
              <span>📺 {{ featured.channelName || siteConfig()?.broadcastChannelName }}</span>
              <span>•</span>
              <span>{{ featured.publishedDate | date:'mediumDate' }}</span>
            </div>

            <div class="cta-actions-row">
              @if (featured.channelUrl) {
                <a [href]="featured.channelUrl" target="_blank" rel="noopener" class="btn-yt-sub">
                  ▶️ सब्सक्राइब करें (Subscribe Channel)
                </a>
              }
              <button (click)="shareVideo(featured)" class="btn-share">
                📲 शेयर करें (Share)
              </button>
              <button (click)="theaterMode.set(!theaterMode())" class="btn-theater" type="button">
                {{ theaterMode() ? '⤢ Standard View' : '⛶ Theater Mode' }}
              </button>
            </div>
          </div>
        </section>
      }

      <!-- Date Filter Strip (Default: Date-wise view of latest/selected date; toggle All available) -->
      <section class="date-filter-bar" aria-label="Filter videos by date">
        <div class="date-mode-row">
          <div class="date-controls-left">
            <span class="df-label">📅 Date Filter:</span>
            <button
              type="button"
              class="df-btn"
              [disabled]="showAllDates()"
              (click)="shiftDate(-1)"
              title="Previous day">
              ‹ Prev
            </button>
            <input
              type="date"
              class="df-input"
              [value]="selectedDate()"
              (change)="onDateInput($event)"
              aria-label="Select date for videos" />
            <button
              type="button"
              class="df-btn"
              [disabled]="showAllDates()"
              (click)="shiftDate(1)"
              title="Next day">
              Next ›
            </button>
            <button
              type="button"
              class="df-btn"
              [class.active]="!showAllDates() && selectedDate() === todayDateKey()"
              (click)="jumpToDate(todayDateKey())">
              Today
            </button>
          </div>

          <button
            type="button"
            class="df-all-btn"
            [class.active]="showAllDates()"
            (click)="toggleAllDates()">
            {{ showAllDates() ? '✓ Showing All Dates (' + totalAllCount() + ')' : '🌐 All Videos (' + totalAllCount() + ')' }}
          </button>
        </div>

        @if (availableDates().length > 0) {
          <div class="available-dates-row">
            <span class="avail-lbl">Recent dates with videos:</span>
            @for (d of availableDates().slice(0, 8); track d) {
              <button
                type="button"
                class="avail-chip"
                [class.active]="!showAllDates() && selectedDate() === d"
                (click)="jumpToDate(d)">
                {{ d }}
              </button>
            }
          </div>
        }
      </section>

      <!-- Category Filter Tabs -->
      <div class="category-tabs">
        @for (cat of categories(); track cat) {
          <button
            type="button"
            [class.active]="selectedCategory() === cat"
            (click)="selectCategory(cat)"
            class="cat-tab">
            {{ cat }}
          </button>
        }
      </div>

      <!-- Empty State OR Video Grid -->
      @if (loadingVideos()) {
        <app-loading-state variant="cards" [count]="6" label="आध्यात्मिक वीडियो लोड हो रहे हैं… / Loading spiritual videos…" />
      } @else if (filteredVideos().length === 0) {
        <section class="empty-videos-card">
          <div class="empty-icon">📺</div>
          @if (!showAllDates() && selectedDate()) {
            <h3>{{ selectedDate() }} के लिए इस श्रेणी में कोई वीडियो उपलब्ध नहीं है</h3>
            <p>
              चयनित दिनांक (<strong>{{ selectedDate() }}</strong>) पर कोई वीडियो सिंक या सेव नहीं है। आप नीचे से सभी दिनांक के वीडियो देख सकते हैं या नवीनतम उपलब्ध दिनांक पर जा सकते हैं:
            </p>
          } @else {
            <h3>अभी इस श्रेणी में कोई सिंक किया गया वीडियो उपलब्ध नहीं है</h3>
            <p>
              यूट्यूब चैनलों व AnimStudio Auto-Post से वीडियो यहाँ दिनांक के अनुसार स्वतः सिंक होते हैं।
            </p>
          }
          <div class="empty-actions">
            @if (!showAllDates() && totalAllCount() > 0) {
              <button type="button" class="btn-admin-sync" (click)="showAllVideos()">
                🌐 सभी वीडियो देखें (Show All {{ totalAllCount() }})
              </button>
              @if (availableDates().length > 0 && availableDates()[0] !== selectedDate()) {
                <button type="button" class="btn-theater" (click)="jumpToDate(availableDates()[0])">
                  📅 Latest ({{ availableDates()[0] }})
                </button>
              }
            }
            @for (ch of connectedChannels(); track ch.id || ch.key) {
              @if (ch.url) {
                <a [href]="ch.url" target="_blank" rel="noopener" class="btn-yt-sub">
                  ▶️ Watch {{ ch.name }}
                </a>
              }
            }
            @if (auth.isAdmin()) {
              <a routerLink="/admin" class="btn-admin-sync">
                ⚙️ Open Admin Sync Console →
              </a>
            }
          </div>
        </section>
      } @else {
        <section class="videos-grid">
          @for (vid of filteredVideos(); track vid.id) {
            <article
              class="video-card"
              [class.now-playing]="featuredVideo()?.id === vid.id"
              (click)="setFeatured(vid)">
              <div class="thumb-container">
                <img [src]="vid.thumbnailUrl" [alt]="vid.title" class="video-thumb" loading="lazy" decoding="async" />
                @if (vid.isAutoSynced) {
                  <span class="live-chip">📡 {{ vid.format === 'Short' ? 'Short' : 'Auto' }}</span>
                }
                <span class="duration-badge">{{ vid.dateKey || vid.durationMinutes || 'Video' }}</span>
                <div class="play-overlay">▶</div>
              </div>
              <div class="card-info">
                <span class="card-cat">{{ vid.category }}</span>
                <h4>{{ vid.title }}</h4>
                <div class="card-footer">
                  <span>{{ vid.channelName }}</span>
                  <span>{{ vid.viewCount > 0 ? (vid.viewCount | number) + ' views' : (vid.dateKey || (vid.publishedDate | date:'MMM d')) }}</span>
                </div>
              </div>
            </article>
          }
        </section>
      }

      <!-- Dynamic Ecosystem Cross-Funnel Banner (only shown when configured in Central Config) -->
      @if (ecosystem.studioPortalUrl()) {
        <section class="cross-funnel-card">
          <div class="funnel-content">
            <span class="pill-funnel">🚀 {{ ecosystem.creatorName() }} Ecosystem</span>
            <h3>क्या आप भी ऐसे धार्मिक एवं एनिमेटेड वीडियो बनाना चाहते हैं?</h3>
            <p>
              हमारे <strong>Studio AI</strong> प्लेटफॉर्म की सहायता से आप मात्र कुछ मिनटों में यूट्यूब और इंस्टाग्राम रील्स के लिए
              2D/3D एनिमेटेड कहानियां, भजन वीडियो एवं वॉयसओवर तैयार कर सकते हैं।
            </p>
          </div>
          <a [href]="ecosystem.studioPortalUrl()" target="_blank" rel="noopener" class="btn-funnel-cta">
            🎬 Open Studio AI →
          </a>
        </section>
      }
    </div>
  `,
  styles: [`
    .video-hub-page {
      max-width: 1300px;
      margin: 0 auto;
      padding: 1.5rem 1rem 3rem;
      display: flex;
      flex-direction: column;
      gap: 1.75rem;
    }
    .header-box {
      border-bottom: 1px solid #fed7aa;
      padding-bottom: 1.1rem;
      display: flex;
      flex-direction: column;
      gap: 0.45rem;
    }
    .hub-tag {
      font-size: 0.85rem;
      font-weight: 700;
      color: #ea580c;
      text-transform: uppercase;
      letter-spacing: 0.04em;
    }
    .header-box h1 {
      margin: 0.25rem 0;
      font-size: 1.9rem;
      font-weight: 800;
      color: #431407;
    }
    .header-box p {
      margin: 0;
      color: #78350f;
      font-size: 1rem;
      max-width: 850px;
      line-height: 1.5;
    }
    .channel-bar {
      display: flex;
      align-items: center;
      gap: 0.5rem;
      flex-wrap: wrap;
      margin-top: 0.75rem;
    }
    .ch-bar-label {
      font-size: 0.78rem;
      font-weight: 700;
      color: #9a3412;
      text-transform: uppercase;
    }
    .ch-chip {
      background: #fff7ed;
      border: 1px solid #fdba74;
      color: #7c2d12;
      padding: 0.35rem 0.8rem;
      border-radius: 999px;
      font-size: 0.82rem;
      font-weight: 600;
      cursor: pointer;
    }
    .ch-chip.active {
      background: #9a3412;
      color: #ffffff;
      border-color: #9a3412;
    }

    .featured-video-player {
      background: #ffffff;
      border: 1px solid #fed7aa;
      border-radius: 16px;
      overflow: hidden;
      box-shadow: 0 6px 20px rgba(124, 45, 18, 0.08);
      display: grid;
      grid-template-columns: 1.45fr 1fr;
    }
    .featured-video-player.theater-mode {
      grid-template-columns: 1fr;
    }
    @media (max-width: 900px) {
      .featured-video-player { grid-template-columns: 1fr; }
    }
    .player-wrapper {
      position: relative;
      padding-top: 56.25%;
      background: #000000;
    }
    .player-wrapper iframe {
      position: absolute;
      inset: 0;
      width: 100%;
      height: 100%;
      border: none;
    }
    .featured-meta {
      padding: 1.65rem;
      display: flex;
      flex-direction: column;
      justify-content: center;
      gap: 0.75rem;
    }
    .meta-top {
      display: flex;
      justify-content: space-between;
      align-items: center;
      gap: 0.5rem;
      flex-wrap: wrap;
    }
    .badges-row { display: flex; gap: 0.45rem; align-items: center; }
    .category-badge {
      background: #ffedd5;
      color: #c2410c;
      font-weight: 700;
      font-size: 0.8rem;
      padding: 0.25rem 0.65rem;
      border-radius: 9999px;
    }
    .sync-badge {
      background: #dcfce7;
      color: #166534;
      font-weight: 700;
      font-size: 0.74rem;
      padding: 0.2rem 0.55rem;
      border-radius: 999px;
    }
    .view-count { font-size: 0.85rem; color: #64748b; font-weight: 600; }
    .featured-meta h2 {
      margin: 0;
      font-size: 1.38rem;
      color: #1e293b;
      line-height: 1.35;
    }
    .featured-desc {
      margin: 0;
      color: #475569;
      font-size: 0.94rem;
      line-height: 1.5;
    }
    .channel-attribution {
      display: flex;
      gap: 0.5rem;
      font-size: 0.82rem;
      color: #9a3412;
      font-weight: 600;
    }
    .cta-actions-row {
      display: flex;
      gap: 0.65rem;
      margin-top: 0.4rem;
      flex-wrap: wrap;
    }
    .btn-yt-sub {
      background: #dc2626;
      color: #ffffff;
      text-decoration: none;
      font-weight: 700;
      padding: 0.58rem 1rem;
      border-radius: 8px;
      font-size: 0.88rem;
      transition: background 0.2s;
      display: inline-flex;
      align-items: center;
      gap: 0.35rem;
    }
    .btn-yt-sub:hover { background: #b91c1c; }
    .btn-share {
      background: #16a34a;
      color: #ffffff;
      border: none;
      font-weight: 700;
      padding: 0.58rem 1rem;
      border-radius: 8px;
      cursor: pointer;
      font-size: 0.88rem;
    }
    .btn-theater {
      background: #fff7ed;
      color: #7c2d12;
      border: 1px solid #fdba74;
      font-weight: 700;
      padding: 0.58rem 0.9rem;
      border-radius: 8px;
      cursor: pointer;
      font-size: 0.85rem;
    }

    .category-tabs {
      display: flex;
      gap: 0.5rem;
      flex-wrap: wrap;
    }
    .cat-tab {
      background: #ffffff;
      border: 1px solid #fed7aa;
      color: #78350f;
      padding: 0.5rem 1rem;
      border-radius: 8px;
      font-size: 0.88rem;
      font-weight: 600;
      cursor: pointer;
      transition: all 0.15s ease;
    }
    .cat-tab.active {
      background: #ea580c;
      color: #ffffff;
      border-color: #ea580c;
    }

    .empty-videos-card {
      background: #fffaf5;
      border: 2px dashed #fdba74;
      border-radius: 16px;
      padding: 2.5rem 1.5rem;
      text-align: center;
      display: flex;
      flex-direction: column;
      align-items: center;
      gap: 0.75rem;
    }
    .empty-icon { font-size: 2.5rem; }
    .empty-videos-card h3 { margin: 0; color: #431407; font-size: 1.25rem; }
    .empty-videos-card p { margin: 0; color: #78350f; max-width: 650px; line-height: 1.5; }
    .empty-actions {
      display: flex;
      gap: 0.75rem;
      flex-wrap: wrap;
      justify-content: center;
      margin-top: 0.5rem;
    }
    .btn-admin-sync {
      background: #ea580c;
      color: #ffffff;
      text-decoration: none;
      font-weight: 700;
      padding: 0.58rem 1.1rem;
      border-radius: 8px;
      font-size: 0.88rem;
    }

    .videos-grid {
      display: grid;
      grid-template-columns: repeat(auto-fill, minmax(280px, 1fr));
      gap: 1.25rem;
    }
    .video-card {
      background: #ffffff;
      border: 1px solid #fed7aa;
      border-radius: 12px;
      overflow: hidden;
      cursor: pointer;
      box-shadow: 0 2px 6px rgba(124, 45, 18, 0.04);
      transition: transform 0.2s, box-shadow 0.2s;
    }
    .video-card:hover {
      transform: translateY(-3px);
      box-shadow: 0 10px 22px rgba(124, 45, 18, 0.1);
    }
    .video-card.now-playing {
      border: 2px solid #ea580c;
    }
    .thumb-container {
      position: relative;
      height: 165px;
      background: #000;
      overflow: hidden;
    }
    .video-thumb {
      width: 100%;
      height: 100%;
      object-fit: cover;
      opacity: 0.92;
    }
    .live-chip {
      position: absolute;
      top: 8px;
      left: 8px;
      background: rgba(22, 101, 52, 0.9);
      color: #ffffff;
      font-size: 0.7rem;
      font-weight: 700;
      padding: 0.15rem 0.45rem;
      border-radius: 999px;
    }
    .duration-badge {
      position: absolute;
      bottom: 8px;
      right: 8px;
      background: rgba(0, 0, 0, 0.82);
      color: #ffffff;
      font-size: 0.75rem;
      padding: 0.15rem 0.45rem;
      border-radius: 4px;
      font-weight: 600;
    }
    .play-overlay {
      position: absolute;
      top: 50%;
      left: 50%;
      transform: translate(-50%, -50%);
      width: 46px;
      height: 46px;
      background: rgba(220, 38, 38, 0.88);
      border-radius: 50%;
      color: #ffffff;
      display: flex;
      align-items: center;
      justify-content: center;
      font-size: 1.2rem;
      opacity: 0;
      transition: opacity 0.2s;
    }
    .video-card:hover .play-overlay { opacity: 1; }
    .card-info {
      padding: 1rem;
      display: flex;
      flex-direction: column;
      gap: 0.4rem;
    }
    .card-cat {
      font-size: 0.75rem;
      font-weight: 700;
      color: #c2410c;
    }
    .card-info h4 {
      margin: 0;
      font-size: 0.98rem;
      color: #1e293b;
      line-height: 1.38;
      display: -webkit-box;
      -webkit-line-clamp: 2;
      -webkit-box-orient: vertical;
      overflow: hidden;
    }
    .card-footer {
      display: flex;
      justify-content: space-between;
      font-size: 0.78rem;
      color: #64748b;
      margin-top: 0.25rem;
    }

    .cross-funnel-card {
      background: linear-gradient(135deg, #4f46e5 0%, #3730a3 100%);
      color: #ffffff;
      border-radius: 16px;
      padding: 1.75rem 2rem;
      display: flex;
      justify-content: space-between;
      align-items: center;
      gap: 1.5rem;
      flex-wrap: wrap;
    }
    .pill-funnel {
      background: rgba(255, 255, 255, 0.2);
      padding: 0.25rem 0.65rem;
      border-radius: 9999px;
      font-size: 0.75rem;
      font-weight: 700;
    }
    .funnel-content h3 { margin: 0.6rem 0 0.4rem; font-size: 1.3rem; }
    .funnel-content p { margin: 0; max-width: 720px; color: #e0e7ff; font-size: 0.95rem; line-height: 1.5; }
    .btn-funnel-cta {
      background: #ffffff;
      color: #3730a3;
      text-decoration: none;
      font-weight: 700;
      padding: 0.75rem 1.4rem;
      border-radius: 10px;
      white-space: nowrap;
      transition: transform 0.2s;
    }
    .btn-funnel-cta:hover { transform: scale(1.03); }

    .date-filter-bar {
      background: #fffaf5;
      border: 1px solid #fed7aa;
      border-radius: 14px;
      padding: 0.85rem 1.1rem;
      display: flex;
      flex-direction: column;
      gap: 0.65rem;
    }
    .date-mode-row {
      display: flex;
      justify-content: space-between;
      align-items: center;
      flex-wrap: wrap;
      gap: 0.75rem;
    }
    .date-controls-left {
      display: flex;
      align-items: center;
      flex-wrap: wrap;
      gap: 0.45rem;
    }
    .df-label {
      font-size: 0.85rem;
      font-weight: 700;
      color: #9a3412;
      margin-right: 0.2rem;
    }
    .df-btn, .avail-chip {
      background: #ffffff;
      border: 1px solid #fdba74;
      color: #7c2d12;
      padding: 0.35rem 0.7rem;
      border-radius: 8px;
      font-size: 0.82rem;
      font-weight: 600;
      cursor: pointer;
    }
    .df-btn:disabled {
      opacity: 0.45;
      cursor: not-allowed;
    }
    .df-btn.active, .avail-chip.active {
      background: #ea580c;
      color: #ffffff;
      border-color: #ea580c;
    }
    .df-input {
      border: 1px solid #fdba74;
      border-radius: 8px;
      padding: 0.32rem 0.6rem;
      font-size: 0.85rem;
      color: #431407;
      background: #ffffff;
    }
    .df-all-btn {
      background: #ffffff;
      border: 1px solid #9a3412;
      color: #9a3412;
      padding: 0.42rem 0.9rem;
      border-radius: 999px;
      font-size: 0.82rem;
      font-weight: 700;
      cursor: pointer;
    }
    .df-all-btn.active {
      background: #9a3412;
      color: #ffffff;
    }
    .available-dates-row {
      display: flex;
      align-items: center;
      flex-wrap: wrap;
      gap: 0.4rem;
    }
    .avail-lbl {
      font-size: 0.78rem;
      color: #78350f;
      font-weight: 600;
    }
    .short-badge {
      background: #fef3c7;
      color: #92400e;
      font-weight: 700;
      font-size: 0.74rem;
      padding: 0.2rem 0.5rem;
      border-radius: 999px;
    }
    .date-badge {
      background: #e0f2fe;
      color: #0369a1;
      font-weight: 700;
      font-size: 0.74rem;
      padding: 0.2rem 0.5rem;
      border-radius: 999px;
    }
  `]
})
export class VideoHubComponent implements OnInit {
  private panchangService = inject(PanchangService);
  private sanitizer = inject(DomSanitizer);
  readonly ecosystem = inject(EcosystemConfigService);
  readonly auth = inject(AuthService);
  readonly locale = inject(LocaleService);

  readonly allVideos = signal<SpiritualVideo[]>([]);
  readonly featuredVideo = signal<SpiritualVideo | null>(null);
  readonly loadingVideos = signal<boolean>(true);
  readonly selectedCategory = signal<string>('All');
  readonly selectedChannel = signal<string>('All');
  readonly selectedDate = signal<string>('');
  readonly todayDateKey = signal<string>(new Date().toISOString().slice(0, 10));
  readonly availableDates = signal<string[]>([]);
  readonly showAllDates = signal<boolean>(false);
  readonly totalAllCount = signal<number>(0);
  readonly theaterMode = signal<boolean>(false);
  readonly siteConfig = signal<PanchangSiteConfig | null>(null);
  readonly categories = signal<string[]>([
    'All',
    'Daily News & Rashifal',
    'Pauranik Katha',
    'Mantra & Meditation',
    'Temple Darshan'
  ]);

  /**
   * Panchang's own channels, plus any other channel (e.g. Keshav Singh World) that has videos on the hub
   * because they were published here explicitly from AnimStudio Auto-Post.
   */
  readonly connectedChannels = computed<CentralChannelItem[]>(() => {
    const result = [...this.ecosystem.panchangChannels()];
    const seen = new Set(result.map(c => (c.name || '').trim().toLowerCase()));
    const known = this.ecosystem.enabledChannels();
    for (const v of this.allVideos()) {
      const name = (v.channelName || '').trim();
      const key = name.toLowerCase();
      if (!name || seen.has(key)) continue;
      seen.add(key);
      const match = known.find(c => (c.name || '').trim().toLowerCase() === key);
      result.push(match ?? {
        id: `video-${key}`,
        key: `video-${key}`,
        name,
        handle: '',
        platform: 'youtube',
        channelId: '',
        url: v.channelUrl || '',
        niche: '',
        showInAnalytics: false,
        showInPanchang: true,
        showInStudio: false,
        showInFooter: false,
        isPrimary: false,
        isEnabled: true,
        sortOrder: 999
      });
    }
    return result;
  });

  readonly filteredVideos = computed(() => {
    const ch = this.selectedChannel();
    const list = this.allVideos();
    if (ch === 'All') return list;
    return list.filter(v => (v.channelName || '').toLowerCase() === ch.toLowerCase());
  });

  readonly safeEmbedUrl = computed<SafeResourceUrl>(() => {
    const vid = this.featuredVideo();
    const raw = vid?.embedUrl || (vid?.youTubeVideoId ? `https://www.youtube.com/embed/${vid.youTubeVideoId}` : '');
    return this.sanitizer.bypassSecurityTrustResourceUrl(raw);
  });

  ngOnInit(): void {
    this.ecosystem.load();
    this.panchangService.getSiteConfig().subscribe(cfg => {
      this.siteConfig.set(cfg);
      if (cfg.videoCategories?.length) {
        this.categories.set(['All', ...cfg.videoCategories]);
      }
    });
    this.loadVideos(true);
  }

  selectCategory(cat: string): void {
    this.selectedCategory.set(cat);
    this.loadVideos(false);
  }

  onDateInput(event: Event): void {
    const value = (event.target as HTMLInputElement)?.value;
    if (!value) return;
    this.jumpToDate(value);
  }

  jumpToDate(dateKey: string): void {
    this.showAllDates.set(false);
    this.selectedDate.set(dateKey);
    this.loadVideos(false);
  }

  shiftDate(deltaDays: number): void {
    const base = this.selectedDate() || this.todayDateKey();
    const dt = new Date(`${base}T00:00:00Z`);
    if (Number.isNaN(dt.getTime())) return;
    dt.setUTCDate(dt.getUTCDate() + deltaDays);
    this.jumpToDate(dt.toISOString().slice(0, 10));
  }

  toggleAllDates(): void {
    this.showAllDates.set(!this.showAllDates());
    this.loadVideos(false);
  }

  showAllVideos(): void {
    this.showAllDates.set(true);
    this.loadVideos(false);
  }

  loadVideos(initialLoad = false): void {
    this.loadingVideos.set(true);
    this.panchangService.getVideoFeed({
      category: this.selectedCategory(),
      date: initialLoad || this.showAllDates() ? null : (this.selectedDate() || null),
      allDates: this.showAllDates()
    })
      .pipe(finalize(() => this.loadingVideos.set(false)))
      .subscribe(feed => {
        this.allVideos.set(feed.videos ?? []);
        if (feed.todayDateKey) this.todayDateKey.set(feed.todayDateKey);
        if (feed.selectedDate) this.selectedDate.set(feed.selectedDate);
        this.availableDates.set(feed.availableDates ?? []);
        this.totalAllCount.set(feed.totalAllDatesCount ?? (feed.videos?.length ?? 0));
        this.showAllDates.set(!!feed.showAllDates);

        const vids = feed.videos ?? [];
        const current = this.featuredVideo();
        if (!current || !vids.some(v => v.id === current.id)) {
          const picked = feed.featuredVideo || vids.find(v => v.isFeaturedDailyVideo) || vids[0] || null;
          this.featuredVideo.set(picked);
        }
      });
  }

  setFeatured(vid: SpiritualVideo): void {
    this.featuredVideo.set(vid);
    if (typeof window !== 'undefined') {
      window.scrollTo({ top: 0, behavior: 'smooth' });
    }
  }

  shareVideo(vid: SpiritualVideo): void {
    const base = typeof window !== 'undefined' ? window.location.href : '';
    const text = `🕉️ ${vid.title} — ${vid.youTubeUrl || base}`;
    if (typeof navigator !== 'undefined' && 'share' in navigator) {
      navigator.share({ title: vid.title, text, url: vid.youTubeUrl || base }).catch(() => {});
      return;
    }
    if (typeof window !== 'undefined') {
      window.open(`https://wa.me/?text=${encodeURIComponent(text)}`, '_blank', 'noopener');
    }
  }
}
