# DynamicWebApi
インターフェイス定義とその実装クラスから、WebクライアントとWebサーバーを自動実装します。

## 構成図
丸括弧部分を自動生成します。実装クラスのデバッグのため、Webサーバーを経由しないローカルモードがあります。

```
Webモード
[アプリ] - [インターフェイス] - (Webクライアント) - (Webサーバー) - [実装クラス]

ローカルモード
[アプリ] - [インターフェイス] - (ローカルクライアント) ------------- [実装クラス]
```

## インターフェイス
次のようなインターフェイスを定義します。
```csharp
[DynamicWebApi.AccessModel(typeof(Sample), "Sample1")]
public interface ISample {
    int GetValue(int value);
    Task<int> GetValueAsync(int value);
}
```

AccessModelAttributeの第1引数は、後述の実装クラスを指定します。第2引数は、Webサーバーでのルーティングに使用する名称です。

## 実装クラス
定義したインターフェイスを実装したクラスを作成します。
```csharp
public class Sample : ISample {
    public int GetValue(int value) {
        return value + 1;
    }
    
    public Task<int> GetValueAsync(int value) {
        return Task.FromResult(value + 1);
    }
}
```

*注：戻り値がTaskクラスの場合、メソッド内でTask.Runをする必要はありません。Webサーバー側で非同期に実行されます。*

## クライアント生成
インターフェイス定義からクライアントを生成します。
```csharp
bool useWeb = true; // Webクライアントを生成する（falseのときはローカルクライアント）

// クライアントを生成
ISample sample = DynamicWebApi.Client.AccessorFactory.Create<ISample>(useWeb, w => {
    // HttpClientの設定
    w.HttpClient.BaseAddress = new Uri("http://localhost:4321/");
    w.HttpClient.Timeout = TimeSpan.FromSeconds(20);
});

// 同期実行 結果: 11
Console.WriteLine(sample.GetValue(10));

// 非同期実行 結果: 101
Console.WriteLine(await sample.GetValueAsync(100));
```

useWebをfalseにすると、ローカルクライアントが生成されます。

## Webサーバー起動
コマンドラインに設定を記述しWebサーバーを起動します。
```
DynamicWebApi.Server.exe^
 --baseUrl http://localhost:4321/^
 --baseDirectory "C:\test\xxx\"^
 --assemblyFile "Sample.dll"
```

コマンドラインオプション
* baseUrl
  * ベースのURL。
* baseDirectory
  * ベースのディレクトリ。相対パス可。
* assemblyFile
  * インターフェイス定義を含むアセンブリのファイル名。

Webサーバーを起動すると指定されたアセンブリがメモリ上に読み込まれます。アセンブリファイルはロックされず、書き換えることができます。

Webサーバーはアセンブリファイルを監視しており、アセンブリファイルが変更されたことを検出すると、Webサーバーを再起動します。

## 実装内容
WebサーバーとWebクライアント間の通信はHTTPで行われ、JSONフォーマットのファイルをPOSTメソッドで送信しています。
```
POST http://localhost:4321/Sample1/GetValue
{"value":10}

200
{"value":11}
```

## JSONフォーマットの変更
AccessModelAttributeのJsonConvertersに、JsonConverterを実装したクラスの型を指定すると、型ごとのJSONフォーマットを変更することができます。

## 制限事項
* 戻り値をvoidにすることはできません。
* [Json.NET](https://www.newtonsoft.com/json)でシリアライズできない型は使用できません。




## TODO

* ロギング