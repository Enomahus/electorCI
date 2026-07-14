namespace Application.Features.Common.GridData
{
    public class GridDataResponse<T>
    {
        public required IEnumerable<T> Data { get; init; }
        public required int Total { get; init; }
    }
}
