import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { EcosystemConfigService } from '../../../core/services/ecosystem-config.service';

@Component({
  selector: 'app-ecosystem-bar',
  standalone: true,
  imports: [CommonModule],
  template: `
    <nav class="ecosystem-strip" aria-label="Ecosystem Navigation">
      <div class="ecosystem-container">
        <span class="eco-label">{{ ecosystem.creatorName() }} Network:</span>
        <div class="eco-links">
          @for (site of sites(); track site.key) {
            <a [href]="site.url"
               [attr.target]="site.key === 'panchang' ? null : '_blank'"
               [attr.rel]="site.key === 'panchang' ? null : 'noopener'"
               class="eco-link"
               [class.active]="site.key === 'panchang'"
               [class.admin-pill]="site.key === 'admin'">
              <span>{{ site.icon || '🌐' }}</span>
              <span>{{ site.name }}</span>
            </a>
          }
          @for (ch of channels(); track ch.id || ch.key) {
            <a [href]="ch.url"
               target="_blank"
               rel="noopener"
               class="eco-link channel-pill"
               [title]="ch.tagline || ch.name">
              <span>📺</span>
              <span>{{ ch.handle || ch.name }}</span>
            </a>
          }
        </div>
      </div>
    </nav>
  `,
  styles: [`
    .ecosystem-strip {
      background: linear-gradient(90deg, #1a0f00 0%, #2a1302 100%);
      color: #fed7aa;
      font-size: 0.8rem;
      border-bottom: 1px solid #451a03;
      padding: 0.4rem 1rem;
    }
    .ecosystem-container {
      max-width: 1300px;
      margin: 0 auto;
      display: flex;
      align-items: center;
      gap: 0.75rem;
      overflow-x: auto;
      scrollbar-width: none;
    }
    .ecosystem-container::-webkit-scrollbar { display: none; }
    .eco-label {
      font-weight: 700;
      color: #fb923c;
      letter-spacing: 0.04em;
      text-transform: uppercase;
      font-size: 0.72rem;
      white-space: nowrap;
      flex-shrink: 0;
    }
    .eco-links {
      display: flex;
      align-items: center;
      gap: 0.55rem;
      flex-wrap: nowrap;
    }
    .eco-link {
      color: #ffedd5;
      text-decoration: none;
      padding: 0.2rem 0.55rem;
      border-radius: 999px;
      transition: all 0.15s ease;
      display: inline-flex;
      align-items: center;
      gap: 0.3rem;
      white-space: nowrap;
      border: 1px solid transparent;
    }
    .eco-link:hover {
      background: #7c2d12;
      color: #ffffff;
    }
    .eco-link.active {
      background: #c2410c;
      color: #ffffff;
      font-weight: 600;
    }
    .admin-pill {
      border-color: #7c2d12;
      color: #fef08a;
    }
    .admin-pill:hover {
      background: #9a3412;
    }
    .channel-pill {
      border-color: rgba(251, 146, 60, 0.3);
      background: rgba(124, 45, 18, 0.4);
      color: #ffedd5;
    }
  `]
})
export class EcosystemBarComponent {
  readonly ecosystem = inject(EcosystemConfigService);
  readonly sites = this.ecosystem.ecosystemBarSites;
  readonly channels = this.ecosystem.panchangChannels;
}
