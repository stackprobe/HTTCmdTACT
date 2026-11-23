using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HLTStudio.Commons;
using HLTStudio.WebServices;

namespace HLTStudio.Actions
{
	public class Action_YourRequest
	{
		public void Run(HTTPServerChannel channel)
		{
			string file = Path.Combine(ActionUtility.GetRequestScopedTemporaryDirectoryPath(channel), "YourRequest.json");

			byte[] content = channel.Body.ToByteArray();

			if (3000 < content.Length) // rough limit
				throw null; // 2bs

			using (var writer = SCommon.OpenFile_m.OpenTextFileForWrite(file, Encoding.UTF8))
			{
				writer.WriteLine("{");

				WriteJsonKeyValue(writer, "FirstLine", channel.FirstLine, true);
				WriteJsonKeyValue(writer, "Method", channel.Method, true);
				WriteJsonKeyValue(writer, "PathQuery", channel.PathQuery, true);
				WriteJsonKeyValue(writer, "HTTPVersion", channel.HTTPVersion, true);

				writer.WriteLine("\"HeaderPairs\": [");

				for (int i = 0; i < channel.HeaderPairs.Count; i++)
				{
					writer.WriteLine("{");

					WriteJsonKeyValue(writer, "Key", channel.HeaderPairs[i][0], true);
					WriteJsonKeyValue(writer, "Value", channel.HeaderPairs[i][1], false);

					writer.WriteLine("}" + (i + 1 < channel.HeaderPairs.Count ? "," : ""));
				}
				writer.WriteLine("],");

				WriteJsonKeyValue(writer, "Content-Length", content.Length.ToString(), true);
				WriteJsonKeyValue(writer, "Content-UTF-8", SCommon.UTF8Conv.ToJString(content), true);
				WriteJsonKeyValue(writer, "Content", SCommon.Base64.I.Encode(content), false);

				writer.WriteLine("}");
			}

			channel.ResStatus = 200;
			channel.ResHeaderPairs.Add(new string[] { "Content-Type", "application/json; charset=UTF-8" });
			channel.ResBody = ActionUtility.E_ReadFile(file);
			//channel.ResBodyLength = new FileInfo(file).Length;
			channel.ResBodyLength = -1L;
		}

		private static void WriteJsonKeyValue(StreamWriter writer, string key, string value, bool hasNext)
		{
			writer.WriteLine($"\"{key}\": \"{GetJsonString(value)}\"{(hasNext ? "," : "")}");
		}

		private static string GetJsonString(string value)
		{
			var buff = new StringBuilder();

			foreach (char chr in value)
			{
				if (chr < 0x20)
				{
					buff.Append($"\\u{(int)chr:x4}");
				}
				else
				{
					switch (chr)
					{
						case '"':
							buff.Append("\\\"");
							break;

						case '\\':
							buff.Append("\\\\");
							break;

						case '\b':
							buff.Append("\\b");
							break;

						case '\f':
							buff.Append("\\f");
							break;

						case '\n':
							buff.Append("\\n");
							break;

						case '\r':
							buff.Append("\\r");
							break;

						case '\t':
							buff.Append("\\t");
							break;

						default:
							buff.Append(chr);
							break;
					}
				}
			}
			return buff.ToString();
		}
	}
}
