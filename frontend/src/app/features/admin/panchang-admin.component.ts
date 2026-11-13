import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { AuthService } from '../../core/services/auth.service';
import { PanchangService } from '../../core/services/panchang.service';
import { EcosystemConfigService } from '../../core/services/ecosystem-config.service';
import { PanchangSyncService } from '../../core/services/panchang-sync.service';
import {
  PanchangIntegrationSettingsView,
  PanchangSyncSummary
} from '../../core/models/ecosystem.models';
import {
  DailyKatha,
  DailySutra,
  DevotionalKitItem,
  FestivalItem,
  HistoricalIncident,
  PanchangAdminStatus,
  PanchangCity,
  PanchangOverride,
  PanchangSiteConfig,
  toLocalDateKey
} from '../../core/models/panchang.models';
import { SpiritualVideo } from '../../core/models/video.models';

type AdminTab = 'videos' | 'festivals' | 'history' | 'sutras' | 'cities' | 'site';

@Component({
  selector: 'app-panchang-admin',
  standalone: true,
  imports: [CommonModule, FormsModule],
  template: `
    <div class="admin-page">
      @if (!auth.isAdmin()) {
        <div class="denied-card">
          <h1>🛡️ Admin Access Required</h1>
          <p>Only users holding the <strong>Admin</strong> role can manage Hindu Panchang &amp; Cultural content.</p>
          <button (click)="auth.loginRedirect()" class="btn-primary">Sign in with SSO</button>
        </div>
      } @else {
        <!-- Top Admin Hero -->
        <section class="admin-hero">
          <div>
            <span class="admin-chip">🛡️ Admin-Only Control Plane</span>
            <h1>🕉️ पंचांगम् &amp; संस्कृति Admin Console</h1>
            <p>
              Manage daily Vedic videos, festivals, Aaj Ka Itihaas incidents, kathas, Sanskrit sutras, date overrides &amp; cities.
              @if (status(); as st) {
                <br>Storage: <strong>{{ st.storageMode }}</strong> • Signed in as <strong>{{ st.adminUser }}</strong>
              }
            </p>
          </div>
          <div class="hero-actions">
            <a [href]="ecosystem.adminPortalUrl() + '/website'" target="_blank" rel="noopener" class="btn-outline">
              🌐 Global Website Registry
            </a>
            <a [href]="ecosystem.adminPortalUrl() + '/settings'" target="_blank" rel="noopener" class="btn-outline">
              ⚙️ Central App Config
            </a>
          </div>
        </section>

        @if (feedback(); as fb) {
          <div class="feedback-bar" [class.error]="fb.isError">
            <span>{{ fb.message }}</span>
            <button type="button" (click)="feedback.set(null)">✕</button>
          </div>
        }

        <!-- Tab Navigation -->
        <nav class="tab-bar">
          <button [class.active]="activeTab() === 'videos'" (click)="activeTab.set('videos')">
            🎥 Videos ({{ videos().length }})
          </button>
          <button [class.active]="activeTab() === 'festivals'" (click)="activeTab.set('festivals')">
            🪔 Festivals ({{ festivals().length }}) &amp; Kit ({{ kits().length }})
          </button>
          <button [class.active]="activeTab() === 'history'" (click)="activeTab.set('history')">
            📜 Itihaas ({{ incidents().length }}) &amp; Kathas ({{ kathas().length }})
          </button>
          <button [class.active]="activeTab() === 'sutras'" (click)="activeTab.set('sutras')">
            🕉️ Sutras &amp; Overrides
          </button>
          <button [class.active]="activeTab() === 'cities'" (click)="activeTab.set('cities')">
            📍 Cities ({{ cities().length }})
          </button>
          <button [class.active]="activeTab() === 'site'" (click)="activeTab.set('site')">
            ⚙️ Site &amp; Global Config
          </button>
        </nav>

        <!-- TAB 1: SPIRITUAL VIDEOS -->
        @if (activeTab() === 'videos') {
          <section class="panel">
            <h2>{{ videoForm.id ? 'Edit Video' : 'Publish Spiritual / Rashifal Video' }}</h2>
            <div class="form-grid">
              <div class="field field-wide"><label>Video Title</label><input [(ngModel)]="videoForm.title" placeholder="आज का दैनिक पंचांग एवं राशिफल" /></div>
              <div class="field"><label>YouTube URL</label><input [(ngModel)]="videoForm.youTubeUrl" placeholder="https://www.youtube.com/watch?v=..." /></div>
              <div class="field"><label>YouTube Video ID (auto if blank)</label><input [(ngModel)]="videoForm.youTubeVideoId" /></div>
              <div class="field">
                <label>Category</label>
                <select [(ngModel)]="videoForm.category">
                  @for (cat of siteForm.videoCategories; track cat) {
                    <option [value]="cat">{{ cat }}</option>
                  }
                </select>
              </div>
              <div class="field"><label>Duration (mm:ss)</label><input [(ngModel)]="videoForm.durationMinutes" placeholder="14:20" /></div>
              <div class="field"><label>Channel Name</label><input [(ngModel)]="videoForm.channelName" /></div>
              <div class="field"><label>Channel URL</label><input [(ngModel)]="videoForm.channelUrl" /></div>
              <div class="field checkbox-field">
                <label><input type="checkbox" [(ngModel)]="videoForm.isFeaturedDailyVideo" /> Feature as Today's Primary Broadcast</label>
              </div>
              <div class="field field-wide"><label>Description</label><textarea rows="2" [(ngModel)]="videoForm.description"></textarea></div>
            </div>
            <div class="sync-panel">
              <div class="sync-header">
                <div>
                  <h3>📡 Central Channel &amp; Live YouTube Feed Automation</h3>
                  <p>Pulls enabled Panchang channels from Central Config and imports live YouTube videos via RSS / API v3.</p>
                </div>
                <div class="sync-actions">
                  <button type="button" (click)="syncNow()" [disabled]="syncing()" class="btn-primary">
                    {{ syncing() ? '⏳ Syncing Live Feeds...' : '🔄 Sync Central Channels & Live Videos' }}
                  </button>
                  <button type="button" (click)="purgeLegacy()" class="btn-secondary">🧹 Purge Demo Videos</button>
                  <button type="button" (click)="exportBackup()" class="btn-secondary">⬇️ Export JSON Backup</button>
                </div>
              </div>

              @if (lastSync(); as sum) {
                <div class="sync-summary-pill">
                  ✅ Synced {{ sum.channelsSynced }} channel(s), {{ sum.videosSynced }} video(s), purged {{ sum.legacyVideosPurged }} demo row(s)
                  @if (sum.warnings.length > 0) {
                    <span>• ⚠️ {{ sum.warnings.join(' | ') }}</span>
                  }
                </div>
              }

              <div class="integration-form">
                <div class="field">
                  <label>YouTube Data API v3 Key (Optional — Keyless RSS active automatically)</label>
                  <input type="password" [(ngModel)]="ytApiKeyInput" [placeholder]="integrations()?.hasYouTubeApiKey ? integrations()!.maskedYouTubeApiKey : 'AIza... (optional for rich stats)'" />
                </div>
                <div class="field">
                  <label>Central Config API URL</label>
                  <input [(ngModel)]="centralUrlInput" placeholder="https://id.keshavsingh.in/api/config" />
                </div>
                <div class="field checkbox-field">
                  <label><input type="checkbox" [(ngModel)]="autoSyncOnStartup" /> Auto-sync on Backend Startup</label>
                </div>
                <div class="btn-row">
                  <button type="button" (click)="saveIntegrations(false)" class="btn-primary">🔐 Save Integration Settings</button>
                  @if (integrations()?.hasYouTubeApiKey) {
                    <button type="button" (click)="saveIntegrations(true)" class="btn-sm danger">Clear Saved Key</button>
                  }
                </div>
              </div>
            </div>

            <div class="btn-row">
              <button (click)="saveVideo()" class="btn-primary">💾 Save Video</button>
              <button (click)="videoForm = emptyVideo()" class="btn-secondary">Reset</button>
            </div>

            <div class="table-wrap">
              <table class="admin-table">
                <thead><tr><th>Title</th><th>Category</th><th>Featured</th><th>Views</th><th>Actions</th></tr></thead>
                <tbody>
                  @for (v of videos(); track v.id) {
                    <tr>
                      <td><strong>{{ v.title }}</strong><br><small>{{ v.youTubeUrl }}</small></td>
                      <td>{{ v.category }}</td>
                      <td>{{ v.isFeaturedDailyVideo ? '⭐ Featured' : '—' }}</td>
                      <td>{{ v.viewCount | number }}</td>
                      <td class="act-cell">
                        <button (click)="editVideo(v)" class="btn-sm">Edit</button>
                        <button (click)="removeVideo(v.id)" class="btn-sm danger">Delete</button>
                      </td>
                    </tr>
                  }
                </tbody>
              </table>
            </div>
          </section>
        }

        <!-- TAB 2: FESTIVALS & DEVOTIONAL KIT -->
        @if (activeTab() === 'festivals') {
          <section class="panel">
            <h2>{{ festivalForm.id ? 'Edit Festival' : 'Add Upcoming Festival / Vrat' }}</h2>
            <div class="form-grid">
              <div class="field"><label>Hindi Name</label><input [(ngModel)]="festivalForm.nameHindi" placeholder="देवउठनी एकादशी" /></div>
              <div class="field"><label>English Name</label><input [(ngModel)]="festivalForm.nameEnglish" placeholder="Devutthana Ekadashi" /></div>
              <div class="field"><label>Date</label><input type="date" [(ngModel)]="festivalDateInput" /></div>
              <div class="field"><label>Vedic Month</label><input [(ngModel)]="festivalForm.monthVedic" placeholder="Kartika" /></div>
              <div class="field"><label>Tithi</label><input [(ngModel)]="festivalForm.tithi" placeholder="Shukla Ekadashi" /></div>
              <div class="field checkbox-field"><label><input type="checkbox" [(ngModel)]="festivalForm.isMajorFestival" /> Major Festival</label></div>
              <div class="field field-wide"><label>Description</label><textarea rows="2" [(ngModel)]="festivalForm.description"></textarea></div>
              <div class="field field-wide"><label>Rituals &amp; Puja Vidhi</label><textarea rows="2" [(ngModel)]="festivalForm.ritualsAndPujaVidhi"></textarea></div>
              <div class="field field-wide"><label>Fasting Rules</label><input [(ngModel)]="festivalForm.fastingRules" /></div>
            </div>
            <div class="btn-row">
              <button (click)="saveFestival()" class="btn-primary">💾 Save Festival</button>
              <button (click)="resetFestivalForm()" class="btn-secondary">Reset</button>
            </div>

            <div class="table-wrap">
              <table class="admin-table">
                <thead><tr><th>Festival</th><th>Date</th><th>Month / Tithi</th><th>Actions</th></tr></thead>
                <tbody>
                  @for (f of festivals(); track f.id) {
                    <tr>
                      <td><strong>{{ f.nameHindi }}</strong><br><small>{{ f.nameEnglish }}</small></td>
                      <td>{{ f.date | date:'mediumDate' }}</td>
                      <td>{{ f.monthVedic }} • {{ f.tithi }}</td>
                      <td class="act-cell">
                        <button (click)="editFestival(f)" class="btn-sm">Edit</button>
                        <button (click)="removeFestival(f.id)" class="btn-sm danger">Delete</button>
                      </td>
                    </tr>
                  }
                </tbody>
              </table>
            </div>

            <hr class="divider" />
            <h2>{{ kitForm.id ? 'Edit Devotional Kit Item' : 'Add Devotional Samagri Kit Item' }}</h2>
            <div class="form-grid">
              <div class="field"><label>Icon</label><input [(ngModel)]="kitForm.icon" placeholder="🪔" /></div>
              <div class="field"><label>Title</label><input [(ngModel)]="kitForm.title" /></div>
              <div class="field"><label>Button Label</label><input [(ngModel)]="kitForm.actionLabel" /></div>
              <div class="field"><label>External / Destination URL</label><input [(ngModel)]="kitForm.externalUrl" /></div>
              <div class="field"><label>Sort Order</label><input type="number" [(ngModel)]="kitForm.sortOrder" /></div>
              <div class="field checkbox-field"><label><input type="checkbox" [(ngModel)]="kitForm.isActive" /> Active</label></div>
              <div class="field field-wide"><label>Description</label><input [(ngModel)]="kitForm.description" /></div>
            </div>
            <div class="btn-row">
              <button (click)="saveKit()" class="btn-primary">💾 Save Kit Item</button>
              <button (click)="kitForm = emptyKit()" class="btn-secondary">Reset</button>
            </div>

            <div class="table-wrap">
              <table class="admin-table">
                <thead><tr><th>Item</th><th>URL</th><th>Order</th><th>Actions</th></tr></thead>
                <tbody>
                  @for (k of kits(); track k.id) {
                    <tr>
                      <td>{{ k.icon }} <strong>{{ k.title }}</strong></td>
                      <td><small>{{ k.externalUrl }}</small></td>
                      <td>{{ k.sortOrder }}</td>
                      <td class="act-cell">
                        <button (click)="editKit(k)" class="btn-sm">Edit</button>
                        <button (click)="removeKit(k.id)" class="btn-sm danger">Delete</button>
                      </td>
                    </tr>
                  }
                </tbody>
              </table>
            </div>
          </section>
        }

        <!-- TAB 3: AAJ KA ITIHAAS & DAILY KATHAS -->
        @if (activeTab() === 'history') {
          <section class="panel">
            <h2>{{ incidentForm.id ? 'Edit Historical Incident' : 'Add Historical Incident (Aaj Ka Itihaas)' }}</h2>
            <div class="form-grid">
              <div class="field"><label>Date Key (MM-dd)</label><input [(ngModel)]="incidentForm.dateKey" placeholder="11-13" /></div>
              <div class="field"><label>Year</label><input type="number" [(ngModel)]="incidentForm.year" /></div>
              <div class="field"><label>Category</label><input [(ngModel)]="incidentForm.category" placeholder="Indian History" /></div>
              <div class="field"><label>Title</label><input [(ngModel)]="incidentForm.title" /></div>
              <div class="field field-wide"><label>Description</label><textarea rows="2" [(ngModel)]="incidentForm.description"></textarea></div>
              <div class="field field-wide"><label>Cultural Significance</label><input [(ngModel)]="incidentForm.culturalSignificance" /></div>
            </div>
            <div class="btn-row">
              <button (click)="saveIncident()" class="btn-primary">💾 Save Incident</button>
              <button (click)="incidentForm = emptyIncident()" class="btn-secondary">Reset</button>
            </div>

            <div class="table-wrap">
              <table class="admin-table">
                <thead><tr><th>Date / Year</th><th>Title</th><th>Category</th><th>Actions</th></tr></thead>
                <tbody>
                  @for (inc of incidents(); track inc.id) {
                    <tr>
                      <td><code>{{ inc.dateKey }}</code> ({{ inc.year }})</td>
                      <td><strong>{{ inc.title }}</strong></td>
                      <td>{{ inc.category }}</td>
                      <td class="act-cell">
                        <button (click)="editIncident(inc)" class="btn-sm">Edit</button>
                        <button (click)="removeIncident(inc.id)" class="btn-sm danger">Delete</button>
                      </td>
                    </tr>
                  }
                </tbody>
              </table>
            </div>

            <hr class="divider" />
            <h2>{{ kathaForm.id ? 'Edit Daily Katha' : 'Add Daily Pauranik Katha' }}</h2>
            <div class="form-grid">
              <div class="field"><label>Title</label><input [(ngModel)]="kathaForm.title" /></div>
              <div class="field"><label>Deity / Sage</label><input [(ngModel)]="kathaForm.deityOrSage" /></div>
              <div class="field field-wide"><label>Story Summary</label><textarea rows="2" [(ngModel)]="kathaForm.storySummary"></textarea></div>
              <div class="field"><label>Moral Teaching</label><input [(ngModel)]="kathaForm.moralTeaching" /></div>
              <div class="field"><label>Sanskrit Mantra</label><input [(ngModel)]="kathaForm.sanskritMantra" /></div>
            </div>
            <div class="btn-row">
              <button (click)="saveKatha()" class="btn-primary">💾 Save Katha</button>
              <button (click)="kathaForm = emptyKatha()" class="btn-secondary">Reset</button>
            </div>

            <div class="table-wrap">
              <table class="admin-table">
                <thead><tr><th>Katha</th><th>Deity / Sage</th><th>Actions</th></tr></thead>
                <tbody>
                  @for (k of kathas(); track k.id) {
                    <tr>
                      <td><strong>{{ k.title }}</strong></td>
                      <td>{{ k.deityOrSage }}</td>
                      <td class="act-cell">
                        <button (click)="editKatha(k)" class="btn-sm">Edit</button>
                        <button (click)="removeKatha(k.id)" class="btn-sm danger">Delete</button>
                      </td>
                    </tr>
                  }
                </tbody>
              </table>
            </div>
          </section>
        }

        <!-- TAB 4: SUTRAS & PANCHANG OVERRIDES -->
        @if (activeTab() === 'sutras') {
          <section class="panel">
            <h2>{{ sutraForm.id ? 'Edit Sanskrit Sutra' : 'Add Daily Sanskrit Sutra' }}</h2>
            <div class="form-grid">
              <div class="field field-wide"><label>Sanskrit Shloka</label><input [(ngModel)]="sutraForm.sanskritText" /></div>
              <div class="field field-wide"><label>English Meaning</label><input [(ngModel)]="sutraForm.englishMeaning" /></div>
              <div class="field"><label>Source Scripture</label><input [(ngModel)]="sutraForm.source" placeholder="Bhagavad Gita" /></div>
            </div>
            <div class="btn-row">
              <button (click)="saveSutra()" class="btn-primary">💾 Save Sutra</button>
              <button (click)="sutraForm = emptySutra()" class="btn-secondary">Reset</button>
            </div>

            <div class="table-wrap">
              <table class="admin-table">
                <thead><tr><th>Shloka</th><th>Source</th><th>Actions</th></tr></thead>
                <tbody>
                  @for (s of sutras(); track s.id) {
                    <tr>
                      <td><strong>{{ s.sanskritText }}</strong><br><small>{{ s.englishMeaning }}</small></td>
                      <td>{{ s.source }}</td>
                      <td class="act-cell">
                        <button (click)="editSutra(s)" class="btn-sm">Edit</button>
                        <button (click)="removeSutra(s.id)" class="btn-sm danger">Delete</button>
                      </td>
                    </tr>
                  }
                </tbody>
              </table>
            </div>

            <hr class="divider" />
            <h2>Per-Date Panchang Overrides (Custom Vrat, Tithi, Nakshatra or Sutra)</h2>
            <div class="form-grid">
              <div class="field"><label>Date (yyyy-MM-dd)</label><input type="date" [(ngModel)]="overrideForm.dateKey" /></div>
              <div class="field"><label>Custom Tithi (optional)</label><input [(ngModel)]="overrideForm.customTithi" /></div>
              <div class="field"><label>Custom Nakshatra (optional)</label><input [(ngModel)]="overrideForm.customNakshatra" /></div>
              <div class="field"><label>Special Vrat / Event Note</label><input [(ngModel)]="overrideForm.specialVratOrEvent" /></div>
              <div class="field field-wide"><label>Custom Sutra (optional)</label><input [(ngModel)]="overrideForm.customSutra" /></div>
            </div>
            <div class="btn-row">
              <button (click)="saveOverride()" class="btn-primary">💾 Save Date Override</button>
            </div>

            <div class="table-wrap">
              <table class="admin-table">
                <thead><tr><th>Date</th><th>Custom Tithi</th><th>Event Note</th><th>Actions</th></tr></thead>
                <tbody>
                  @for (ov of overrides(); track ov.dateKey) {
                    <tr>
                      <td><code>{{ ov.dateKey }}</code></td>
                      <td>{{ ov.customTithi || '—' }}</td>
                      <td>{{ ov.specialVratOrEvent || '—' }}</td>
                      <td class="act-cell">
                        <button (click)="editOverride(ov)" class="btn-sm">Edit</button>
                        <button (click)="removeOverride(ov.dateKey)" class="btn-sm danger">Delete</button>
                      </td>
                    </tr>
                  }
                </tbody>
              </table>
            </div>
          </section>
        }

        <!-- TAB 5: CITIES CRUD -->
        @if (activeTab() === 'cities') {
          <section class="panel">
            <h2>{{ cityForm.id ? 'Edit Panchang City' : 'Add City for Solar Muhurat Calculation' }}</h2>
            <div class="form-grid">
              <div class="field"><label>City Key Name</label><input [(ngModel)]="cityForm.name" placeholder="Ujjain, India" /></div>
              <div class="field"><label>Display Label (Hindi/English)</label><input [(ngModel)]="cityForm.labelHindi" placeholder="उज्जैन (Ujjain)" /></div>
              <div class="field"><label>Country</label><input [(ngModel)]="cityForm.country" placeholder="India" /></div>
              <div class="field"><label>Latitude</label><input type="number" step="0.0001" [(ngModel)]="cityForm.latitude" /></div>
              <div class="field"><label>Longitude</label><input type="number" step="0.0001" [(ngModel)]="cityForm.longitude" /></div>
              <div class="field"><label>Timezone Offset (hrs)</label><input type="number" step="0.5" [(ngModel)]="cityForm.timezoneOffsetHours" /></div>
              <div class="field checkbox-field"><label><input type="checkbox" [(ngModel)]="cityForm.isActive" /> Active in Dropdown</label></div>
            </div>
            <div class="btn-row">
              <button (click)="saveCity()" class="btn-primary">💾 Save City</button>
              <button (click)="cityForm = emptyCity()" class="btn-secondary">Reset</button>
            </div>

            <div class="table-wrap">
              <table class="admin-table">
                <thead><tr><th>City</th><th>Coordinates</th><th>TZ Offset</th><th>Status</th><th>Actions</th></tr></thead>
                <tbody>
                  @for (c of cities(); track c.id) {
                    <tr>
                      <td><strong>{{ c.labelHindi }}</strong><br><small>{{ c.name }}</small></td>
                      <td>{{ c.latitude }}, {{ c.longitude }}</td>
                      <td>UTC {{ c.timezoneOffsetHours >= 0 ? '+' : '' }}{{ c.timezoneOffsetHours }}</td>
                      <td>{{ c.isActive ? 'Active' : 'Hidden' }}</td>
                      <td class="act-cell">
                        <button (click)="editCity(c)" class="btn-sm">Edit</button>
                        <button (click)="removeCity(c.id)" class="btn-sm danger">Delete</button>
                      </td>
                    </tr>
                  }
                </tbody>
              </table>
            </div>
          </section>
        }

        <!-- TAB 6: SITE CONFIG & CENTRAL GLOBAL ECOSYSTEM -->
        @if (activeTab() === 'site') {
          <section class="panel">
            <h2>Panchang App Configuration</h2>
            <div class="form-grid">
              <div class="field"><label>Site Title (Hindi)</label><input [(ngModel)]="siteForm.siteNameHindi" /></div>
              <div class="field"><label>Site Subtitle</label><input [(ngModel)]="siteForm.siteSubtitle" /></div>
              <div class="field"><label>Default City</label><input [(ngModel)]="siteForm.defaultCityName" /></div>
              <div class="field"><label>Broadcast Channel Name</label><input [(ngModel)]="siteForm.broadcastChannelName" /></div>
              <div class="field"><label>Broadcast YouTube URL</label><input [(ngModel)]="siteForm.broadcastChannelUrl" /></div>
              <div class="field field-wide"><label>Video Categories (comma separated)</label><input [(ngModel)]="categoriesCsv" /></div>
              <div class="field field-wide"><label>Daily Broadcast Banner</label><textarea rows="2" [(ngModel)]="siteForm.dailyBroadcastBanner"></textarea></div>
            </div>
            <div class="btn-row">
              <button (click)="saveSiteConfig()" class="btn-primary">💾 Save Panchang Config</button>
            </div>

            <hr class="divider" />
            <h2>Central Global Configuration (Synced Live from Central IDP)</h2>
            <div class="global-summary">
              <div><strong>Creator Name:</strong> {{ ecosystem.creatorName() }}</div>
              <div><strong>Primary Channel:</strong> {{ ecosystem.youtubeChannelName() || 'Synced via Central Channels' }} {{ ecosystem.youtubeChannelUrl() ? '(' + ecosystem.youtubeChannelUrl() + ')' : '' }}</div>
              <div><strong>Connected Panchang Channels:</strong> {{ ecosystem.panchangChannels().length }} enabled channel(s)</div>
              <div><strong>Footer Copyright:</strong> {{ ecosystem.copyrightNotice() }}</div>
              <div><strong>Central Websites Synced:</strong> {{ ecosystem.websites().length }} apps</div>
            </div>
            <div class="btn-row">
              <a [href]="ecosystem.adminPortalUrl() + '/website'" target="_blank" rel="noopener" class="btn-primary">
                Edit Websites &amp; Channels in Central Admin →
              </a>
              <button type="button" (click)="syncNow()" [disabled]="syncing()" class="btn-secondary">
                {{ syncing() ? '⏳ Syncing...' : '🔄 Sync Now from Central Config' }}
              </button>
            </div>
          </section>
        }
      }
    </div>
  `,
  styles: [`
    .admin-page { max-width: 1300px; margin: 0 auto; padding: 1.5rem 1rem 3rem; display: flex; flex-direction: column; gap: 1.5rem; }
    .denied-card { background: #fff; border: 1px solid #fecaca; border-radius: 14px; padding: 2.5rem; text-align: center; max-width: 500px; margin: 3rem auto; }
    .admin-hero { background: linear-gradient(135deg, #431407 0%, #9a3412 100%); color: #fff; border-radius: 14px; padding: 1.75rem; display: flex; justify-content: space-between; align-items: center; flex-wrap: wrap; gap: 1rem; }
    .admin-chip { display: inline-block; background: #ea580c; color: #fff; font-size: 0.75rem; font-weight: 700; padding: 0.25rem 0.65rem; border-radius: 999px; margin-bottom: 0.5rem; }
    .admin-hero h1 { margin: 0 0 0.35rem; font-size: 1.55rem; }
    .admin-hero p { margin: 0; color: #fed7aa; font-size: 0.9rem; line-height: 1.45; }
    .hero-actions { display: flex; gap: 0.65rem; flex-wrap: wrap; }
    .btn-outline { border: 1px solid #fdba74; color: #fff7ed; padding: 0.55rem 0.95rem; border-radius: 8px; text-decoration: none; font-size: 0.85rem; font-weight: 600; }
    .btn-outline:hover { background: rgba(255,255,255,0.12); }
    .feedback-bar { background: #ecfdf5; border: 1px solid #a7f3d0; color: #065f46; padding: 0.75rem 1rem; border-radius: 8px; display: flex; justify-content: space-between; align-items: center; font-weight: 600; font-size: 0.9rem; }
    .feedback-bar.error { background: #fef2f2; border-color: #fecaca; color: #991b1b; }
    .feedback-bar button { background: transparent; border: none; cursor: pointer; font-weight: 700; }
    .tab-bar { display: flex; gap: 0.5rem; flex-wrap: wrap; }
    .tab-bar button { background: #fff; border: 1px solid #fed7aa; color: #431407; padding: 0.6rem 1rem; border-radius: 8px; font-weight: 600; cursor: pointer; font-size: 0.9rem; }
    .tab-bar button.active { background: #ea580c; color: #fff; border-color: #ea580c; }
    .panel { background: #fff; border: 1px solid #fed7aa; border-radius: 14px; padding: 1.5rem; display: flex; flex-direction: column; gap: 1.25rem; }
    .panel h2 { margin: 0; font-size: 1.15rem; color: #7c2d12; }
    .form-grid { display: grid; grid-template-columns: repeat(auto-fit, minmax(210px, 1fr)); gap: 0.9rem; }
    .field { display: flex; flex-direction: column; gap: 0.3rem; }
    .field-wide { grid-column: 1 / -1; }
    .field label { font-size: 0.78rem; font-weight: 700; color: #78350f; text-transform: uppercase; letter-spacing: 0.02em; }
    .field input, .field select, .field textarea { border: 1px solid #fed7aa; border-radius: 7px; padding: 0.5rem 0.65rem; font-size: 0.9rem; color: #1e293b; font-family: inherit; }
    .checkbox-field { justify-content: flex-end; }
    .checkbox-field label { text-transform: none; font-size: 0.9rem; display: flex; align-items: center; gap: 0.45rem; cursor: pointer; }
    .btn-row { display: flex; gap: 0.75rem; flex-wrap: wrap; }
    .btn-primary { background: #ea580c; color: #fff; border: none; padding: 0.6rem 1.2rem; border-radius: 8px; font-weight: 600; cursor: pointer; text-decoration: none; font-size: 0.9rem; }
    .btn-primary:hover { background: #c2410c; }
    .btn-secondary { background: #fff7ed; color: #7c2d12; border: 1px solid #fed7aa; padding: 0.6rem 1rem; border-radius: 8px; font-weight: 600; cursor: pointer; }
    .table-wrap { overflow-x: auto; }
    .admin-table { width: 100%; border-collapse: collapse; font-size: 0.88rem; }
    .admin-table th, .admin-table td { text-align: left; padding: 0.65rem 0.75rem; border-bottom: 1px solid #ffedd5; }
    .admin-table th { background: #fff7ed; font-weight: 700; color: #7c2d12; font-size: 0.78rem; text-transform: uppercase; }
    .act-cell { display: flex; gap: 0.4rem; flex-wrap: wrap; }
    .btn-sm { background: #fff7ed; color: #c2410c; border: 1px solid #fed7aa; padding: 0.3rem 0.6rem; border-radius: 6px; font-size: 0.8rem; font-weight: 600; cursor: pointer; }
    .btn-sm.danger { background: #fef2f2; color: #dc2626; border-color: #fecaca; }
    .divider { border: none; border-top: 1px solid #fed7aa; margin: 0.5rem 0; }
    .global-summary { background: #fff7ed; border: 1px solid #fed7aa; border-radius: 10px; padding: 1rem; display: flex; flex-direction: column; gap: 0.4rem; font-size: 0.9rem; color: #431407; }
    .sync-panel { background: #fffaf5; border: 1px dashed #fdba74; border-radius: 12px; padding: 1rem 1.15rem; display: flex; flex-direction: column; gap: 0.9rem; }
    .sync-header { display: flex; justify-content: space-between; align-items: flex-start; gap: 1rem; flex-wrap: wrap; }
    .sync-header h3 { margin: 0 0 0.25rem; font-size: 1rem; color: #7c2d12; }
    .sync-header p { margin: 0; font-size: 0.84rem; color: #9a3412; }
    .sync-actions { display: flex; gap: 0.5rem; flex-wrap: wrap; }
    .sync-summary-pill { background: #ecfdf5; border: 1px solid #86efac; color: #166534; border-radius: 8px; padding: 0.55rem 0.85rem; font-size: 0.84rem; font-weight: 600; }
    .integration-form { display: grid; grid-template-columns: repeat(auto-fit, minmax(220px, 1fr)); gap: 0.85rem; align-items: end; border-top: 1px solid #ffedd5; padding-top: 0.85rem; }
  `]
})
export class PanchangAdminComponent implements OnInit {
  readonly auth = inject(AuthService);
  private panchangService = inject(PanchangService);
  readonly ecosystem = inject(EcosystemConfigService);

