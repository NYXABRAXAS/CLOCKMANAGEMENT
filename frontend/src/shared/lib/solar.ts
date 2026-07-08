// Sunrise/sunset via standard solar-position formulas (the same NOAA/Montenbruck & Pfleger-derived
// approximations used by most sunrise calculators) - no external API, just the sun's position
// given a date, latitude and longitude. Accurate to within a minute or two, which is plenty for a
// dashboard widget.
const RAD = Math.PI / 180;
const DAY_MS = 1000 * 60 * 60 * 24;
const J1970 = 2440588;
const J2000 = 2451545;
const OBLIQUITY = RAD * 23.4397;

function toJulian(date: Date): number {
  return date.valueOf() / DAY_MS - 0.5 + J1970;
}

function fromJulian(j: number): Date {
  return new Date((j + 0.5 - J1970) * DAY_MS);
}

function toDays(date: Date): number {
  return toJulian(date) - J2000;
}

function solarMeanAnomaly(d: number): number {
  return RAD * (357.5291 + 0.98560028 * d);
}

function eclipticLongitude(m: number): number {
  const center = RAD * (1.9148 * Math.sin(m) + 0.02 * Math.sin(2 * m) + 0.0003 * Math.sin(3 * m));
  const perihelion = RAD * 102.9372;
  return m + center + perihelion + Math.PI;
}

function declination(l: number): number {
  return Math.asin(Math.sin(0) * Math.cos(OBLIQUITY) + Math.cos(0) * Math.sin(OBLIQUITY) * Math.sin(l));
}

function julianCycle(d: number, lw: number): number {
  return Math.round(d - 0.0009 - lw / (2 * Math.PI));
}

function approxTransit(ht: number, lw: number, n: number): number {
  return 0.0009 + (ht + lw) / (2 * Math.PI) + n;
}

function solarTransitJ(ds: number, m: number, l: number): number {
  return J2000 + ds + 0.0053 * Math.sin(m) - 0.0069 * Math.sin(2 * l);
}

function hourAngle(h: number, phi: number, d: number): number {
  return Math.acos((Math.sin(h) - Math.sin(phi) * Math.sin(d)) / (Math.cos(phi) * Math.cos(d)));
}

export interface SunTimes {
  sunrise: Date;
  sunset: Date;
  solarNoon: Date;
}

const SUNRISE_SUNSET_ANGLE = -0.833 * RAD;

export function getSunTimes(date: Date, latitude: number, longitude: number): SunTimes {
  const lw = RAD * -longitude;
  const phi = RAD * latitude;
  const d = toDays(date);
  const n = julianCycle(d, lw);
  const approxSolarNoon = approxTransit(0, lw, n);
  const m = solarMeanAnomaly(approxSolarNoon);
  const l = eclipticLongitude(m);
  const dec = declination(l);
  const jNoon = solarTransitJ(approxSolarNoon, m, l);

  const w = hourAngle(SUNRISE_SUNSET_ANGLE, phi, dec);
  const a = approxTransit(w, lw, n);
  const jSet = solarTransitJ(a, m, l);
  const jRise = jNoon - (jSet - jNoon);

  return { sunrise: fromJulian(jRise), sunset: fromJulian(jSet), solarNoon: fromJulian(jNoon) };
}
