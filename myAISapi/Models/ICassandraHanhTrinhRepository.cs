namespace myAISapi.Models
{
	public interface ICassandraHanhTrinhRepository
	{
		Task InsertBatchAsync(IEnumerable<DM_HanhTrinh> routes, CancellationToken ct = default);
		Task<IReadOnlyList<DM_HanhTrinh>> GetHanhTrinhAsync(int mmsi, int hours, CancellationToken ct = default);
	}
}
