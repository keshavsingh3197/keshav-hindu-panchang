import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, catchError, of } from 'rxjs';
import { environment } from '../../../environments/environment';
import { PanchangData, HistoricalIncident, DailyKatha, FestivalItem } from '../models/panchang.models';
import { SpiritualVideo } from '../models/video.models';

@Injectable({ providedIn: 'root' })
export class PanchangService {
  private http = inject(HttpClient);
  private api = environment.apiUrl;

  getDailyPanchang(date?: string, location: string = 'New Delhi, India'): Observable<PanchangData> {
    const query = date ? `?date=${encodeURIComponent(date)}&location=${encodeURIComponent(location)}` : `?location=${encodeURIComponent(location)}`;
    return this.http.get<PanchangData>(`${this.api}/panchang/daily${query}`).pipe(
      catchError(() => of(this.getFallbackPanchang(date, location)))
    );
  }

  getAajKaItihaas(date?: string): Observable<HistoricalIncident[]> {
    const query = date ? `?date=${encodeURIComponent(date)}` : '';
    return this.http.get<HistoricalIncident[]>(`${this.api}/culturalhistory/aaj-ka-itihaas${query}`).pipe(
      catchError(() => of(this.getFallbackIncidents()))
    );
  }

  getDailyKatha(date?: string): Observable<DailyKatha> {
    const query = date ? `?date=${encodeURIComponent(date)}` : '';
    return this.http.get<DailyKatha>(`${this.api}/culturalhistory/daily-katha${query}`).pipe(
      catchError(() => of(this.getFallbackKatha()))
    );
  }

  getUpcomingFestivals(): Observable<FestivalItem[]> {
    return this.http.get<FestivalItem[]>(`${this.api}/festivals/upcoming`).pipe(
      catchError(() => of(this.getFallbackFestivals()))
    );
  }

  getVideos(category?: string): Observable<SpiritualVideo[]> {
    const query = category ? `?category=${encodeURIComponent(category)}` : '';
    return this.http.get<SpiritualVideo[]>(`${this.api}/videohub/videos${query}`).pipe(
      catchError(() => of(this.getFallbackVideos(category)))
    );
  }

  getFeaturedVideo(): Observable<SpiritualVideo> {
    return this.http.get<SpiritualVideo>(`${this.api}/videohub/featured`).pipe(
      catchError(() => of(this.getFallbackVideos()[0]))
    );
  }

  saveVideo(video: SpiritualVideo): Observable<SpiritualVideo> {
    return this.http.post<SpiritualVideo>(`${this.api}/videohub/demo-add`, video).pipe(
      catchError(() => {
        if (!video.id) video.id = 'vid-' + Date.now();
        return of(video);
      })
    );
  }