  readonly activeTab = signal<AdminTab>('videos');
  readonly status = signal<PanchangAdminStatus | null>(null);
  readonly feedback = signal<{ message: string; isError: boolean } | null>(null);

  readonly videos = signal<SpiritualVideo[]>([]);
  readonly festivals = signal<FestivalItem[]>([]);
  readonly kits = signal<DevotionalKitItem[]>([]);
  readonly incidents = signal<HistoricalIncident[]>([]);
  readonly kathas = signal<DailyKatha[]>([]);
  readonly sutras = signal<DailySutra[]>([]);
  readonly overrides = signal<PanchangOverride[]>([]);
  readonly cities = signal<PanchangCity[]>([]);

  videoForm: SpiritualVideo = this.emptyVideo();
  festivalForm: FestivalItem = this.emptyFestival();
  private syncService = inject(PanchangSyncService);
  readonly syncing = signal(false);
  readonly integrations = signal<PanchangIntegrationSettingsView | null>(null);
  readonly lastSync = signal<PanchangSyncSummary | null>(null);
  ytApiKeyInput = '';
  centralUrlInput = '';
  autoSyncOnStartup = true;

  festivalDateInput = toLocalDateKey();
  kitForm: DevotionalKitItem = this.emptyKit();
  incidentForm: HistoricalIncident = this.emptyIncident();
  kathaForm: DailyKatha = this.emptyKatha();
  sutraForm: DailySutra = this.emptySutra();
  overrideForm: PanchangOverride = { dateKey: toLocalDateKey() };
  cityForm: PanchangCity = this.emptyCity();
  siteForm: PanchangSiteConfig = {
    siteNameHindi: 'पंचांगम् & संस्कृति',
    siteSubtitle: 'Vedic Panchang & Aaj Ka Itihaas',
    defaultCityName: 'New Delhi, India',
    broadcastChannelName: '',
    broadcastChannelUrl: '',
    dailyBroadcastBanner: '',
    videoCategories: ['Daily News & Rashifal', 'Pauranik Katha', 'Mantra & Meditation', 'Temple Darshan']
  };
  categoriesCsv = this.siteForm.videoCategories.join(', ');

