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
			channel.ResStatus = 200;
			channel.ResHeaderPairs.Add(new string[] { "Content-Type", "application/octet-stream" });
			channel.ResBody = new byte[][] { channel.Body.ToByteArray() };
			channel.ResBodyLength = -1L;
		}
	}
}
