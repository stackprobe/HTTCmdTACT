# HTTCmdTACT

**静的ファイルの配信と、C# のプログラムによる応答を組み合わせる軽量 HTTP サーバー。**

HTTCmdTACT は、静的ファイルを配信する HTTCmd に、リクエストに応じて C# の処理を実行し、応答を生成する仕組みを加えたものです。独自の API や Web アプリケーションを作るためのベアボーン（最小限の土台）として、ソースコードを拡張して使えます。

- **プログラムで応答を生成。** URL に対応するアクションを C# で実装し、HTTP ステータス、ヘッダー、本文を設定できます。サンプルとして `/API-Echo` を用意しています。
- **静的ファイルも配信。** アクションに対応しない URL は、公開フォルダー（DocRoot）内のファイルを配信します。
- **EXE ひとつで起動。** 公開したいフォルダーを `HTTCmdTACT.exe` にドラッグ＆ドロップすると、そのフォルダーを DocRoot としてポート `80` で起動します。
- **ドメイン名ごとに公開先を切り替え。** `/H` を使えば、同じサーバー・同じポートで、アクセス先のホスト名に応じて別々のフォルダーを配信できます。

## プログラムによる応答

サーバーをポート `8080` で起動し、ブラウザーで `http://localhost:8080/API-Echo` を開くと、サンプルのアクションが HTTP 200、`Content-Type: text/plain; charset=US-ASCII` で `Echo` という本文を返します。DocRoot に対応するファイルを置く必要はありません。

```bat
HTTCmdTACT.exe . 8080
```

`POST` でも同じアクションを呼び出せます。現在のサンプルはリクエスト本文の内容にかかわらず、固定文字列 `Echo` を返します。

```bat
curl.exe -X POST --data "Hello" http://localhost:8080/API-Echo
```

独自の処理を実装するときは、次のファイルを起点にしてください。アクションの変更にはソースコードの編集と再ビルドが必要です。

| ファイル | 役割 |
| --- | --- |
| [ActionServer.cs](HLTConsole/HLTConsole/ActionServer.cs) | `GetAction` で URL パスとアクションを対応付けます。クエリ文字列を除いたパスで判定します。 |
| [Action_Echo.cs](HLTConsole/HLTConsole/Actions/Action_Echo.cs) | 応答処理のサンプルです。`Run` 内で `HTTPServerChannel` に応答を設定します。 |
| [Program.cs](HLTConsole/HLTConsole/Program.cs) | アクションの呼び出しと静的ファイル配信を行います。アクションを先に判定します。 |

アクションでは `channel.Method`、`channel.PathQuery`、`channel.HeaderPairs`、`channel.Body` からリクエストを取得できます。応答は `channel.ResStatus`、`channel.ResHeaderPairs`、`channel.ResBody`、`channel.ResBodyLength` に設定します。具体的な設定方法は `Action_Echo.cs` を参照してください。

アクションでは `GET` や `POST` などのリクエストを処理できますが、現在の実装では `HEAD` によるアクション呼び出しは例外になります。必要なメソッドの判定は各アクションに実装してください。

開発環境は Microsoft Visual Studio Community 2022、対象フレームワークは .NET Framework 4.8 です。ソリューションは [HLTConsole.sln](HLTConsole/HLTConsole.sln) にあります。

## 同時接続数とリクエストボディのバッファ容量

リクエストボディは、アクションを実行する前に接続ごとのバッファへ受信します。[Program.cs](HLTConsole/HLTConsole/Program.cs) では、次の値を設定しています。変更する場合はソースコードを編集して再ビルドしてください。

| 設定 | 現在の値 | 意味 |
| --- | --- | --- |
| `ConnectMax` | `100` | 最大同時接続数です。 |
| `HTTPServerChannel.BodySizeMax` | `50_000_000` | 1接続で受け付けるリクエストボディの最大サイズです。単位はバイトで、現在は 50 MB です。 |
| `HTTPServerChannel.BodyOnStorage` | `true` | バッファの保存先です。`true` はディスク、`false` はメモリを使います。 |

**すべての接続で上限サイズのボディを同時に保持すると、リクエストボディのバッファだけで `ConnectMax × HTTPServerChannel.BodySizeMax` 相当の容量が必要になり得ます。** 現在の設定では、次の計算になります。

```text
100接続 × 50,000,000バイト = 5,000,000,000バイト（約 5 GB）
```

現在は `BodyOnStorage = true` のため、この容量はディスク上のバッファとして使用される可能性があります。`false` に変更すると、同じ容量をメモリ上に保持する可能性があります。起動時に全容量を確保するわけではなく、実際の使用量は同時接続数や受信したボディのサイズによって変わります。

