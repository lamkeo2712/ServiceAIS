using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using static myAISapi.Decoder.SharedFunction;
using static myAISapi.Decoder.MessageTypes;
using myAISapi.Models;

namespace myAISapi.Decoder
{
    public class MainDecode
    {
        public static object AisDecode(string message)
        {
            message = message.Trim().Replace("\r", "");

            var aisMessageMatch = Regex.Match(message, @"(!AIVDM.+)$");
            if (aisMessageMatch.Success)
            {
                message = aisMessageMatch.Value;
            }

            if (!message.StartsWith("!AIVDM"))
            {
                return "a";
            }

            var fields = message.Split(',');

            if (fields.Length < 7)
            {
				return "a";
			}

            if (!IsChecksumValid(message))
            {
				return "a";
			}

            var messageCount = fields[1];
            var fragNumber = fields[2];
            var messageID = fields[3];
            var channel = fields[4];
            var payload = fields[5];

            if (payload.Length < 10)
            {
				return "a";
            } 

            object decodedData = new
            {
                MessageCount = messageCount,
                FragNumber = fragNumber,
                MessageID = messageID,
                Channel = channel,
                Payload = payload,
                Fields = fields
            };

            return decodedData;
        }

        public static object PayloadDecode(string payload)
        {
            string payloadBit = "";
            var bitArray = new List<string>();

            foreach (char c in payload)
            {
                payloadBit += charTo6Bit(c);
                bitArray.Add(charTo6Bit(c));
            }

            int messageType = (int)parseData(payloadBit.Substring(0, 6), "u");

            switch (messageType)
            {
                case 1:
                case 2:
                case 3:
                    return Type123 (payloadBit);
                case 4:
                    return Type4(payloadBit);
                case 5:
                    return Type5(payloadBit);
                //case 6:
                //    return DecodeType6(payloadBit);
                //case 7:
                //    return DecodeType7(payloadBit);
                //case 9:
                //    return DecodeType9(payloadBit);
                case 18:
                    return Type18(payloadBit);
                case 19:
                    return Type19(payloadBit);
                //case 14:
                //    return DecodeType14(payloadBit);
                case 21:
                    return Type21(payloadBit);
                case 23:
                    return Type23(payloadBit);
                case 24:
                    return Type24(payloadBit);
                default:
                    return $"Unknown type {messageType}";
            }
        }



        private static bool IsChecksumValid(string sentence)
        {
            var match = Regex.Match(sentence, @"^([!$].+)\*([0-9A-F]{2})$", RegexOptions.IgnoreCase);
            if (!match.Success) return false;

            string coreSentence = match.Groups[1].Value;
            string providedChecksum = match.Groups[2].Value;
            string calculatedChecksum = CalculateChecksum(coreSentence);

            return calculatedChecksum.Equals(providedChecksum, StringComparison.OrdinalIgnoreCase);
        }

        private static string CalculateChecksum(string sentence)
        {
            int checksum = 0;

            for (int i = 1; i < sentence.Length; i++)
            {
                if (sentence[i] == '*') break;
                checksum ^= sentence[i];
            }

            return checksum.ToString("X2");
        }
    }

}