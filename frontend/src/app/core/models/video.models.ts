export interface SpiritualVideo {
  id: string;
  title: string;
  description: string;
  youTubeUrl: string;
  youTubeVideoId: string;
  embedUrl: string;
  thumbnailUrl: string;
  category: string;
  publishedDate: string;
  durationMinutes: string;
  viewCount: number;
  isFeaturedDailyVideo: boolean;
  channelName: string;
  channelUrl: string;
  sourceChannelHandle?: string;
  isAutoSynced?: boolean;
}
