import { Component, OnInit, computed, input, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { PanchangData } from '../../core/models/panchang.models';
import { ClockFormatMode, convertDisplayString, formatMinutesAsClock, formatMinutesRange } from './panchang-clock.util';

type AccordionKey =
  | 'choghadiya'
  | 'hora'
  | 'transitions'
  | 'extendedMuhurat'
  | 'epochs'
  | 'nivasShool'
  | 'taraChandra'
  | 'lagnas'
  | 'credits';

const STORAGE_KEY = 'panchang.accordions.v1';

@Component({
  selector: 'app-panchang-extended-sections',
  standalone: true,
  imports: [CommonModule],
  template: `
    @if (panchang(); as p) {
      <section class="more-header-bar">
        <div>
          <h3>🔎 Detailed Astronomical Tables &amp; "More" Jyotish Sections</h3>
          <p class="more-sub">Click any panel to expand/collapse (saved locally) or switch Day/Night tabs.</p>
        </div>
        <div class="more-actions">
          <button type="button" class="btn-chip" (click)="setAll(true)">Expand All</button>
          <button type="button" class="btn-chip" (click)="setAll(false)">Collapse All</button>
        </div>
      </section>

      <!-- 1. Choghadiya (Day & Night) -->
      <div class="acc-card">
        <button type="button" class="acc-head" (click)="toggle('choghadiya')">
          <span><strong>🕐 Choghadiya (Day &amp; Night Muhurats)</strong> — शुभ कार्य एवं यात्रा</span>
          <span class="acc-arrow">{{ isOpen('choghadiya') ? '▾' : '▸' }}</span>
        </button>
        @if (isOpen('choghadiya')) {
          <div class="acc-body">
            <div class="tab-bar">
              <button type="button" class="tab-btn" [class.active]="chogTab() === 'day'" (click)="chogTab.set('day')">☀️ Day Choghadiya (Sunrise–Sunset)</button>
              <button type="button" class="tab-btn" [class.active]="chogTab() === 'night'" (click)="chogTab.set('night')">🌙 Night Choghadiya (Sunset–Next Sunrise)</button>
            </div>
            <div class="period-grid">
              @for (chog of (chogTab() === 'day' ? p.dayChoghadiya : p.nightChoghadiya); track chog.timeRange) {
                <div class="period-pill" [class.is-shubh]="chog.type === 'Shubh'" [class.is-ashubh]="chog.type !== 'Shubh'">
                  <strong>{{ chog.name }}</strong>
                  <span>{{ convertStr(chog.timeRange) }}</span>
                  <small>{{ chog.type }}</small>
                </div>
              }
            </div>
          </div>
        }
      </div>

      <!-- 2. Planetary Hora (Day & Night) -->
      @if (p.dayHora?.length || p.nightHora?.length) {
        <div class="acc-card">
          <button type="button" class="acc-head" (click)="toggle('hora')">
            <span><strong>🪐 Planetary Hora (24-Hour Lords)</strong> — Day &amp; Night Rulership</span>
            <span class="acc-arrow">{{ isOpen('hora') ? '▾' : '▸' }}</span>
          </button>
          @if (isOpen('hora')) {
            <div class="acc-body">
              <div class="tab-bar">
                <button type="button" class="tab-btn" [class.active]="horaTab() === 'day'" (click)="horaTab.set('day')">☀️ Day Horas (12)</button>
                <button type="button" class="tab-btn" [class.active]="horaTab() === 'night'" (click)="horaTab.set('night')">🌙 Night Horas (12)</button>
              </div>
              <div class="period-grid">
                @for (h of (horaTab() === 'day' ? (p.dayHora ?? []) : (p.nightHora ?? [])); track h.startMinutes) {
                  <div class="period-pill" [class.is-shubh]="h.quality === 'Shubh'" [class.is-ashubh]="h.quality === 'Ashubh'">
                    <strong>{{ h.planet }} ({{ h.planetSanskrit }})</strong>
                    <span>{{ fmtRange(h.startMinutes, h.endMinutes) }}</span>
                    <small>{{ h.bestFor }}</small>
                  </div>
                }
              </div>
            </div>
          }
        </div>
      }

      @if (p.extended; as ex) {
        <!-- 3. Pancha-Anga Full-Day Transitions & 4 Nakshatra Padas -->
        <div class="acc-card">
          <button type="button" class="acc-head" (click)="toggle('transitions')">
            <span><strong>🌗 Rashi, 4 Nakshatra Padas &amp; Full-Day Transitions</strong> (Sunrise → Next Sunrise)</span>
            <span class="acc-arrow">{{ isOpen('transitions') ? '▾' : '▸' }}</span>
          </button>
          @if (isOpen('transitions')) {
            <div class="acc-body two-col-grid">
              <div class="mini-table">
                <h4>Tithi Transitions ({{ ex.tithiPercentElapsedAtSunrise }}% over at Sunrise)</h4>
                @for (item of ex.tithiTransitions; track item.name + item.startMinutesFromMidnight) {
                  <div class="kv-row"><span>{{ item.name }}</span><strong>{{ item.isFullNight ? 'Full Night' : 'Upto ' + convertEnd(item.endMinutesFromMidnight) }}</strong></div>
                }
                <h4>Nakshatra Transitions ({{ ex.nakshatraPercentElapsedAtSunrise }}% over at Sunrise)</h4>
                @for (item of ex.nakshatraTransitions; track item.name + item.startMinutesFromMidnight) {
                  <div class="kv-row"><span>{{ item.name }}</span><strong>{{ item.isFullNight ? 'Full Night' : 'Upto ' + convertEnd(item.endMinutesFromMidnight) }}</strong></div>
                }
                <h4>Yoga &amp; Karana Transitions</h4>
                @for (item of ex.yogaTransitions; track item.name + item.startMinutesFromMidnight) {
                  <div class="kv-row"><span>Yoga: {{ item.name }}</span><strong>{{ item.isFullNight ? 'Full Night' : 'Upto ' + convertEnd(item.endMinutesFromMidnight) }}</strong></div>
                }
                @for (item of ex.karanaTransitions; track item.name + item.startMinutesFromMidnight) {
                  <div class="kv-row"><span>Karana: {{ item.name }}</span><strong>{{ item.isFullNight ? 'Full Night' : 'Upto ' + convertEnd(item.endMinutesFromMidnight) }}</strong></div>
                }
              </div>

              <div class="mini-table">
                <h4>Nakshatra Pada Quarters &amp; Moon Sign</h4>
                @for (pada of ex.nakshatraPadas; track pada.name + pada.startMinutesFromMidnight) {
                  <div class="kv-row"><span>{{ pada.name }}</span><strong>{{ pada.isFullNight ? 'Full Night' : 'Upto ' + convertEnd(pada.endMinutesFromMidnight) }}</strong></div>
                }
                @for (ms of ex.moonSignTransitions; track ms.name + ms.startMinutesFromMidnight) {
                  <div class="kv-row accent-row"><span>Moonsign: {{ ms.name }}</span><strong>{{ ms.isFullNight ? 'Full Night' : 'Upto ' + convertEnd(ms.endMinutesFromMidnight) }}</strong></div>
                }
                <div class="kv-row accent-row"><span>Surya Nakshatra</span><strong>{{ ex.suryaNakshatra }}</strong></div>
              </div>
            </div>
          }
        </div>

        <!-- 4. Extended Auspicious & Inauspicious Timings -->
        <div class="acc-card">
          <button type="button" class="acc-head" (click)="toggle('extendedMuhurat')">
            <span><strong>⏳ Complete Muhurats</strong> — Sandhya, Vijaya, Godhuli, Nishita, Varjyam, Dur Muhurtam, Baana, Panchaka</span>
            <span class="acc-arrow">{{ isOpen('extendedMuhurat') ? '▾' : '▸' }}</span>
          </button>
          @if (isOpen('extendedMuhurat')) {
            <div class="acc-body two-col-grid">
              <div class="mini-table">
                <h4>Additional Auspicious Timings</h4>
                <div class="kv-row good"><span>Pratah Sandhya</span><strong>{{ convertStr(ex.pratahSandhya) }}</strong></div>
                <div class="kv-row good"><span>Vijaya Muhurta</span><strong>{{ convertStr(ex.vijayaMuhurat) }}</strong></div>
                <div class="kv-row good"><span>Godhuli Muhurta</span><strong>{{ convertStr(ex.godhuliMuhurat) }}</strong></div>
                <div class="kv-row good"><span>Sayahna Sandhya</span><strong>{{ convertStr(ex.sayahnaSandhya) }}</strong></div>
                <div class="kv-row good"><span>Nishita Muhurta</span><strong>{{ convertStr(ex.nishitaMuhurat) }}</strong></div>
                <div class="kv-row good"><span>Ravi Yoga</span><strong>{{ ex.raviYoga }}</strong></div>
              </div>
              <div class="mini-table">
                <h4>Dosha &amp; Inauspicious Windows</h4>
                <div class="kv-row bad"><span>Varjyam (Thyajyam)</span><strong>{{ convertStr(ex.varjyam) }}</strong></div>
                <div class="kv-row bad"><span>Dur Muhurtam</span><strong>{{ convertStr(ex.durMuhurtam) }}</strong></div>
                <div class="kv-row bad"><span>Panchaka Status</span><strong>{{ ex.panchakStatus }}</strong></div>
                <div class="kv-row bad"><span>Baana</span><strong>{{ ex.baana }}</strong></div>
                <div class="kv-row"><span>Anandadi Yoga</span><strong>{{ ex.anandadiYoga }}</strong></div>
                <div class="kv-row"><span>Tamil Yoga</span><strong>{{ ex.tamilYoga }}</strong></div>
              </div>
            </div>
          }
        </div>

        <!-- 5. Samvat, Ritu, Ayana, Epochs & Exact Ayanamsha -->
        <div class="acc-card">
          <button type="button" class="acc-head" (click)="toggle('epochs')">
            <span><strong>📜 Lunar Month, Ritu, Ayana, Epochs &amp; Exact Lahiri Ayanamsha</strong></span>
            <span class="acc-arrow">{{ isOpen('epochs') ? '▾' : '▸' }}</span>
          </button>
          @if (isOpen('epochs')) {
            <div class="acc-body two-col-grid">
              <div class="mini-table">
                <div class="kv-row"><span>Dinamana (Day Length)</span><strong>{{ ex.dinamana }}</strong></div>
                <div class="kv-row"><span>Ratrimana (Night Length)</span><strong>{{ ex.ratrimana }}</strong></div>
                <div class="kv-row"><span>Madhyahna (Solar Noon)</span><strong>{{ convertStr(ex.madhyahna) }}</strong></div>
                <div class="kv-row"><span>Drik / Vedic Ritu</span><strong>{{ ex.ritu }}</strong></div>
                <div class="kv-row"><span>Ayana</span><strong>{{ ex.ayana }}</strong></div>
                <div class="kv-row"><span>Deity / Swami (Tithi · Nak)</span><strong>{{ ex.tithiSwami }} · {{ ex.nakshatraSwami }}</strong></div>
              </div>
              <div class="mini-table">
                <div class="kv-row"><span>Lahiri Ayanamsha</span><strong>{{ ex.lahiriAyanamsaDeg.toFixed(6) }}°</strong></div>
                <div class="kv-row"><span>Kaliyuga Year</span><strong>{{ ex.kaliyugaYear }}</strong></div>
                <div class="kv-row"><span>Kali Ahargana</span><strong>{{ ex.kaliAhargana }} days</strong></div>
                <div class="kv-row"><span>Julian Day / MJD</span><strong>JD {{ ex.julianDay }} / MJD {{ ex.modifiedJulianDay }}</strong></div>
                <div class="kv-row"><span>National Civil Date (Shaka)</span><strong>{{ ex.nationalCivilDate }}</strong></div>
                <div class="kv-row"><span>Twilight (Civil / Nautical / Astr)</span><strong>{{ convertStr(ex.civilDawn) }} / {{ convertStr(ex.nauticalDawn) }} / {{ convertStr(ex.astronomicalDawn) }}</strong></div>
              </div>
            </div>
          }
        </div>

        <!-- 6. Nivas & Shool -->
        <div class="acc-card">
          <button type="button" class="acc-head" (click)="toggle('nivasShool')">
            <span><strong>🧭 Nivas &amp; Shool (Travel Direction, Agnivasa, Shivavasa)</strong></span>
            <span class="acc-arrow">{{ isOpen('nivasShool') ? '▾' : '▸' }}</span>
          </button>
          @if (isOpen('nivasShool')) {
            <div class="acc-body two-col-grid">
              <div class="mini-table">
                <div class="kv-row"><span>Disha Shool</span><strong>{{ ex.dishaShool }}</strong></div>
                <div class="kv-row"><span>Nakshatra Shool</span><strong>{{ ex.nakshatraShool }}</strong></div>
                <div class="kv-row"><span>Rahu Vasa</span><strong>{{ ex.rahuVasa }}</strong></div>
              </div>
              <div class="mini-table">
                <div class="kv-row"><span>Agnivasa (Homa)</span><strong>{{ ex.agnivasa }}</strong></div>
                <div class="kv-row"><span>Shivavasa</span><strong>{{ ex.shivavasa }}</strong></div>
                <div class="kv-row"><span>Chandravasa &amp; Homahuti</span><strong>{{ ex.chandravasa }} · {{ ex.homahuti }}</strong></div>
              </div>
            </div>
          }
        </div>

        <!-- 7. Chandrabalam & Tarabalam -->
        <div class="acc-card">
          <button type="button" class="acc-head" (click)="toggle('taraChandra')">
            <span><strong>✨ Chandrabalam &amp; Tarabalam (Favorable Rashis &amp; Birth Nakshatras)</strong></span>
            <span class="acc-arrow">{{ isOpen('taraChandra') ? '▾' : '▸' }}</span>
          </button>
          @if (isOpen('taraChandra')) {
            <div class="acc-body">
              <div class="pill-group">
                <span class="grp-lbl">Good Chandrabalam Rashis:</span>
                @for (r of ex.chandrabalamGoodRashis; track r) {
                  <span class="tag-pill ok">{{ r }}</span>
                }
              </div>
              <div class="pill-group">
                <span class="grp-lbl">Good Tarabalam Nakshatras:</span>
                @for (n of ex.tarabalamGoodNakshatras; track n) {
                  <span class="tag-pill">{{ n }}</span>
                }
              </div>
            </div>
          }
        </div>

        <!-- 8. 12 Udaya Lagna & Panchaka Rahita -->
        @if (p.udayaLagnas?.length || p.panchakaRahitaPeriods?.length) {
          <div class="acc-card">
            <button type="button" class="acc-head" (click)="toggle('lagnas')">
              <span><strong>🌅 Udaya Lagna (Rising Signs) &amp; Panchaka Rahita Muhurta</strong></span>
              <span class="acc-arrow">{{ isOpen('lagnas') ? '▾' : '▸' }}</span>
            </button>
            @if (isOpen('lagnas')) {
              <div class="acc-body">
                <div class="tab-bar">
                  <button type="button" class="tab-btn" [class.active]="lagnaTab() === 'lagna'" (click)="lagnaTab.set('lagna')">12 Udaya Lagnas ({{ p.udayaLagnas?.length ?? 0 }})</button>
                  <button type="button" class="tab-btn" [class.active]="lagnaTab() === 'panchaka'" (click)="lagnaTab.set('panchaka')">Panchaka Rahita Windows ({{ p.panchakaRahitaPeriods?.length ?? 0 }})</button>
                </div>
                <div class="period-grid">
                  @for (row of (lagnaTab() === 'lagna' ? (p.udayaLagnas ?? []) : (p.panchakaRahitaPeriods ?? [])); track row.startMinutes + row.name) {
                    <div class="period-pill" [class.is-shubh]="row.quality.includes('Good') || row.quality.includes('Shubh')" [class.is-ashubh]="row.quality.includes('Avoid') || row.quality.includes('Caution')">
                      <strong>{{ row.name }}</strong>
                      <span>{{ fmtRange(row.startMinutes, row.endMinutes) }}</span>
                      <small>{{ row.quality }}</small>
                    </div>
                  }
                </div>
              </div>
            }
          </div>
        }

        <!-- 9. Calculation Credits & Transparency -->
        @if (ex.credits?.length) {
          <div class="acc-card credits-card">
            <button type="button" class="acc-head" (click)="toggle('credits')">
              <span><strong>🛡️ Calculation Credits, Astronomical Standards &amp; Timezone Verification</strong></span>
              <span class="acc-arrow">{{ isOpen('credits') ? '▾' : '▸' }}</span>
            </button>
            @if (isOpen('credits')) {
              <div class="acc-body credits-grid">
                @for (cr of ex.credits; track cr.component) {
                  <div class="credit-box">
                    <div class="cr-comp">{{ cr.component }}</div>
                    <div class="cr-std">{{ cr.standard }}</div>
                    <p>{{ cr.details }}</p>
                  </div>
                }
              </div>
            }
          </div>
        }
      }
    }
  `,
  styles: [`
    .more-header-bar { display: flex; justify-content: space-between; align-items: center; gap: 1rem; flex-wrap: wrap; }
    .more-header-bar h3 { margin: 0; font-size: 1.2rem; color: #431407; font-weight: 800; }
    .more-sub { margin: 0.2rem 0 0; font-size: 0.85rem; color: #78350f; }
    .more-actions { display: flex; gap: 0.5rem; }
    .btn-chip { background: #fff7ed; color: #9a3412; border: 1px solid #fdba74; padding: 0.35rem 0.75rem; border-radius: 8px; font-weight: 700; font-size: 0.8rem; cursor: pointer; }
    .btn-chip:hover { background: #ffedd5; }

    .acc-card { background: #ffffff; border: 1px solid #fed7aa; border-radius: 12px; overflow: hidden; box-shadow: 0 1px 4px rgba(124, 45, 18, 0.03); }
    .acc-head {
      width: 100%; background: #fffaf5; border: none; border-bottom: 1px solid transparent; padding: 0.9rem 1.15rem;
      display: flex; justify-content: space-between; align-items: center; gap: 0.75rem; text-align: left;
      font-size: 0.95rem; color: #431407; cursor: pointer;
    }
    .acc-head:hover { background: #ffedd5; }
    .acc-arrow { font-weight: 800; color: #ea580c; font-size: 1rem; }
    .acc-body { padding: 1rem 1.15rem; border-top: 1px solid #fed7aa; display: flex; flex-direction: column; gap: 0.9rem; }

    .tab-bar { display: flex; flex-wrap: wrap; gap: 0.5rem; }
    .tab-btn { background: #fff7ed; color: #9a3412; border: 1px solid #fed7aa; border-radius: 8px; padding: 0.4rem 0.85rem; font-weight: 700; font-size: 0.82rem; cursor: pointer; }
    .tab-btn.active { background: #ea580c; color: #ffffff; border-color: #ea580c; }

    .period-grid { display: grid; grid-template-columns: repeat(auto-fill, minmax(170px, 1fr)); gap: 0.6rem; }
    .period-pill {
      padding: 0.65rem 0.75rem; border-radius: 9px; border: 1px solid #fed7aa; background: #fffaf5;
      display: flex; flex-direction: column; gap: 0.2rem; font-size: 0.82rem;
    }
    .period-pill strong { color: #1e293b; }
    .period-pill span { color: #475569; font-weight: 600; font-size: 0.78rem; }
    .period-pill small { color: #78350f; font-size: 0.72rem; }
    .period-pill.is-shubh { background: #f0fdf4; border-color: #bbf7d0; }
    .period-pill.is-ashubh { background: #fef2f2; border-color: #fecaca; }

    .two-col-grid { display: grid; grid-template-columns: 1fr 1fr; gap: 1rem; }
    @media (max-width: 820px) { .two-col-grid { grid-template-columns: 1fr; } }
    .mini-table { display: flex; flex-direction: column; gap: 0.45rem; }
    .mini-table h4 { margin: 0.35rem 0 0.15rem; font-size: 0.88rem; color: #9a3412; font-weight: 800; }
    .kv-row { display: flex; justify-content: space-between; gap: 0.75rem; padding: 0.4rem 0.65rem; border-radius: 6px; background: #fffaf5; font-size: 0.85rem; flex-wrap: wrap; }
    .kv-row span { color: #475569; font-weight: 600; }
    .kv-row strong { color: #0f172a; }
    .kv-row.good { background: #f0fdf4; }
    .kv-row.bad { background: #fef2f2; }
    .kv-row.accent-row { background: #fff7ed; border: 1px solid #fed7aa; }

    .pill-group { display: flex; flex-wrap: wrap; align-items: center; gap: 0.45rem; }
    .grp-lbl { font-size: 0.84rem; font-weight: 700; color: #7c2d12; margin-right: 0.25rem; }
    .tag-pill { background: #fff7ed; border: 1px solid #fdba74; color: #7c2d12; padding: 0.22rem 0.6rem; border-radius: 999px; font-size: 0.78rem; font-weight: 700; }
    .tag-pill.ok { background: #dcfce7; border-color: #86efac; color: #166534; }

    .credits-grid { display: grid; grid-template-columns: repeat(auto-fit, minmax(220px, 1fr)); gap: 0.75rem; }
    .credit-box { background: #fffaf5; border: 1px solid #fed7aa; border-radius: 10px; padding: 0.85rem; }
    .cr-comp { font-size: 0.78rem; text-transform: uppercase; font-weight: 800; color: #c2410c; }
    .cr-std { font-size: 0.92rem; font-weight: 800; color: #431407; margin: 0.2rem 0; }
    .credit-box p { margin: 0; font-size: 0.8rem; color: #475569; line-height: 1.45; }
  `]
})
export class PanchangExtendedSectionsComponent implements OnInit {
  readonly panchang = input.required<PanchangData>();
  readonly clockMode = input<ClockFormatMode>('12h');

  readonly chogTab = signal<'day' | 'night'>('day');
  readonly horaTab = signal<'day' | 'night'>('day');
  readonly lagnaTab = signal<'lagna' | 'panchaka'>('lagna');

  readonly openState = signal<Record<AccordionKey, boolean>>({
    choghadiya: true,
    hora: false,
    transitions: true,
    extendedMuhurat: true,
    epochs: false,
    nivasShool: false,
    taraChandra: false,
    lagnas: false,
    credits: true
  });

  readonly sunriseMins = computed(() => this.panchang().extended?.sunriseMinutes ?? 406);

  ngOnInit(): void {
    try {
      const saved = localStorage.getItem(STORAGE_KEY);
      if (saved) {
        this.openState.set({ ...this.openState(), ...JSON.parse(saved) });
      }
    } catch {
      // ignore storage errors
    }
  }

  isOpen(k: AccordionKey): boolean {
    return !!this.openState()[k];
  }

  toggle(k: AccordionKey): void {
    const next = { ...this.openState(), [k]: !this.openState()[k] };
    this.openState.set(next);
    try { localStorage.setItem(STORAGE_KEY, JSON.stringify(next)); } catch {}
  }

  setAll(open: boolean): void {
    const next: Record<AccordionKey, boolean> = {
      choghadiya: open,
      hora: open,
      transitions: open,
      extendedMuhurat: open,
      epochs: open,
      nivasShool: open,
      taraChandra: open,
      lagnas: open,
      credits: open
    };
    this.openState.set(next);
    try { localStorage.setItem(STORAGE_KEY, JSON.stringify(next)); } catch {}
  }

  convertStr(raw: string): string {
    return convertDisplayString(raw, this.clockMode(), this.sunriseMins());
  }

  convertEnd(mins: number): string {
    return formatMinutesAsClock(mins, this.clockMode(), this.sunriseMins());
  }

  fmtRange(startMins: number, endMins: number): string {
    return formatMinutesRange(startMins, endMins, this.clockMode(), this.sunriseMins());
  }
}
