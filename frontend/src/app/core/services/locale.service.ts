import { HttpClient } from '@angular/common/http';
import { Injectable, computed, effect, inject, signal } from '@angular/core';
import { catchError, of, tap } from 'rxjs';
import { environment } from '../../../environments/environment';

export type LanguageCode = 'hi' | 'en';

const STORAGE_KEY = 'panchang.language';
const OVERRIDDEN_KEY = 'panchang.language.visitorSet';

export const SUPPORTED_LANGUAGES: { code: LanguageCode; label: string; shortLabel: string }[] = [
  { code: 'hi', label: 'हिन्दी (Hindi)', shortLabel: 'हिं' },
  { code: 'en', label: 'English', shortLabel: 'EN' },
];

export interface LocalizationPolicy {
  siteKey: string;
  defaultLocale: LanguageCode;
  enabledLocales: string[];
  allowVisitorLanguageSwitch: boolean;
  showLanguagePicker: boolean;
  forceLocale: boolean;
  controlledBy: string;
  overrideCentralLocalization: boolean;
}

export interface SiteLocalizationConfigDto {
  id?: string;
  overrideCentralLocalization: boolean;
  defaultLocale: LanguageCode;
  enabledLocales: string[];
  allowVisitorLanguageSwitch: boolean;
  showLanguagePicker: boolean;
  forceLocale: boolean;
}

export interface SiteLocalizationAdminState {
  centralAdminPolicy: LocalizationPolicy;
  localSettings: SiteLocalizationConfigDto;
  effectivePolicy: LocalizationPolicy;
}

interface BundleResponse {
  locale: LanguageCode;
  version: string;
  policy: LocalizationPolicy;
  entries: Record<string, string>;
  source: string;
}

const BUILTIN_HI: Record<string, string> = {
  'panchang.brand.title': '🕉️ पंचांगम् & संस्कृति',
  'panchang.brand.sub': 'वैदिक पंचांग एवं आज का इतिहास',
  'panchang.nav.home': 'दैनिक पंचांग',
  'panchang.nav.videoHub': '३डी वीडियो हब',
  'panchang.nav.itihaas': 'आज का इतिहास',
  'panchang.nav.vrat': 'व्रत एवं त्यौहार',
  'panchang.nav.admin': 'एडमिन',
  'panchang.hero.title': 'दैनिक वैदिक पंचांग एवं शुभ मुहूर्त',
  'panchang.limbs.heading': 'पंचांग के पांच अंग (The 5 Core Limbs)',
  'panchang.card.tithi': 'तिथि',
  'panchang.card.nakshatra': 'नक्षत्र',
  'panchang.card.yoga': 'योग',
  'panchang.card.karana': 'करण',
  'panchang.card.vaara': 'वार',
  'panchang.sunMoon.heading': '☀️ सूर्य एवं चन्द्रमा का समय',
  'panchang.sun.sunrise': 'सूर्योदय',
  'panchang.sun.sunset': 'सूर्यास्त',
  'panchang.sun.moonrise': 'चन्द्रोदय',
  'panchang.sun.moonset': 'चन्द्रास्त',
  'panchang.sun.sunSign': 'सूर्य राशि',
  'panchang.sun.moonSign': 'चन्द्र राशि',
  'panchang.muhurat.heading': '⏳ शुभ एवं अशुभ मुहूर्त',
  'panchang.card.brahma': 'ब्रह्म मुहूर्त',
  'panchang.card.abhijit': 'अभिजीत मुहूर्त',
  'panchang.card.amrit': 'अमृत काल',
  'panchang.card.rahuKaal': 'राहुकाल (अशुभ)',
  'panchang.card.yamaganda': 'यमगण्ड',
  'panchang.card.gulika': 'गुलिक काल',
  'panchang.choghadiya.heading': 'दिन का चौघड़िया',
  'panchang.choghadiya.sub': 'शुभ कार्य एवं यात्रा हेतु',
  'panchang.cta.tag': '🎥 दैनिक वीडियो प्रसारण',
  'panchang.cta.title': 'आज का दैनिक राशिफल और पंचांग विचार वीडियो देखें',
  'panchang.cta.desc': 'आज के ग्रहों की स्थिति, 12 राशियों का फल, और दैनिक उपाय वीडियो में देखें। सीधे यूट्यूब पर जुड़ें।',
  'panchang.cta.btn': 'दैनिक वीडियो देखें →',
  'panchang.videoHub.title': 'दैनिक आध्यात्मिक वीडियो हब',
  'panchang.itihaas.title': 'आज का इतिहास — भारतीय एवं धर्मिक धरोहर',
  'panchang.itihaas.empty': 'इस तिथि के लिए अभी कोई ऐतिहासिक प्रसंग दर्ज नहीं है।',
  'panchang.itihaas.significance': 'सांस्कृतिक महत्व:',
  'panchang.festivals.title': 'आगामी प्रमुख व्रत एवं त्यौहार',
  'panchang.festivals.pujaLabel': 'पूजा विधि एवं अनुष्ठान:',
  'panchang.festivals.fastingLabel': 'व्रत नियम:',
  'panchang.festivals.samagriHeading': '📦 प्रामाणिक पूजा सामग्री, वैदिक ग्रंथ एवं आध्यात्मिक संसाधन',
  'panchang.festivals.samagriSub': 'त्यौहारों एवं दैनिक पूजा के लिए आवश्यक सामग्री, मंत्र वीडियो एवं पावन संसाधन:',
  'panchang.prevDay': '‹ पिछला दिन',
  'panchang.today': 'आज',
  'panchang.nextDay': 'अगला दिन ›',
  'panchang.cityLabel': 'स्थान:',
  'ecosystem.language.label': 'भाषा',
};

