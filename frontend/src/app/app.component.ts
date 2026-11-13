import { Component, OnInit, inject } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { EcosystemBarComponent } from './features/shared/ecosystem-bar/ecosystem-bar.component';
import { HeaderComponent } from './features/shared/header/header.component';
import { AdSlotComponent } from './features/shared/ad-slot/ad-slot.component';
import { AuthService } from './core/services/auth.service';
import { EcosystemConfigService } from './core/services/ecosystem-config.service';
import { PanchangService } from './core/services/panchang.service';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [RouterOutlet, EcosystemBarComponent, HeaderComponent, AdSlotComponent],
  template: `
    <app-ecosystem-bar />
    <app-header />
    <main class="main-content">
      <router-outlet />
    </main>
    <app-ad-slot placement="footer" />
    <footer class="app-footer">
      <div class="footer-container">
        <div class="footer-col">
          <strong>🕉️ पंचांगम् & संस्कृति — {{ creatorName() }} Network</strong>
          <p>दैनिक वैदिक पंचांग, शुभ मुहूर्त, आज का इतिहास एवं सनातन ज्ञान प्रसार।</p>
        </div>
        <div class="footer-links">
          @for (site of footerSites(); track site.key) {
            <a [href]="site.url" target="_blank" rel="noopener">{{ site.name }}</a>
          }
        </div>
      </div>
      <div class="footer-bottom">
        {{ copyrightNotice() }}
      </div>
    </footer>
  `,
  styles: [`
    :host {
      display: flex;
      flex-direction: column;
      min-height: 100vh;
    }
    .main-content {
      flex: 1;
    }
    .app-footer {
      background: #1c0a00;
      color: #fed7aa;
      border-top: 1px solid #7c2d12;
      padding: 2.5rem 1rem 1.5rem;
      margin-top: auto;
    }
    .footer-container {
      max-width: 1300px;
      margin: 0 auto;
      display: flex;
      justify-content: space-between;
      align-items: center;
      flex-wrap: wrap;
      gap: 1.5rem;
      padding-bottom: 1.5rem;
      border-bottom: 1px solid #431407;
    }
    .footer-col strong {
      color: #ffedd5;
      font-size: 1rem;
      display: block;
      margin-bottom: 0.25rem;
    }
    .footer-col p {
      margin: 0;
      font-size: 0.85rem;
    }
    .footer-links {
      display: flex;
      gap: 1.25rem;
      flex-wrap: wrap;
    }
    .footer-links a {
      color: #fed7aa;
      text-decoration: none;
      font-size: 0.85rem;
      transition: color 0.15s;
    }
    .footer-links a:hover {
      color: #f97316;
    }
    .footer-bottom {
      max-width: 1300px;
      margin: 1.25rem auto 0;
      font-size: 0.8rem;
      color: #fdba74;
      text-align: center;
    }
  `]
})
export class AppComponent implements OnInit {
  private auth = inject(AuthService);
  private panchangService = inject(PanchangService);
  private ecosystem = inject(EcosystemConfigService);

  readonly footerSites = this.ecosystem.footerSites;
  readonly copyrightNotice = this.ecosystem.copyrightNotice;
  readonly creatorName = this.ecosystem.creatorName;

  ngOnInit(): void {
    this.ecosystem.load();
    this.auth.ensureInitialized().subscribe();
    this.panchangService.getSiteConfig().subscribe();
  }
}
