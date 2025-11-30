using System.Text;

namespace myAISapi.Encoder
{
	public class AisAtonEncoder
	{
		public static string EncodeType21(
			int mmsi,
			double latitudeDeg,
			double longitudeDeg,
			string name,
			int atonType = 30,
			bool isVirtual = true
		)
		{
			var bits = new List<int>(272);
			AppendUnsigned(bits, 21u, 6);
			AppendUnsigned(bits, 0u, 2);
			AppendUnsigned(bits, (uint)mmsi, 30);
			AppendUnsigned(bits, (uint)atonType, 5);
			AppendName(bits, name);
			AppendUnsigned(bits, 1u, 1);
			int lonRaw = EncodeLon(longitudeDeg);
			AppendSigned(bits, lonRaw, 28);
			int latRaw = EncodeLat(latitudeDeg);
			AppendSigned(bits, latRaw, 27);
			AppendUnsigned(bits, 0u, 30);
			AppendUnsigned(bits, 7u, 4);
			AppendUnsigned(bits, 60u, 6);
			AppendUnsigned(bits, 0u, 1);
			AppendUnsigned(bits, 0u, 8);
			AppendUnsigned(bits, 0u, 1);
			AppendUnsigned(bits, isVirtual ? 1u : 0u, 1);
			AppendUnsigned(bits, 0u, 1);
			AppendUnsigned(bits, 0u, 1);
			int padBits = (6 - (bits.Count % 6)) % 6;
			for (int i = 0; i < padBits; i++)
				bits.Add(0);

			string payload = BitsToSixBitAscii(bits);
			string sentenceNoChecksum = $"!AIVDM,1,1,,A,{payload},{padBits}";
			string checksum = CalcNmeaChecksum(sentenceNoChecksum);

			return sentenceNoChecksum + "*" + checksum;
		}

		private static void AppendUnsigned(List<int> bits, uint value, int bitCount)
		{
			for (int i = bitCount - 1; i >= 0; i--)
			{
				int bit = ((int)(value >> i)) & 1;
				bits.Add(bit);
			}
		}

		private static void AppendSigned(List<int> bits, int value, int bitCount)
		{
			uint v = (uint)value & ((1u << bitCount) - 1u);
			AppendUnsigned(bits, v, bitCount);
		}

		private static int EncodeLon(double lonDeg)
		{
			if (double.IsNaN(lonDeg) || double.IsInfinity(lonDeg))
				return 0; // hoặc set về special value nếu muốn

			if (lonDeg > 180.0) lonDeg = 180.0;
			if (lonDeg < -180.0) lonDeg = -180.0;

			return (int)Math.Round(lonDeg * 600000.0);
		}

		private static int EncodeLat(double latDeg)
		{
			if (double.IsNaN(latDeg) || double.IsInfinity(latDeg))
				return 0;

			if (latDeg > 90.0) latDeg = 90.0;
			if (latDeg < -90.0) latDeg = -90.0;

			return (int)Math.Round(latDeg * 600000.0);
		}

		private static void AppendName(List<int> bits, string name)
		{
			string upper = (name ?? "").ToUpperInvariant();
			if (upper.Length > 20)
				upper = upper.Substring(0, 20);

			// pad phải đủ 20 ký tự (spec Name of AtoN = 20 chars)
			upper = upper.PadRight(20, '@'); // '@' = 0 trong bảng 6-bit

			foreach (char c in upper)
			{
				int code = CharToSixBit(c);
				AppendUnsigned(bits, (uint)code, 6);
			}
		}

		private static int CharToSixBit(char c)
		{
			if (c == '@') return 0;

			if (c >= 'A' && c <= 'Z')
				return c - 'A' + 1;

			if (c >= '0' && c <= '9')
				return c - '0' + 48;

			if (c == ' ')
				return 32;

			return 32;
		}

		private static string BitsToSixBitAscii(List<int> bits)
		{
			var sb = new StringBuilder(bits.Count / 6 + 2);

			for (int i = 0; i < bits.Count; i += 6)
			{
				int val = 0;
				for (int j = 0; j < 6; j++)
				{
					val = (val << 1) | bits[i + j];
				}

				int c = val + 48;
				if (c > 87) c += 8; // chuẩn AIS 6-bit -> ASCII
				sb.Append((char)c);
			}

			return sb.ToString();
		}

		private static string CalcNmeaChecksum(string sentenceNoChecksum)
		{
			int cs = 0;
			for (int i = 1; i < sentenceNoChecksum.Length; i++)
			{
				char ch = sentenceNoChecksum[i];
				if (ch == '*') break;
				cs ^= (byte)ch;
			}
			return cs.ToString("X2");
		}

	}
}