const BUILTIN_EN: Record<string, string> = {
  'panchang.brand.title': '🕉️ Panchangam & Culture',
  'panchang.brand.sub': 'Vedic Panchang & Today in History',
  'panchang.nav.home': 'Daily Panchang',
  'panchang.nav.videoHub': '3D Video Hub',
  'panchang.nav.itihaas': 'Today in History',
  'panchang.nav.vrat': 'Vrat & Festivals',
  'panchang.nav.admin': 'Admin',
  'panchang.hero.title': 'Daily Vedic Panchang & Shubh Muhurat',
  'panchang.limbs.heading': 'The 5 Core Limbs (Pancha-Anga)',
  'panchang.card.tithi': 'Tithi',
  'panchang.card.nakshatra': 'Nakshatra',
  'panchang.card.yoga': 'Yoga',
  'panchang.card.karana': 'Karana',
  'panchang.card.vaara': 'Vaara',
  'panchang.sunMoon.heading': '☀️ Sun & Moon Timings',
  'panchang.sun.sunrise': 'Sunrise',
  'panchang.sun.sunset': 'Sunset',
  'panchang.sun.moonrise': 'Moonrise',
  'panchang.sun.moonset': 'Moonset',
  'panchang.sun.sunSign': 'Sun Sign',
  'panchang.sun.moonSign': 'Moon Sign',
  'panchang.muhurat.heading': '⏳ Auspicious & Inauspicious Muhurat',
  'panchang.card.brahma': 'Brahma Muhurat',
  'panchang.card.abhijit': 'Abhijit Muhurat',
  'panchang.card.amrit': 'Amrit Kaal',
  'panchang.card.rahuKaal': 'Rahu Kaal (Inauspicious)',
  'panchang.card.yamaganda': 'Yamaganda',
  'panchang.card.gulika': 'Gulika Kaal',
  'panchang.choghadiya.heading': 'Day Choghadiya',
  'panchang.choghadiya.sub': 'For Auspicious Works & Travel',
  'panchang.cta.tag': '🎥 Daily Video Broadcast',
  'panchang.cta.title': 'Watch Today’s Rashifal & Panchang Reflection Video',
  'panchang.cta.desc': 'Watch planetary insights, 12-rashi guidance, and daily remedies on video.',
  'panchang.cta.btn': 'Watch Daily Videos →',
  'panchang.videoHub.title': 'Daily Spiritual Videos & 3D Katha Hub',
  'panchang.itihaas.title': 'Today in Indian & Dharmik History',
  'panchang.itihaas.empty': 'No historical events recorded for this date yet.',
  'panchang.itihaas.significance': 'Cultural Significance:',
  'panchang.festivals.title': 'Upcoming Hindu Festivals & Vrats',
  'panchang.festivals.pujaLabel': 'Puja Vidhi & Rituals:',
  'panchang.festivals.fastingLabel': 'Fasting Rules:',
  'panchang.festivals.samagriHeading': '📦 Authentic Puja Samagri, Vedic Texts & Spiritual Resources',
  'panchang.festivals.samagriSub': 'Essential items, mantra videos, and sacred resources for festivals & daily worship:',
  'panchang.prevDay': '‹ Previous Day',
  'panchang.today': 'Today',
  'panchang.nextDay': 'Next Day ›',
  'panchang.cityLabel': 'City:',
  'ecosystem.language.label': 'Language',
};

