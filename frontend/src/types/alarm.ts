export type AlarmChallengeType = "None" | "Math";

export interface Alarm {
  id: string;
  label: string;
  hour: number;
  minute: number;
  repeatDaysMask: number;
  isEnabled: boolean;
  soundId: string;
  snoozeEnabled: boolean;
  snoozeMinutes: number;
  challengeType: AlarmChallengeType;
}

export interface AlarmFormValues {
  label: string;
  hour: number;
  minute: number;
  repeatDaysMask: number;
  soundId: string;
  snoozeEnabled: boolean;
  snoozeMinutes: number;
  challengeType: AlarmChallengeType;
}
