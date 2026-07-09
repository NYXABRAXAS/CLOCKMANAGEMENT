export interface WeatherForecastDay {
  date: string;
  minTempC: number;
  maxTempC: number;
  condition: string;
  icon: string;
}

export interface Weather {
  tempC: number;
  feelsLikeC: number;
  humidity: number;
  windSpeedKph: number;
  condition: string;
  icon: string;
  forecast: WeatherForecastDay[];
}
