import { Component, OnInit, computed, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { Subject, debounceTime, distinctUntilChanged, finalize, switchMap } from 'rxjs';
import { PanchangService } from '../../core/services/panchang.service';
import { EcosystemConfigService } from '../../core/services/ecosystem-config.service';
import { LocaleService } from '../../core/services/locale.service';
import { PanchangCity, PanchangData, toLocalDateKey } from '../../core/models/panchang.models';
import { AdSlotComponent } from '../shared/ad-slot/ad-slot.component';
import { LoadingStateComponent } from '../shared/loading/loading-state.component';
import { ClockFormatMode, convertDisplayString } from './panchang-clock.util';
import { PanchangTimelineChartComponent } from './panchang-timeline-chart.component';
import { PanchangExtendedSectionsComponent } from './panchang-extended-sections.component';
import { InfoPopupContent, PanchangInfoModalComponent } from './panchang-info-modal.component';

const POPULAR_CITIES = [
  'New Delhi, India',
  'Varanasi, India',
  'Mumbai, India',
  'Bengaluru, India',
  'Ujjain, India',
  'London, UK',
  'New York City, USA'
];

@Component({
  selector: 'app-daily-panchang',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    RouterLink,
    AdSlotComponent,
    LoadingStateComponent,
    PanchangTimelineChartComponent,
    PanchangExtendedSectionsComponent,
    PanchangInfoModalComponent
  ],
  template: `
    <div class="panchang-page">
      @if (adminDenied()) {
        <div class="alert-denied">
          <span>🛡️ <strong>Admin Access Required:</strong> Only users with the <code>Admin</code> role can access the Panchang Admin Console.</span>
          <button type="button" (click)="adminDenied.set(false)" class="btn-close-alert">✕</button>
        </div>
      }

      <!-- Date, Global City & Instant Toolbar -->
      <section class="control-strip">
        <div class="control-row">
          <div class="date-selector">
            <button type="button" (click)="changeDay(-1)" class="btn-nav">‹ Prev</button>
            <input type="date" [ngModel]="selectedDate()" (ngModelChange)="selectedDate.set($event); loadPanchang()" class="date-picker" />
            <button type="button" (click)="today()" class="btn-today">Today</button>
            <button type="button" (click)="changeDay(1)" class="btn-nav">Next ›</button>
            <button type="button" (click)="showMonthJump.set(!showMonthJump())" class="btn-nav">📅 {{ showMonthJump() ? 'Hide' : 'Month/Year' }}</button>
          </div>

          <div class="city-search-wrap">
            <label for="global-city-input">📍 Location:</label>
            <div class="city-input-box">
              <input
                id="global-city-input"
                type="text"
                class="city-input"
                [ngModel]="citySearchText()"
                (ngModelChange)="onCitySearchType($event)"
                (focus)="dropdownOpen.set(true)"
                placeholder="Search any city worldwide (Mumbai, New York, Ujjain…)" />
              @if (searchingCities()) {
                <span class="mini-spin inline-spin" aria-hidden="true"></span>
              }
              @if (dropdownOpen() && citySuggestions().length > 0) {
                <div class="city-dropdown" role="listbox">
                  @for (c of citySuggestions(); track c.id + c.latitude) {
                    <button type="button" class="city-option" (click)="selectCity(c)">
                      <span class="co-main">{{ c.name }}</span>
                      <span class="co-meta">{{ c.latitude.toFixed(2) }}°, {{ c.longitude.toFixed(2) }}° • {{ c.timeZoneId || 'Local' }}</span>
                    </button>
                  }
                </div>
              }
            </div>
            <button type="button" class="btn-nav" (click)="showCustomCoords.set(!showCustomCoords())" title="Enter custom latitude & longitude">
              🌐 {{ showCustomCoords() ? 'Close Coords' : 'Lat/Lon' }}
            </button>
          </div>
        </div>

        <!-- Quick City Chips + Clock Format + Tradition Mode -->
        <div class="toolbar-subrow">
          <div class="quick-cities">
            @for (cityLabel of popularCities; track cityLabel) {
              <button
                type="button"
                class="pill-toggle"
                [class.active]="selectedLocation() === cityLabel && !customCoords()"
                (click)="pickQuickCity(cityLabel)">
                {{ cityLabel.split(',')[0] }}
              </button>
            }
          </div>

          <div class="format-switches">
            <div class="seg-group" aria-label="Clock format">
              @for (m of clockModes; track m.value) {
                <button type="button" class="seg-btn" [class.active]="clockMode() === m.value" (click)="setClockMode(m.value)">{{ m.label }}</button>
              }
            </div>
            <div class="seg-group" aria-label="Lunar month system">
              <button type="button" class="seg-btn" [class.active]="schoolMode() === 'purnimanta'" (click)="setSchoolMode('purnimanta')">Purnimanta</button>
              <button type="button" class="seg-btn" [class.active]="schoolMode() === 'amanta'" (click)="setSchoolMode('amanta')">Amanta</button>
            </div>
          </div>
        </div>

        @if (showMonthJump()) {
          <div class="jump-strip">
            <label>Year:
              <input type="number" class="jump-input" [ngModel]="selectedYear()" (ngModelChange)="jumpToYear($event)" min="1800" max="2200" />
            </label>
            <div class="months-grid">
              @for (mn of monthNames; track mn; let idx = $index) {
                <button type="button" class="pill-toggle" [class.active]="selectedMonthIndex() === idx" (click)="jumpToMonth(idx)">{{ mn }}</button>
              }
            </div>
          </div>
        }

        @if (showCustomCoords()) {
          <div class="coords-strip">
            <input type="text" [(ngModel)]="coordLabel" placeholder="Label (e.g. Kedarnath Shrine)" class="date-picker" />
            <input type="number" step="0.0001" [(ngModel)]="coordLat" placeholder="Latitude (e.g. 30.7346)" class="jump-input wide-num" />
            <input type="number" step="0.0001" [(ngModel)]="coordLon" placeholder="Longitude (e.g. 79.0669)" class="jump-input wide-num" />
            <input type="text" [(ngModel)]="coordTz" placeholder="IANA Tz (optional e.g. Asia/Kolkata)" class="date-picker" />
            <button type="button" class="btn-today" (click)="applyCustomCoords()">Apply Location</button>
          </div>
        }
      </section>

      <app-ad-slot placement="header" />

      @if (!panchang() && (loading() || error())) {
        <app-loading-state
          variant="panchang"
          [error]="error()"
          label="आज का वैदिक पंचांग लोड हो रहा है… / Loading Daily Panchang & Muhurat…"
          (retry)="loadPanchang()" />
      }

      @if (panchang() && loading()) {
        <div class="refreshing-pill" role="status" aria-live="polite">
          <span class="mini-spin" aria-hidden="true"></span>
          <span>Refreshing astronomical Panchang for {{ selectedDate() }} ({{ selectedLocation() }})…</span>
        </div>
      }

      @if (panchang(); as p) {
        <!-- Hero Header Card -->
        <section class="panchang-hero">
          <div class="hero-top">
            <div class="hero-title-box">
              <div class="hero-badges">
                <span class="vedic-date-pill">
                  {{ schoolMode() === 'amanta' ? (p.extended?.amantaDayBanner || p.monthAmanta) : (p.extended?.purnimantaDayBanner || p.monthPurnimanta) }} • {{ p.paksha }}
                </span>
                @if (p.isAdhikaMaas) {
                  <span class="vedic-date-pill adhika">अधिक मास (Adhika Maas)</span>
                }
                @if (p.locationMeta; as lm) {
                  <span class="vedic-date-pill meta-loc">
                    {{ lm.latitude.toFixed(2) }}°, {{ lm.longitude.toFixed(2) }}° • {{ lm.timeZoneId }} ({{ formatOffset(lm.effectiveUtcOffsetHours) }}{{ lm.isDaylightSavingActive ? ' DST' : '' }})
                  </span>
                }
              </div>
              <h1>{{ p.formattedDate }}</h1>
              <p class="samvat-text">
                विक्रम संवत: <strong>{{ p.vikramSamvat }}</strong> |
                शक संवत: <strong>{{ p.shakaSamvat }}</strong> |
                पूर्णिमांत: <strong>{{ p.monthPurnimanta }}</strong> |
                अमांत: <strong>{{ p.monthAmanta }}</strong>
              </p>
            </div>
            <div class="quick-muhurat-badge">
              <div class="q-label">अभिजीत मुहूर्त (Abhijit)</div>
              <div class="q-val">{{ convertStr(p.abhijitMuhurat) }}</div>
            </div>
          </div>

          <div class="sutra-banner">
            <div class="sutra-text">🕉️ {{ p.dailySanskritSutra }}</div>
            <div class="sutra-meaning">अर्थ: {{ p.dailySutraMeaning }}</div>
          </div>
        </section>

        <!-- 5 Limbs (Pancha-Anga) Cards with (i) Popups & Next Transitions -->
        <section class="section">
          <div class="section-title">
            <h2>{{ locale.t('panchang.limbs.heading') }}</h2>
            <span class="sub-badge">{{ p.ayanamsaName || 'Lahiri Chitrapaksha' }} • {{ p.calculationNote }}</span>
          </div>

          <div class="grid grid-5">
            <div class="limb-card">
              <div class="limb-head-row">
                <span class="limb-label">{{ locale.t('panchang.card.tithi') }}</span>
                <button type="button" class="info-dot" (click)="openInfo('tithi', p)" title="View Tithi Swami & % elapsed">ⓘ</button>
              </div>
              <div class="limb-val">{{ p.tithi }}</div>
              <div class="limb-sub">{{ convertStr(p.tithiEndTime) }}</div>
              @if (p.extended) {
                <div class="limb-next">Next: {{ p.extended.nextTithi }} • {{ p.extended.tithiPercentElapsedAtSunrise }}% over</div>
              }
            </div>

            <div class="limb-card">
              <div class="limb-head-row">
                <span class="limb-label">{{ locale.t('panchang.card.nakshatra') }}</span>
                <button type="button" class="info-dot" (click)="openInfo('nakshatra', p)" title="View Nakshatra Deity, Pada & % elapsed">ⓘ</button>
              </div>
              <div class="limb-val">{{ p.nakshatra }}</div>
              <div class="limb-sub">{{ convertStr(p.nakshatraEndTime) }}</div>
              @if (p.extended) {
                <div class="limb-next">Next: {{ p.extended.nextNakshatra }} • {{ p.extended.nakshatraPercentElapsedAtSunrise }}% over</div>
              }
            </div>

            <div class="limb-card">
              <div class="limb-head-row">
                <span class="limb-label">{{ locale.t('panchang.card.yoga') }}</span>
                <button type="button" class="info-dot" (click)="openInfo('yoga', p)" title="View Yoga % elapsed at Sunrise">ⓘ</button>
              </div>
              <div class="limb-val">{{ p.yoga }}</div>
              <div class="limb-sub">{{ convertStr(p.yogaEndTime) }}</div>
              @if (p.extended) {
                <div class="limb-next">Next: {{ p.extended.nextYoga }} • {{ p.extended.yogaPercentElapsedAtSunrise }}% over</div>
              }
            </div>

            <div class="limb-card">
              <div class="limb-head-row">
                <span class="limb-label">{{ locale.t('panchang.card.karana') }}</span>
                <button type="button" class="info-dot" (click)="openInfo('karana', p)" title="View All Day Karana transitions">ⓘ</button>
              </div>
              <div class="limb-val">{{ p.karana }}</div>
              <div class="limb-sub">{{ convertStr(p.karanaEndTime) }}</div>
              @if (p.extended) {
                <div class="limb-next">Next: {{ p.extended.nextKarana }}</div>
              }
            </div>

            <div class="limb-card">
              <div class="limb-head-row">
                <span class="limb-label">{{ locale.t('panchang.card.vaara') }}</span>
                <button type="button" class="info-dot" (click)="openInfo('vaara', p)" title="View Weekday Shool & Anandadi Yoga">ⓘ</button>
              </div>
              <div class="limb-val">{{ p.vaara }}</div>
              <div class="limb-sub">{{ p.paksha }}</div>
              @if (p.extended) {
                <div class="limb-next">{{ p.extended.anandadiYoga }}</div>
              }
            </div>
          </div>
        </section>

        <!-- Interactive 24-Hour Sunrise-to-Sunrise Panchang Graph -->
        @if (p.timeline) {
          <app-panchang-timeline-chart
            [timeline]="p.timeline"
            [clockMode]="clockMode()"
            [visible]="showChart()"
            [moonriseMinutes]="p.extended?.moonriseMinutes"
            [moonsetMinutes]="p.extended?.moonsetMinutes"
            (toggleVisibility)="toggleChart()" />
        }

        <!-- Sun, Moon & Auspicious/Inauspicious Summary -->
        <div class="timing-grid">
          <div class="card timing-card">
            <div class="card-head-flex">
              <h3>{{ locale.t('panchang.sunMoon.heading') }}</h3>
              <button type="button" class="info-dot" (click)="openInfo('sunrise', p)" title="Civil, Nautical & Astronomical Dawn">ⓘ</button>
            </div>
            <div class="two-col-list">
              <div class="time-row"><span class="t-name">{{ locale.t('panchang.sun.sunrise') }}</span><span class="t-time">{{ convertStr(p.sunrise) }}</span></div>
              <div class="time-row"><span class="t-name">{{ locale.t('panchang.sun.sunset') }}</span><span class="t-time">{{ convertStr(p.sunset) }}</span></div>
              <div class="time-row"><span class="t-name">{{ locale.t('panchang.sun.moonrise') }}</span><span class="t-time">{{ convertStr(p.moonrise) }}</span></div>
              <div class="time-row"><span class="t-name">{{ locale.t('panchang.sun.moonset') }}</span><span class="t-time">{{ convertStr(p.moonset) }}</span></div>
              <div class="time-row"><span class="t-name">{{ locale.t('panchang.sun.sunSign') }}</span><span class="t-time">{{ p.sunSign }}</span></div>
              <div class="time-row"><span class="t-name">{{ locale.t('panchang.sun.moonSign') }}</span><span class="t-time">{{ p.moonSign }}</span></div>
            </div>
          </div>

          <div class="card timing-card">
            <div class="card-head-flex">
              <h3>{{ locale.t('panchang.muhurat.heading') }}</h3>
              <button type="button" class="info-dot" (click)="openInfo('muhurat', p)" title="Muhurat & Dosha calculation notes">ⓘ</button>
            </div>
            <div class="two-col-list">
              <div class="time-row auspicious"><span class="t-name">{{ locale.t('panchang.card.brahma') }}</span><span class="t-time">{{ convertStr(p.brahmaMuhurat) }}</span></div>
              <div class="time-row auspicious"><span class="t-name">{{ locale.t('panchang.card.abhijit') }}</span><span class="t-time">{{ convertStr(p.abhijitMuhurat) }}</span></div>
              <div class="time-row auspicious"><span class="t-name">{{ locale.t('panchang.card.amrit') }}</span><span class="t-time">{{ convertStr(p.amritKaal) }}</span></div>
              <div class="time-row inauspicious"><span class="t-name">{{ locale.t('panchang.card.rahuKaal') }}</span><span class="t-time alert-tag">{{ convertStr(p.rahuKaal) }}</span></div>
              <div class="time-row inauspicious"><span class="t-name">{{ locale.t('panchang.card.yamaganda') }}</span><span class="t-time">{{ convertStr(p.yamaganda) }}</span></div>
              <div class="time-row inauspicious"><span class="t-name">{{ locale.t('panchang.card.gulika') }}</span><span class="t-time">{{ convertStr(p.gulikaKaal) }}</span></div>
            </div>
          </div>
        </div>

        <!-- Expandable "More" Tables, Hora, Choghadiya, Padas, Udaya Lagna & Calculation Credits -->
        <app-panchang-extended-sections [panchang]="p" [clockMode]="clockMode()" />

        <!-- Quick Video Feature Preview -->
        <section class="video-cta-card">
          <div class="v-cta-content">
            <span class="v-tag">{{ locale.t('panchang.cta.tag') }}</span>
            <h3>{{ locale.t('panchang.cta.title') }}</h3>
            <p>{{ locale.t('panchang.cta.desc') }}</p>
            <div class="v-cta-actions">
              <a routerLink="/videos" class="btn-video">{{ locale.t('panchang.cta.btn') }}</a>
              @if (studioPortalUrl(); as studioUrl) {
                <a [href]="studioUrl + '/automation?source=HinduPanchang&date=' + selectedDate()" target="_blank" rel="noopener" class="btn-video btn-video-studio">
                  ⚡ Auto-Post / AI Video in Studio →
                </a>
              }
            </div>
          </div>
        </section>

        <app-ad-slot placement="infeed" />
      }

      <app-panchang-info-modal [info]="activeInfo()" (closed)="activeInfo.set(null)" />
    </div>
  `,
  styles: [`
    .panchang-page { max-width: 1300px; margin: 0 auto; padding: 1.4rem 1rem 3rem; display: flex; flex-direction: column; gap: 1.3rem; }
    .control-strip, .card, .limb-card { background: #fff; border: 1px solid #fed7aa; border-radius: 12px; }
    .control-strip { padding: 0.9rem 1.1rem; display: flex; flex-direction: column; gap: 0.7rem; }
    .control-row, .toolbar-subrow, .hero-top, .section-title, .limb-head-row, .card-head-flex, .alert-denied { display: flex; justify-content: space-between; align-items: center; gap: 0.75rem; flex-wrap: wrap; }
    .date-selector, .city-search-wrap, .quick-cities, .format-switches, .coords-strip { display: flex; align-items: center; gap: 0.4rem; flex-wrap: wrap; }
    .date-picker, .city-input, .jump-input { padding: 0.4rem 0.6rem; border: 1px solid #fdba74; border-radius: 7px; font: 600 0.86rem inherit; color: #1e293b; }
    .city-input-box { position: relative; min-width: min(310px, 75vw); }
    .city-input { width: 100%; box-sizing: border-box; padding-right: 1.8rem; }
    .inline-spin { position: absolute; right: 8px; top: 10px; }
    .city-dropdown { position: absolute; left: 0; right: 0; top: 100%; z-index: 40; margin-top: 4px; background: #fff; border: 1px solid #fdba74; border-radius: 10px; max-height: 260px; overflow: auto; box-shadow: 0 10px 24px rgba(67, 20, 7, 0.14); }
    .city-option { width: 100%; border: none; border-bottom: 1px solid #fff7ed; background: #fff; padding: 0.5rem 0.75rem; text-align: left; display: flex; flex-direction: column; gap: 2px; cursor: pointer; }
    .city-option:hover { background: #fff7ed; }
    .co-main { font-weight: 700; font-size: 0.86rem; color: #431407; }
    .co-meta { font-size: 0.74rem; color: #78350f; }

    .btn-nav, .btn-today { background: #fff7ed; color: #9a3412; border: 1px solid #fed7aa; padding: 0.4rem 0.7rem; border-radius: 7px; font-weight: 700; font-size: 0.82rem; cursor: pointer; }
    .btn-today, .pill-toggle.active, .seg-btn.active { background: #ea580c; color: #fff; border-color: #ea580c; }
    .pill-toggle, .seg-btn { background: #fffaf5; color: #7c2d12; border: 1px solid #fed7aa; padding: 0.24rem 0.58rem; border-radius: 999px; font-size: 0.76rem; font-weight: 700; cursor: pointer; }
    .seg-group { display: inline-flex; gap: 3px; background: #fff7ed; padding: 2px; border-radius: 999px; border: 1px solid #fed7aa; }
    .jump-strip, .months-grid { display: flex; flex-wrap: wrap; align-items: center; gap: 0.4rem; padding-top: 0.45rem; border-top: 1px dashed #fed7aa; }
    .jump-input { width: 84px; }
    .wide-num { width: 145px; }

    .panchang-hero { background: linear-gradient(135deg, #7c2d12 0%, #431407 100%); color: #fff; border-radius: 16px; padding: 1.55rem; display: flex; flex-direction: column; gap: 1.1rem; }
    .hero-badges { display: flex; flex-wrap: wrap; gap: 0.4rem; margin-bottom: 0.35rem; }
    .vedic-date-pill { background: rgba(254, 215, 170, 0.2); color: #fed7aa; padding: 0.2rem 0.65rem; border-radius: 999px; font-size: 0.78rem; font-weight: 700; }
    .vedic-date-pill.adhika { background: #dc2626; color: #fff; }
    .vedic-date-pill.meta-loc { background: rgba(255, 255, 255, 0.12); color: #fef08a; }
    .hero-title-box h1 { margin: 0 0 0.25rem; font-size: 1.75rem; font-weight: 800; }
    .samvat-text { margin: 0; color: #fed7aa; font-size: 0.88rem; }
    .quick-muhurat-badge { background: rgba(255, 255, 255, 0.1); border: 1px solid rgba(254, 215, 170, 0.3); padding: 0.75rem 1.1rem; border-radius: 12px; text-align: center; }
    .q-label { font-size: 0.74rem; color: #fdba74; font-weight: 700; text-transform: uppercase; }
    .q-val { font-size: 1.12rem; font-weight: 800; color: #fff; margin-top: 0.2rem; }
    .sutra-banner { background: rgba(0, 0, 0, 0.25); border-left: 4px solid #f97316; padding: 0.8rem 1rem; border-radius: 8px; }
    .sutra-text { font-size: 1.04rem; font-weight: 700; color: #fef08a; margin-bottom: 0.2rem; }
    .sutra-meaning { font-size: 0.85rem; color: #ffedd5; }

    .section { display: flex; flex-direction: column; gap: 0.8rem; }
    .section-title h2 { margin: 0; font-size: 1.2rem; color: #431407; font-weight: 800; }
    .sub-badge { font-size: 0.74rem; background: #ffedd5; color: #c2410c; padding: 0.2rem 0.58rem; border-radius: 999px; font-weight: 600; }
    .grid-5 { display: grid; gap: 0.8rem; grid-template-columns: repeat(auto-fit, minmax(200px, 1fr)); }
    .limb-card { padding: 0.95rem 0.8rem; text-align: center; }
    .limb-label { font-size: 0.8rem; font-weight: 700; color: #c2410c; }
    .limb-val { font-size: 1.15rem; font-weight: 800; color: #431407; margin: 0.22rem 0; }
    .limb-sub { font-size: 0.78rem; color: #78350f; font-weight: 600; }
    .limb-next { font-size: 0.72rem; color: #9a3412; margin-top: 0.35rem; background: #fff7ed; padding: 2px 6px; border-radius: 6px; }
    .info-dot { border: 1px solid #fdba74; background: #fff7ed; color: #9a3412; border-radius: 999px; width: 22px; height: 22px; font-size: 0.76rem; font-weight: 800; cursor: pointer; }
    .info-dot:hover { background: #ea580c; color: #fff; }

    .timing-grid { display: grid; grid-template-columns: 1fr 1fr; gap: 1rem; }
    @media (max-width: 800px) { .timing-grid { grid-template-columns: 1fr; } }
    .card { padding: 1.05rem; }
    .card h3 { margin: 0; font-size: 1.06rem; color: #7c2d12; }
    .two-col-list { display: flex; flex-direction: column; gap: 0.5rem; margin-top: 0.7rem; }
    .time-row { display: flex; justify-content: space-between; align-items: center; padding: 0.38rem 0.55rem; border-radius: 6px; background: #fffaf5; font-size: 0.85rem; }
    .time-row.auspicious { background: #f0fdf4; }
    .time-row.inauspicious { background: #fef2f2; }
    .t-name { font-weight: 600; color: #334155; }
    .t-time { font-weight: 700; color: #0f172a; }
    .alert-tag { color: #dc2626; }
    .video-cta-card { background: linear-gradient(135deg, #ea580c, #9a3412); color: #fff; padding: 1.35rem; border-radius: 14px; }
    .v-tag { font: 700 0.75rem inherit; color: #fef08a; text-transform: uppercase; }
    .v-cta-content h3 { margin: 0.3rem 0; font-size: 1.2rem; }
    .v-cta-content p { margin: 0; font-size: 0.88rem; color: #ffedd5; max-width: 650px; }
    .v-cta-actions { display: flex; flex-wrap: wrap; gap: 0.65rem; margin-top: 0.75rem; }
    .btn-video { background: #fff; color: #7c2d12; padding: 0.55rem 1.05rem; border-radius: 8px; font: 700 0.88rem inherit; text-decoration: none; }
    .btn-video-studio { background: #431407; color: #fef08a; border: 1px solid rgba(254, 240, 138, 0.45); }
    .alert-denied { background: #fef2f2; border: 1px solid #fecaca; color: #991b1b; padding: 0.65rem 0.9rem; border-radius: 10px; }
    .btn-close-alert { background: transparent; border: 0; font: 800 1rem inherit; cursor: pointer; color: inherit; }
    .refreshing-pill { display: inline-flex; align-items: center; gap: 0.4rem; background: #fff7ed; border: 1px solid #fdba74; color: #9a3412; padding: 0.3rem 0.7rem; border-radius: 999px; font: 600 0.8rem inherit; align-self: flex-start; }
    .mini-spin { width: 14px; height: 14px; border-radius: 999px; border: 2px solid rgba(234, 88, 12, 0.25); border-top-color: #ea580c; animation: p-spin 0.75s linear infinite; }
    @keyframes p-spin { to { transform: rotate(360deg); } }
  `]
})
export class DailyPanchangComponent implements OnInit {
  private panchangService = inject(PanchangService);
  private ecosystem = inject(EcosystemConfigService);
  readonly locale = inject(LocaleService);
  private route = inject(ActivatedRoute);

