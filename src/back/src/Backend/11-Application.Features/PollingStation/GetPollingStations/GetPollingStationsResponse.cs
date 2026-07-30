namespace Application.Features.PollingStation.GetPollingStations
{
    public record GetPollingStationsResponse(
        long? RegionId,
        string RegionCode,
        string RegionName,
        long? DepartmentId,
        string DepartmentCode,
        string DepartmentName,
        long? SubPrefectureId,
        string SubPrefectureCode,
        string SubPrefectureName,
        long? MunicipalityId,
        string MunicipalityCode,
        string MunicipalityName,
        long? VotingLocationId,
        string VotingLocationCode,
        string VotingLocationName,
        long StationId,
        string StationNumber,
        bool IsDisabled,
        DateTimeOffset? DisabledDate
    );
    
}
