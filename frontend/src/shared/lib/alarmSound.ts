// Synthesizes alarm tones with the Web Audio API - no audio asset files needed. Only rings while
// this tab is open (a service worker + push subscription for background ringing lands with the
// Smart Notifications milestone's real push delivery).
let audioContext: AudioContext | null = null;
let intervalId: ReturnType<typeof setInterval> | null = null;

export const ALARM_SOUNDS = [
  { id: "classic", label: "Classic beep" },
  { id: "gentle", label: "Gentle chime" },
  { id: "urgent", label: "Urgent triple-beep" },
] as const;

function playTone(ctx: AudioContext, frequency: number, type: OscillatorType, startOffset: number, duration: number, peakGain: number) {
  const osc = ctx.createOscillator();
  const gain = ctx.createGain();
  const startTime = ctx.currentTime + startOffset;
  osc.type = type;
  osc.frequency.value = frequency;
  gain.gain.setValueAtTime(0.0001, startTime);
  gain.gain.exponentialRampToValueAtTime(peakGain, startTime + 0.02);
  gain.gain.exponentialRampToValueAtTime(0.0001, startTime + duration);
  osc.connect(gain).connect(ctx.destination);
  osc.start(startTime);
  osc.stop(startTime + duration + 0.05);
}

function playPattern(ctx: AudioContext, soundId: string) {
  switch (soundId) {
    case "gentle":
      playTone(ctx, 440, "sine", 0, 0.6, 0.15);
      break;
    case "urgent":
      playTone(ctx, 1046, "square", 0, 0.12, 0.25);
      playTone(ctx, 1046, "square", 0.18, 0.12, 0.25);
      playTone(ctx, 1046, "square", 0.36, 0.12, 0.25);
      break;
    case "classic":
    default:
      playTone(ctx, 880, "square", 0, 0.35, 0.2);
      break;
  }
}

const REPEAT_INTERVAL_MS: Record<string, number> = { gentle: 1200, urgent: 900, classic: 700 };

export function startAlarmSound(soundId: string) {
  if (intervalId !== null) return;
  audioContext ??= new AudioContext();
  if (audioContext.state === "suspended") void audioContext.resume();
  playPattern(audioContext, soundId);
  intervalId = setInterval(() => {
    if (audioContext) playPattern(audioContext, soundId);
  }, REPEAT_INTERVAL_MS[soundId] ?? 700);
}

export function stopAlarmSound() {
  if (intervalId !== null) {
    clearInterval(intervalId);
    intervalId = null;
  }
}