  readonly studioPortalUrl = this.ecosystem.studioPortalUrl;
  readonly popularCities = POPULAR_CITIES;
  readonly monthNames = ['Jan', 'Feb', 'Mar', 'Apr', 'May', 'Jun', 'Jul', 'Aug', 'Sep', 'Oct', 'Nov', 'Dec'];
  readonly clockModes: { value: ClockFormatMode; label: string }[] = [
    { value: '12h', label: '12h' },
    { value: '24h', label: '24h' },
    { value: '24+', label: '24+' },
    { value: 'vedic', label: 'Ghati' }
  ];

  readonly selectedDate = signal<string>(toLocalDateKey());
  readonly selectedLocation = signal<string>('New Delhi, India');
  readonly citySearchText = signal<string>('New Delhi, India');
  readonly customCoords = signal<{ lat: number; lon: number; tz?: string } | null>(null);
  readonly cities = signal<PanchangCity[]>([]);
  readonly citySuggestions = signal<PanchangCity[]>([]);
  readonly dropdownOpen = signal(false);
  readonly searchingCities = signal(false);
  readonly showCustomCoords = signal(false);
  readonly showMonthJump = signal(false);
  coordLabel = '';
  coordLat: number | null = null;
  coordLon: number | null = null;
  coordTz = '';

