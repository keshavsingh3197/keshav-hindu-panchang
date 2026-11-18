import { Component, input, output } from '@angular/core';
import { CommonModule } from '@angular/common';

export interface InfoPopupContent {
  title: string;
  subtitle: string;
  items: { label: string; value: string }[];
  note: string;
}

@Component({
  selector: 'app-panchang-info-modal',
  standalone: true,
  imports: [CommonModule],
  template: `
    @if (info(); as item) {
      <div class="info-backdrop" (click)="closed.emit()">
        <div class="info-modal" (click)="$event.stopPropagation()" role="dialog" aria-modal="true">
          <div class="info-modal-head">
            <div>
              <h4>{{ item.title }}</h4>
              <small>{{ item.subtitle }}</small>
            </div>
            <button type="button" class="btn-close-info" (click)="closed.emit()">✕</button>
          </div>
          <div class="info-items">
            @for (it of item.items; track it.label) {
              <div class="info-row"><span>{{ it.label }}</span><strong>{{ it.value }}</strong></div>
            }
          </div>
          <p class="info-note">{{ item.note }}</p>
        </div>
      </div>
    }
  `,
  styles: [`
    .info-backdrop { position: fixed; inset: 0; background: rgba(15, 23, 42, 0.55); z-index: 90; display: flex; align-items: center; justify-content: center; padding: 1rem; }
    .info-modal { width: min(480px, 100%); background: #fff; border: 1px solid #fdba74; border-radius: 14px; padding: 1.25rem; box-shadow: 0 20px 45px rgba(0, 0, 0, 0.25); display: flex; flex-direction: column; gap: 0.85rem; }
    .info-modal-head { display: flex; justify-content: space-between; align-items: flex-start; color: #431407; }
    .info-modal-head h4 { margin: 0; font-size: 1.1rem; }
    .info-modal-head small { color: #9a3412; font-weight: 600; }
    .btn-close-info { background: transparent; border: none; font-weight: 800; cursor: pointer; font-size: 1rem; color: inherit; }
    .info-items { display: flex; flex-direction: column; gap: 0.4rem; }
    .info-row { display: flex; justify-content: space-between; background: #fffaf5; padding: 0.4rem 0.6rem; border-radius: 6px; font-size: 0.86rem; }
    .info-note { margin: 0; font-size: 0.82rem; color: #475569; line-height: 1.45; }
  `]
})
export class PanchangInfoModalComponent {
  readonly info = input<InfoPopupContent | null>(null);
  readonly closed = output<void>();
}
