using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HLTStudio.WebServices;

namespace HLTStudio.Actions
{
	public class Action_Dummy
	{
		public void Run(HTTPServerChannel channel)
		{
			channel.ResStatus = 200;
			channel.ResHeaderPairs.Add(new string[] { "Content-Type", "text/plain; charset=US-ASCII" });
			channel.ResBody = new byte[][] { Encoding.ASCII.GetBytes("Dummy") };
			channel.ResBodyLength = -1L;
		}
	}
}
