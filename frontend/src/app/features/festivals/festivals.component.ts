import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { PanchangService } from '../../core/services/panchang.service';
import { FestivalItem } from '../../core/models/panchang.models';

@Component({
  selector: 'app-festivals',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="festivals-page">
      <div class="header-box">
        <span class="sub-pill">🪔 सनातन पर्व एवं व्रत सारणी</span>
        <h1>आगामी प्रमुख व्रत एवं त्यौहार (Upcoming Hindu Festivals & Vrats)</h1>
        <p>
          वैदिक तिथियों के अनुसार आगामी एकादशी, प्रदोष, पूर्णिमा, अमावस्या एवं प्रमुख हिंदू पर्वों की सूची, 
          व्रत नियम एवं शास्त्रसम्मत पूजा विधि।
        </p>
      </div>

      <!-- Festivals Grid -->
      <section class="festivals-grid">
        @for (fest of festivals; track fest.id) {
          <div class="fest-card">
            <div class="fest-header">
              <span class="tithi-tag">{{ fest.monthVedic }} • {{ fest.tithi }}</span>
              <span class="fest-date">{{ fest.date | date:'fullDate' }}</span>
            </div>

            <h2>{{ fest.nameHindi }}</h2>
            <span class="eng-title">({{ fest.nameEnglish }})</span>

            <p class="fest-desc">{{ fest.description }}</p>

            <div class="info-block">
              <strong>पूजा विधि एवं अनुष्ठान:</strong>
              <p>{{ fest.ritualsAndPujaVidhi }}</p>
            </div>

            <div class="fasting-block">
              <strong>व्रत नियम (Fasting Rules):</strong>
              <p>{{ fest.fastingRules }}</p>
            </div>
          </div>
        }
      </section>

      <!-- Devotional Affiliate & Monetization Samagri Block -->
      <section class="affiliate-samagri-section">
        <div class="samagri-header">
          <h3>📦 प्रामाणिक पूजा सामग्री एवं वैदिक ग्रंथ (Monetized Devotional Kit)</h3>
          <p>त्यौहारों एवं दैनिक पूजा के लिए शुद्ध गंगाजल, पीतल दिया, तुलसी माला एवं शास्त्रसम्मत ग्रंथ:</p>
        </div>

        <div class="samagri-grid">
          <div class="item-card">
            <span class="item-icon">🪔</span>
            <h4>पीतल अखंड ज्योति दिया</h4>
            <p>शुद्ध पीतल से निर्मित दीर्घकालिक पूजा दीप।</p>
            <button class="btn-buy" (click)="openAffiliate('diya')">अमेज़न से प्राप्त करें →</button>
          </div>

          <div class="item-card">
            <span class="item-icon">📿</span>
            <h4>पंचमुखी रुद्राक्ष एवं तुलसी माला</h4>
            <p>हरिद्वार एवं काशी से अभिमंत्रित 108 मनकों की माला।</p>
            <button class="btn-buy" (click)="openAffiliate('mala')">अमेज़न से प्राप्त करें →</button>
          </div>

          <div class="item-card">
            <span class="item-icon">📖</span>
            <h4>श्रीमद्भगवद्गीता यथारूप (सटीक)</h4>
            <p>संस्कृत श्लोक, हिंदी अन्वय एवं विशद व्याख्या सहित।</p>
            <button class="btn-buy" (click)="openAffiliate('geeta')">अमेज़न से प्राप्त करें →</button>
          </div>
        </div>
      </section>
    </div>
  `,
  styles: [`
    .festivals-page {
      max-width: 1300px;
      margin: 0 auto;
      padding: 1.5rem 1rem 3rem;
      display: flex;
      flex-direction: column;
      gap: 1.75rem;
    }
    .header-box {
      border-bottom: 1px solid #fed7aa;
      padding-bottom: 1rem;
    }
    .sub-pill { font-size: 0.8rem; font-weight: 700; color: #ea580c; text-transform: uppercase; }
    .header-box h1 { margin: 0.4rem 0; font-size: 1.9rem; font-weight: 800; color: #431407; }
    .header-box p { margin: 0; color: #78350f; font-size: 1rem; max-width: 850px; line-height: 1.5; }

    .festivals-grid {
      display: grid;
      grid-template-columns: repeat(auto-fit, minmax(360px, 1fr));
      gap: 1.5rem;
    }
    .fest-card {
      background: #ffffff;
      border: 1px solid #fed7aa;
      border-radius: 14px;
      padding: 1.5rem;
      display: flex;
      flex-direction: column;
      gap: 0.8rem;
      box-shadow: 0 2px 5px rgba(124, 45, 18, 0.04);
    }
    .fest-header {
      display: flex;
      justify-content: space-between;
      align-items: center;
      flex-wrap: wrap;
      gap: 0.5rem;
    }
    .tithi-tag {
      background: #ffedd5;
      color: #c2410c;
      font-weight: 700;
      font-size: 0.8rem;
      padding: 0.2rem 0.6rem;
      border-radius: 9999px;
    }
    .fest-date { font-size: 0.85rem; color: #64748b; font-weight: 600; }
    .fest-card h2 { margin: 0; font-size: 1.4rem; color: #431407; }
    .eng-title { font-size: 0.85rem; color: #9a3412; font-weight: 600; }
    .fest-desc { margin: 0; color: #475569; font-size: 0.95rem; line-height: 1.5; }

    .info-block, .fasting-block {
      background: #fffaf5;
      border-radius: 8px;
      padding: 0.75rem 1rem;
      font-size: 0.88rem;
      line-height: 1.5;
    }
    .info-block strong { color: #7c2d12; display: block; margin-bottom: 0.2rem; }
    .fasting-block strong { color: #9a3412; display: block; margin-bottom: 0.2rem; }
    .info-block p, .fasting-block p { margin: 0; color: #334155; }

    .affiliate-samagri-section {
      background: #fdfaf7;
      border: 1px solid #fed7aa;
      border-radius: 16px;
      padding: 1.75rem;
    }
    .samagri-header h3 { margin: 0 0 0.3rem; font-size: 1.25rem; color: #431407; }
    .samagri-header p { margin: 0 0 1.25rem; color: #78350f; font-size: 0.95rem; }
    .samagri-grid {
      display: grid;
      grid-template-columns: repeat(auto-fit, minmax(260px, 1fr));
      gap: 1.25rem;
    }
    .item-card {
      background: #ffffff;
      border: 1px solid #fed7aa;
      border-radius: 12px;
      padding: 1.25rem;
      text-align: center;
      display: flex;
      flex-direction: column;
      align-items: center;
      gap: 0.5rem;
    }
    .item-icon { font-size: 2.2rem; }
    .item-card h4 { margin: 0; font-size: 1.05rem; color: #1e293b; }
    .item-card p { margin: 0; font-size: 0.85rem; color: #64748b; }
    .btn-buy {
      margin-top: auto;
      background: #ea580c;
      color: #ffffff;
      border: none;
      padding: 0.5rem 1rem;
      border-radius: 6px;
      font-weight: 600;
      font-size: 0.85rem;
      cursor: pointer;
      transition: background 0.2s;
    }
    .btn-buy:hover { background: #c2410c; }
  `]
})
export class FestivalsComponent implements OnInit {
  private panchangService = inject(PanchangService);

  festivals: FestivalItem[] = [];

  ngOnInit(): void {
    this.panchangService.getUpcomingFestivals().subscribe(f => this.festivals = f);
  }

  openAffiliate(item: string): void {
    alert(`Affiliate referral link activated for ${item}. (Funneling user traffic to monetized partner page).`);
  }
}
