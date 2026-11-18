export interface ChoghadiyaPeriod {
  name: string;
  type: string;
  timeRange: string;
}

export interface PanchangLocationMeta {
  cityId: string;
  displayName: string;
  state: string;
  country: string;
  latitude: number;
  longitude: number;
  elevationMeters: number;
  timeZoneId: string;
  effectiveUtcOffsetHours: number;
  isDaylightSavingActive: boolean;
  sunriseMethod: string;
}

export interface PanchangTransitionItem {
  name: string;
  startMinutesFromMidnight: number;
  endMinutesFromMidnight: number;
  timeRangeDisplay: string;
  endDisplay: string;
  isFullNight: boolean;
  note: string;
}

export interface PanchangHoraPeriod {
  planet: string;
  planetSanskrit: string;
  quality: string;
  bestFor: string;
  timeRange: string;
  startMinutes: number;
  endMinutes: number;
}

export interface PanchangLagnaPeriod {
  name: string;
  quality: string;
  timeRange: string;
  startMinutes: number;
  endMinutes: number;
}

export interface PanchangTimelineSegment {
  label: string;
  category: string;
  tone: 'shubh' | 'ashubh' | 'neutral' | string;
  startMinutes: number;
  endMinutes: number;
  startPercent: number;
  widthPercent: number;
  timeDisplay: string;
}

export interface PanchangTimelineData {
  dayStartMinutes: number;
  dayEndMinutes: number;
  sunsetMinutes: number;
  tithiBars: PanchangTimelineSegment[];
  nakshatraBars: PanchangTimelineSegment[];
  yogaBars: PanchangTimelineSegment[];
  karanaBars: PanchangTimelineSegment[];
  doshaBands: PanchangTimelineSegment[];
  auspiciousBands: PanchangTimelineSegment[];
}

export interface PanchangCalculationCredit {
  component: string;
  standard: string;
  details: string;
}

export interface PanchangExtendedInfo {
  lahiriAyanamsaDeg: number;
  purnimantaDayBanner: string;
  amantaDayBanner: string;
  dinamana: string;
  ratrimana: string;
  madhyahna: string;
  civilDawn: string;
  nauticalDawn: string;
  astronomicalDawn: string;
  ritu: string;
  vedicRitu: string;
  ayana: string;
  kaliyugaYear: string;
  kaliAhargana: string;
  julianDay: string;
  modifiedJulianDay: string;
  nationalCivilDate: string;
  tithiPercentElapsedAtSunrise: number;
  nakshatraPercentElapsedAtSunrise: number;
  yogaPercentElapsedAtSunrise: number;
  tithiSwami: string;
  nakshatraSwami: string;
  yogaSwami: string;
  suryaNakshatra: string;
  nextTithi: string;
  nextNakshatra: string;
  nextYoga: string;
  nextKarana: string;
  tithiTransitions: PanchangTransitionItem[];
  nakshatraTransitions: PanchangTransitionItem[];
  yogaTransitions: PanchangTransitionItem[];
  karanaTransitions: PanchangTransitionItem[];
  nakshatraPadas: PanchangTransitionItem[];
  moonSignTransitions: PanchangTransitionItem[];
  pratahSandhya: string;
  vijayaMuhurat: string;
  godhuliMuhurat: string;
  sayahnaSandhya: string;
  nishitaMuhurat: string;
  raviYoga: string;
  varjyam: string;
  durMuhurtam: string;
  panchakStatus: string;
  baana: string;
  anandadiYoga: string;
  tamilYoga: string;
  dishaShool: string;
  nakshatraShool: string;
  agnivasa: string;
  chandravasa: string;
  shivavasa: string;
  rahuVasa: string;
  homahuti: string;
  chandrabalamGoodRashis: string[];
  tarabalamGoodNakshatras: string[];
  sunriseMinutes: number;
  sunsetMinutes: number;
  nextSunriseMinutes: number;
  moonriseMinutes?: number | null;
  moonsetMinutes?: number | null;
  credits: PanchangCalculationCredit[];
}

export interface PanchangData {
  date: string;
  formattedDate: string;
  location: string;
  vikramSamvat: string;
  shakaSamvat: string;
  monthPurnimanta: string;
  monthAmanta: string;
  isAdhikaMaas?: boolean;
  paksha: string;
  ayanamsaName?: string;
  calculationNote?: string;
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
  locationMeta?: PanchangLocationMeta;
  extended?: PanchangExtendedInfo;
  dayHora?: PanchangHoraPeriod[];
  nightHora?: PanchangHoraPeriod[];
  udayaLagnas?: PanchangLagnaPeriod[];
  panchakaRahitaPeriods?: PanchangLagnaPeriod[];
  timeline?: PanchangTimelineData;
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
  isExactDateMatch?: boolean;
  isVerified?: boolean;
  sourceName?: string;
  sourceUrl?: string;
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
  category?: string;
  isDynamicCalculated?: boolean;
  isVerified?: boolean;
  verificationSource?: string;
  verificationUrl?: string;
}

export interface PanchangCity {
  id: string;
  name: string;
  labelHindi: string;
  state?: string;
  country: string;
  latitude: number;
  longitude: number;
  elevationMeters?: number;
  timeZoneId?: string;
  timezoneOffsetHours: number;
  usesDst?: boolean;
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

export interface AdMonetizationConfig {
  adsEnabled: boolean;
  adSenseClientId: string;
  headerAdSlotId: string;
  inFeedAdSlotId: string;
  sidebarAdSlotId: string;
  footerAdSlotId: string;
  adLabel: string;
  sponsorBannerEnabled: boolean;
  sponsorBadge: string;
  sponsorTitle: string;
  sponsorDescription: string;
  sponsorCtaText: string;
  sponsorUrl: string;
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
  ads?: AdMonetizationConfig;
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
