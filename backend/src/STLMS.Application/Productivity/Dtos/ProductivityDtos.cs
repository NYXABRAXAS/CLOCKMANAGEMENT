namespace STLMS.Application.Productivity.Dtos;

public record ProductivityComponentsDto(double? HabitsPercent, double? MedicinesPercent, double? SleepScore, int FocusMinutes, double? PrayersPercent);

public record ProductivityDayDto(DateOnly Date, double? Score, ProductivityComponentsDto Components);

public record ProductivitySummaryDto(
    IReadOnlyList<ProductivityDayDto> Days,
    double? AverageScore,
    int CurrentStreak,
    int BestStreak,
    int TotalFocusMinutes,
    int TotalHabitCheckIns,
    int TotalPrayersLogged);