  ngOnInit(): void {
    this.ecosystem.load();
    if (this.auth.isAdmin()) {
      this.loadAll();
    }
  }

  private loadAll(): void {
    this.panchangService.getAdminStatus().subscribe({
      next: s => this.status.set(s),
      error: err => this.notifyError('Failed to verify Admin API session', err)
    });
    this.panchangService.getAdminVideos().subscribe(v => this.videos.set(v));
    this.panchangService.getAdminFestivals().subscribe(f => this.festivals.set(f));
    this.panchangService.getAdminKits().subscribe(k => this.kits.set(k));
    this.panchangService.getAdminIncidents().subscribe(i => this.incidents.set(i));
    this.panchangService.getAdminKathas().subscribe(k => this.kathas.set(k));
    this.panchangService.getAdminSutras().subscribe(s => this.sutras.set(s));
    this.panchangService.getAdminOverrides().subscribe(o => this.overrides.set(o));
    this.panchangService.getAdminCities().subscribe(c => this.cities.set(c));
    this.panchangService.getSiteConfig().subscribe(cfg => {
      this.siteForm = cfg;
      this.categoriesCsv = (cfg.videoCategories || []).join(', ');
    });
    this.syncService.getIntegrations().subscribe({
      next: iv => {
        this.integrations.set(iv);
        this.centralUrlInput = iv.centralConfigUrl || '';
        this.autoSyncOnStartup = iv.autoSyncOnStartup;
      }
    });
  }