  readonly clockMode = signal<ClockFormatMode>('12h');
  readonly schoolMode = signal<'purnimanta' | 'amanta'>('purnimanta');
  readonly showChart = signal<boolean>(true);
  readonly activeInfo = signal<InfoPopupContent | null>(null);
  readonly panchang = signal<PanchangData | null>(null);
  readonly loading = signal(true);
  readonly error = signal<string | null>(null);
  readonly adminDenied = signal(false);

  readonly selectedYear = computed(() => Number(this.selectedDate().split('-')[0]) || 2026);
  readonly selectedMonthIndex = computed(() => (Number(this.selectedDate().split('-')[1]) || 1) - 1);

  private readonly searchInput$ = new Subject<string>();

  ngOnInit(): void {
    this.ecosystem.load();
    try {
      const savedChart = localStorage.getItem('panchang.chart.visible');
      if (savedChart === 'false') this.showChart.set(false);
      const savedClock = localStorage.getItem('panchang.clockMode') as ClockFormatMode | null;
      if (savedClock && ['12h', '24h', '24+', 'vedic'].includes(savedClock)) this.clockMode.set(savedClock);
      const savedSchool = localStorage.getItem('panchang.school');
      if (savedSchool === 'amanta' || savedSchool === 'purnimanta') this.schoolMode.set(savedSchool);
    } catch {}

    this.route.queryParamMap.subscribe(params => {
      this.adminDenied.set(params.get('denied') === 'admin');
    });

    this.panchangService.getCities().subscribe(list => {
      this.cities.set(list);
      this.citySuggestions.set(list);
    });

    this.searchInput$
      .pipe(
        debounceTime(200),
        distinctUntilChanged(),
        switchMap(q => {
          this.searchingCities.set(true);
          return this.panchangService.searchCities(q, 15).pipe(finalize(() => this.searchingCities.set(false)));
        })
      )
      .subscribe(results => {
        if (results.length > 0) this.citySuggestions.set(results);
        this.dropdownOpen.set(true);
      });

    this.loadPanchang();
  }

