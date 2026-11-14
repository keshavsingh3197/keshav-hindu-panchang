import { Component, effect, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { LoadingService } from '../../../core/services/loading.service';

@Component({
  selector: 'app-global-loader',
  standalone: true,
  imports: [CommonModule],
  template: `
    @if (loading.active()) {
      <div class="global-progress" role="progressbar" aria-label="Loading Panchang data">
        <div class="global-progress-track"></div>
      </div>
    }
    @if (loading.active() && loading.slow() && !dismissed()) {
      <div class="wakeup-toast" role="status" aria-live="polite">
        <span class="toast-spinner" aria-hidden="true"></span>
        <div class="toast-copy">
          <strong>सर्फ़र से कनेक्ट हो रहा है…</strong>
          <span>Waking up Panchang backend &amp; fetching live data…</span>
        </div>
        <button type="button" class="toast-close" aria-label="Dismiss" (click)="dismissed.set(true)">✕</button>
      </div>
    }
  `,
  styles: [`
    .global-progress {
      position: fixed;
      top: 0;
      left: 0;
      right: 0;
      height: 3px;
      z-index: 9999;
      pointer-events: none;
      overflow: hidden;
      background: rgba(234, 88, 12, 0.16);
    }
    .global-progress-track {
      position: absolute;
      inset: 0 auto 0 0;
      width: 40%;
      background: linear-gradient(90deg, transparent, #ea580c, #f59e0b);
      animation: panchang-slide 1.15s ease-in-out infinite;
    }
    .wakeup-toast {
      position: fixed;
      bottom: 1.15rem;
      right: 1.15rem;
      z-index: 9998;
      display: flex;
      align-items: center;
      gap: 0.7rem;
      padding: 0.7rem 0.95rem;
      border-radius: 12px;
      background: #431407;
      color: #fff7ed;
      border: 1px solid rgba(254, 215, 170, 0.35);
      box-shadow: 0 12px 32px rgba(67, 20, 7, 0.32);
      max-width: min(92vw, 370px);
    }
    .toast-spinner {
      width: 18px;
      height: 18px;
      border-radius: 999px;
      border: 2.5px solid rgba(254, 215, 170, 0.25);
      border-top-color: #fb923c;
      animation: panchang-spin 0.75s linear infinite;
      flex: none;
    }
    .toast-copy {
      display: flex;
      flex-direction: column;
      gap: 0.1rem;
      min-width: 0;
      font-size: 0.82rem;
      line-height: 1.3;
    }
    .toast-copy strong { font-size: 0.84rem; color: #fef08a; }
    .toast-copy span { color: #ffedd5; font-size: 0.76rem; }
    .toast-close {
      border: none;
      background: transparent;
      color: #fed7aa;
      cursor: pointer;
      font-size: 0.9rem;
      padding: 0.15rem 0.25rem;
      margin-left: auto;
    }
    .toast-close:hover { color: #ffffff; }
    @keyframes panchang-slide {
      0% { transform: translateX(-110%); }
      100% { transform: translateX(310%); }
    }
    @keyframes panchang-spin {
      to { transform: rotate(360deg); }
    }
    @media (prefers-reduced-motion: reduce) {
      .global-progress-track { animation-duration: 2.2s; }
      .toast-spinner { animation-duration: 1.5s; }
    }
  `],
})
export class GlobalLoaderComponent {
  readonly loading = inject(LoadingService);
  readonly dismissed = signal(false);

  constructor() {
    effect(() => {
      if (!this.loading.slow()) {
        this.dismissed.set(false);
      }
    });
  }
}
