//using System.Data;
//using Microsoft.Data.SqlClient;
//using Microsoft.EntityFrameworkCore;
//using Microsoft.Extensions.Configuration;

//namespace myAISapi.Services
//{
//	public class ProcMaster
//	{
//		private readonly IConfiguration _config;
//		private readonly ILogger<ProcMaster> _logger;

//		public ProcMaster(IConfiguration config, ILogger<ProcMaster> logger)
//		{
//			_config = config;
//			_logger = logger;
//		}

//		public async Task Execute(string procedureName, Dictionary<string, object> parameters)
//		{
//			try
//			{
//				// Lấy connection string từ configuration
//				var connectionString = _config.GetConnectionString("DefaultConnection");

//				using (var connection = new SqlConnection(connectionString))
//				{
//					await connection.OpenAsync();

//					using (var command = new SqlCommand(procedureName, connection))
//					{
//						command.CommandType = CommandType.StoredProcedure;

//						// Thêm các tham số động
//						if (parameters != null)
//						{
//							foreach (var param in parameters)
//							{
//								var sqlParam = command.Parameters.Add(
//									$"@{param.Key}",
//									GetSqlDbType(param.Value),
//									GetParameterSize(param.Value)
//								);
//								sqlParam.Value = param.Value ?? DBNull.Value;
//							}
//						}

//						// Thực thi procedure
//						await command.ExecuteNonQueryAsync();

//						_logger.LogInformation($"Executed stored procedure: {procedureName}");
//					}
//				}
//			}
//			catch (Exception ex)
//			{
//				_logger.LogError($"Error executing stored procedure {procedureName}: {ex.Message}");
//				throw; // Re-throw để cho phép xử lý lỗi ở tầng trên nếu cần
//			}
//		}

//		private SqlDbType GetSqlDbType(object value)
//		{
//			if (value == null) return SqlDbType.Variant;

//			switch (Type.GetTypeCode(value.GetType()))
//			{
//				case TypeCode.String:
//					return SqlDbType.NVarChar;
//				case TypeCode.Int32:
//					return SqlDbType.Int;
//				case TypeCode.Int64:
//					return SqlDbType.BigInt;
//				case TypeCode.Double:
//					return SqlDbType.Float;
//				case TypeCode.Decimal:
//					return SqlDbType.Decimal;
//				case TypeCode.DateTime:
//					return SqlDbType.DateTime;
//				case TypeCode.Boolean:
//					return SqlDbType.Bit;
//				default:
//					return SqlDbType.Variant;
//			}
//		}

//		// Hàm hỗ trợ xác định kích thước tham số
//		private int GetParameterSize(object value)
//		{
//			if (value == null) return -1;

//			switch (Type.GetTypeCode(value.GetType()))
//			{
//				case TypeCode.String:
//					return ((string)value).Length > 0 ? ((string)value).Length : 255;
//				default:
//					return -1;
//			}
//		}
//	}
//}