  onCitySearchType(term: string): void {
    this.citySearchText.set(term);
    this.searchInput$.next(term);
  }

  selectCity(city: PanchangCity): void {
    this.customCoords.set(null);
    this.selectedLocation.set(city.name);
    this.citySearchText.set(city.name);
    this.dropdownOpen.set(false);
    this.loadPanchang();
  }

  pickQuickCity(cityName: string): void {
    this.customCoords.set(null);
    this.selectedLocation.set(cityName);
    this.citySearchText.set(cityName);
    this.dropdownOpen.set(false);
    this.loadPanchang();
  }

  applyCustomCoords(): void {
    if (this.coordLat === null || this.coordLon === null || !Number.isFinite(this.coordLat) || !Number.isFinite(this.coordLon)) {
      return;
    }
    const label = this.coordLabel.trim() || `${this.coordLat.toFixed(3)}°, ${this.coordLon.toFixed(3)}°`;
    this.customCoords.set({ lat: this.coordLat, lon: this.coordLon, tz: this.coordTz.trim() || undefined });
    this.selectedLocation.set(label);
    this.citySearchText.set(label);
    this.showCustomCoords.set(false);
    this.loadPanchang();
  }

  loadPanchang(): void {
    this.loading.set(true);
    this.error.set(null);
    this.panchangService
      .getDailyPanchang(this.selectedDate(), this.selectedLocation(), this.customCoords() ?? undefined)
      .pipe(finalize(() => this.loading.set(false)))
      .subscribe({
        next: data => this.panchang.set(data),
        error: () => this.error.set('Could not connect to Panchang API right now. Please try again in a moment.')
      });
  }

