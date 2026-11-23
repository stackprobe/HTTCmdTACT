using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HLTStudio.WebServices;

namespace HLTStudio.Actions
{
	public class Action_FizzBuzz
	{
		public void Run(HTTPServerChannel channel)
		{
			try
			{
				var parameterPairs = GetParameterPairs(channel);

				var start = int.Parse(parameterPairs["start"]);
				var end = int.Parse(parameterPairs["end"]);

				channel.ResStatus = 200;
				channel.ResHeaderPairs.Add(new string[] { "Content-Type", "text/plain; charset=US-ASCII" });
				channel.ResBody = E_ToBody(E_FizzBuzz(start, end));
				channel.ResBodyLength = -1L;
			}
			catch (Exception ex)
			{
				SockCommon.WriteLog(SockCommon.ErrorLevel_e.NETWORK_OR_SERVER_LOGIC, ex);

				channel.ResStatus = 400;
				//channel.ResHeaderPairs.Add();
				channel.ResBody = null;
				channel.ResBodyLength = -1L;
			}
		}

		private IEnumerable<byte[]> E_ToBody(IEnumerable<string> strs)
		{
			foreach (var str in strs)
				yield return Encoding.ASCII.GetBytes(str + "\r\n");
		}

		private IEnumerable<string> E_FizzBuzz(int start, int end)
		{
			for (int i = start; i <= end; i++)
			{
				if (i % 15 == 0)
					yield return "FizzBuzz";
				else if (i % 3 == 0)
					yield return "Fizz";
				else if (i % 5 == 0)
					yield return "Buzz";
				else
					yield return i.ToString();
			}
		}

		private Dictionary<string, string> GetParameterPairs(HTTPServerChannel channel)
		{
			int i = channel.PathQuery.IndexOf('?');

			if (i == -1)
				throw new Exception("no parameters");

			string sps = channel.PathQuery.Substring(i + 1);
			string[] ps = sps.Split('&');

			Dictionary<string, string> dest = new Dictionary<string, string>();

			foreach (string p in ps)
			{
				string[] kv = p.Split('=');

				if (kv.Length != 2)
					throw new Exception("Bad parameter key-value");

				dest.Add(kv[0], kv[1]);
			}
			return dest;
		}
	}
}
