import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { PanchangService } from '../../core/services/panchang.service';
import { PanchangCity, PanchangData, toLocalDateKey } from '../../core/models/panchang.models';
import { AdSlotComponent } from '../shared/ad-slot/ad-slot.component';

@Component({
  selector: 'app-daily-panchang',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink, AdSlotComponent],
  template: `
    <div class="panchang-page">
      @if (adminDenied()) {
        <div class="alert-denied">
          <span>🛡️ <strong>Admin Access Required:</strong> Only users with the <code>Admin</code> role can access the Panchang Admin Console.</span>
          <button type="button" (click)="adminDenied.set(false)" class="btn-close-alert">✕</button>
        </div>
      }

      <!-- Date & Location Bar -->
      <section class="control-strip">
        <div class="date-selector">
          <button (click)="changeDay(-1)" class="btn-nav">‹ पिछला दिन (Previous)</button>
          <input type="date" [ngModel]="selectedDate()" (ngModelChange)="selectedDate.set($event); loadPanchang()" class="date-picker" />
          <button (click)="today()" class="btn-today">आज (Today)</button>
          <button (click)="changeDay(1)" class="btn-nav">अगला दिन (Next) ›</button>
        </div>

        <div class="city-selector">
          <label>स्थान (City):</label>
          <select [ngModel]="selectedLocation()" (ngModelChange)="selectedLocation.set($event); loadPanchang()" class="city-select">
            @for (city of cities(); track city.id) {
              <option [value]="city.name">{{ city.labelHindi || city.name }}</option>
            }
          </select>
        </div>
      </section>

      <app-ad-slot placement="header" />

      @if (panchang(); as p) {
        <!-- Hero Header Card -->
        <section class="panchang-hero">
          <div class="hero-top">
            <div class="hero-title-box">
              <span class="vedic-date-pill">
                {{ p.paksha }} • पूर्णिमांत: {{ p.monthPurnimanta }} • अमांत: {{ p.monthAmanta }}@if (p.isAdhikaMaas) { • अधिक मास}
              </span>
              <h1>{{ p.formattedDate }}</h1>
              <p class="samvat-text">
                विक्रम संवत: <strong>{{ p.vikramSamvat }}</strong> | 
                शक संवत: <strong>{{ p.shakaSamvat }}</strong> | 
                स्थान: <strong>{{ p.location }}</strong>
              </p>
            </div>
            <div class="quick-muhurat-badge">
              <div class="q-label">अभिजीत मुहूर्त (शुभ)</div>
              <div class="q-val">{{ p.abhijitMuhurat }}</div>
            </div>
          </div>

          <!-- Daily Sanskrit Sutra -->
          <div class="sutra-banner">
            <div class="sutra-text">🕉️ {{ p.dailySanskritSutra }}</div>
            <div class="sutra-meaning">अर्थ: {{ p.dailySutraMeaning }}</div>
          </div>
        </section>

        <!-- 5 Limbs (Pancha-Anga) Cards -->
        <section class="section">
          <div class="section-title">
            <h2>पंचांग के पांच अंग (The 5 Core Limbs)</h2>
            <span class="sub-badge">{{ p.ayanamsaName || 'Lahiri Chitrapaksha' }} • {{ p.calculationNote || 'Sunrise Astronomical Ephemeris' }}</span>
          </div>

          <div class="grid grid-5">
            <div class="limb-card">
              <div class="limb-label">तिथि (Tithi)</div>
              <div class="limb-val">{{ p.tithi }}</div>
              <div class="limb-sub">{{ p.tithiEndTime }}</div>
            </div>

            <div class="limb-card">
              <div class="limb-label">नक्षत्र (Nakshatra)</div>
              <div class="limb-val">{{ p.nakshatra }}</div>
              <div class="limb-sub">{{ p.nakshatraEndTime }}</div>
            </div>

            <div class="limb-card">
              <div class="limb-label">योग (Yoga)</div>
              <div class="limb-val">{{ p.yoga }}</div>
              <div class="limb-sub">{{ p.yogaEndTime }}</div>
            </div>

            <div class="limb-card">
              <div class="limb-label">करण (Karana)</div>
              <div class="limb-val">{{ p.karana }}</div>
              <div class="limb-sub">{{ p.karanaEndTime }}</div>
            </div>

            <div class="limb-card">
              <div class="limb-label">वार (Vaara)</div>
              <div class="limb-val">{{ p.vaara }}</div>
              <div class="limb-sub">{{ p.dayOfWeek }}</div>
            </div>
          </div>
        </section>

        <!-- Sun, Moon & Auspicious Timings -->
        <div class="timing-grid">
          <!-- Celestial Timings -->
          <div class="card timing-card">
            <h3>☀️ सूर्य एवं चन्द्रमा का समय (Sun & Moon)</h3>
            <div class="two-col-list">
              <div class="time-row">
                <span class="t-name">सूर्योदय (Sunrise)</span>
                <span class="t-time">{{ p.sunrise }}</span>
              </div>
              <div class="time-row">
                <span class="t-name">सूर्यास्त (Sunset)</span>
                <span class="t-time">{{ p.sunset }}</span>
              </div>
              <div class="time-row">
                <span class="t-name">चन्द्रोदय (Moonrise)</span>
                <span class="t-time">{{ p.moonrise }}</span>
              </div>
              <div class="time-row">
                <span class="t-name">चन्द्रास्त (Moonset)</span>
                <span class="t-time">{{ p.moonset }}</span>
              </div>
              <div class="time-row">
                <span class="t-name">सूर्य राशि (Sun Sign)</span>
                <span class="t-time">{{ p.sunSign }}</span>
              </div>
              <div class="time-row">
                <span class="t-name">चन्द्र राशि (Moon Sign)</span>
                <span class="t-time">{{ p.moonSign }}</span>
              </div>
            </div>
          </div>

          <!-- Muhurat & Kaal -->
          <div class="card timing-card">
            <h3>⏳ शुभ एवं अशुभ मुहूर्त (Muhurat Timings)</h3>
            <div class="two-col-list">
              <div class="time-row auspicious">
                <span class="t-name">ब्रह्म मुहूर्त (Brahma Muhurat)</span>
                <span class="t-time">{{ p.brahmaMuhurat }}</span>
              </div>
              <div class="time-row auspicious">
                <span class="t-name">अभिजीत मुहूर्त (Abhijit Muhurat)</span>
                <span class="t-time">{{ p.abhijitMuhurat }}</span>
              </div>
              <div class="time-row auspicious">
                <span class="t-name">अमृत काल (Amrit Kaal)</span>
                <span class="t-time">{{ p.amritKaal }}</span>
              </div>
              <div class="time-row inauspicious">
                <span class="t-name">राहुकाल (Rahu Kaal - अशुभ)</span>
                <span class="t-time alert-tag">{{ p.rahuKaal }}</span>
              </div>
              <div class="time-row inauspicious">
                <span class="t-name">यमगण्ड (Yamaganda)</span>
                <span class="t-time">{{ p.yamaganda }}</span>
              </div>
              <div class="time-row inauspicious">
                <span class="t-name">गुलिक काल (Gulika Kaal)</span>
                <span class="t-time">{{ p.gulikaKaal }}</span>
              </div>
            </div>
          </div>
        </div>

        <!-- Choghadiya Section -->
        <section class="section">
          <div class="section-title">
            <h2>दिन का चौघड़िया (Day Choghadiya)</h2>
            <span class="sub-badge">For Auspicious Works & Travel</span>
          </div>

          <div class="choghadiya-row">
            @for (chog of p.dayChoghadiya; track chog.timeRange) {
              <div class="chog-card" [class.shubh-bg]="chog.type === 'Shubh'" [class.ashubh-bg]="chog.type === 'Ashubh'">
                <span class="chog-name">{{ chog.name }}</span>
                <span class="chog-time">{{ chog.timeRange }}</span>
                <span class="chog-type-pill">{{ chog.type }}</span>
              </div>
            }
          </div>
        </section>

        <!-- Quick Video Feature Preview -->
        <section class="video-cta-card">
          <div class="v-cta-content">
            <span class="v-tag">🎥 दैनिक वीडियो प्रसारण</span>
            <h3>आज का दैनिक राशिफल और पंचांग विचार वीडियो देखें</h3>
            <p>आज के ग्रहों की स्थिति, 12 राशियों का फल, और दैनिक उपाय वीडियो में देखें। सीधे यूट्यूब पर जुड़ें।</p>
            <a routerLink="/videos" class="btn-video">दैनिक वीडियो देखें (Watch Daily Videos) →</a>
          </div>
        </section>

        <app-ad-slot placement="infeed" />
      }
    </div>
  `,
  styles: [`
    .panchang-page {
      max-width: 1300px;
      margin: 0 auto;
      padding: 1.5rem 1rem 3rem;
      display: flex;
      flex-direction: column;
      gap: 1.75rem;
    }
    .control-strip {
      background: #ffffff;
      border: 1px solid #fed7aa;
      border-radius: 12px;
      padding: 0.85rem 1.25rem;
      display: flex;
      justify-content: space-between;
      align-items: center;
      gap: 1rem;
      flex-wrap: wrap;
    }
    .date-selector {
      display: flex;
      align-items: center;
      gap: 0.5rem;
      flex-wrap: wrap;
    }
    .date-picker, .city-select {
      padding: 0.45rem 0.75rem;
      border: 1px solid #fdba74;
      border-radius: 6px;
      font-size: 0.9rem;
      font-weight: 500;
    }
    .btn-nav, .btn-today {
      background: #fff7ed;
      color: #9a3412;
      border: 1px solid #fed7aa;
      padding: 0.45rem 0.8rem;
      border-radius: 6px;
      font-weight: 600;
      font-size: 0.85rem;
      cursor: pointer;
    }
    .btn-today { background: #ea580c; color: white; border-color: #ea580c; }
    .city-selector {
      display: flex;
      align-items: center;
      gap: 0.5rem;
    }
    .city-selector label { font-size: 0.85rem; font-weight: 600; color: #78350f; }

    .panchang-hero {
      background: linear-gradient(135deg, #7c2d12 0%, #431407 100%);
      color: #ffffff;
      border-radius: 16px;
      padding: 2rem;
      display: flex;
      flex-direction: column;
      gap: 1.5rem;
      box-shadow: 0 10px 25px -5px rgba(67, 20, 7, 0.3);
    }
    .hero-top {
      display: flex;
      justify-content: space-between;
      align-items: center;
      gap: 1.5rem;
      flex-wrap: wrap;
    }
    .vedic-date-pill {
      background: rgba(254, 215, 170, 0.2);
      color: #fed7aa;
      padding: 0.25rem 0.75rem;
      border-radius: 9999px;
      font-size: 0.85rem;
      font-weight: 700;
      display: inline-block;
      margin-bottom: 0.5rem;
    }
    .hero-title-box h1 {
      margin: 0 0 0.4rem;
      font-size: 2rem;
      font-weight: 800;
    }
    .samvat-text {
      margin: 0;
      color: #fed7aa;
      font-size: 0.95rem;
    }
    .quick-muhurat-badge {
      background: rgba(255, 255, 255, 0.1);
      border: 1px solid rgba(254, 215, 170, 0.3);
      padding: 1rem 1.4rem;
      border-radius: 12px;
      text-align: center;
    }
    .q-label { font-size: 0.8rem; color: #fdba74; font-weight: 700; text-transform: uppercase; }
    .q-val { font-size: 1.35rem; font-weight: 800; color: #ffffff; margin-top: 0.2rem; }

    .sutra-banner {
      background: rgba(0, 0, 0, 0.25);
      border-left: 4px solid #f97316;
      padding: 1rem 1.25rem;
      border-radius: 8px;
    }
    .sutra-text {
      font-size: 1.15rem;
      font-weight: 700;
      color: #fef08a;
      margin-bottom: 0.3rem;
    }
    .sutra-meaning {
      font-size: 0.9rem;
      color: #ffedd5;
    }

    .section {
      display: flex;
      flex-direction: column;
      gap: 1rem;
    }
    .section-title {
      display: flex;
      justify-content: space-between;
      align-items: center;
    }
    .section-title h2 {
      margin: 0;
      font-size: 1.3rem;
      color: #431407;
      font-weight: 800;
    }
    .sub-badge {
      font-size: 0.8rem;
      background: #ffedd5;
      color: #c2410c;
      padding: 0.2rem 0.6rem;
      border-radius: 9999px;
      font-weight: 600;
    }
    .grid { display: grid; gap: 1rem; }
    .grid-5 { grid-template-columns: repeat(auto-fit, minmax(200px, 1fr)); }

    .limb-card {
      background: #ffffff;
      border: 1px solid #fed7aa;
      border-radius: 12px;
      padding: 1.25rem 1rem;
      text-align: center;
      box-shadow: 0 1px 3px rgba(0,0,0,0.02);
      transition: transform 0.2s;
    }
    .limb-card:hover { transform: translateY(-2px); }
    .limb-label {
      font-size: 0.85rem;
      font-weight: 700;
      color: #c2410c;
      margin-bottom: 0.4rem;
    }
    .limb-val {
      font-size: 1.25rem;
      font-weight: 800;
      color: #431407;
      margin-bottom: 0.3rem;
    }
    .limb-sub {
      font-size: 0.8rem;
      color: #78350f;
    }

    .timing-grid {
      display: grid;
      grid-template-columns: 1fr 1fr;
      gap: 1.25rem;
    }
    @media (max-width: 800px) {
      .timing-grid { grid-template-columns: 1fr; }
    }
    .card {
      background: #ffffff;
      border: 1px solid #fed7aa;
      border-radius: 12px;
      padding: 1.25rem;
      box-shadow: 0 2px 8px rgba(124, 45, 18, 0.04);
      transition: box-shadow 0.2s ease, transform 0.2s ease;
    }
    .card:hover {
      box-shadow: 0 8px 20px rgba(124, 45, 18, 0.08);
      transform: translateY(-1px);
    }
    .card h3 {
      margin: 0 0 1rem;
      font-size: 1.15rem;
      color: #7c2d12;
    }
    .two-col-list {
      display: flex;
      flex-direction: column;
      gap: 0.75rem;
    }
    .time-row {
      display: flex;
      justify-content: space-between;
      align-items: center;
      padding: 0.45rem 0.6rem;
      border-radius: 6px;
      background: #fffaf5;
      font-size: 0.9rem;
    }
    .time-row.auspicious { background: #f0fdf4; }
    .time-row.inauspicious { background: #fef2f2; }
    .t-name { font-weight: 600; color: #334155; }
    .t-time { font-weight: 700; color: #0f172a; }
    .alert-tag { color: #dc2626; }

    .choghadiya-row {
      display: grid;
      grid-template-columns: repeat(auto-fit, minmax(130px, 1fr));
      gap: 0.75rem;
    }
    .chog-card {
      padding: 0.85rem 0.6rem;
      border-radius: 10px;
      text-align: center;
      display: flex;
      flex-direction: column;
      gap: 0.25rem;
      border: 1px solid #fed7aa;
    }
    .shubh-bg { background: #f0fdf4; border-color: #bbf7d0; }
    .ashubh-bg { background: #fef2f2; border-color: #fecaca; }
    .chog-name { font-weight: 700; font-size: 0.85rem; color: #1e293b; }
    .chog-time { font-size: 0.75rem; color: #64748b; }
    .chog-type-pill {
      font-size: 0.7rem;
      font-weight: 700;
      text-transform: uppercase;
      margin-top: 0.2rem;
    }
    .shubh-bg .chog-type-pill { color: #16a34a; }
    .ashubh-bg .chog-type-pill { color: #dc2626; }

    .video-cta-card {
      background: linear-gradient(135deg, #ea580c 0%, #9a3412 100%);
      color: #ffffff;
      padding: 1.75rem;
      border-radius: 14px;
      display: flex;
      justify-content: space-between;
      align-items: center;
      gap: 1.5rem;
      flex-wrap: wrap;
    }
    .v-tag { font-size: 0.8rem; font-weight: 700; color: #fef08a; text-transform: uppercase; }
    .v-cta-content h3 { margin: 0.4rem 0; font-size: 1.35rem; }
    .v-cta-content p { margin: 0; font-size: 0.95rem; color: #ffedd5; max-width: 650px; }
    .btn-video {
      background: #ffffff;
      color: #7c2d12;
      padding: 0.7rem 1.3rem;
      border-radius: 8px;
      font-weight: 700;
      text-decoration: none;
      font-size: 0.95rem;
      white-space: nowrap;
      transition: background 0.2s;
    }
    .btn-video:hover { background: #fff7ed; }
    .alert-denied { background: #fef2f2; border: 1px solid #fecaca; color: #991b1b; padding: 0.75rem 1rem; border-radius: 10px; display: flex; justify-content: space-between; align-items: center; font-size: 0.9rem; }
    .btn-close-alert { background: transparent; border: none; color: #991b1b; font-weight: 700; cursor: pointer; font-size: 1rem; }
  `]
})
export class DailyPanchangComponent implements OnInit {
  private panchangService = inject(PanchangService);
  private route = inject(ActivatedRoute);

  readonly selectedDate = signal<string>(toLocalDateKey());
  readonly selectedLocation = signal<string>('New Delhi, India');
  readonly cities = signal<PanchangCity[]>([]);
  readonly panchang = signal<PanchangData | null>(null);
  readonly adminDenied = signal(false);

  ngOnInit(): void {
    this.route.queryParamMap.subscribe(params => {
      this.adminDenied.set(params.get('denied') === 'admin');
    });
    this.panchangService.getCities().subscribe(list => {
      this.cities.set(list);
      if (list.length > 0 && !list.some(c => c.name === this.selectedLocation())) {
        this.selectedLocation.set(list[0].name);
      }
    });
    this.loadPanchang();
  }

  loadPanchang(): void {
    this.panchangService.getDailyPanchang(this.selectedDate(), this.selectedLocation())
      .subscribe(data => this.panchang.set(data));
  }

  changeDay(days: number): void {
    const [y, m, d] = this.selectedDate().split('-').map(Number);
    const current = new Date(y, m - 1, d + days);
    this.selectedDate.set(toLocalDateKey(current));
    this.loadPanchang();
  }

  today(): void {
    this.selectedDate.set(toLocalDateKey());
    this.loadPanchang();
  }
}