  changeDay(days: number): void {
    const [y, m, d] = this.selectedDate().split('-').map(Number);
    this.selectedDate.set(toLocalDateKey(new Date(y, m - 1, d + days)));
    this.loadPanchang();
  }

  today(): void {
    this.selectedDate.set(toLocalDateKey());
    this.loadPanchang();
  }

  jumpToYear(year: number): void {
    if (!Number.isFinite(year) || year < 1800 || year > 2200) return;
    const [, m, d] = this.selectedDate().split('-').map(Number);
    this.selectedDate.set(toLocalDateKey(new Date(year, (m || 1) - 1, d || 1)));
    this.loadPanchang();
  }

  jumpToMonth(monthIdx: number): void {
    const [y, , d] = this.selectedDate().split('-').map(Number);
    this.selectedDate.set(toLocalDateKey(new Date(y, monthIdx, Math.min(d || 1, 28))));
    this.loadPanchang();
  }

  setClockMode(mode: ClockFormatMode): void {
    this.clockMode.set(mode);
    try { localStorage.setItem('panchang.clockMode', mode); } catch {}
  }

  setSchoolMode(mode: 'purnimanta' | 'amanta'): void {
    this.schoolMode.set(mode);
    try { localStorage.setItem('panchang.school', mode); } catch {}
  }

