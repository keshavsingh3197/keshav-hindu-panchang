import { Component, EventEmitter, Input, Output } from '@angular/core';
import { CommonModule } from '@angular/common';

export type PanchangLoadingVariant = 'spinner' | 'cards' | 'rows' | 'panchang';

@Component({
  selector: 'app-loading-state',
  standalone: true,
  imports: [CommonModule],
  template: `
    @if (error) {
      <div class="error-card" role="alert">
        <div class="error-head">
          <span class="error-icon">⚠️</span>
          <div>
            <strong>डेटा लोड नहीं हो सका (Unable to load data)</strong>
            <p>{{ error }}</p>
          </div>
        </div>
        @if (showRetry) {
          <button type="button" class="btn-retry" (click)="retry.emit()">🔄 पुनः प्रयास करें (Retry)</button>
        }
      </div>
    } @else if (variant === 'panchang') {
      <div class="panchang-skel" role="status" aria-busy="true" [attr.aria-label]="label">
        <div class="loading-banner">
          <span class="mini-spinner" aria-hidden="true"></span>
          <span>{{ label }}</span>
        </div>
        <div class="hero-skel">
          <span class="skel-bar w-30 h-sm"></span>
          <span class="skel-bar w-60 h-xl"></span>
          <span class="skel-bar w-45 h-sm"></span>
          <span class="skel-bar w-100 h-box"></span>
        </div>
        <div class="limbs-skel">
          @for (i of items(5); track i) {
            <div class="card-skel">
              <span class="skel-bar w-45 h-xs"></span>
              <span class="skel-bar w-75 h-md"></span>
              <span class="skel-bar w-55 h-xs"></span>
            </div>
          }
        </div>
        <div class="two-col-skel">
          @for (i of items(2); track i) {
            <div class="card-skel">
              <span class="skel-bar w-55 h-md"></span>
              <span class="skel-bar w-100 h-sm"></span>
              <span class="skel-bar w-90 h-sm"></span>
              <span class="skel-bar w-85 h-sm"></span>
            </div>
          }
        </div>
      </div>
    } @else if (variant === 'cards') {
      <div class="cards-skel" role="status" aria-busy="true" [attr.aria-label]="label">
        @if (label) {
          <div class="loading-banner">
            <span class="mini-spinner" aria-hidden="true"></span>
            <span>{{ label }}</span>
          </div>
        }
        <div class="cards-grid">
          @for (i of items(count); track i) {
            <div class="card-skel">
              <span class="skel-bar w-70 h-md"></span>
              <span class="skel-bar w-100 h-sm"></span>
              <span class="skel-bar w-85 h-sm"></span>
              <span class="skel-bar w-45 h-xs"></span>
            </div>
          }
        </div>
      </div>
    } @else if (variant === 'rows') {
      <div class="rows-skel" role="status" aria-busy="true" [attr.aria-label]="label">
        @if (label) {
          <div class="loading-banner">
            <span class="mini-spinner" aria-hidden="true"></span>
            <span>{{ label }}</span>
          </div>
        }
        @for (i of items(count); track i) {
          <div class="card-skel row-skel">
            <span class="skel-bar w-35 h-sm"></span>
            <span class="skel-bar w-50 h-sm"></span>
            <span class="skel-bar w-20 h-sm"></span>
          </div>
        }
      </div>
    } @else {
      <div class="spinner-panel" role="status" aria-busy="true">
        <span class="mini-spinner lg" aria-hidden="true"></span>
        <span class="spinner-text">{{ label }}</span>
        @if (hint) {
          <span class="spinner-hint">{{ hint }}</span>
        }
      </div>
    }
  `,
  styles: [`
    :host { display: block; width: 100%; }
    .loading-banner {
      display: inline-flex;
      align-items: center;
      gap: 0.55rem;
      background: #fff7ed;
      border: 1px solid #fed7aa;
      color: #9a3412;
      padding: 0.45rem 0.85rem;
      border-radius: 999px;
      font-size: 0.86rem;
      font-weight: 600;
      margin-bottom: 0.9rem;
    }
    .mini-spinner {
      width: 15px;
      height: 15px;
      border-radius: 999px;
      border: 2.5px solid rgba(234, 88, 12, 0.22);
      border-top-color: #ea580c;
      animation: skel-spin 0.75s linear infinite;
      flex: none;
    }
    .mini-spinner.lg {
      width: 28px;
      height: 28px;
      border-width: 3px;
    }
    .panchang-skel, .rows-skel, .cards-skel { display: flex; flex-direction: column; }
    .hero-skel {
      background: linear-gradient(135deg, #7c2d12 0%, #431407 100%);
      border-radius: 16px;
      padding: 2rem;
      display: flex;
      flex-direction: column;
      gap: 0.85rem;
      margin-bottom: 1.25rem;
    }
    .hero-skel .skel-bar {
      background: linear-gradient(90deg, rgba(255,255,255,0.12) 20%, rgba(254,215,170,0.3) 50%, rgba(255,255,255,0.12) 80%);
      background-size: 200% 100%;
    }
    .limbs-skel {
      display: grid;
      grid-template-columns: repeat(auto-fit, minmax(200px, 1fr));
      gap: 1rem;
      margin-bottom: 1.25rem;
    }
    .two-col-skel {
      display: grid;
      grid-template-columns: repeat(auto-fit, minmax(300px, 1fr));
      gap: 1.25rem;
    }
    .cards-grid {
      display: grid;
      grid-template-columns: repeat(auto-fit, minmax(270px, 1fr));
      gap: 1rem;
    }
    .card-skel {
      background: #ffffff;
      border: 1px solid #fed7aa;
      border-radius: 12px;
      padding: 1.1rem;
      display: flex;
      flex-direction: column;
      gap: 0.65rem;
    }
    .rows-skel { gap: 0.65rem; }
    .row-skel {
      flex-direction: row;
      align-items: center;
      justify-content: space-between;
      gap: 0.75rem;
    }
    .skel-bar {
      display: block;
      border-radius: 999px;
      background: linear-gradient(90deg, #ffedd5 20%, #fed7aa 50%, #ffedd5 80%);
      background-size: 200% 100%;
      animation: skel-shimmer 1.35s ease-in-out infinite;
    }
    .h-xs { height: 10px; }
    .h-sm { height: 13px; }
    .h-md { height: 18px; }
    .h-xl { height: 28px; }
    .h-box { height: 64px; border-radius: 10px; }
    .w-20 { width: 20%; }
    .w-30 { width: 30%; }
    .w-35 { width: 35%; }
    .w-45 { width: 45%; }
    .w-50 { width: 50%; }
    .w-55 { width: 55%; }
    .w-60 { width: 60%; }
    .w-70 { width: 70%; }
    .w-75 { width: 75%; }
    .w-85 { width: 85%; }
    .w-90 { width: 90%; }
    .w-100 { width: 100%; }
    .spinner-panel {
      background: #fffaf5;
      border: 1px dashed #fdba74;
      border-radius: 14px;
      padding: 2.25rem 1.25rem;
      display: flex;
      flex-direction: column;
      align-items: center;
      text-align: center;
      gap: 0.6rem;
    }
    .spinner-text { font-weight: 700; color: #7c2d12; font-size: 0.98rem; }
    .spinner-hint { color: #9a3412; font-size: 0.84rem; }
    .error-card {
      background: #fef2f2;
      border: 1px solid #fecaca;
      color: #991b1b;
      border-radius: 12px;
      padding: 1.2rem 1.35rem;
      display: flex;
      justify-content: space-between;
      align-items: center;
      gap: 1rem;
      flex-wrap: wrap;
    }
    .error-head { display: flex; align-items: flex-start; gap: 0.75rem; }
    .error-icon { font-size: 1.35rem; line-height: 1.2; }
    .error-head p { margin: 0.25rem 0 0; font-size: 0.88rem; color: #7f1d1d; }
    .btn-retry {
      background: #ea580c;
      color: #ffffff;
      border: none;
      border-radius: 8px;
      padding: 0.5rem 1rem;
      font-weight: 700;
      cursor: pointer;
    }
    .btn-retry:hover { background: #c2410c; }
    @keyframes skel-shimmer {
      0% { background-position: 200% 0; }
      100% { background-position: -200% 0; }
    }
    @keyframes skel-spin {
      to { transform: rotate(360deg); }
    }
    @media (prefers-reduced-motion: reduce) {
      .skel-bar { animation: none; }
      .mini-spinner { animation-duration: 1.5s; }
    }
  `],
})
export class LoadingStateComponent {
  @Input() variant: PanchangLoadingVariant = 'spinner';
  @Input() count = 4;
  @Input() label = 'लोड हो रहा है… Loading…';
  @Input() hint = '';
  @Input() error: string | null = null;
  @Input() showRetry = true;
  @Output() retry = new EventEmitter<void>();

  items(n: number): number[] {
    return Array.from({ length: Math.min(12, Math.max(1, Math.trunc(n) || 1)) }, (_, i) => i);
  }
}
