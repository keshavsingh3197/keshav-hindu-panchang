import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { AuthService } from '../../core/services/auth.service';
import { PanchangService } from '../../core/services/panchang.service';
import { SpiritualVideo } from '../../core/models/video.models';

@Component({
  selector: 'app-manage-content',
  standalone: true,
  imports: [CommonModule, FormsModule],
  template: `
    <div class="manage-page">
      <div class="header-box">
        <span class="role-badge">🛡️ Role-Gated Admin / Editor Console</span>
        <h1>दैनिक वीडियो एवं पंचांग सामग्री प्रबंधन (Manage Daily Content)</h1>
        <p>
          केवल अधिकृत संपादक (Editors) एवं प्रशासक (Admins) ही दैनिक वीडियो लिंक एवं समाचार अपडेट कर सकते हैं। 
          यह सुविधा केंद्रीय SSO (admin.keshavsingh.in) द्वारा सुरक्षित है।
        </p>
      </div>

      @if (!auth.isAuthenticated()) {
        <div class="auth-gate-card">
          <div class="gate-icon">🔒</div>
          <h2>प्रमाणीकरण आवश्यक है (Authentication Required)</h2>
          <p>दैनिक वीडियो लिंक अपडेट करने के लिए कृपया केंद्रीय SSO द्वारा लॉगिन करें।</p>
          <button (click)="auth.loginRedirect()" class="btn-sso">
            Sign In with Central SSO
          </button>
        </div>
      } @else if (!auth.isEditorOrAdmin()) {
        <div class="role-restricted-card">
          <div class="gate-icon">⚠️</div>
          <h2>अपर्याप्त अधिकार (Access Restricted)</h2>
          <p>
            आप <strong>{{ auth.user()?.displayName }}</strong> के रूप में लॉगिन हैं (रोल: <code>{{ auth.user()?.roles?.join(', ') }}</code>)। 
            इस पृष्ठ को एक्सेस करने के लिए <strong>Admin</strong> या <strong>Editor</strong> रोल आवश्यक है।
          </p>
          <p class="help-text">यदि आपको अधिकारों की आवश्यकता है, तो कृपया मुख्य Admin पोर्टल से संपर्क करें।</p>
        </div>
      } @else {
        <!-- Authorized Editor Panel -->
        <section class="editor-panel">
          <h3>➕ नया दैनिक वीडियो लिंक जोड़ें (Add / Update Daily Video)</h3>
          <p class="panel-sub">यूट्यूब वीडियो का लिंक या आईडी दर्ज करें। यह वीडियो तुरंत मुख्य पृष्ठ एवं वीडियो हब पर लाइव हो जाएगा।</p>

          <form (ngSubmit)="submitVideo()" class="video-form">
            <div class="form-group">
              <label>वीडियो शीर्षक (Title)</label>
              <input type="text" [(ngModel)]="newVideo.title" name="title" required placeholder="उदा: आज का दैनिक पंचांग एवं राशिफल..." class="f-input" />
            </div>

            <div class="form-group">
              <label>यूट्यूब वीडियो लिंक या Video ID (YouTube URL or ID)</label>
              <input 
                type="text" 
                [(ngModel)]="newVideo.youTubeUrl" 
                name="url" 
                (input)="extractVideoId()"
                required 
                placeholder="https://www.youtube.com/watch?v=... या केवल ID" 
                class="f-input" />
            </div>

            <div class="form-row">
              <div class="form-group">
                <label>श्रेणी (Category)</label>
                <select [(ngModel)]="newVideo.category" name="category" class="f-select">
                  <option value="Daily News & Rashifal">दैनिक समाचार एवं राशिफल (Daily News & Rashifal)</option>
                  <option value="Pauranik Katha">पौराणिक कथा (Pauranik Katha)</option>
                  <option value="Mantra & Meditation">मंत्र एवं ध्यान (Mantra & Meditation)</option>
                  <option value="Temple Darshan">मंदिर लाइव दर्शन (Temple Darshan)</option>
                </select>
              </div>

              <div class="form-group">
                <label>वीडियो अवधि (Duration)</label>
                <input type="text" [(ngModel)]="newVideo.durationMinutes" name="duration" placeholder="उदा: 14:20" class="f-input" />
              </div>
            </div>

            <div class="form-group">
              <label>विवरण (Description)</label>
              <textarea [(ngModel)]="newVideo.description" name="desc" rows="3" placeholder="वीडियो का संक्षिप्त विवरण..." class="f-textarea"></textarea>
            </div>

            <div class="form-checkbox">
              <input type="checkbox" [(ngModel)]="newVideo.isFeaturedDailyVideo" name="featured" id="featCheck" />
              <label for="featCheck">इसे आज का मुख्य फीचर्ड वीडियो बनाएं (Set as Featured Daily Video)</label>
            </div>

            <button type="submit" class="btn-publish">🚀 वीडियो लाइव प्रकाशित करें (Publish Video)</button>
          </form>

          @if (publishSuccess) {
            <div class="success-banner">
              ✅ वीडियो सफलतापूर्वक प्रकाशित हो गया है! अब यह दैनिक वीडियो हब में प्रदर्शित हो रहा है।
            </div>
          }
        </section>
      }
    </div>
  `,
  styles: [`
    .manage-page {
      max-width: 900px;
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
    .role-badge {
      font-size: 0.8rem;
      font-weight: 700;
      color: #9a3412;
      background: #fed7aa;
      padding: 0.2rem 0.6rem;
      border-radius: 9999px;
      display: inline-block;
      margin-bottom: 0.4rem;
    }
    .header-box h1 { margin: 0.2rem 0 0.5rem; font-size: 1.8rem; color: #431407; }
    .header-box p { margin: 0; color: #78350f; font-size: 0.95rem; line-height: 1.5; }

    .auth-gate-card, .role-restricted-card {
      background: #ffffff;
      border: 1px solid #fed7aa;
      border-radius: 16px;
      padding: 3rem 2rem;
      text-align: center;
      box-shadow: 0 4px 15px rgba(0,0,0,0.03);
    }
    .gate-icon { font-size: 3rem; margin-bottom: 1rem; }
    .auth-gate-card h2, .role-restricted-card h2 { margin: 0 0 0.5rem; color: #431407; }
    .auth-gate-card p, .role-restricted-card p { color: #64748b; font-size: 1rem; margin-bottom: 1.5rem; }
    .btn-sso {
      background: #ea580c;
      color: white;
      border: none;
      padding: 0.75rem 1.6rem;
      border-radius: 8px;
      font-weight: 700;
      font-size: 1rem;
      cursor: pointer;
    }

    .editor-panel {
      background: #ffffff;
      border: 1px solid #fed7aa;
      border-radius: 14px;
      padding: 1.75rem;
      box-shadow: 0 1px 3px rgba(0,0,0,0.02);
    }
    .editor-panel h3 { margin: 0 0 0.3rem; font-size: 1.25rem; color: #431407; }
    .panel-sub { margin: 0 0 1.5rem; font-size: 0.9rem; color: #78350f; }

    .video-form {
      display: flex;
      flex-direction: column;
      gap: 1.2rem;
    }
    .form-group {
      display: flex;
      flex-direction: column;
      gap: 0.4rem;
    }
    .form-group label {
      font-size: 0.85rem;
      font-weight: 600;
      color: #431407;
    }
    .form-row {
      display: grid;
      grid-template-columns: 1fr 1fr;
      gap: 1rem;
    }
    .f-input, .f-select, .f-textarea {
      padding: 0.65rem 0.85rem;
      border: 1px solid #fdba74;
      border-radius: 8px;
      font-size: 0.95rem;
      font-family: inherit;
    }
    .form-checkbox {
      display: flex;
      align-items: center;
      gap: 0.6rem;
    }
    .form-checkbox label { font-size: 0.9rem; color: #431407; cursor: pointer; }
    .btn-publish {
      background: #059669;
      color: white;
      border: none;
      padding: 0.75rem 1.5rem;
      border-radius: 8px;
      font-weight: 700;
      font-size: 1rem;
      cursor: pointer;
      align-self: flex-start;
      transition: background 0.2s;
    }
    .btn-publish:hover { background: #047857; }
    .success-banner {
      margin-top: 1.2rem;
      background: #ecfdf5;
      color: #065f46;
      border: 1px solid #a7f3d0;
      padding: 0.85rem 1.2rem;
      border-radius: 8px;
      font-weight: 600;
      font-size: 0.95rem;
    }
  `]
})
export class ManageContentComponent implements OnInit {
  auth = inject(AuthService);
  private panchangService = inject(PanchangService);

