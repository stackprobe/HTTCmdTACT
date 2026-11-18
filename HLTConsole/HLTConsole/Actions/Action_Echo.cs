using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HLTStudio.WebServices;

namespace HLTStudio.Actions
{
	public class Action_Echo
	{
		public void Run(HTTPServerChannel channel)
		{
			byte[] body = channel.Body.ToByteArray();

			if (3000 < body.Length)// rough limit
				throw null; // 2bs 

			channel.ResStatus = 200;
			channel.ResHeaderPairs.Add(new string[] { "Content-Type", "application/octet-stream" });
			channel.ResBody = new byte[][] { body };
			channel.ResBodyLength = -1L;
		}
	}
}