この計算はボディのデータ量の目安であり、サーバー全体のディスク・メモリ使用量の厳密な上限ではありません。管理情報や応答処理にも容量を使い、ディスク保存の場合でも、アクションで `channel.Body.ToByteArray()` を呼ぶとボディ全体をメモリに読み込みます。利用できるディスク容量・メモリ容量に合わせて、同時接続数とボディサイズの上限を調整してください。

## 自宅の PC 1台で、ドメインごとにサイトを配信

ブログと写真ギャラリーなど、複数のドメインで静的サイトを運用する自宅サーバーにも使えます。たとえば `blog.example.com` と `gallery.example.net` を同じ自宅サーバーに向けておけば、HTTCmdTACT を1つ起動するだけで、ドメインごとに別々のフォルダーを配信できます。

`/H` に渡す `C:\www\DocRoots.tsv` に、ホスト名と公開フォルダーをタブで区切って記述します。

```text
blog.example.com	C:\www\Blog
gallery.example.net	C:\www\Gallery
```

```bat
HTTCmdTACT.exe "C:\www\Default" 8080 /H "C:\www\DocRoots.tsv"
```

未登録のホスト名には `C:\www\Default` が使われます。インターネットに HTTPS で公開する場合は、手前のリバースプロキシで HTTPS を受け、元の `Host` ヘッダーを保ったまま HTTCmdTACT に転送します。

## コマンドで起動する

1. Windows 11 で配布アーカイブを展開します。HTTCmdTACT は `HTTCmdTACT.exe` 単体で起動できます。
2. 公開用フォルダーを用意し、トップページとして表示するファイルを `index.html` という名前で配置します。
3. コマンドプロンプトで `HTTCmdTACT.exe` のあるフォルダーに移動し、次のコマンドを実行します。例では `C:\www\DocRoot` を公開します。

   ```bat
   HTTCmdTACT.exe "C:\www\DocRoot" 8080
   ```

4. ブラウザーで `http://localhost:8080/` を開きます。例えば、公開用フォルダーの `files\guide.pdf` には `http://localhost:8080/files/guide.pdf` でアクセスできます。

停止するには、別のコマンドプロンプトから次を実行します。複数の HTTCmdTACT サーバーを起動している場合は、まとめて停止します。

```bat
HTTCmdTACT.exe /S
```

アンインストールするには、サーバーを停止して展開したファイルを削除します。

## 起動コマンド

```text
HTTCmdTACT.exe [DOC-ROOT [PORT-NO [OPTIONS...]]]
```

- `DOC-ROOT` は公開するフォルダーです。省略すると、コマンドを実行した時点のカレントディレクトリを公開します。`.` でも指定できます。
- `PORT-NO` は待ち受けポートです。省略時は `80` です。使用中のポートを避けるには、空いているポートを指定します。
- オプションを使うときは、公開フォルダーとポート番号の両方を指定します。

```bat
HTTCmdTACT.exe . 8080 /K
```

| オプション | 動作 |
| --- | --- |
| `/K` | 起動したコンソールでキーを押すと停止します。 |
| `/T TSV-FILE` | 拡張子と `Content-Type` の対応を追加・上書きします。 |
| `/H TSV-FILE` | リクエストの `Host` ヘッダーに応じて公開フォルダーを切り替えます。未登録のホストには起動時に指定した公開フォルダーを使います。 |
| `/N HTML-FILE` | ファイルが見つからない場合に、指定した HTML を HTTP 404 のページとして返します。 |
| `/C` | ファイルのレスポンス本文をチャンク形式で転送します。 |

`/T` と `/H` の設定ファイルは UTF-8 のタブ区切りテキストです。見出し行を付けず、各行に2列を記述します。

- `/T` はドット付きの拡張子、`Content-Type` の順です。例は `.svg` と `image/svg+xml` です。
- `/H` はポート番号を除いたホスト名、公開フォルダーの順です。例は `localhost` と `C:\www\LocalSite` です。

## 配信の仕様

- ディレクトリの URL では `index.htm`、次に `index.html` を探します。どちらもなければ HTTP 404 を返します。
- 静的ファイル配信に対応するメソッドは `GET` と `HEAD` です。それ以外のメソッドは、対応するアクションがある場合に処理します。
- サーバーはすべての IPv4 インターフェースで HTTP 接続を待ち受けます。ネットワークやファイアウォールの設定によっては別の端末からもアクセスできるため、公開してよいファイルだけを配置してください。