  toggleChart(): void {
    const next = !this.showChart();
    this.showChart.set(next);
    try { localStorage.setItem('panchang.chart.visible', String(next)); } catch {}
  }

  convertStr(raw: string): string {
    return convertDisplayString(raw, this.clockMode(), this.panchang()?.extended?.sunriseMinutes ?? 406);
  }

  formatOffset(hours: number): string {
    const sign = hours >= 0 ? '+' : '-';
    const absMins = Math.round(Math.abs(hours) * 60);
    return `UTC${sign}${String(Math.floor(absMins / 60)).padStart(2, '0')}:${String(absMins % 60).padStart(2, '0')}`;
  }

  openInfo(kind: 'tithi' | 'nakshatra' | 'yoga' | 'karana' | 'vaara' | 'sunrise' | 'muhurat', p: PanchangData): void {
    const ex = p.extended;
    if (kind === 'tithi') {
      this.activeInfo.set({
        title: `Tithi: ${p.tithi}`,
        subtitle: p.paksha,
        items: [
          { label: 'End Time', value: this.convertStr(p.tithiEndTime) },
          { label: 'Elapsed at Sunrise', value: `${ex?.tithiPercentElapsedAtSunrise ?? 0}%` },
          { label: 'Ruling Deity (Swami)', value: ex?.tithiSwami ?? '-' },
          { label: 'Next Tithi', value: ex?.nextTithi ?? '-' }
        ],
        note: 'Each Tithi spans 12° of angular Moon-Sun separation evaluated in the Sidereal Lahiri frame.'
      });
    } else if (kind === 'nakshatra') {
      this.activeInfo.set({
        title: `Nakshatra: ${p.nakshatra}`,
        subtitle: `Moon Sign: ${p.moonSign}`,
        items: [
          { label: 'End Time', value: this.convertStr(p.nakshatraEndTime) },
          { label: 'Elapsed at Sunrise', value: `${ex?.nakshatraPercentElapsedAtSunrise ?? 0}%` },
          { label: 'Deity / Swami', value: ex?.nakshatraSwami ?? '-' },
          { label: 'Next Nakshatra', value: ex?.nextNakshatra ?? '-' },
          { label: 'Varjyam (Avoid)', value: this.convertStr(ex?.varjyam ?? '-') }
        ],
        note: 'Each Nakshatra spans 13°20′ (4 Padas of 3°20′ each). Varjyam & Amrit Kalam derive from classical Nakshatra ghatis.'
      });
    } else if (kind === 'yoga') {
      this.activeInfo.set({
        title: `Yoga: ${p.yoga}`,
        subtitle: 'Sun + Moon Sidereal Longitude Sum',
        items: [
          { label: 'End Time', value: this.convertStr(p.yogaEndTime) },
          { label: 'Elapsed at Sunrise', value: `${ex?.yogaPercentElapsedAtSunrise ?? 0}%` },
          { label: 'Next Yoga', value: ex?.nextYoga ?? '-' }
        ],
        note: 'Nitya Yoga increments every 13°20′ of combined Sun + Moon sidereal longitudes.'
      });
    } else if (kind === 'karana') {
      this.activeInfo.set({
        title: `Karana: ${p.karana}`,
        subtitle: 'Half-Tithi (6° Elongation Span)',
        items: [
          { label: 'End Time', value: this.convertStr(p.karanaEndTime) },
          { label: 'Next Karana', value: ex?.nextKarana ?? '-' }
        ],
        note: 'Two Karanas occur in every Tithi across the 7 movable and 4 fixed Karanas.'
      });
    } else if (kind === 'vaara') {
      this.activeInfo.set({
        title: `Vaara: ${p.vaara}`,
        subtitle: p.location,
        items: [
          { label: 'Anandadi Yoga', value: ex?.anandadiYoga ?? '-' },
          { label: 'Tamil Yoga', value: ex?.tamilYoga ?? '-' },
          { label: 'Disha Shool', value: ex?.dishaShool ?? '-' },
          { label: 'Rahu Vasa', value: ex?.rahuVasa ?? '-' }
        ],
        note: 'Vedic day begins at local Sunrise and runs until tomorrow’s local Sunrise.'
      });
    } else if (kind === 'sunrise') {
      this.activeInfo.set({
        title: `Solar & Twilight Timings — ${p.location}`,
        subtitle: p.locationMeta?.sunriseMethod || 'Upper-Limb Horizon Crossing',
        items: [
          { label: 'Sunrise (Upper Limb)', value: this.convertStr(p.sunrise) },
          { label: 'Civil Dawn (-6°)', value: this.convertStr(ex?.civilDawn ?? '-') },
          { label: 'Nautical Dawn (-12°)', value: this.convertStr(ex?.nauticalDawn ?? '-') },
          { label: 'Astronomical Dawn (-18°)', value: this.convertStr(ex?.astronomicalDawn ?? '-') },
          { label: 'Dinamana / Ratrimana', value: `${ex?.dinamana ?? '-'} / ${ex?.ratrimana ?? '-'}` }
        ],
        note: 'Computed from Jean Meeus solar RA/declination with standard -50′ atmospheric refraction & solar semidiameter.'
      });
    } else {
      this.activeInfo.set({
        title: 'Shubh & Ashubh Muhurat Rules',
        subtitle: `${p.location} • Day/Night ÷ 15 Muhurta System`,
        items: [
          { label: 'Brahma Muhurta (14th Night)', value: this.convertStr(p.brahmaMuhurat) },
          { label: 'Vijaya Muhurta (11th Day)', value: this.convertStr(ex?.vijayaMuhurat ?? '-') },
          { label: 'Nishita Muhurta (Mid-Night)', value: this.convertStr(ex?.nishitaMuhurat ?? '-') },
          { label: 'Varjyam / Dur Muhurtam', value: `${this.convertStr(ex?.varjyam ?? '-')} • ${this.convertStr(ex?.durMuhurtam ?? '-')}` }
        ],
        note: 'Abhijit Muhurta is marked avoided on Wednesdays. Expand the Complete Muhurats section below for Sandhya & Panchak.'
      });
    }
  }
}
