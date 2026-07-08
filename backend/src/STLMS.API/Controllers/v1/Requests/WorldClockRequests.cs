namespace STLMS.API.Controllers.v1.Requests;

public record AddWorldClockCityRequest(Guid CityId);
public record ReorderWorldClockCitiesRequest(IReadOnlyList<Guid> OrderedIds);
