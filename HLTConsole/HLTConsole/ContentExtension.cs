using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HLTStudio.Commons;
using HLTStudio.WebServices;

namespace HLTStudio
{
	public class ContentExtension
	{
		/// <summary>
		/// コンテンツ拡張ファイルのサフィックス
		/// </summary>
		private const string CONTENT_EXTENSION_FILE_SUFFIX = ".HTTCmdTACT-Content-Extension.txt";

		/// <summary>
		/// コンテンツ拡張対象拡張子
		/// </summary>
		private static readonly string[] CONTENT_EXTENSION_TARGET_EXTS = new string[]
		{
			".html",
		};

		private string StrPath;
		private HTTPServerChannel Channel;

		public ContentExtension(string path, HTTPServerChannel channel)
		{
			this.StrPath = path;
			this.Channel = channel;
		}

		public byte[] TryGetContent()
		{
			if (!CONTENT_EXTENSION_TARGET_EXTS.Any(ext => this.StrPath.EndsWithIgnoreCase(ext))) // ? コンテンツ拡張対象拡張子ではない -> 拡張不要
				return null;

			string contentExtensionFile = this.StrPath + CONTENT_EXTENSION_FILE_SUFFIX;

			if (!File.Exists(contentExtensionFile)) // ? コンテンツ拡張ファイル無し -> 拡張不要
				return null;

			string[] lines = File.ReadAllLines(contentExtensionFile, SCommon.ENCODING_SJIS)
				.Select(line => line.Trim())
				.Where(line => line != "")
				.ToArray();
			string command = lines[0];
			string[] parameters = lines.Skip(1).ToArray();

			Func<string[], byte[]> contentGetter = SelectContentGetter(command);

			byte[] content = contentGetter(parameters);
			return content;
		}

		/// <summary>
		/// コンテンツ拡張メソッドを取得する。
		/// </summary>
		/// <param name="command">コマンド</param>
		/// <returns>コンテンツ拡張メソッド</returns>
		private Func<string[], byte[]> SelectContentGetter(string command)
		{
			if (command == "DUMMY")
				return this.GetDummyContent;

			if (command == "REPLACE-WHEN-SMP")
				return this.ReplaceWhenSmartphone;

			throw new Exception("Bad command");
		}

		// =====
		// ここから
		// コンテンツ拡張メソッド
		// =====

		private byte[] GetDummyContent(string[] parameters)
		{
			return Encoding.UTF8.GetBytes($"<meta charset=\"UTF-8\"><div>これは \"{Path.GetFileName(this.StrPath)}\" というファイルのコンテンツ拡張です。(これはダミーです)</div>");
		}

		private byte[] ReplaceWhenSmartphone(string[] parameters)
		{
			if (!IsClientMaybeSmartphone()) // ? スマートフォンではない -> 拡張不要
				return null;

			string srcPattern = parameters[0];
			string destPattern = parameters[1];

			string fileText = File.ReadAllText(this.StrPath, Encoding.UTF8);

			fileText = fileText.Replace(srcPattern, destPattern);

			byte[] content = Encoding.UTF8.GetBytes(fileText);
			return content;
		}

		// =====
		// ここから
		// ユーティリティ・ツールとか
		// =====

		private bool IsClientMaybeSmartphone()
		{
			string value = GetHeaderValue(this.Channel, "User-Agent");

			if (value == null)
				return false;

			bool ret =
				value.Contains("iPhone") ||
				value.Contains("Android") ||
				value.Contains("Mobile");

			return ret;
		}

		private static string GetHeaderValue(HTTPServerChannel channel, string name)
		{
			foreach (string[] pair in channel.HeaderPairs)
				if (pair[0].EqualsIgnoreCase(name))
					return pair[1];

			return null;
		}
	}
}
