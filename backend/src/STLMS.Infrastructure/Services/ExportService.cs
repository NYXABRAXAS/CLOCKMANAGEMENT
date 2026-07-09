using System.Globalization;
using ClosedXML.Excel;
using CsvHelper;
using CsvHelper.Configuration;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using STLMS.Application.Common.Interfaces;

namespace STLMS.Infrastructure.Services;

/// <summary>Shared CSV/Excel/PDF generation backing every module's export button - built once here
/// rather than each feature hand-rolling its own writer. Everything is generated in-memory and
/// streamed straight back as the HTTP response body (see the controllers' File(...) calls) rather
/// than written to wwwroot, avoiding the same ephemeral-filesystem problem already flagged for
/// profile photo uploads on hosts like Render's free tier.</summary>
public class ExportService : IExportService
{
    public ExportFile ToCsv(string fileNameWithoutExtension, ExportSheet sheet)
    {
        using var stream = new MemoryStream();
        using (var writer = new StreamWriter(stream, leaveOpen: true))
        using (var csv = new CsvWriter(writer, new CsvConfiguration(CultureInfo.InvariantCulture)))
        {
            foreach (var header in sheet.Headers) csv.WriteField(header);
            csv.NextRecord();

            foreach (var row in sheet.Rows)
            {
                foreach (var cell in row) csv.WriteField(cell);
                csv.NextRecord();
            }
        }

        return new ExportFile(stream.ToArray(), "text/csv", $"{fileNameWithoutExtension}.csv");
    }

    public ExportFile ToExcel(string fileNameWithoutExtension, IReadOnlyList<ExportSheet> sheets)
    {
        using var workbook = new XLWorkbook();
        foreach (var sheet in sheets)
        {
            var worksheet = workbook.Worksheets.Add(SanitizeSheetName(sheet.Name));
            for (var c = 0; c < sheet.Headers.Count; c++) worksheet.Cell(1, c + 1).Value = sheet.Headers[c];
            worksheet.Row(1).Style.Font.Bold = true;

            for (var r = 0; r < sheet.Rows.Count; r++)
            {
                for (var c = 0; c < sheet.Rows[r].Count; c++) worksheet.Cell(r + 2, c + 1).Value = sheet.Rows[r][c];
            }

            worksheet.Columns().AdjustToContents();
        }

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return new ExportFile(
            stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"{fileNameWithoutExtension}.xlsx");
    }

    public ExportFile ToPdf(string fileNameWithoutExtension, string title, IReadOnlyList<ExportSheet> sheets)
    {
        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Margin(30);
                page.Header().Text(title).FontSize(18).Bold();
                page.Content().Column(column =>
                {
                    foreach (var sheet in sheets)
                    {
                        column.Item().PaddingTop(12).Text(sheet.Name).FontSize(13).Bold();
                        column.Item().Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                foreach (var _ in sheet.Headers) columns.RelativeColumn();
                            });

                            table.Header(header =>
                            {
                                foreach (var h in sheet.Headers)
                                {
                                    header.Cell().Background(Colors.Grey.Lighten2).Padding(4).Text(h).FontSize(9).Bold();
                                }
                            });

                            foreach (var row in sheet.Rows)
                            {
                                foreach (var cell in row)
                                {
                                    table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(4).Text(cell).FontSize(8);
                                }
                            }
                        });
                    }
                });
                page.Footer().AlignCenter().Text(x =>
                {
                    x.Span("Generated ").FontSize(8);
                    x.Span(DateTime.UtcNow.ToString("u")).FontSize(8);
                });
            });
        });

        return new ExportFile(document.GeneratePdf(), "application/pdf", $"{fileNameWithoutExtension}.pdf");
    }

    private static string SanitizeSheetName(string name)
    {
        var invalidChars = new[] { '\\', '/', '?', '*', '[', ']', ':' };
        var clean = new string(name.Where(c => !invalidChars.Contains(c)).ToArray());
        return clean.Length > 31 ? clean[..31] : clean;
    }
}
