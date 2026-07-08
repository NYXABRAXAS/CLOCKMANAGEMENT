// Matches STLMS.Domain.Entities.AlarmDayMask exactly - a bitmask so an alarm can repeat on any
// combination of days. 0 means "one-time".
export const DAY_BITS: { bit: number; short: string; label: string }[] = [
  { bit: 1, short: "Sun", label: "Sunday" },
  { bit: 2, short: "Mon", label: "Monday" },
  { bit: 4, short: "Tue", label: "Tuesday" },
  { bit: 8, short: "Wed", label: "Wednesday" },
  { bit: 16, short: "Thu", label: "Thursday" },
  { bit: 32, short: "Fri", label: "Friday" },
  { bit: 64, short: "Sat", label: "Saturday" },
];

export const EVERYDAY_MASK = 127;

export function describeRepeatMask(mask: number): string {
  if (mask === 0) return "One-time";
  if (mask === EVERYDAY_MASK) return "Every day";
  const weekdays = 2 | 4 | 8 | 16 | 32;
  const weekend = 1 | 64;
  if (mask === weekdays) return "Weekdays";
  if (mask === weekend) return "Weekends";
  return DAY_BITS.filter((d) => (mask & d.bit) !== 0)
    .map((d) => d.short)
    .join(", ");
}

/** JS Date.getDay(): Sunday=0...Saturday=6 - maps directly onto DAY_BITS' index. */
export function dayBitForJsWeekday(jsWeekday: number): number {
  return DAY_BITS[jsWeekday]?.bit ?? 0;
}
