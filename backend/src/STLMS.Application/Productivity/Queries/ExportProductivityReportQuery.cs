using STLMS.Application.Common.Interfaces;
using STLMS.Application.Common.Mediator;

namespace STLMS.Application.Productivity.Queries;

public record ExportProductivityReportQuery(Guid UserId, DateOnly From, DateOnly To, string Format) : IRequest<ExportFile>;

public class ExportProductivityReportQueryHandler(IAppMediator mediator, IExportService exportService)
    : IRequestHandler<ExportProductivityReportQuery, ExportFile>
{
    public async Task<ExportFile> HandleAsync(ExportProductivityReportQuery request, CancellationToken ct)
    {
        var summary = await mediator.SendAsync(new GetProductivitySummaryQuery(request.UserId, request.From, request.To), ct);

        var dailyHeaders = new[] { "Date", "Score", "Habits %", "Medicines %", "Sleep Score", "Focus Minutes", "Prayers %" };
        var dailyRows = summary.Days
            .Select(d => (IReadOnlyList<string>)new[]
            {
                d.Date.ToString("yyyy-MM-dd"),
                d.Score?.ToString("F1") ?? "",
                d.Components.HabitsPercent?.ToString("F0") ?? "",
                d.Components.MedicinesPercent?.ToString("F0") ?? "",
                d.Components.SleepScore?.ToString("F0") ?? "",
                d.Components.FocusMinutes.ToString(),
                d.Components.PrayersPercent?.ToString("F0") ?? "",
            })
            .ToList();
        var dailySheet = new ExportSheet("Daily Scores", dailyHeaders, dailyRows);

        var summaryHeaders = new[] { "Metric", "Value" };
        var summaryRows = new List<IReadOnlyList<string>>
        {
            new[] { "Date range", $"{request.From:yyyy-MM-dd} to {request.To:yyyy-MM-dd}" },
            new[] { "Average score", summary.AverageScore?.ToString("F1") ?? "No data" },
            new[] { "Current streak (days)", summary.CurrentStreak.ToString() },
            new[] { "Best streak (days)", summary.BestStreak.ToString() },
            new[] { "Total focus minutes", summary.TotalFocusMinutes.ToString() },
            new[] { "Total habit check-ins", summary.TotalHabitCheckIns.ToString() },
            new[] { "Total prayers logged", summary.TotalPrayersLogged.ToString() },
        };
        var summarySheet = new ExportSheet("Summary", summaryHeaders, summaryRows);

        const string fileName = "productivity-report";
        return request.Format.ToLowerInvariant() switch
        {
            "excel" => exportService.ToExcel(fileName, [summarySheet, dailySheet]),
            "pdf" => exportService.ToPdf(fileName, "Productivity Report", [summarySheet, dailySheet]),
            _ => exportService.ToCsv(fileName, dailySheet),
        };
    }
}
