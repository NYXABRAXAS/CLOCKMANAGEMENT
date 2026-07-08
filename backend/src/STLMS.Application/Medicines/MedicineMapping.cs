using STLMS.Application.Medicines.Dtos;
using STLMS.Domain.Entities;

namespace STLMS.Application.Medicines;

public static class MedicineMapping
{
    // Times are passed in explicitly (not read off m.Times) because the repository never eager-
    // loads navigation collections - callers query MedicineTime separately by MedicineId.
    public static MedicineDto ToDto(Medicine m, IEnumerable<MedicineTime> times) => new(
        m.Id, m.Name, m.Dosage, m.Notes, m.StartDate, m.EndDate, m.RepeatDaysMask, m.IsActive,
        times.OrderBy(t => t.Hour).ThenBy(t => t.Minute).Select(t => new MedicineTimeDto(t.Hour, t.Minute)).ToList());
}
