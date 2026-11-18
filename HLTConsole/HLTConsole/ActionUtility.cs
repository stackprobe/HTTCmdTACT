using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HLTStudio.Commons;
using HLTStudio.WebServices;

namespace HLTStudio
{
	public static class ActionUtility
	{
		/// <summary>
		/// 現在のHTTPリクエスト専用の一時ディレクトリを作成してパスを返します。
		/// 同じリクエスト内でも、呼び出すたびに新しいディレクトリを作成して異なるパスを返します。
		/// 応答の送信完了後、または通信が途中で切断された際に、このディレクトリは自動的に削除されます。
		/// ※このメソッドを使用するには HTTPServerChannel.BodyOnStorage を true にする必要があります。
		/// </summary>
		/// <param name="channel">接続チャネル</param>
		/// <returns>一時ディレクトリのパス</returns>
		public static string GetRequestScopedTemporaryDirectoryPath(HTTPServerChannel channel)
		{
			WorkingDir wd = (WorkingDir)channel.Body.GetType().GetField("WD")?.GetValue(channel.Body);

			if (wd == null)
				throw new Exception("Maybe HTTPServerChannel.BodyOnStorage == false");

			string dir = wd.MakePath();
			SCommon.CreateDir(dir);
			return dir;
		}

		public static IEnumerable<byte[]> E_ReadFile(string file)
		{
			return Program.PUB_E_ReadFile(file);
		}
	}
}
