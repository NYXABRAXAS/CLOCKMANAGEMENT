export interface City {
  id: string;
  name: string;
  country: string;
  countryCode: string;
  timezoneId: string;
  latitude: number;
  longitude: number;
}

export interface WorldClockCity {
  id: string;
  city: City;
  sortOrder: number;
}
