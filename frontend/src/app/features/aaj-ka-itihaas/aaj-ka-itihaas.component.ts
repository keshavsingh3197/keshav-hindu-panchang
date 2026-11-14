import { Component, OnInit, computed, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { finalize } from 'rxjs';
import { PanchangService } from '../../core/services/panchang.service';
import { EcosystemConfigService } from '../../core/services/ecosystem-config.service';
import { HistoricalIncident, DailyKatha, toLocalDateKey } from '../../core/models/panchang.models';
import { LoadingStateComponent } from '../shared/loading/loading-state.component';

@Component({
  selector: 'app-aaj-ka-itihaas',
  standalone: true,
  imports: [CommonModule, FormsModule, LoadingStateComponent],
  template: `
    <div class="itihaas-page">
      <div class="header-box">
        <span class="sub-pill">📜 दैनिक ऐतिहासिक एवं सांस्कृतिक पटल</span>
        <h1>आज का इतिहास (Today in Indian & Dharmik History)</h1>
        <p>
          भारतीय इतिहास, वैदिक गाथाएं, महान संतों का जीवन दर्शन, एवं स्वतंत्रता संग्राम की प्रमुख घटनाएं, 
          जो आज के दिन घटित हुईं। प्रतिदिन बदलती ऐतिहासिक एवं प्रेरक गाथाएं।
        </p>
      </div>

      <!-- Date Navigator -->
      <div class="date-strip">
        <label>तिथि का इतिहास देखें (Select Date):</label>
        <input type="date" [ngModel]="currentDate()" (ngModelChange)="currentDate.set($event); loadData()" class="f-date" />
        @if (studioPortalUrl(); as studioUrl) {
          <a [href]="studioUrl + '/automation?source=AajKaItihaas&date=' + currentDate()" target="_blank" rel="noopener" class="btn-studio-handoff">
            ⚡ Auto-Post History Reel in Studio →
          </a>
        }
      </div>

      <!-- Daily Dharmik Katha Highlight -->
      @if (loadingKatha() && !dailyKatha()) {
        <app-loading-state variant="rows" [count]="1" label="आज की प्रेरक पौराणिक कथा लोड हो रही है…" />
      }
      @if (dailyKatha(); as katha) {
        <section class="katha-hero-card">
          <div class="katha-badge">✨ आज की प्रेरक पौराणिक कथा (Story of the Day)</div>
          <h2>{{ katha.title }}</h2>
          <span class="katha-characters">पात्र / ऋषि: {{ katha.deityOrSage }}</span>

          <p class="katha-body">{{ katha.storySummary }}</p>

          <div class="moral-box">
            <strong>सार एवं शिक्षा (Moral):</strong>
            <span>{{ katha.moralTeaching }}</span>
          </div>

          <div class="mantra-line">
            {{ katha.sanskritMantra }}
          </div>
        </section>
      }

      <!-- Historical Incidents Timeline -->
      <section class="timeline-section">
        <div class="timeline-header-row">
          <h2>
            {{ hasExactDateMatches()
              ? '🇮🇳 आज के दिन के ऐतिहासिक प्रसंग एवं घटनाएं (' + filteredIncidents().length + ')'
              : '📜 सांस्कृतिक एवं ऐतिहासिक धरोहर संग्रह (' + filteredIncidents().length + ')' }}
          </h2>
          @if (categories().length > 1) {
            <div class="cat-pills">
              @for (cat of categories(); track cat) {
                <button
                  type="button"
                  class="cat-pill"
                  [class.active]="selectedCategory() === cat"
                  (click)="selectedCategory.set(cat)">
                  {{ cat }}
                </button>
              }
            </div>
          }
        </div>

        @if (loadingIncidents()) {
          <app-loading-state variant="rows" [count]="4" label="आज का इतिहास लोड हो रहा है… / Loading historical events…" />
        } @else if (filteredIncidents().length === 0) {
          <div class="empty-history">इस तिथि के लिए अभी कोई ऐतिहासिक प्रसंग दर्ज नहीं है।</div>
        } @else {
          <div class="timeline-list">
            @for (inc of filteredIncidents(); track inc.id) {
              <div class="timeline-item">
                <div class="year-marker">{{ inc.year }}</div>
                <div class="incident-card">
                  <div class="inc-head">
                    <span class="inc-cat">{{ inc.category }}</span>
                    <h3>{{ inc.title }}</h3>
                  </div>
                  <p class="inc-desc">{{ inc.description }}</p>
                  <div class="significance-box">
                    <strong>सांस्कृतिक महत्व:</strong> {{ inc.culturalSignificance }}
                  </div>
                </div>
              </div>
            }
          </div>
        }
      </section>
    </div>
  `,
  styles: [`
    .itihaas-page {
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
    .sub-pill {
      font-size: 0.8rem;
      font-weight: 700;
      color: #ea580c;
      text-transform: uppercase;
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

    .date-strip {
      background: #ffffff;
      border: 1px solid #fed7aa;
      padding: 0.85rem 1.25rem;
      border-radius: 12px;
      display: flex;
      align-items: center;
      gap: 1rem;
    }
    .date-strip label { font-size: 0.9rem; font-weight: 600; color: #78350f; }
    .f-date {
      padding: 0.4rem 0.75rem;
      border: 1px solid #fdba74;
      border-radius: 6px;
      font-size: 0.9rem;
    }

    .katha-hero-card {
      background: linear-gradient(135deg, #fff7ed 0%, #ffedd5 100%);
      border: 2px solid #fdba74;
      border-radius: 16px;
      padding: 2rem;
      display: flex;
      flex-direction: column;
      gap: 0.85rem;
      box-shadow: 0 4px 15px rgba(234, 88, 12, 0.08);
    }
    .katha-badge {
      font-size: 0.85rem;
      font-weight: 800;
      color: #c2410c;
      text-transform: uppercase;
    }
    .katha-hero-card h2 {
      margin: 0;
      font-size: 1.6rem;
      color: #431407;
      font-weight: 800;
    }
    .katha-characters {
      font-size: 0.9rem;
      color: #9a3412;
      font-weight: 600;
    }
    .katha-body {
      margin: 0;
      color: #334155;
      font-size: 1.05rem;
      line-height: 1.7;
    }
    .moral-box {
      background: #ffffff;
      border-left: 4px solid #ea580c;
      padding: 0.85rem 1.2rem;
      border-radius: 6px;
      font-size: 0.95rem;
      color: #431407;
    }
    .mantra-line {
      font-size: 1.05rem;
      font-weight: 700;
      color: #7c2d12;
      text-align: center;
      margin-top: 0.5rem;
      background: rgba(255, 255, 255, 0.6);
      padding: 0.5rem;
      border-radius: 6px;
    }

    .timeline-section h2 {
      font-size: 1.35rem;
      color: #431407;
      margin: 0 0 1.25rem;
    }
    .timeline-list {
      display: flex;
      flex-direction: column;
      gap: 1.5rem;
      position: relative;
      padding-left: 1.5rem;
      border-left: 3px solid #fed7aa;
    }
    .timeline-item {
      position: relative;
    }
    .year-marker {
      position: absolute;
      left: -2.35rem;
      top: 0.75rem;
      background: #ea580c;
      color: #ffffff;
      font-size: 0.75rem;
      font-weight: 800;
      padding: 0.2rem 0.45rem;
      border-radius: 4px;
    }
    .incident-card {
      background: #ffffff;
      border: 1px solid #fed7aa;
      border-radius: 12px;
      padding: 1.25rem;
      display: flex;
      flex-direction: column;
      gap: 0.6rem;
      box-shadow: 0 1px 3px rgba(0,0,0,0.02);
    }
    .inc-head {
      display: flex;
      flex-direction: column;
      gap: 0.2rem;
    }
    .inc-cat {
      font-size: 0.75rem;
      font-weight: 700;
      color: #c2410c;
      text-transform: uppercase;
    }
    .inc-head h3 {
      margin: 0;
      font-size: 1.2rem;
      color: #1e293b;
    }
    .inc-desc {
      margin: 0;
      color: #475569;
      font-size: 0.95rem;
      line-height: 1.55;
    }
    .timeline-header-row {
      display: flex;
      justify-content: space-between;
      align-items: center;
      flex-wrap: wrap;
      gap: 0.75rem;
      margin-bottom: 1.25rem;
    }
    .timeline-header-row h2 { margin: 0; font-size: 1.35rem; color: #431407; }
    .cat-pills { display: flex; gap: 0.45rem; flex-wrap: wrap; }
    .cat-pill {
      background: #ffffff;
      border: 1px solid #fed7aa;
      color: #78350f;
      padding: 0.32rem 0.75rem;
      border-radius: 999px;
      font-size: 0.8rem;
      font-weight: 600;
      cursor: pointer;
    }
    .cat-pill.active {
      background: #ea580c;
      color: #ffffff;
      border-color: #ea580c;
    }
    .empty-history {
      background: #fffaf5;
      border: 1px dashed #fdba74;
      border-radius: 12px;
      padding: 1.5rem;
      text-align: center;
      color: #9a3412;
    }
    .significance-box {
      font-size: 0.85rem;
      background: #fffaf5;
      padding: 0.6rem 0.8rem;
      border-radius: 6px;
      color: #7c2d12;
      border: 1px dashed #fed7aa;
    }
    .btn-studio-handoff {
      margin-left: auto;
      background: #7c2d12;
      color: #ffffff;
      padding: 0.45rem 0.9rem;
      border-radius: 8px;
      font-size: 0.84rem;
      font-weight: 700;
      text-decoration: none;
    }
    .btn-studio-handoff:hover { background: #431407; }
  `]
})
export class AajKaItihaasComponent implements OnInit {
  private panchangService = inject(PanchangService);
  private ecosystem = inject(EcosystemConfigService);

  readonly studioPortalUrl = this.ecosystem.studioPortalUrl;
  readonly currentDate = signal<string>(toLocalDateKey());
  readonly incidents = signal<HistoricalIncident[]>([]);
  readonly dailyKatha = signal<DailyKatha | null>(null);
  readonly loadingIncidents = signal<boolean>(true);
  readonly loadingKatha = signal<boolean>(true);
  readonly selectedCategory = signal<string>('All');

  readonly hasExactDateMatches = computed(() =>
    this.incidents().some(i => i.isExactDateMatch !== false)
  );

  readonly categories = computed(() => {
    const cats = new Set<string>();
    this.incidents().forEach(i => { if (i.category) cats.add(i.category); });
    return ['All', ...Array.from(cats)];
  });

  readonly filteredIncidents = computed(() => {
    const cat = this.selectedCategory();
    if (cat === 'All') return this.incidents();
    return this.incidents().filter(i => i.category === cat);
  });

  ngOnInit(): void {
    this.ecosystem.load();
    this.loadData();
  }

  loadData(): void {
    this.selectedCategory.set('All');
    this.loadingIncidents.set(true);
    this.loadingKatha.set(true);
    this.panchangService.getAajKaItihaas(this.currentDate())
      .pipe(finalize(() => this.loadingIncidents.set(false)))
      .subscribe(inc => this.incidents.set(inc));
    this.panchangService.getDailyKatha(this.currentDate())
      .pipe(finalize(() => this.loadingKatha.set(false)))
      .subscribe(k => this.dailyKatha.set(k));
  }
}
