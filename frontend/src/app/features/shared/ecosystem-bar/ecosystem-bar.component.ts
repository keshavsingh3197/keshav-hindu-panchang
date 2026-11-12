import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-ecosystem-bar',
  standalone: true,
  imports: [CommonModule],
  template: `
    <nav class="ecosystem-strip" aria-label="Keshav Singh Ecosystem">
      <div class="ecosystem-container">
        <span class="eco-label">Keshav Ecosystem:</span>
        <div class="eco-links">
          <a href="https://studio.keshavsingh.in" target="_blank" rel="noopener" class="eco-link">🎬 Studio AI</a>
          <a href="https://social.keshavsingh.in" target="_blank" rel="noopener" class="eco-link">📊 Social Analytics</a>
          <a href="https://panchang.keshavsingh.in" class="eco-link active">🕉️ Hindu Panchang</a>
          <a href="https://ghar.keshavsingh.in" target="_blank" rel="noopener" class="eco-link">🏠 Ghar Ledger</a>
          <a href="https://pratiyogita.keshavsingh.in" target="_blank" rel="noopener" class="eco-link">🏆 Pratiyogita</a>
          <a href="https://git.keshavsingh.in" target="_blank" rel="noopener" class="eco-link">✍️ Content Blog</a>
          <a href="https://admin.keshavsingh.in" target="_blank" rel="noopener" class="eco-link admin-pill">🛡️ Admin & SSO</a>
        </div>
      </div>
    </nav>
  `,
  styles: [`
    .ecosystem-strip {
      background: #1a0f00;
      color: #fed7aa;
      font-size: 0.8rem;
      border-bottom: 1px solid #451a03;
      padding: 0.35rem 1rem;
    }
    .ecosystem-container {
      max-width: 1300px;
      margin: 0 auto;
      display: flex;
      align-items: center;
      gap: 0.8rem;
      flex-wrap: wrap;
    }
    .eco-label {
      font-weight: 600;
      color: #ea580c;
      letter-spacing: 0.03em;
      text-transform: uppercase;
      font-size: 0.72rem;
    }
    .eco-links {
      display: flex;
      align-items: center;
      gap: 0.75rem;
      flex-wrap: wrap;
    }
    .eco-link {
      color: #ffedd5;
      text-decoration: none;
      padding: 0.15rem 0.45rem;
      border-radius: 4px;
      transition: all 0.15s ease;
      display: inline-flex;
      align-items: center;
      gap: 0.25rem;
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
      border: 1px solid #7c2d12;
      color: #fef08a;
    }
    .admin-pill:hover {
      background: #9a3412;
    }
  `]
})
export class EcosystemBarComponent {}