  publishSuccess = false;
  newVideo: SpiritualVideo = {
    id: '',
    title: '',
    description: '',
    youTubeUrl: '',
    youTubeVideoId: '',
    embedUrl: '',
    thumbnailUrl: 'https://images.unsplash.com/photo-1545231027-637d2f6210f8?auto=format&fit=crop&w=600&q=80',
    category: 'Daily News & Rashifal',
    publishedDate: new Date().toISOString(),
    durationMinutes: '15:00',
    viewCount: 1200,
    isFeaturedDailyVideo: true,
    channelName: 'Keshav Singh Official',
    channelUrl: 'https://youtube.com/@KeshavSingh'
  };

  ngOnInit(): void {}

  extractVideoId(): void {
    const url = this.newVideo.youTubeUrl.trim();
    if (url.includes('v=')) {
      const parts = url.split('v=');
      this.newVideo.youTubeVideoId = parts[1].split('&')[0];
    } else if (url.includes('youtu.be/')) {
      this.newVideo.youTubeVideoId = url.split('youtu.be/')[1].split('?')[0];
    } else if (url.length === 11) {
      this.newVideo.youTubeVideoId = url;
    }

    if (this.newVideo.youTubeVideoId) {
      this.newVideo.embedUrl = `https://www.youtube.com/embed/${this.newVideo.youTubeVideoId}`;
      this.newVideo.thumbnailUrl = `https://img.youtube.com/vi/${this.newVideo.youTubeVideoId}/hqdefault.jpg`;
    }
  }

  submitVideo(): void {
    this.extractVideoId();
    if (!this.newVideo.title || !this.newVideo.youTubeVideoId) {
      alert('कृपया शीर्षक एवं मान्य यूट्यूब वीडियो लिंक दर्ज करें।');
      return;
    }

    this.panchangService.saveVideo(this.newVideo).subscribe(() => {
      this.publishSuccess = true;
      setTimeout(() => this.publishSuccess = false, 5000);
      this.newVideo = {
        id: '',
        title: '',
        description: '',
        youTubeUrl: '',
        youTubeVideoId: '',
        embedUrl: '',
        thumbnailUrl: 'https://images.unsplash.com/photo-1545231027-637d2f6210f8?auto=format&fit=crop&w=600&q=80',
        category: 'Daily News & Rashifal',
        publishedDate: new Date().toISOString(),
        durationMinutes: '15:00',
        viewCount: 1200,
        isFeaturedDailyVideo: true,
        channelName: 'Keshav Singh Official',
        channelUrl: 'https://youtube.com/@KeshavSingh'
      };
    });
  }
}
