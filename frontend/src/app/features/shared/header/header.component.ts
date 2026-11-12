import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink, RouterLinkActive } from '@angular/router';
import { AuthService } from '../../../core/services/auth.service';

@Component({
  selector: 'app-header',
  standalone: true,
  imports: [CommonModule, RouterLink, RouterLinkActive],
  template: `
    <header class="main-header">
      <div class="header-container">
        <a routerLink="/" class="brand-link">
          <div class="brand-badge">🕉️ पंचांगम् & संस्कृति</div>
          <span class="brand-sub">Vedic Panchang & Aaj Ka Itihaas</span>
        </a>

        <nav class="nav-links">
          <a routerLink="/panchang" routerLinkActive="active-nav">दैनिक पंचांग (Panchang)</a>
          <a routerLink="/videos" routerLinkActive="active-nav">🎥 दैनिक वीडियो हब (Videos)</a>
          <a routerLink="/itihaas" routerLinkActive="active-nav">📜 आज का इतिहास (History)</a>
          <a routerLink="/festivals" routerLinkActive="active-nav">🪔 व्रत एवं त्यौहार (Festivals)</a>
          @if (auth.isAdmin()) {
            <a routerLink="/admin" routerLinkActive="active-nav" class="admin-link">🛡️ Admin</a>
          }
        </nav>

        <div class="header-actions">
          @if (auth.isAuthenticated()) {
            <div class="user-chip">
              <span class="user-avatar">{{ auth.user()?.displayName?.charAt(0) || 'U' }}</span>
              <span class="user-name">{{ auth.user()?.displayName }}</span>
              <span class="user-role">{{ auth.user()?.roles?.[0] || 'User' }}</span>
              <button (click)="logout()" class="btn-logout" title="Sign out">Sign Out</button>
            </div>
          } @else {
            <button (click)="login()" class="btn-sso-login">
              <span>Sign in with SSO</span>
            </button>
          }
        </div>
      </div>
    </header>
  `,
  styles: [`
    .main-header {
      background: #ffffff;
      border-bottom: 2px solid #fdba74;
      position: sticky;
      top: 0;
      z-index: 50;
      box-shadow: 0 2px 4px rgba(194, 65, 12, 0.05);
    }
    .header-container {
      max-width: 1300px;
      margin: 0 auto;
      padding: 0.85rem 1rem;
      display: flex;
      align-items: center;
      justify-content: space-between;
      gap: 1.5rem;
    }
    .brand-link {
      text-decoration: none;
      display: flex;
      flex-direction: column;
      gap: 0.1rem;
    }
    .brand-badge {
      font-size: 1.35rem;
      font-weight: 800;
      color: #9a3412;
      letter-spacing: -0.01em;
    }
    .brand-sub {
      font-size: 0.75rem;
      color: #78350f;
      font-weight: 600;
    }
    .nav-links {
      display: flex;
      align-items: center;
      gap: 1rem;
    }
    .nav-links a {
      text-decoration: none;
      color: #431407;
      font-weight: 600;
      font-size: 0.9rem;
      padding: 0.4rem 0.65rem;
      border-radius: 6px;
      transition: all 0.2s ease;
    }
    .nav-links a:hover {
      color: #c2410c;
      background: #fff7ed;
    }
    .nav-links a.active-nav {
      color: #ffffff;
      background: #ea580c;
    }
    .admin-link {
      color: #7c2d12 !important;
      border: 1px dashed #ea580c;
    }
    .header-actions {
      display: flex;
      align-items: center;
      gap: 1rem;
    }
    .user-chip {
      display: flex;
      align-items: center;
      gap: 0.5rem;
      background: #fff7ed;
      padding: 0.35rem 0.75rem;
      border-radius: 9999px;
      border: 1px solid #fed7aa;
    }
    .user-avatar {
      width: 26px;
      height: 26px;
      border-radius: 50%;
      background: #c2410c;
      color: #ffffff;
      font-size: 0.8rem;
      font-weight: 700;
      display: flex;
      align-items: center;
      justify-content: center;
    }
    .user-name {
      font-size: 0.85rem;
      font-weight: 600;
      color: #7c2d12;
    }
    .user-role {
      font-size: 0.7rem;
      background: #fed7aa;
      color: #7c2d12;
      padding: 0.15rem 0.4rem;
      border-radius: 4px;
      font-weight: 600;
    }
    .btn-logout {
      background: transparent;
      border: none;
      color: #b91c1c;
      font-size: 0.78rem;
      font-weight: 600;
      cursor: pointer;
      padding: 0.2rem 0.4rem;
      border-radius: 4px;
    }
    .btn-logout:hover {
      background: #fee2e2;
    }
    .btn-sso-login {
      background: #c2410c;
      color: white;
      border: none;
      padding: 0.55rem 1.1rem;
      border-radius: 8px;
      font-weight: 600;
      font-size: 0.9rem;
      cursor: pointer;
      transition: background 0.2s;
    }
    .btn-sso-login:hover {
      background: #9a3412;
    }
  `]
})
export class HeaderComponent {
  auth = inject(AuthService);

  login(): void {
    this.auth.loginRedirect();
  }

  logout(): void {
    this.auth.logout().subscribe();
  }
}