  syncNow(): void {
    this.syncing.set(true);
    this.syncService.syncEcosystem().subscribe({
      next: res => {
        this.syncing.set(false);
        this.lastSync.set(res);
        this.ecosystem.refresh();
        this.loadAll();
        this.notify(`Synced ${res.channelsSynced} channels and ${res.videosSynced} videos.`);
      },
      error: err => {
        this.syncing.set(false);
        this.notifyError('Ecosystem & video sync failed', err);
      }
    });
  }

  purgeLegacy(): void {
    this.syncService.purgeLegacyVideos().subscribe({
      next: res => {
        this.notify(`Purged ${res.removed} legacy/demo video record(s).`);
        this.panchangService.getAdminVideos().subscribe(v => this.videos.set(v));
      },
      error: err => this.notifyError('Purge failed', err)
    });
  }

  saveIntegrations(clearKey: boolean): void {
    this.syncService.updateIntegrations({
      youTubeApiKey: clearKey ? undefined : (this.ytApiKeyInput || undefined),
      clearYouTubeApiKey: clearKey,
      centralConfigUrl: this.centralUrlInput || undefined,
      autoSyncOnStartup: this.autoSyncOnStartup
    }).subscribe({
      next: iv => {
        this.integrations.set(iv);
        this.ytApiKeyInput = '';
        this.notify(clearKey ? 'YouTube API key cleared (keyless RSS active).' : 'Integration settings saved securely.');
      },
      error: err => this.notifyError('Failed to save integration settings', err)
    });
  }

