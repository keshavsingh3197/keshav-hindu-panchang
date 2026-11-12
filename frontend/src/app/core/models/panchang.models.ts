export interface ChoghadiyaPeriod {
  name: string;
  type: string;
  timeRange: string;
}

export interface PanchangData {
  date: string;
  formattedDate: string;
  location: string;
  vikramSamvat: string;
  shakaSamvat: string;
  monthPurnimanta: string;
  monthAmanta: string;
  paksha: string;
  dayOfWeek: string;
  tithi: string;
  tithiEndTime: string;
  nakshatra: string;
  nakshatraEndTime: string;
  yoga: string;
  yogaEndTime: string;
  karana: string;
  karanaEndTime: string;
  vaara: string;
  sunrise: string;
  sunset: string;
  moonrise: string;
  moonset: string;
  sunSign: string;
  moonSign: string;
  abhijitMuhurat: string;
  brahmaMuhurat: string;
  amritKaal: string;
  rahuKaal: string;
  yamaganda: string;
  gulikaKaal: string;
  dayChoghadiya: ChoghadiyaPeriod[];
  nightChoghadiya: ChoghadiyaPeriod[];
  dailySanskritSutra: string;
  dailySutraMeaning: string;
}

export interface HistoricalIncident {
  id: string;
  dateKey: string;
  year: number;
  title: string;
  category: string;
  description: string;
  culturalSignificance: string;
  relatedVideoUrl?: string;
}

export interface DailyKatha {
  id: string;
  title: string;
  deityOrSage: string;
  storySummary: string;
  moralTeaching: string;
  sanskritMantra: string;
}

export interface FestivalItem {
  id: string;
  nameHindi: string;
  nameEnglish: string;
  date: string;
  tithi: string;
  monthVedic: string;
  description: string;
  ritualsAndPujaVidhi: string;
  fastingRules: string;
  isMajorFestival: boolean;
}

export interface PanchangCity {
  id: string;
  name: string;
  labelHindi: string;
  country: string;
  latitude: number;
  longitude: number;
  timezoneOffsetHours: number;
  isActive: boolean;
}

export interface DailySutra {
  id: string;
  sanskritText: string;
  englishMeaning: string;
  source: string;
}

export interface PanchangOverride {
  id?: string;
  dateKey: string;
  customTithi?: string;
  customNakshatra?: string;
  specialVratOrEvent?: string;
  customSutra?: string;
  customSutraMeaning?: string;
}

export interface DevotionalKitItem {
  id: string;
  title: string;
  icon: string;
  description: string;
  actionLabel: string;
  externalUrl: string;
  sortOrder: number;
  isActive: boolean;
}

export interface PanchangSiteConfig {
  id?: string;
  siteNameHindi: string;
  siteSubtitle: string;
  defaultCityName: string;
  broadcastChannelName: string;
  broadcastChannelUrl: string;
  dailyBroadcastBanner: string;
  videoCategories: string[];
  updatedAt?: string;
}

export interface PanchangAdminStatus {
  storageMode: string;
  videoCount: number;
  festivalCount: number;
  incidentCount: number;
  cityCount: number;
  adminUser: string;
}

export interface EcosystemWebsite {
  key: string;
  name: string;
  url: string;
  icon?: string;
  description?: string;
  sortOrder: number;
  showInEcosystemBar: boolean;
}

export interface AppConfigEnvelope {
  values?: Record<string, string>;
  websites?: EcosystemWebsite[];
}

export function toLocalDateKey(d: Date = new Date()): string {
  const y = d.getFullYear();
  const m = String(d.getMonth() + 1).padStart(2, '0');
  const day = String(d.getDate()).padStart(2, '0');
  return `${y}-${m}-${day}`;
}
