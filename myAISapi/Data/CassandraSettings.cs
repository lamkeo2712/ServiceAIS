namespace myAISapi.Data
{
	public class CassandraSettings
	{
		public string[] ContactPoints { get; set; } = Array.Empty<string>();
		public int Port { get; set; } = 9042;
		public string Keyspace { get; set; } = "ais";
		public string? Username { get; set; }
		public string? Password { get; set; }
	}
}