  exportBackup(): void {
    this.syncService.exportBundle().subscribe({
      next: bundle => {
        const blob = new Blob([JSON.stringify(bundle, null, 2)], { type: 'application/json' });
        const url = URL.createObjectURL(blob);
        const a = document.createElement('a');
        a.href = url;
        a.download = `panchang-backup-${toLocalDateKey()}.json`;
        a.click();
        URL.revokeObjectURL(url);
        this.notify('Backup JSON downloaded.');
      },
      error: err => this.notifyError('Export failed', err)
    });
  }

  // ---- Videos ----
  editVideo(v: SpiritualVideo): void { this.videoForm = { ...v }; }
  saveVideo(): void {
    this.panchangService.saveAdminVideo(this.videoForm).subscribe({
      next: () => {
        this.notify('Video saved.');
        this.videoForm = this.emptyVideo();
        this.panchangService.getAdminVideos().subscribe(list => this.videos.set(list));
      },
      error: err => this.notifyError('Failed to save video', err)
    });
  }
  removeVideo(id: string): void {
    this.panchangService.deleteAdminVideo(id).subscribe({
      next: () => { this.videos.update(l => l.filter(x => x.id !== id)); this.notify('Video deleted.'); },
      error: err => this.notifyError('Failed to delete video', err)
    });
  }

