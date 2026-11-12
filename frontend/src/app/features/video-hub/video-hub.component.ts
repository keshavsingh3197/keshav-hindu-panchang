import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { DomSanitizer, SafeResourceUrl } from '@angular/platform-browser';
import { PanchangService } from '../../core/services/panchang.service';
import { EcosystemConfigService } from '../../core/services/ecosystem-config.service';
import { SpiritualVideo } from '../../core/models/video.models';

@Component({
  selector: 'app-video-hub',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="video-hub-page">
      <div class="header-box">
        <span class="hub-tag">🕉️ दैनिक वीडियो एवं समाचार प्रसारण</span>
        <h1>दैनिक आध्यात्मिक वीडियो हब (Daily Spiritual Videos & News)</h1>
        <p>
          प्रतिदिन नवीनतम पंचांग विश्लेषण, दैनिक राशिफल, पौराणिक कथाएं, और दिव्य मंदिर दर्शन वीडियो देखें। 
          हमारे यूट्यूब चैनल को सब्सक्राइब करें और प्रतिदिन आध्यात्मिक ज्ञान प्राप्त करें।
        </p>
      </div>

      <!-- Featured Main Video Player -->
      @if (featuredVideo(); as featured) {
        <section class="featured-video-player">
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
              <span class="category-badge">{{ featured.category }}</span>
              <span class="view-count">👁️ {{ featured.viewCount | number }} Views</span>
            </div>
            <h2>{{ featured.title }}</h2>
            <p class="featured-desc">{{ featured.description }}</p>

            <div class="cta-actions-row">
              <a [href]="featured.channelUrl" target="_blank" rel="noopener" class="btn-yt-sub">
                ▶️ यूट्यूब पर सब्सक्राइब करें (Subscribe Channel)
              </a>
              <button (click)="shareVideo(featured)" class="btn-share">
                📲 शेयर करें (WhatsApp / Share)
              </button>
            </div>
          </div>
        </section>
      }

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

      <!-- Video Grid -->
      <section class="videos-grid">
        @for (vid of videos(); track vid.id) {
          <div class="video-card" (click)="setFeatured(vid)">
            <div class="thumb-container">
              <img [src]="vid.thumbnailUrl" [alt]="vid.title" class="video-thumb" />
              <span class="duration-badge">{{ vid.durationMinutes }}</span>
              <div class="play-overlay">▶</div>
            </div>
            <div class="card-info">
              <span class="card-cat">{{ vid.category }}</span>
              <h4>{{ vid.title }}</h4>
              <div class="card-footer">
                <span>{{ vid.channelName }}</span>
                <span>{{ vid.viewCount | number }} views</span>
              </div>
            </div>
          </div>
        }
      </section>

      <!-- Monetization & Studio AI Funnel Banner -->
      <section class="cross-funnel-card">
        <div class="funnel-content">
          <span class="pill-funnel">🚀 Powered by Keshav Singh Ecosystem</span>
          <h3>क्या आप भी ऐसे धार्मिक एवं एनिमेटेड वीडियो बनाना चाहते हैं?</h3>
          <p>
            हमारे <strong>AnimStudio AI</strong> टूल की सहायता से आप मात्र कुछ मिनटों में यूट्यूब और इंस्टाग्राम रील्स के लिए 
            2D/3D एनिमेटेड कहानियां, भजन वीडियो एवं वॉयसओवर तैयार कर सकते हैं।
          </p>
        </div>
        <a [href]="ecosystem.studioPortalUrl()" target="_blank" rel="noopener" class="btn-funnel-cta">
          🎬 Open AnimStudio AI Free →
        </a>
      </section>
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
      padding-bottom: 1rem;
    }
    .hub-tag {
      font-size: 0.85rem;
      font-weight: 700;
      color: #ea580c;
      text-transform: uppercase;
      letter-spacing: 0.04em;
    }
    .header-box h1 {
      margin: 0.4rem 0;
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

    .featured-video-player {
      background: #ffffff;
      border: 1px solid #fed7aa;
      border-radius: 16px;
      overflow: hidden;
      box-shadow: 0 4px 15px rgba(124, 45, 18, 0.08);
      display: grid;
      grid-template-columns: 1.4fr 1fr;
    }
    @media (max-width: 900px) {
      .featured-video-player { grid-template-columns: 1fr; }
    }
    .player-wrapper {
      position: relative;
      padding-top: 56.25%; /* 16:9 Aspect Ratio */
      background: #000000;
    }
    .player-wrapper iframe {
      position: absolute;
      top: 0;
      left: 0;
      width: 100%;
      height: 100%;
      border: none;
    }
    .featured-meta {
      padding: 1.75rem;
      display: flex;
      flex-direction: column;
      justify-content: center;
      gap: 0.8rem;
    }
    .meta-top {
      display: flex;
      justify-content: space-between;
      align-items: center;
    }
    .category-badge {
      background: #ffedd5;
      color: #c2410c;
      font-weight: 700;
      font-size: 0.8rem;
      padding: 0.25rem 0.65rem;
      border-radius: 9999px;
    }
    .view-count { font-size: 0.85rem; color: #64748b; font-weight: 500; }
    .featured-meta h2 {
      margin: 0;
      font-size: 1.4rem;
      color: #1e293b;
      line-height: 1.3;
    }
    .featured-desc {
      margin: 0;
      color: #475569;
      font-size: 0.95rem;
      line-height: 1.5;
    }
    .cta-actions-row {
      display: flex;
      gap: 0.75rem;
      margin-top: 0.5rem;
      flex-wrap: wrap;
    }
    .btn-yt-sub {
      background: #dc2626;
      color: #ffffff;
      text-decoration: none;
      font-weight: 700;
      padding: 0.6rem 1.1rem;
      border-radius: 8px;
      font-size: 0.9rem;
      transition: background 0.2s;
    }
    .btn-yt-sub:hover { background: #b91c1c; }
    .btn-share {
      background: #25d366;
      color: #ffffff;
      border: none;
      font-weight: 700;
      padding: 0.6rem 1.1rem;
      border-radius: 8px;
      cursor: pointer;
      font-size: 0.9rem;
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
      font-size: 0.9rem;
      font-weight: 600;
      cursor: pointer;
      transition: all 0.15s ease;
    }
    .cat-tab.active {
      background: #ea580c;
      color: #ffffff;
      border-color: #ea580c;
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
      box-shadow: 0 1px 3px rgba(0,0,0,0.02);
      transition: transform 0.2s;
    }
    .video-card:hover { transform: translateY(-3px); }
    .thumb-container {
      position: relative;
      height: 160px;
      background: #000;
      overflow: hidden;
    }
    .video-thumb {
      width: 100%;
      height: 100%;
      object-fit: cover;
      opacity: 0.9;
    }
    .duration-badge {
      position: absolute;
      bottom: 8px;
      right: 8px;
      background: rgba(0, 0, 0, 0.8);
      color: #ffffff;
      font-size: 0.75rem;
      padding: 0.15rem 0.4rem;
      border-radius: 4px;
      font-weight: 600;
    }
    .play-overlay {
      position: absolute;
      top: 50%;
      left: 50%;
      transform: translate(-50%, -50%);
      width: 44px;
      height: 44px;
      background: rgba(220, 38, 38, 0.85);
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
      font-size: 0.95rem;
      color: #1e293b;
      line-height: 1.35;
    }
    .card-footer {
      display: flex;
      justify-content: space-between;
      font-size: 0.8rem;
      color: #64748b;
      margin-top: 0.3rem;
    }

    .cross-funnel-card {
      background: linear-gradient(135deg, #1e1b4b 0%, #312e81 100%);
      color: #ffffff;
      padding: 2rem;
      border-radius: 14px;
      display: flex;
      justify-content: space-between;
      align-items: center;
      gap: 1.5rem;
      flex-wrap: wrap;
    }
    .pill-funnel {
      font-size: 0.75rem;
      font-weight: 700;
      text-transform: uppercase;
      background: rgba(255, 255, 255, 0.15);
      padding: 0.2rem 0.6rem;
      border-radius: 9999px;
      color: #a5b4fc;
      display: inline-block;
      margin-bottom: 0.5rem;
    }
    .funnel-content h3 { margin: 0 0 0.5rem; font-size: 1.35rem; }
    .funnel-content p { margin: 0; color: #c7d2fe; max-width: 680px; line-height: 1.5; font-size: 0.95rem; }
    .btn-funnel-cta {
      background: #6366f1;
      color: #ffffff;
      padding: 0.75rem 1.4rem;
      border-radius: 8px;
      font-weight: 700;
      text-decoration: none;
      font-size: 0.95rem;
      transition: background 0.2s;
    }
    .btn-funnel-cta:hover { background: #4f46e5; }
  `]
})
export class VideoHubComponent implements OnInit {
  private panchangService = inject(PanchangService);
  private sanitizer = inject(DomSanitizer);
  readonly ecosystem = inject(EcosystemConfigService);

  readonly categories = signal<string[]>(['All', 'Daily News & Rashifal', 'Pauranik Katha', 'Mantra & Meditation', 'Temple Darshan']);
  readonly selectedCategory = signal<string>('All');
  readonly videos = signal<SpiritualVideo[]>([]);
  readonly featuredVideo = signal<SpiritualVideo | null>(null);
  readonly safeEmbedUrl = signal<SafeResourceUrl>('');

  ngOnInit(): void {
    this.panchangService.getSiteConfig().subscribe(cfg => {
      if (cfg.videoCategories?.length) {
        this.categories.set(['All', ...cfg.videoCategories]);
      }
    });
    this.loadVideos();
  }

  loadVideos(): void {
    this.panchangService.getVideos(this.selectedCategory()).subscribe(vids => {
      this.videos.set(vids);
      if (!this.featuredVideo() && vids.length > 0) {
        this.setFeatured(vids[0]);
      }
    });
  }

  selectCategory(cat: string): void {
    this.selectedCategory.set(cat);
    this.loadVideos();
  }

  setFeatured(video: SpiritualVideo): void {
    this.featuredVideo.set(video);
    const url = video.embedUrl || `https://www.youtube.com/embed/${video.youTubeVideoId}`;
    this.safeEmbedUrl.set(this.sanitizer.bypassSecurityTrustResourceUrl(url));
    window.scrollTo({ top: 0, behavior: 'smooth' });
  }

  shareVideo(video: SpiritualVideo): void {
    const base = typeof window !== 'undefined' ? window.location.origin : 'https://panchang.keshavsingh.in';
    const text = encodeURIComponent(`🕉️ ${video.title}\nवीडियो देखें: ${video.youTubeUrl}\nपंचांग एवं दैनिक वीडियो: ${base}`);
    window.open(`https://api.whatsapp.com/send?text=${text}`, '_blank');
  }
}
