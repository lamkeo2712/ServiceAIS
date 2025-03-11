//using System.IO;
//using Microsoft.Extensions.Logging;
//using myAISapi.Decoder;
//using myAISapi.Models;
//using System;
//using System.Text.Json;

//namespace myAISapi.Services
//{
//	public class AisDecoderService
//	{
//		//private readonly ProcMaster _procMaster;
//		private readonly ILogger<AisDecoderService> _logger;
//		private readonly IUdpMessageStore _messageStore;
//		private readonly string _aisDataFilePath = Path.Combine(AppContext.BaseDirectory, "RawAIS", "data.txt");

//		public AisDecoderService(ILogger<AisDecoderService> logger, IUdpMessageStore messageStore)
//		{
//			_logger = logger;
//			_messageStore = messageStore;
//		}

//		public async Task DecodeAisDataFromUdp()
//		{
//			try
//			{
//				string msg = _messageStore.GetAllMessages()[0];
//				msg = msg.Trim();
//				string[] splitMsg = msg.Split("\n");
//				foreach (string s in splitMsg)
//				{
//					var decodedData = MainDecode.AisDecode(s);
//					_logger.LogInformation(JsonSerializer.Serialize(decodedData));

//				}
//				_messageStore.Delete();

//			}
//			catch (Exception ex)
//			{
//				_logger.LogError($"Error decoding AIS data: {ex.Message}");
//			}
//		}

//		//public async Task DecodeAisDataFromFile()
//		//{
//		//	try
//		//	{
//		//		string[] lines = await File.ReadAllLinesAsync(_aisDataFilePath);

//		//		foreach (string line in lines)
//		//		{
//		//			try
//		//			{
//		//				var decodedData = MainDecode.AisDecode(line);

//		//				// Gọi stored procedure PROC_TAUUPDATE
//		//				await _procMaster.Execute(
//		//					"PROC_TAUUPDATE",
//		//					new Dictionary<string, object>
//		//					{
//		//						//{ "MMSI", decodedData.MMSI ?? "" },
//		//						//{ "TENTAU", decodedData.TenTau ?? "" },
//		//						//{ "LAT", decodedData.Latitude ?? 0 },
//		//						//{ "LON", decodedData.Longitude ?? 0 }
//		//					}
//		//				);
//		//			}
//		//			catch (Exception lineDecodeEx)
//		//			{
//		//				_logger.LogError($"Error decoding line: {line}. Error: {lineDecodeEx.Message}");
//		//			}
//		//		}
//		//	}
//		//	catch (Exception ex)
//		//	{
//		//		_logger.LogError($"Error reading AIS data file: {ex.Message}");
//		//	}
//		//}
//	}
//}
