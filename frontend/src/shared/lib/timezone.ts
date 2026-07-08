// Converts a "wall clock" date/time that's meant to represent local time in a given IANA
// timezone into the UTC instant it corresponds to. Built on Intl.DateTimeFormat alone (no
// date library installed) using the standard guess-then-correct technique: format a guessed UTC
// instant in the target zone, measure the drift between the guess and the intended wall time, and
// correct for it. Run twice so a DST-boundary edge case in the first correction doesn't linger.
function timeZoneOffsetMs(instant: Date, timeZone: string): number {
  const parts = new Intl.DateTimeFormat("en-US", {
    timeZone,
    hourCycle: "h23",
    year: "numeric",
    month: "2-digit",
    day: "2-digit",
    hour: "2-digit",
    minute: "2-digit",
    second: "2-digit",
  }).formatToParts(instant);

  const get = (type: string) => Number(parts.find((p) => p.type === type)?.value ?? 0);
  const asUtc = Date.UTC(get("year"), get("month") - 1, get("day"), get("hour"), get("minute"), get("second"));
  return asUtc - instant.getTime();
}

export function zonedWallTimeToUtc(year: number, month: number, day: number, hour: number, minute: number, timeZone: string): Date {
  let utcMs = Date.UTC(year, month - 1, day, hour, minute);
  for (let i = 0; i < 2; i++) {
    const offset = timeZoneOffsetMs(new Date(utcMs), timeZone);
    utcMs = Date.UTC(year, month - 1, day, hour, minute) - offset;
  }
  return new Date(utcMs);
}
