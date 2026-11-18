import { Component, computed, input, output, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { PanchangTimelineData, PanchangTimelineSegment } from '../../core/models/panchang.models';
import { ClockFormatMode, formatMinutesAsClock, formatMinutesRange } from './panchang-clock.util';

@Component({
  selector: 'app-panchang-timeline-chart',
  standalone: true,
  imports: [CommonModule],
  template: `
    <section class="chart-card" aria-label="24-Hour Sunrise-to-Sunrise Panchang Timeline Graph">
      <div class="chart-head">
        <div>
          <h3>📊 24-Hour Panchang Shuddhi &amp; Dosha Graph</h3>
          <p class="chart-sub">
            Visual timeline from today’s Sunrise ({{ fmtTime(timeline().dayStartMinutes) }}) through Sunset ({{ fmtTime(timeline().sunsetMinutes) }}) to next Sunrise ({{ fmtTime(timeline().dayEndMinutes) }})
          </p>
        </div>
        <button type="button" class="btn-chart-toggle" (click)="toggleVisibility.emit()">
          {{ visible() ? 'Hide Chart ▲' : 'Show Chart ▼' }}
        </button>
      </div>

      @if (visible()) {
        <div class="chart-shell">
          <!-- Solar & Lunar Sky Horizon Header -->
          <div class="sky-header">
            <div class="track-label">Day / Night</div>
            <div class="sky-bar">
              <div class="day-span" [style.width.%]="sunsetPercent()">
                <span>☀️ Daytime ({{ fmtTime(timeline().dayStartMinutes) }} – {{ fmtTime(timeline().sunsetMinutes) }})</span>
              </div>
              <div class="night-span" [style.width.%]="100 - sunsetPercent()">
                <span>🌙 Nighttime</span>
              </div>
              @if (moonrisePercent(); as mr) {
                <span class="cel-pin moonrise" [style.left.%]="mr" [title]="'Moonrise: ' + fmtTime(moonriseMinutes()!)">🌒 Rise</span>
              }
              @if (moonsetPercent(); as ms) {
                <span class="cel-pin moonset" [style.left.%]="ms" [title]="'Moonset: ' + fmtTime(moonsetMinutes()!)">🌘 Set</span>
              }
            </div>
          </div>

          <!-- 5 Timeline Tracks -->
          @for (row of trackRows(); track row.title) {
            <div class="timeline-row">
              <div class="track-label">{{ row.title }}</div>
              <div class="track-lane">
                <div class="sunset-line" [style.left.%]="sunsetPercent()"></div>
                @for (seg of row.segments; track seg.label + seg.startPercent) {
                  <button
                    type="button"
                    class="seg-block"
                    [class.tone-shubh]="seg.tone === 'shubh'"
                    [class.tone-ashubh]="seg.tone === 'ashubh'"
                    [class.tone-neutral]="seg.tone === 'neutral'"
                    [class.selected]="hovered()?.label === seg.label && hovered()?.startPercent === seg.startPercent"
                    [style.left.%]="seg.startPercent"
                    [style.width.%]="seg.widthPercent"
                    [title]="seg.label + ': ' + fmtRange(seg.startMinutes, seg.endMinutes)"
                    (mouseenter)="hovered.set(seg)"
                    (click)="hovered.set(seg)">
                    <span class="seg-text">{{ seg.label }}</span>
                  </button>
                }
              </div>
            </div>
          }

          <!-- Interactive Legend & Inspector Strip -->
          <div class="chart-foot">
            <div class="legend-items">
              <span class="leg"><i class="dot limb"></i> Pancha-Anga Limb Spans</span>
              <span class="leg"><i class="dot shubh"></i> Shubh Muhurat (Abhijit / Vijaya / Nishita)</span>
              <span class="leg"><i class="dot ashubh"></i> Avoid / Dosha (Rahu / Yamaganda / Gulika / Varjyam)</span>
            </div>
            @if (hovered(); as sel) {
              <div class="inspector-pill">
                <strong>{{ sel.category }} · {{ sel.label }}</strong>
                <span>{{ fmtRange(sel.startMinutes, sel.endMinutes) }}</span>
              </div>
            } @else {
              <div class="inspector-muted">Hover or tap any segment to inspect exact {{ clockMode() }} timestamps</div>
            }
          </div>
        </div>
      }
    </section>
  `,
  styles: [`
    .chart-card {
      background: #ffffff;
      border: 1px solid #fed7aa;
      border-radius: 14px;
      padding: 1.15rem 1.25rem;
      box-shadow: 0 2px 10px rgba(124, 45, 18, 0.04);
      display: flex;
      flex-direction: column;
      gap: 0.9rem;
    }
    .chart-head {
      display: flex;
      justify-content: space-between;
      align-items: flex-start;
      gap: 1rem;
      flex-wrap: wrap;
    }
    .chart-head h3 { margin: 0; font-size: 1.1rem; color: #7c2d12; font-weight: 800; }
    .chart-sub { margin: 0.25rem 0 0; font-size: 0.83rem; color: #78350f; }
    .btn-chart-toggle {
      background: #fff7ed;
      color: #9a3412;
      border: 1px solid #fdba74;
      border-radius: 8px;
      padding: 0.4rem 0.85rem;
      font-weight: 700;
      font-size: 0.8rem;
      cursor: pointer;
    }
    .btn-chart-toggle:hover { background: #ffedd5; }

    .chart-shell {
      display: flex;
      flex-direction: column;
      gap: 0.55rem;
      overflow-x: auto;
      padding-bottom: 0.25rem;
    }
    .sky-header, .timeline-row {
      display: grid;
      grid-template-columns: 112px minmax(520px, 1fr);
      align-items: center;
      gap: 0.65rem;
    }
    .track-label {
      font-size: 0.8rem;
      font-weight: 700;
      color: #7c2d12;
      white-space: nowrap;
    }
    .sky-bar {
      position: relative;
      height: 30px;
      border-radius: 8px;
      overflow: hidden;
      display: flex;
      border: 1px solid #fed7aa;
      font-size: 0.72rem;
      font-weight: 700;
    }
    .day-span {
      background: linear-gradient(90deg, #fef3c7 0%, #fde68a 100%);
      color: #78350f;
      display: flex;
      align-items: center;
      padding: 0 0.55rem;
      white-space: nowrap;
      overflow: hidden;
    }
    .night-span {
      background: linear-gradient(90deg, #312e81 0%, #1e1b4b 100%);
      color: #e0e7ff;
      display: flex;
      align-items: center;
      justify-content: flex-end;
      padding: 0 0.55rem;
      white-space: nowrap;
    }
    .cel-pin {
      position: absolute;
      top: 50%;
      transform: translate(-50%, -50%);
      background: rgba(15, 23, 42, 0.85);
      color: #fef08a;
      padding: 1px 6px;
      border-radius: 999px;
      font-size: 0.68rem;
      border: 1px solid rgba(254, 240, 138, 0.45);
      pointer-events: auto;
      white-space: nowrap;
    }

    .track-lane {
      position: relative;
      height: 34px;
      background: #fffaf5;
      border: 1px solid #fed7aa;
      border-radius: 8px;
      overflow: hidden;
    }
    .sunset-line {
      position: absolute;
      top: 0;
      bottom: 0;
      width: 2px;
      background: rgba(124, 45, 18, 0.35);
      pointer-events: none;
      z-index: 1;
    }
    .seg-block {
      position: absolute;
      top: 3px;
      bottom: 3px;
      border-radius: 6px;
      border: 1px solid rgba(0, 0, 0, 0.08);
      display: flex;
      align-items: center;
      justify-content: center;
      padding: 0 6px;
      cursor: pointer;
      font-size: 0.75rem;
      font-weight: 700;
      overflow: hidden;
      transition: transform 0.12s ease, box-shadow 0.12s ease;
      z-index: 2;
    }
    .seg-block:hover, .seg-block.selected {
      transform: translateY(-1px);
      box-shadow: 0 3px 8px rgba(0, 0, 0, 0.14);
      z-index: 4;
    }
    .tone-neutral { background: linear-gradient(135deg, #fff7ed 0%, #ffedd5 100%); color: #431407; border-color: #fdba74; }
    .tone-shubh { background: linear-gradient(135deg, #dcfce7 0%, #bbf7d0 100%); color: #14532d; border-color: #22c55e; }
    .tone-ashubh { background: linear-gradient(135deg, #fee2e2 0%, #fecaca 100%); color: #7f1d1d; border-color: #ef4444; }
    .seg-text {
      overflow: hidden;
      text-overflow: ellipsis;
      white-space: nowrap;
    }

    .chart-foot {
      display: flex;
      justify-content: space-between;
      align-items: center;
      flex-wrap: wrap;
      gap: 0.75rem;
      padding-top: 0.35rem;
      border-top: 1px dashed #fed7aa;
      font-size: 0.78rem;
    }
    .legend-items { display: flex; flex-wrap: wrap; gap: 0.85rem; color: #475569; font-weight: 600; }
    .leg { display: inline-flex; align-items: center; gap: 0.35rem; }
    .dot { width: 10px; height: 10px; border-radius: 3px; display: inline-block; }
    .dot.limb { background: #fdba74; border: 1px solid #c2410c; }
    .dot.shubh { background: #4ade80; border: 1px solid #16a34a; }
    .dot.ashubh { background: #f87171; border: 1px solid #dc2626; }
    .inspector-pill {
      display: inline-flex;
      gap: 0.5rem;
      align-items: center;
      background: #fff7ed;
      border: 1px solid #f97316;
      color: #7c2d12;
      padding: 0.25rem 0.65rem;
      border-radius: 999px;
      font-size: 0.78rem;
    }
    .inspector-muted { color: #94a3b8; font-style: italic; }
  `]
})
export class PanchangTimelineChartComponent {
  readonly timeline = input.required<PanchangTimelineData>();
  readonly clockMode = input<ClockFormatMode>('12h');
  readonly visible = input<boolean>(true);
  readonly moonriseMinutes = input<number | null | undefined>(undefined);
  readonly moonsetMinutes = input<number | null | undefined>(undefined);
  readonly toggleVisibility = output<void>();

  readonly hovered = signal<PanchangTimelineSegment | null>(null);

  readonly sunsetPercent = computed(() => {
    const tl = this.timeline();
    const total = Math.max(60, tl.dayEndMinutes - tl.dayStartMinutes);
    return Math.min(92, Math.max(8, ((tl.sunsetMinutes - tl.dayStartMinutes) / total) * 100));
  });

  readonly moonrisePercent = computed(() => this.percentForMinute(this.moonriseMinutes()));
  readonly moonsetPercent = computed(() => this.percentForMinute(this.moonsetMinutes()));

  readonly trackRows = computed(() => {
    const tl = this.timeline();
    return [
      { title: 'Tithi', segments: tl.tithiBars },
      { title: 'Nakshatra', segments: tl.nakshatraBars },
      { title: 'Yoga', segments: tl.yogaBars },
      { title: 'Karana', segments: tl.karanaBars },
      { title: 'Shubh / Dosha', segments: [...tl.auspiciousBands, ...tl.doshaBands] }
    ];
  });

  fmtTime(mins: number): string {
    return formatMinutesAsClock(mins, this.clockMode(), this.timeline().dayStartMinutes);
  }

  fmtRange(startMins: number, endMins: number): string {
    return formatMinutesRange(startMins, endMins, this.clockMode(), this.timeline().dayStartMinutes);
  }

  private percentForMinute(val: number | null | undefined): number | null {
    if (val === null || val === undefined || !Number.isFinite(val)) return null;
    const tl = this.timeline();
    let adjusted = val;
    if (adjusted < tl.dayStartMinutes) adjusted += 1440;
    if (adjusted < tl.dayStartMinutes || adjusted > tl.dayEndMinutes) return null;
    const total = Math.max(60, tl.dayEndMinutes - tl.dayStartMinutes);
    return Math.min(97, Math.max(3, ((adjusted - tl.dayStartMinutes) / total) * 100));
  }
}
