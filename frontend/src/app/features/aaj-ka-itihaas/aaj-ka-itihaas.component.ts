import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { PanchangService } from '../../core/services/panchang.service';
import { HistoricalIncident, DailyKatha } from '../../core/models/panchang.models';

@Component({
  selector: 'app-aaj-ka-itihaas',
  standalone: true,
  imports: [CommonModule, FormsModule],
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
        <input type="date" [(ngModel)]="currentDate" (change)="loadData()" class="f-date" />
      </div>

      <!-- Daily Dharmik Katha Highlight -->
      @if (dailyKatha) {
        <section class="katha-hero-card">
          <div class="katha-badge">✨ आज की प्रेरक पौराणिक कथा (Story of the Day)</div>
          <h2>{{ dailyKatha.title }}</h2>
          <span class="katha-characters">पात्र / ऋषि: {{ dailyKatha.deityOrSage }}</span>

          <p class="katha-body">{{ dailyKatha.storySummary }}</p>

          <div class="moral-box">
            <strong>सार एवं शिक्षा (Moral):</strong>
            <span>{{ dailyKatha.moralTeaching }}</span>
          </div>

          <div class="mantra-line">
            {{ dailyKatha.sanskritMantra }}
          </div>
        </section>
      }

      <!-- Historical Incidents Timeline -->
      <section class="timeline-section">
        <h2>🇮🇳 आज के दिन के ऐतिहासिक प्रसंग एवं घटनाएं (Historical Incidents)</h2>

        <div class="timeline-list">
          @for (inc of incidents; track inc.id) {
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
    .significance-box {
      font-size: 0.85rem;
      background: #fffaf5;
      padding: 0.6rem 0.8rem;
      border-radius: 6px;
      color: #7c2d12;
      border: 1px dashed #fed7aa;
    }
  `]
})
export class AajKaItihaasComponent implements OnInit {
  private panchangService = inject(PanchangService);

  currentDate: string = new Date().toISOString().substring(0, 10);
  incidents: HistoricalIncident[] = [];
  dailyKatha: DailyKatha | null = null;

  ngOnInit(): void {
    this.loadData();
  }

  loadData(): void {
    this.panchangService.getAajKaItihaas(this.currentDate).subscribe(inc => this.incidents = inc);
    this.panchangService.getDailyKatha(this.currentDate).subscribe(k => this.dailyKatha = k);
  }
}
