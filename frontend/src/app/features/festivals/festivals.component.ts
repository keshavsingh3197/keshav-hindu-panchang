import { Component, OnInit, computed, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterModule } from '@angular/router';
import { PanchangService } from '../../core/services/panchang.service';
import { DevotionalKitItem, FestivalItem } from '../../core/models/panchang.models';

@Component({
  selector: 'app-festivals',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterModule],
  template: `
    <div class="festivals-page">
      <div class="header-box">
        <span class="sub-pill">🪔 सनातन पर्व एवं व्रत सारणी</span>
        <h1>आगामी प्रमुख व्रत एवं त्यौहार (Upcoming Hindu Festivals &amp; Vrats)</h1>
        <p>
          वैदिक तिथियों के अनुसार आगामी एकादशी, प्रदोष, पूर्णिमा, अमावस्या एवं प्रमुख हिंदू पर्वों की सूची,
          व्रत नियम एवं शास्त्रसम्मत पूजा विधि।
        </p>

        <div class="filter-toolbar">
          <div class="pill-group">
            <button type="button" class="f-pill" [class.active]="filterMode() === 'all'" (click)="filterMode.set('all')">
              सभी पर्व ({{ festivals().length }})
            </button>
            <button type="button" class="f-pill" [class.active]="filterMode() === 'major'" (click)="filterMode.set('major')">
              ⭐ प्रमुख त्यौहार (Major)
            </button>
          </div>
          <input
            type="search"
            class="search-input"
            [ngModel]="searchTerm()"
            (ngModelChange)="searchTerm.set($event)"
            placeholder="खोजें (जैसे Ekadashi, Shivratri, Tithi)..."
            aria-label="Search festivals" />
        </div>
      </div>

      <!-- Festivals Grid -->
      @if (filteredFestivals().length === 0) {
        <div class="empty-box">कोई व्रत या त्यौहार इस फ़िल्टर से मेल नहीं खाता।</div>
      } @else {
        <section class="festivals-grid">
          @for (fest of filteredFestivals(); track fest.id) {
            <article class="fest-card" [class.major-card]="fest.isMajorFestival">
              <div class="fest-header">
                <span class="tithi-tag">{{ fest.monthVedic }} • {{ fest.tithi }}</span>
                <span class="countdown-badge" [class.today-badge]="countdownText(fest.date) === 'आज (Today)'">
                  {{ countdownText(fest.date) }}
                </span>
              </div>

              <div class="fest-title-row">
                <div>
                  <h2>{{ fest.nameHindi }}</h2>
                  <span class="eng-title">({{ fest.nameEnglish }})</span>
                </div>
                <span class="fest-date">{{ fest.date | date:'mediumDate' }}</span>
              </div>

              <p class="fest-desc">{{ fest.description }}</p>

              <div class="info-block">
                <strong>पूजा विधि एवं अनुष्ठान:</strong>
                <p>{{ fest.ritualsAndPujaVidhi }}</p>
              </div>

              <div class="fasting-block">
                <strong>व्रत नियम (Fasting Rules):</strong>
                <p>{{ fest.fastingRules }}</p>
              </div>
            </article>
          }
        </section>
      }

      <!-- Devotional Samagri & Dynamic Ecosystem Block -->
      @if (kits().length > 0) {
        <section class="affiliate-samagri-section">
          <div class="samagri-header">
            <h3>📦 प्रामाणिक पूजा सामग्री, वैदिक ग्रंथ एवं आध्यात्मिक संसाधन</h3>
            <p>त्यौहारों एवं दैनिक पूजा के लिए आवश्यक सामग्री, मंत्र वीडियो एवं पावन संसाधन:</p>
          </div>

          <div class="samagri-grid">
            @for (kit of kits(); track kit.id) {
              <div class="item-card">
                <span class="item-icon">{{ kit.icon || '🪔' }}</span>
                <h4>{{ kit.title }}</h4>
                <p>{{ kit.description }}</p>
                @if (isInternalRoute(kit.externalUrl)) {
                  <a [routerLink]="kit.externalUrl" class="btn-buy">
                    {{ kit.actionLabel || 'Explore →' }}
                  </a>
                } @else {
                  <a [href]="kit.externalUrl" target="_blank" rel="noopener" class="btn-buy">
                    {{ kit.actionLabel || 'Explore →' }}
                  </a>
                }
              </div>
            }
          </div>
        </section>
      }
    </div>
  `,
  styles: [`
    .festivals-page {
      max-width: 1300px;
      margin: 0 auto;
      padding: 1.5rem 1rem 3rem;
      display: flex;
      flex-direction: column;
      gap: 1.75rem;
    }
    .header-box {
      border-bottom: 1px solid #fed7aa;
      padding-bottom: 1.15rem;
      display: flex;
      flex-direction: column;
      gap: 0.5rem;
    }
    .sub-pill { font-size: 0.8rem; font-weight: 700; color: #ea580c; text-transform: uppercase; letter-spacing: 0.04em; }
    .header-box h1 { margin: 0.25rem 0; font-size: 1.9rem; font-weight: 800; color: #431407; }
    .header-box p { margin: 0; color: #78350f; font-size: 1rem; max-width: 850px; line-height: 1.5; }

    .filter-toolbar {
      display: flex;
      justify-content: space-between;
      align-items: center;
      gap: 0.75rem;
      margin-top: 0.75rem;
      flex-wrap: wrap;
    }
    .pill-group { display: flex; gap: 0.5rem; flex-wrap: wrap; }
    .f-pill {
      background: #ffffff;
      border: 1px solid #fed7aa;
      color: #78350f;
      padding: 0.45rem 0.9rem;
      border-radius: 999px;
      font-weight: 600;
      font-size: 0.85rem;
      cursor: pointer;
      transition: all 0.15s ease;
    }
    .f-pill.active {
      background: #ea580c;
      color: #ffffff;
      border-color: #ea580c;
    }
    .search-input {
      padding: 0.48rem 0.9rem;
      border: 1px solid #fdba74;
      border-radius: 999px;
      font-size: 0.88rem;
      min-width: 240px;
      background: #ffffff;
    }

    .empty-box {
      background: #fffaf5;
      border: 1px dashed #fdba74;
      border-radius: 12px;
      padding: 2rem;
      text-align: center;
      color: #9a3412;
      font-weight: 600;
    }

    .festivals-grid {
      display: grid;
      grid-template-columns: repeat(auto-fit, minmax(340px, 1fr));
      gap: 1.35rem;
    }
    .fest-card {
      background: #ffffff;
      border: 1px solid #fed7aa;
      border-radius: 14px;
      padding: 1.4rem;
      display: flex;
      flex-direction: column;
      gap: 0.8rem;
      box-shadow: 0 2px 6px rgba(124, 45, 18, 0.05);
      transition: transform 0.2s ease, box-shadow 0.2s ease;
    }
    .fest-card:hover {
      transform: translateY(-2px);
      box-shadow: 0 8px 20px rgba(124, 45, 18, 0.1);
    }
    .major-card {
      border-top: 4px solid #ea580c;
    }
    .fest-header {
      display: flex;
      justify-content: space-between;
      align-items: center;
      flex-wrap: wrap;
      gap: 0.5rem;
    }
    .tithi-tag {
      background: #ffedd5;
      color: #c2410c;
      font-weight: 700;
      font-size: 0.78rem;
      padding: 0.22rem 0.65rem;
      border-radius: 9999px;
    }
    .countdown-badge {
      background: #f8fafc;
      border: 1px solid #e2e8f0;
      color: #475569;
      font-size: 0.75rem;
      font-weight: 700;
      padding: 0.2rem 0.55rem;
      border-radius: 999px;
    }
    .today-badge {
      background: #dcfce7;
      border-color: #86efac;
      color: #166534;
    }
    .fest-title-row {
      display: flex;
      justify-content: space-between;
      align-items: flex-start;
      gap: 0.75rem;
    }
    .fest-date { font-size: 0.82rem; color: #64748b; font-weight: 700; white-space: nowrap; }
    .fest-card h2 { margin: 0; font-size: 1.35rem; color: #431407; }
    .eng-title { font-size: 0.85rem; color: #9a3412; font-weight: 600; }
    .fest-desc { margin: 0; color: #475569; font-size: 0.94rem; line-height: 1.55; }

    .info-block, .fasting-block {
      background: #fffaf5;
      border-radius: 8px;
      padding: 0.75rem 1rem;
      font-size: 0.88rem;
      line-height: 1.5;
    }
    .info-block strong { color: #7c2d12; display: block; margin-bottom: 0.2rem; }
    .fasting-block strong { color: #9a3412; display: block; margin-bottom: 0.2rem; }
    .info-block p, .fasting-block p { margin: 0; color: #334155; }

    .affiliate-samagri-section {
      background: #fdfaf7;
      border: 1px solid #fed7aa;
      border-radius: 16px;
      padding: 1.75rem;
    }
    .samagri-header h3 { margin: 0 0 0.3rem; font-size: 1.25rem; color: #431407; }
    .samagri-header p { margin: 0 0 1.25rem; color: #78350f; font-size: 0.95rem; }
    .samagri-grid {
      display: grid;
      grid-template-columns: repeat(auto-fit, minmax(260px, 1fr));
      gap: 1.25rem;
    }
    .item-card {
      background: #ffffff;
      border: 1px solid #fed7aa;
      border-radius: 12px;
      padding: 1.25rem;
      text-align: center;
      display: flex;
      flex-direction: column;
      align-items: center;
      gap: 0.55rem;
      transition: transform 0.15s ease;
    }
    .item-card:hover { transform: translateY(-2px); }
    .item-icon { font-size: 2.2rem; }
    .item-card h4 { margin: 0; font-size: 1.05rem; color: #1e293b; }
    .item-card p { margin: 0; font-size: 0.85rem; color: #64748b; }
    .btn-buy {
      margin-top: auto;
      background: #ea580c;
      color: #ffffff;
      border: none;
      padding: 0.5rem 1.05rem;
      border-radius: 8px;
      font-weight: 600;
      font-size: 0.85rem;
      text-decoration: none;
      display: inline-block;
      cursor: pointer;
      transition: background 0.2s;
    }
    .btn-buy:hover { background: #c2410c; }
  `]
})
export class FestivalsComponent implements OnInit {
  private panchangService = inject(PanchangService);

