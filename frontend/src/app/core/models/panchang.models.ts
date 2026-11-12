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