  // ---- Festivals & Kits ----
  editFestival(f: FestivalItem): void {
    this.festivalForm = { ...f };
    this.festivalDateInput = (f.date || '').substring(0, 10) || toLocalDateKey();
  }
  resetFestivalForm(): void {
    this.festivalForm = this.emptyFestival();
    this.festivalDateInput = toLocalDateKey();
  }
  saveFestival(): void {
    const payload = { ...this.festivalForm, date: new Date(this.festivalDateInput + 'T00:00:00Z').toISOString() };
    this.panchangService.saveAdminFestival(payload).subscribe({
      next: () => {
        this.notify('Festival saved.');
        this.resetFestivalForm();
        this.panchangService.getAdminFestivals().subscribe(list => this.festivals.set(list));
      },
      error: err => this.notifyError('Failed to save festival', err)
    });
  }
  removeFestival(id: string): void {
    this.panchangService.deleteAdminFestival(id).subscribe({
      next: () => { this.festivals.update(l => l.filter(x => x.id !== id)); this.notify('Festival deleted.'); },
      error: err => this.notifyError('Failed to delete festival', err)
    });
  }
  editKit(k: DevotionalKitItem): void { this.kitForm = { ...k }; }
  saveKit(): void {
    this.panchangService.saveAdminKit(this.kitForm).subscribe({
      next: () => {
        this.notify('Devotional kit item saved.');
        this.kitForm = this.emptyKit();
        this.panchangService.getAdminKits().subscribe(list => this.kits.set(list));
      },
      error: err => this.notifyError('Failed to save kit item', err)
    });
  }
  removeKit(id: string): void {
    this.panchangService.deleteAdminKit(id).subscribe({
      next: () => { this.kits.update(l => l.filter(x => x.id !== id)); this.notify('Kit item removed.'); },
      error: err => this.notifyError('Failed to delete kit item', err)
    });
  }