  readonly festivals = signal<FestivalItem[]>([]);
  readonly kits = signal<DevotionalKitItem[]>([]);
  readonly filterMode = signal<'all' | 'major'>('all');
  readonly searchTerm = signal<string>('');

  readonly filteredFestivals = computed(() => {
    const q = this.searchTerm().trim().toLowerCase();
    return this.festivals().filter(f => {
      if (this.filterMode() === 'major' && !f.isMajorFestival) return false;
      if (!q) return true;
      return (
        (f.nameHindi || '').toLowerCase().includes(q) ||
        (f.nameEnglish || '').toLowerCase().includes(q) ||
        (f.tithi || '').toLowerCase().includes(q) ||
        (f.monthVedic || '').toLowerCase().includes(q)
      );
    });
  });

  ngOnInit(): void {
    this.panchangService.getUpcomingFestivals().subscribe(f => this.festivals.set(f));
    this.panchangService.getDevotionalKits().subscribe(k => this.kits.set(k));
  }

  isInternalRoute(url: string): boolean {
    return !!url && url.startsWith('/');
  }

  countdownText(dateIso: string): string {
    if (!dateIso) return 'Upcoming';
    const target = new Date(dateIso);
    const now = new Date();
    const t0 = Date.UTC(now.getUTCFullYear(), now.getUTCMonth(), now.getUTCDate());
    const t1 = Date.UTC(target.getUTCFullYear(), target.getUTCMonth(), target.getUTCDate());
    const diffDays = Math.round((t1 - t0) / 86400000);
    if (diffDays === 0) return 'आज (Today)';
    if (diffDays === 1) return 'कल (Tomorrow)';
    if (diffDays > 1) return `${diffDays} दिन शेष`;
    return `${Math.abs(diffDays)}d ago`;
  }
}
