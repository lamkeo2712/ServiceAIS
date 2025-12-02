using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using myAISapi.Models;
using System;
using System.Data;
using System.Threading.Tasks;

namespace myAISapi.Data
{
	public class AppDBContext : DbContext
	{
		public AppDBContext(DbContextOptions<AppDBContext> options) : base(options) { }

		public async Task<object> ExecuteProcedureAsync(string procedureName, string jsonParams, string userRequest)
		{
			var dbConnection = Database.GetDbConnection();
			if (dbConnection == null)
			{
				throw new InvalidOperationException("Database connection is null.");
			}

			if (dbConnection.State != ConnectionState.Open)
			{
				await dbConnection.OpenAsync();
			}

			if (dbConnection is not SqlConnection sqlConnection)
			{
				throw new InvalidOperationException("Database connection is not SqlConnection.");
			}

			using (var command = new SqlCommand(procedureName, sqlConnection))
			{
				command.CommandType = CommandType.StoredProcedure;
				command.Parameters.AddWithValue("@ThamSo", jsonParams ?? (object)DBNull.Value);
				command.Parameters.AddWithValue("@UserRequest", userRequest ?? (object)DBNull.Value);

				using (var adapter = new SqlDataAdapter(command))
				{
					var ds = new DataSet();
					adapter.Fill(ds);

					if (ds.Tables.Count > 0)
					{
						DataTable dtName = ds.Tables[0]; // chứa danh sách tên dữ liệu trả về
						if (dtName.Columns.Contains("DataName"))
						{
							dtName.TableName = "DataNames";
							for (int i = 1; i < ds.Tables.Count; i++)
							{
								DataTable dt = ds.Tables[i];
								if (i - 1 < dtName.Rows.Count)
								{
									dt.TableName = dtName.Rows[i - 1]["DataName"].ToString();
								}
							}
							ds.Tables.Remove(dtName);
							ds.Tables.Add(dtName);
						}
					}

					return ToJsonObject(ds);
				}
			}
		}

		private object ToJsonObject(DataSet ds)
		{
			var result = new Dictionary<string, object>();

			foreach (DataTable table in ds.Tables)
			{
				var rows = new List<Dictionary<string, object?>>();
				foreach (DataRow row in table.Rows)
				{
					var rowData = new Dictionary<string, object?>();
					foreach (DataColumn column in table.Columns)
					{
						var value = row[column];
						rowData[column.ColumnName] = value is DBNull ? null : value;
					}
					rows.Add(rowData);
				}
				result[table.TableName] = rows;
			}

			return result;
		}

		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			base.OnModelCreating(modelBuilder);

			modelBuilder.Entity<BeaconRef>()
				.HasIndex(b => b.MMSI)
				.IsUnique();
		}

		public DbSet<User> Users { get; set; }

		public DbSet<DM_Tau> DM_Tau { get; set; }

		public DbSet<DM_Tau_HS> DM_Tau_HS { get; set; }

		public DbSet<DM_HanhTrinh> QL_HanhTrinh { get; set; }
		public DbSet<BeaconRef> DM_BeaconRef { get; set; }

	}
}