  // ---- Itihaas & Kathas ----
  editIncident(i: HistoricalIncident): void { this.incidentForm = { ...i }; }
  saveIncident(): void {
    this.panchangService.saveAdminIncident(this.incidentForm).subscribe({
      next: () => {
        this.notify('Historical incident saved.');
        this.incidentForm = this.emptyIncident();
        this.panchangService.getAdminIncidents().subscribe(list => this.incidents.set(list));
      },
      error: err => this.notifyError('Failed to save incident', err)
    });
  }
  removeIncident(id: string): void {
    this.panchangService.deleteAdminIncident(id).subscribe({
      next: () => { this.incidents.update(l => l.filter(x => x.id !== id)); this.notify('Incident removed.'); },
      error: err => this.notifyError('Failed to delete incident', err)
    });
  }
  editKatha(k: DailyKatha): void { this.kathaForm = { ...k }; }
  saveKatha(): void {
    this.panchangService.saveAdminKatha(this.kathaForm).subscribe({
      next: () => {
        this.notify('Daily Katha saved.');
        this.kathaForm = this.emptyKatha();
        this.panchangService.getAdminKathas().subscribe(list => this.kathas.set(list));
      },
      error: err => this.notifyError('Failed to save katha', err)
    });
  }
  removeKatha(id: string): void {
    this.panchangService.deleteAdminKatha(id).subscribe({
      next: () => { this.kathas.update(l => l.filter(x => x.id !== id)); this.notify('Katha deleted.'); },
      error: err => this.notifyError('Failed to delete katha', err)
    });
  }