  // Graceful local fallbacks
  private getFallbackPanchang(dateStr?: string, loc: string = 'New Delhi, India'): PanchangData {
    const now = dateStr ? new Date(dateStr) : new Date();
    return {
      date: now.toISOString(),
      formattedDate: now.toLocaleDateString('en-US', { weekday: 'long', year: 'numeric', month: 'long', day: 'numeric' }),
      location: loc,
      vikramSamvat: '2083 (Krodhi / Pingala)',
      shakaSamvat: '1948 (Shubhakrit)',
      monthPurnimanta: 'Kartika',
      monthAmanta: 'Ashvina',
      paksha: 'Shukla Paksha',
      dayOfWeek: now.toLocaleDateString('en-US', { weekday: 'long' }),
      tithi: 'Shukla Tritiya',
      tithiEndTime: 'Until 04:22 PM',
      nakshatra: 'Mula',
      nakshatraEndTime: 'Until 08:45 PM',
      yoga: 'Atiganda',
      yogaEndTime: 'Until 06:12 PM',
      karana: 'Bava / Balava',
      karanaEndTime: 'Until 04:22 PM',
      vaara: 'Guruvara (Thursday)',
      sunrise: '06:42 AM',
      sunset: '05:28 PM',
      moonrise: '08:15 AM',
      moonset: '07:30 PM',
      sunSign: 'Vrischika (Scorpio)',
      moonSign: 'Dhanu (Sagittarius)',
      abhijitMuhurat: '11:44 AM - 12:28 PM',
      brahmaMuhurat: '05:01 AM - 05:51 AM',
      amritKaal: '02:18 PM - 03:52 PM',
      rahuKaal: '01:27 PM - 02:48 PM',
      yamaganda: '06:42 AM - 08:03 AM',
      gulikaKaal: '09:24 AM - 10:45 AM',
      dayChoghadiya: [
        { name: 'Shubh (Auspicious)', type: 'Shubh', timeRange: '06:42 AM - 08:03 AM' },
        { name: 'Rog (Inauspicious)', type: 'Ashubh', timeRange: '08:03 AM - 09:24 AM' },
        { name: 'Udveg (Inauspicious)', type: 'Ashubh', timeRange: '09:24 AM - 10:45 AM' },
        { name: 'Char (Neutral)', type: 'Shubh', timeRange: '10:45 AM - 12:06 PM' },
        { name: 'Labh (Gain)', type: 'Shubh', timeRange: '12:06 PM - 01:27 PM' },
        { name: 'Amrit (Nectar)', type: 'Shubh', timeRange: '01:27 PM - 02:48 PM' },
        { name: 'Kaal (Loss)', type: 'Ashubh', timeRange: '02:48 PM - 04:09 PM' },
        { name: 'Shubh (Auspicious)', type: 'Shubh', timeRange: '04:09 PM - 05:28 PM' }
      ],
      nightChoghadiya: [
        { name: 'Amrit (Nectar)', type: 'Shubh', timeRange: '05:28 PM - 07:07 PM' },
        { name: 'Char (Neutral)', type: 'Shubh', timeRange: '07:07 PM - 08:46 PM' },
        { name: 'Rog (Inauspicious)', type: 'Ashubh', timeRange: '08:46 PM - 10:25 PM' },
        { name: 'Kaal (Loss)', type: 'Ashubh', timeRange: '10:25 PM - 12:04 AM' },
        { name: 'Labh (Gain)', type: 'Shubh', timeRange: '12:04 AM - 01:43 AM' },
        { name: 'Udveg (Inauspicious)', type: 'Ashubh', timeRange: '01:43 AM - 03:22 AM' },
        { name: 'Shubh (Auspicious)', type: 'Shubh', timeRange: '03:22 AM - 05:01 AM' },
        { name: 'Amrit (Nectar)', type: 'Shubh', timeRange: '05:01 AM - 06:42 AM' }
      ],
      dailySanskritSutra: 'कर्मण्येवाधिकारस्ते मा फलेषु कदाचन । मा कर्मफलहेतुर्भूर्मा ते सङ्गोऽस्त्वकर्मणि ॥',
      dailySutraMeaning: 'You have a right to perform your prescribed duty, but you are not entitled to the fruits of actions.'
    };
  }

  private getFallbackIncidents(): HistoricalIncident[] {
    return [
      {
        id: 'inc-1',
        dateKey: '11-12',
        year: 1930,
        title: 'First Round Table Conference Inaugurated',
        category: 'Indian History',
        description: 'The first of three Round Table Conferences convened in London to discuss constitutional reforms in India.',
        culturalSignificance: 'Marked an epochal milestone in India’s struggle for self-governance and constitutional sovereignty.'
      },
      {
        id: 'inc-2',
        dateKey: '11-12',
        year: 1896,
        title: 'Birth of Dr. Salim Ali (Birdman of India)',
        category: 'Science & Ecology',
        description: 'Birth anniversary of natural historian Dr. Salim Ali, who conducted systematic bird surveys across India.',
        culturalSignificance: 'Revered across India as a symbol of compassionate stewardship of nature and living beings.'
      },
      {
        id: 'inc-3',
        dateKey: '11-12',
        year: 1469,
        title: 'Prakash Parv & Teachings of Universal Brotherhood',
        category: 'Spiritual Saint',
        description: 'Observance of the sacred message of unity, selfless service (Sewa), and divine contemplation.',
        culturalSignificance: 'Deep spiritual reverence uniting devotees across India in langar and peaceful devotion.'
      }
    ];
  }

