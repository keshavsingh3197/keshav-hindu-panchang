export type ClockFormatMode = '12h' | '24h' | '24+' | 'vedic';

export function parseRangeOrEndTime(raw: string | null | undefined, sunriseMins: number): [number, number] | null {
  if (!raw) return null;
  const parts = raw.split(/\s*-\s*/);
  if (parts.length === 2) {
    const s = parseSingleTime(parts[0], sunriseMins);
    const e = parseSingleTime(parts[1], s ?? sunriseMins);
    if (s !== null && e !== null) {
      return [s, e < s ? e + 1440 : e];
    }
  }
  return null;
}

export function formatMinutesAsClock(
  minutesFromMidnight: number,
  mode: ClockFormatMode,
  sunriseMinutes: number
): string {
  if (!Number.isFinite(minutesFromMidnight)) return '';

  if (mode === 'vedic') {
    const elapsedMins = ((minutesFromMidnight - sunriseMinutes) % 1440 + 1440) % 1440;
    const totalPalas = Math.round(elapsedMins * 2.5); // 24h = 1440 min = 60 Ghati = 3600 Pala -> 2.5 Pala / min
    const ghati = Math.floor(totalPalas / 60) % 60;
    const pala = totalPalas % 60;
    return `${String(ghati).padStart(2, '0')} Gh ${String(pala).padStart(2, '0')} Pa`;
  }

  const totalRounded = Math.round(minutesFromMidnight);
  const isNextDay = totalRounded >= 1440;
  const wrapped = ((totalRounded % 1440) + 1440) % 1440;
  const h24 = Math.floor(wrapped / 60);
  const m = wrapped % 60;

  if (mode === '24+') {
    const hPlus = isNextDay ? h24 + 24 : h24;
    return `${String(hPlus).padStart(2, '0')}:${String(m).padStart(2, '0')}${isNextDay ? '+' : ''}`;
  }

  if (mode === '24h') {
    return `${String(h24).padStart(2, '0')}:${String(m).padStart(2, '0')}${isNextDay ? ' (+1d)' : ''}`;
  }

  const suffix = h24 >= 12 ? 'PM' : 'AM';
  const h12 = h24 % 12 === 0 ? 12 : h24 % 12;
  return `${String(h12).padStart(2, '0')}:${String(m).padStart(2, '0')} ${suffix}${isNextDay ? ' (+1d)' : ''}`;
}

export function formatMinutesRange(
  startMins: number,
  endMins: number,
  mode: ClockFormatMode,
  sunriseMinutes: number
): string {
  return `${formatMinutesAsClock(startMins, mode, sunriseMinutes)} – ${formatMinutesAsClock(endMins, mode, sunriseMinutes)}`;
}

export function convertDisplayString(
  raw: string | null | undefined,
  mode: ClockFormatMode,
  sunriseMinutes: number
): string {
  if (!raw) return '';
  if (mode === '12h') return raw;

  const trimmed = raw.trim();
  const range = parseRangeOrEndTime(trimmed, sunriseMinutes);
  if (range) {
    return formatMinutesRange(range[0], range[1], mode, sunriseMinutes);
  }

  const single = parseSingleTime(trimmed, sunriseMinutes);
  if (single !== null) {
    const prefix = trimmed.toLowerCase().startsWith('until ') ? 'Until ' : '';
    return `${prefix}${formatMinutesAsClock(single, mode, sunriseMinutes)}`;
  }
  return trimmed;
}

function parseSingleTime(raw: string, refMins: number): number | null {
  const clean = raw.trim();
  const m = clean.match(/(\d{1,2}):(\d{2})\s*(AM|PM)?/i);
  if (!m) return null;
  let h = Number(m[1]);
  const mins = Number(m[2]);
  const ampm = m[3]?.toUpperCase();
  if (ampm === 'PM' && h < 12) h += 12;
  if (ampm === 'AM' && h === 12) h = 0;

  let val = h * 60 + mins;
  if (clean.includes('+1d') || val < refMins - 180) {
    if (clean.includes('+1d')) val += 1440;
  }
  return val;
}