  // ---- Sutras & Overrides ----
  editSutra(s: DailySutra): void { this.sutraForm = { ...s }; }
  saveSutra(): void {
    this.panchangService.saveAdminSutra(this.sutraForm).subscribe({
      next: () => {
        this.notify('Daily sutra saved.');
        this.sutraForm = this.emptySutra();
        this.panchangService.getAdminSutras().subscribe(list => this.sutras.set(list));
      },
      error: err => this.notifyError('Failed to save sutra', err)
    });
  }
  removeSutra(id: string): void {
    this.panchangService.deleteAdminSutra(id).subscribe({
      next: () => { this.sutras.update(l => l.filter(x => x.id !== id)); this.notify('Sutra deleted.'); },
      error: err => this.notifyError('Failed to delete sutra', err)
    });
  }
  editOverride(ov: PanchangOverride): void { this.overrideForm = { ...ov }; }
  saveOverride(): void {
    this.panchangService.saveAdminOverride(this.overrideForm).subscribe({
      next: () => {
        this.notify(`Override saved for ${this.overrideForm.dateKey}.`);
        this.overrideForm = { dateKey: toLocalDateKey() };
        this.panchangService.getAdminOverrides().subscribe(list => this.overrides.set(list));
      },
      error: err => this.notifyError('Failed to save override', err)
    });
  }
  removeOverride(dateKey: string): void {
    this.panchangService.deleteAdminOverride(dateKey).subscribe({
      next: () => { this.overrides.update(l => l.filter(x => x.dateKey !== dateKey)); this.notify('Override removed.'); },
      error: err => this.notifyError('Failed to delete override', err)
    });
  }

  // ---- Cities ----
  editCity(c: PanchangCity): void { this.cityForm = { ...c }; }
  saveCity(): void {
    this.panchangService.saveAdminCity(this.cityForm).subscribe({
      next: () => {
        this.notify('City saved.');
        this.cityForm = this.emptyCity();
        this.panchangService.getAdminCities().subscribe(list => this.cities.set(list));
      },
      error: err => this.notifyError('Failed to save city', err)
    });
  }
  removeCity(id: string): void {
    this.panchangService.deleteAdminCity(id).subscribe({
      next: () => { this.cities.update(l => l.filter(x => x.id !== id)); this.notify('City deleted.'); },
      error: err => this.notifyError('Failed to delete city', err)
    });
  }

  // ---- Site Config ----
  saveSiteConfig(): void {
    const payload: PanchangSiteConfig = {
      ...this.siteForm,
      videoCategories: this.categoriesCsv.split(',').map(c => c.trim()).filter(Boolean)
    };
    this.panchangService.saveSiteConfig(payload).subscribe({
      next: saved => {
        this.siteForm = saved;
        this.notify('Panchang site configuration saved.');
      },
      error: err => this.notifyError('Failed to save site config', err)
    });
  }

  private notify(message: string): void { this.feedback.set({ message, isError: false }); }
  private notifyError(prefix: string, err: { status?: number }): void {
    const code = err?.status ? ` (HTTP ${err.status})` : '';
    this.feedback.set({ message: `${prefix}${code}`, isError: true });
  }

  emptyVideo(): SpiritualVideo {
    return {
      id: '',
      title: '',
      description: '',
      youTubeUrl: '',
      youTubeVideoId: '',
      embedUrl: '',
      thumbnailUrl: '',
      category: 'Daily News & Rashifal',
      publishedDate: new Date().toISOString(),
      durationMinutes: '12:00',
      viewCount: 0,
      isFeaturedDailyVideo: true,
      channelName: this.ecosystem?.youtubeChannelName() || '',
      channelUrl: this.ecosystem?.youtubeChannelUrl() || ''
    };
  }
  emptyFestival(): FestivalItem {
    return { id: '', nameHindi: '', nameEnglish: '', date: toLocalDateKey(), tithi: '', monthVedic: '', description: '', ritualsAndPujaVidhi: '', fastingRules: '', isMajorFestival: true };
  }
  emptyKit(): DevotionalKitItem {
    return { id: '', title: '', icon: '🪔', description: '', actionLabel: 'Explore →', externalUrl: '/videos', sortOrder: 40, isActive: true };
  }
  emptyIncident(): HistoricalIncident {
    return { id: '', dateKey: toLocalDateKey().substring(5), year: new Date().getFullYear(), title: '', category: 'Indian History', description: '', culturalSignificance: '' };
  }
  emptyKatha(): DailyKatha {
    return { id: '', title: '', deityOrSage: '', storySummary: '', moralTeaching: '', sanskritMantra: '' };
  }
  emptySutra(): DailySutra {
    return { id: '', sanskritText: '', englishMeaning: '', source: 'Bhagavad Gita' };
  }
  emptyCity(): PanchangCity {
    return { id: '', name: '', labelHindi: '', country: 'India', latitude: 23.1765, longitude: 75.7885, timezoneOffsetHours: 5.5, isActive: true };
  }
}