  private getFallbackKatha(): DailyKatha {
    return {
      id: 'katha-today',
      title: 'The Unyielding Vow of Satya: King Harishchandra',
      deityOrSage: 'Maharishi Vishwamitra & Raja Harishchandra',
      storySummary: 'King Harishchandra renounced his royal realm, gold, and comfort to preserve his word of truth (Satya). Through profound trials in Kashi, his unwavering adherence to righteousness illuminated cosmic truth.',
      moralTeaching: 'Truth and moral fortitude outlast all fleeting worldly fame. When one upholds Dharma, Dharma protects.',
      sanskritMantra: 'सत्यमेव जयते नानृतम् (Satyam Eva Jayate — Truth alone triumphs).'
    };
  }

  private getFallbackFestivals(): FestivalItem[] {
    return [
      {
        id: 'f-1',
        nameHindi: 'देवउठनी एकादशी (प्रबोधिनी एकादशी)',
        nameEnglish: 'Devutthana Ekadashi',
        date: new Date(Date.now() + 86400000 * 2).toISOString(),
        tithi: 'Shukla Paksha Ekadashi',
        monthVedic: 'Kartika',
        description: 'Awakening of Lord Vishnu from four-month cosmic slumber (Chaturmas). Auspicious marriage season begins.',
        ritualsAndPujaVidhi: 'Devotees observe strict fasting, perform Tulsi Vivah with Shaligram, and light sugarcane mandaps with diyas.',
        fastingRules: 'Falahar fast, grains and legumes abstained.',
        isMajorFestival: true
      },
      {
        id: 'f-2',
        nameHindi: 'तुलसी विवाह',
        nameEnglish: 'Tulsi Vivah',
        date: new Date(Date.now() + 86400000 * 3).toISOString(),
        tithi: 'Shukla Paksha Dwadashi',
        monthVedic: 'Kartika',
        description: 'Ceremonial marriage of the sacred Tulsi plant with Lord Shaligram (Vishnu).',
        ritualsAndPujaVidhi: 'Decorating Tulsi pot with turmeric, vermillion, new saree, and reciting Vedic mangalashtak hymns.',
        fastingRules: 'Optional fast till twilight puja.',
        isMajorFestival: true
      },
      {
        id: 'f-3',
        nameHindi: 'कार्तिक पूर्णिमा (देव दीपावली)',
        nameEnglish: 'Kartik Purnima & Dev Deepawali',
        date: new Date(Date.now() + 86400000 * 5).toISOString(),
        tithi: 'Purnima',
        monthVedic: 'Kartika',
        description: 'Celebration of Lord Shiva victory over Tripurasura. The ghats of Varanasi sparkle with millions of earthen lamps.',
        ritualsAndPujaVidhi: 'Holy dip (Snan) in sacred rivers at sunrise and lighting lamps at temples and water bodies.',
        fastingRules: 'Full day fast until Chandra darshan.',
        isMajorFestival: true
      }
    ];
  }

