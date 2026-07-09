namespace STLMS.Application.Common.Interfaces;

/// <summary>One named table of data - the unit every export format is built from. A CSV/PDF export
/// takes exactly one sheet; an Excel workbook can take several (one tab each).</summary>
public record ExportSheet(string Name, IReadOnlyList<string> Headers, IReadOnlyList<IReadOnlyList<string>> Rows);

public record ExportFile(byte[] Content, string ContentType, string FileName);

/// <summary>Shared report/export generation used across every module's "export my data" button,
/// rather than each feature hand-rolling its own CSV/Excel/PDF writer.</summary>
public interface IExportService
{
    ExportFile ToCsv(string fileNameWithoutExtension, ExportSheet sheet);
    ExportFile ToExcel(string fileNameWithoutExtension, IReadOnlyList<ExportSheet> sheets);
    ExportFile ToPdf(string fileNameWithoutExtension, string title, IReadOnlyList<ExportSheet> sheets);
}