@Injectable({ providedIn: 'root' })
export class LocaleService {
  private http = inject(HttpClient);
  private loaded = false;

  readonly language = signal<LanguageCode>(this.readStored());
  readonly policy = signal<LocalizationPolicy>({
    siteKey: 'panchang',
    defaultLocale: 'hi',
    enabledLocales: ['hi', 'en'],
    allowVisitorLanguageSwitch: true,
    showLanguagePicker: true,
    forceLocale: false,
    controlledBy: 'admin',
    overrideCentralLocalization: false,
  });
  readonly entries = signal<Record<string, string>>({});

  readonly showPicker = computed(() => {
    const p = this.policy();
    return p.showLanguagePicker && !p.forceLocale && p.enabledLocales.length > 1;
  });

  readonly canSwitch = computed(() => {
    const p = this.policy();
    return p.allowVisitorLanguageSwitch && !p.forceLocale;
  });

  readonly availableLanguages = computed(() => {
    const enabled = new Set(this.policy().enabledLocales.map(x => x.toLowerCase()));
    return SUPPORTED_LANGUAGES.filter(l => enabled.has(l.code));
  });

  constructor() {
    effect(() => {
      const lang = this.language();
      localStorage.setItem(STORAGE_KEY, lang);
      if (typeof document !== 'undefined') {
        document.documentElement.lang = lang;
      }
    });
    this.init();
  }

  init(): void {
    if (this.loaded) return;
    this.loaded = true;
    const visitorSet = localStorage.getItem(OVERRIDDEN_KEY) === '1';
    this.http
      .get<BundleResponse>(`${environment.apiUrl}/i18n/bundle/${this.language()}`)
      .pipe(catchError(() => of(null)))
      .subscribe(res => {
        if (!res) return;
        this.policy.set(res.policy);
        this.entries.set(res.entries || {});
        if (res.policy.forceLocale || !res.policy.allowVisitorLanguageSwitch || !visitorSet) {
          const target = this.normalize(res.policy.defaultLocale);
          if (target !== this.language()) {
            this.language.set(target);
            this.fetchBundle(target);
          }
        }
      });
  }

  set(code: LanguageCode, visitorChoice = true): void {
    if (visitorChoice && !this.canSwitch()) return;
    const normalized = this.normalize(code);
    if (visitorChoice) {
      localStorage.setItem(OVERRIDDEN_KEY, '1');
    }
    this.language.set(normalized);
    this.fetchBundle(normalized);
  }

  t(key: string, fallback?: string): string {
    const map = this.entries();
    if (map[key]) return map[key];
    const builtIn = this.language() === 'en' ? BUILTIN_EN[key] : BUILTIN_HI[key];
    return builtIn || BUILTIN_HI[key] || fallback || key;
  }

  getSiteLocalizationAdminState() {
    return this.http.get<SiteLocalizationAdminState>(`${environment.apiUrl}/settings/localization`);
  }

  updateSiteLocalization(req: SiteLocalizationConfigDto) {
    return this.http
      .put<SiteLocalizationAdminState>(`${environment.apiUrl}/settings/localization`, req)
      .pipe(
        tap(state => {
          this.policy.set(state.effectivePolicy);
          if (state.effectivePolicy.forceLocale || !state.effectivePolicy.allowVisitorLanguageSwitch) {
            this.set(this.normalize(state.effectivePolicy.defaultLocale), false);
          } else {
            this.fetchBundle(this.language());
          }
        })
      );
  }

  private fetchBundle(code: LanguageCode): void {
    this.http
      .get<BundleResponse>(`${environment.apiUrl}/i18n/bundle/${code}`)
      .pipe(catchError(() => of(null)))
      .subscribe(res => {
        if (!res) return;
        this.policy.set(res.policy);
        this.entries.set(res.entries || {});
      });
  }

  private normalize(raw: string | null | undefined): LanguageCode {
    return raw?.toLowerCase().startsWith('en') ? 'en' : 'hi';
  }

  private readStored(): LanguageCode {
    const stored = localStorage.getItem(STORAGE_KEY);
    return stored === 'en' ? 'en' : 'hi';
  }
}
