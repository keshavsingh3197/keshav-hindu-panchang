import { Component, OnInit, inject } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { EcosystemBarComponent } from './features/shared/ecosystem-bar/ecosystem-bar.component';
import { HeaderComponent } from './features/shared/header/header.component';
import { AuthService } from './core/services/auth.service';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [RouterOutlet, EcosystemBarComponent, HeaderComponent],
  template: `
    <app-ecosystem-bar />
    <app-header />
    <main class="main-content">
      <router-outlet />
    </main>
    <footer class="app-footer">
      <div class="footer-container">
        <div class="footer-col">
          <strong>🕉️ पंचांगम् & संस्कृति — Keshav Singh Network</strong>
          <p>दैनिक वैदिक पंचांग, शुभ मुहूर्त, आज का इतिहास एवं सनातन ज्ञान प्रसार।</p>
        </div>
        <div class="footer-links">
          <a href="https://studio.keshavsingh.in" target="_blank" rel="noopener">AnimStudio AI</a>
          <a href="https://social.keshavsingh.in" target="_blank" rel="noopener">Social Analytics</a>
          <a href="https://admin.keshavsingh.in" target="_blank" rel="noopener">Central SSO</a>
          <a href="https://git.keshavsingh.in" target="_blank" rel="noopener">Content Blog</a>
        </div>
      </div>
      <div class="footer-bottom">
        © 2026 Keshav Singh. Part of the keshavsingh.in ecosystem. All rights reserved.
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

  ngOnInit(): void {
    this.auth.refresh().subscribe({
      next: (session) => {
        console.log('SSO session active for:', session.user.email);
      },
      error: () => {
        console.log('Operating in public visitor mode.');
      }
    });
  }
}
