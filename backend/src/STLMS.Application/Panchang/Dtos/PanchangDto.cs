namespace STLMS.Application.Panchang.Dtos;

public record PanchangDto(int TithiNumber, string TithiName, string Paksha, int NakshatraNumber, string NakshatraName, bool IsApproximate);
