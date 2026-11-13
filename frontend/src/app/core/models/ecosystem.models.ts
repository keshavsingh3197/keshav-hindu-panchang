export interface CentralChannelItem {
  id: string;
  key: string;
  name: string;
  handle: string;
  platform: string;
  channelId: string;
  url: string;
  niche: string;
  tagline?: string;
  showInAnalytics: boolean;
  showInPanchang: boolean;
  showInStudio: boolean;
  showInFooter: boolean;
  isPrimary: boolean;
  isEnabled: boolean;
  sortOrder: number;
}

export interface PanchangIntegrationSettingsView {
  hasYouTubeApiKey: boolean;
  maskedYouTubeApiKey: string;
  youTubeKeyManagedByCentral: boolean;
  integrationSource: string;
  centralAdSenseConfigured: boolean;
  centralAdSenseClientId: string;
  centralConfigUrl: string;
  autoSyncOnStartup: boolean;
  lastEcosystemSyncUtc?: string | null;
  lastVideoSyncUtc?: string | null;
}

export interface UpdatePanchangIntegrationsRequest {
  youTubeApiKey?: string;
  clearYouTubeApiKey?: boolean;
  centralConfigUrl?: string;
  autoSyncOnStartup?: boolean;
}

export interface PanchangSyncSummary {
  channelsSynced: number;
  videosSynced: number;
  legacyVideosPurged: number;
  kitsUpdated: number;
  siteConfigUpdated: boolean;
  warnings: string[];
  syncedAtUtc: string;
}
