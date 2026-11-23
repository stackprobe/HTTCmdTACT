using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HLTStudio.WebServices;
using HLTStudio.Actions;

namespace HLTStudio
{
	public static class ActionServer
	{
		public static bool TryPerform(bool head, string urlPath, HTTPServerChannel channel)
		{
			Action<HTTPServerChannel> a_action = GetAction(urlPath);

			if (a_action == null) // ? アクション無し
				return false;

			if (head)
				throw new Exception("HEAD-リクエストの場合はアクションを実行できません");

			a_action(channel);
			return true;
		}

		private static Action<HTTPServerChannel> GetAction(string urlPath)
		{
			if (urlPath == "/API-Dummy")
				return channel => new Action_Dummy().Run(channel);

			if (urlPath == "/API-Echo")
				return channel => new Action_Echo().Run(channel);

			if (urlPath == "/API-FizzBuzz")
				return channel => new Action_FizzBuzz().Run(channel);

			if (urlPath == "/API-YourRequest")
				return channel => new Action_YourRequest().Run(channel);

			return null;
		}
	}
}