  private getFallbackVideos(category?: string): SpiritualVideo[] {
    const list: SpiritualVideo[] = [
      {
        id: 'vid-1',
        title: 'आज का दैनिक पंचांग, शुभ मुहूर्त एवं सम्पूर्ण राशिफल | Daily Panchang & Horoscope',
        description: 'Live daily Vedic astrological insights, lucky color, auspicious time, Rahu Kaal precautions, and divine remedies for your zodiac sign today.',
        youTubeUrl: 'https://www.youtube.com/watch?v=dQw4w9WgXcQ',
        youTubeVideoId: 'dQw4w9WgXcQ',
        embedUrl: 'https://www.youtube.com/embed/dQw4w9WgXcQ',
        thumbnailUrl: 'https://images.unsplash.com/photo-1545231027-637d2f6210f8?auto=format&fit=crop&w=600&q=80',
        category: 'Daily News & Rashifal',
        publishedDate: new Date().toISOString(),
        durationMinutes: '14:20',
        viewCount: 42800,
        isFeaturedDailyVideo: true,
        channelName: 'Keshav Singh Official',
        channelUrl: 'https://youtube.com/@KeshavSingh'
      },
      {
        id: 'vid-2',
        title: 'देवउठनी एकादशी व्रत कथा एवं तुलसी विवाह सम्पूर्ण पूजा विधि | Devutthana Ekadashi Katha',
        description: 'Sacred narrative of Prabodhini Ekadashi, how to conduct Tulsi Vivah at home, and why Chaturmas ends today.',
        youTubeUrl: 'https://www.youtube.com/watch?v=dQw4w9WgXcQ',
        youTubeVideoId: 'dQw4w9WgXcQ',
        embedUrl: 'https://www.youtube.com/embed/dQw4w9WgXcQ',
        thumbnailUrl: 'https://images.unsplash.com/photo-1514533450685-4493e01d1fdc?auto=format&fit=crop&w=600&q=80',
        category: 'Pauranik Katha',
        publishedDate: new Date(Date.now() - 86400000).toISOString(),
        durationMinutes: '18:45',
        viewCount: 68400,
        isFeaturedDailyVideo: false,
        channelName: 'Keshav Singh Official',
        channelUrl: 'https://youtube.com/@KeshavSingh'
      },
      {
        id: 'vid-3',
        title: 'श्री महामृत्युंजय मंत्र 108 बार जाप | Maha Mrityunjaya Mantra 108 Times with Lyrics',
        description: 'Vedic resonance for mental clarity, physical vitality, freedom from planetary afflictions, and peace of mind.',
        youTubeUrl: 'https://www.youtube.com/watch?v=dQw4w9WgXcQ',
        youTubeVideoId: 'dQw4w9WgXcQ',
        embedUrl: 'https://www.youtube.com/embed/dQw4w9WgXcQ',
        thumbnailUrl: 'https://images.unsplash.com/photo-1518709268805-4e9042af9f23?auto=format&fit=crop&w=600&q=80',
        category: 'Mantra & Meditation',
        publishedDate: new Date(Date.now() - 86400000 * 2).toISOString(),
        durationMinutes: '45:10',
        viewCount: 125000,
        isFeaturedDailyVideo: false,
        channelName: 'Keshav Singh Official',
        channelUrl: 'https://youtube.com/@KeshavSingh'
      },
      {
        id: 'vid-4',
        title: 'काशी विश्वनाथ मंदिर देव दीपावली महाआरती लाइव दर्शन | Kashi Vishwanath Darshan',
        description: 'Sublime evening Ganga Aarti, temple lighting, and deepotsav celebrations live from Kashi.',
        youTubeUrl: 'https://www.youtube.com/watch?v=dQw4w9WgXcQ',
        youTubeVideoId: 'dQw4w9WgXcQ',
        embedUrl: 'https://www.youtube.com/embed/dQw4w9WgXcQ',
        thumbnailUrl: 'https://images.unsplash.com/photo-1609766857041-ed402ea8069a?auto=format&fit=crop&w=600&q=80',
        category: 'Temple Darshan',
        publishedDate: new Date(Date.now() - 86400000 * 3).toISOString(),
        durationMinutes: '22:15',
        viewCount: 89200,
        isFeaturedDailyVideo: false,
        channelName: 'Keshav Singh Official',
        channelUrl: 'https://youtube.com/@KeshavSingh'
      }
    ];

    if (!category || category === 'All') return list;
    return list.filter(v => v.category.toLowerCase() === category.toLowerCase());
  }
}
