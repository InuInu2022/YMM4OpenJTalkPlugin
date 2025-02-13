# YMM4 Open JTalk プラグイン

## これはなに？

YMM4のボイスとして **「[Open JTalk](https://open-jtalk.sp.nitech.ac.jp/)」(オープンジェイトーク)** を使えるようにしたプラグインです。

「[SHABERU](http://akihiro0105.web.fc2.com/Downloads/Downloads-SHABERU.html)」のボイスも追加して利用することができます。

不具合を見つけたら、YMM4本体ではなく[github issues](https://github.com/InuInu2022/YMM4OpenJTalkPlugin/issues)などで報告してください！

### 対応バージョン

- YMM v4.35.xx 以降

## 使い方

1. プラグインをインストールする
2. （初回）キャラクター設定で「YMM4 Open JTalk プラグインの声質を再読み込み」
3. キャラクターを作る
4. 作ったキャラクターを選んだ状態でセリフを入力する

## パラメータ

セリフ毎に設定できる次のパラメータがあります。

- 話速 (speed)
- 大きさ (volume)
- 高さ (pitch)
- 声質詳細 (alpha）
- 抑揚 (gvWeightLF0)

また詳細パラメータもありますが、基本的に触らなくてOKです。

- 無声音閾値 (msdThreshold)
- 遷移平滑度（beta）
- スペクトル変動重み (gvWeightSpectrum)

## 感情(スタイル)

- v0.1では数値が一番大きいスタイルが選ばれます。感情合成はできません。
- スタイルが１種類のボイスはパラメータを弄っても変化ありません

### インストール方法

[Releases](https://github.com/InuInu2022/YMM4OpenJTalkPlugin/releases) 以下にある最新のバージョンの`YMM4OpenJTalkPlugin.v.***.ymme`をインストールしてください。

`ymme`ファイルをダブルクリックするとインストールが始まります。

インストール後、「キャラクター設定」の「ボイス」で、「YMM4 Open JTalk プラグインの声質を再読み込み」を選択して、現在のボイスライブラリ（音響モデル）を取得してください。
※新しくボイスライブラリ（音響モデル）を増やすたびに必要です

<!--
### ニコニコモンズ

ニコニコに投稿する際には以下のコンテンツIDを親子登録してください。

[*****](https://commons.nicovideo.jp/works/)

(YMM4の素材一覧からも確認できます。)
-->

### プラグインの更新

プラグインの設定画面から更新確認とダウンロードができます。
`ymme`ファイルをダブルクリックするとインストールが始まります。

### デフォルトで使えるボイスライブラリ（音響モデル）

以下のボイスが初期インストールされています。

- 女声: tohoku-f01
- 男性: m001
- 女声: メイ
- 男性: タクミ

>! 注意
> v0.1ではスタイル（感情）の合成には対応していません

### ボイスライブラリ（音響モデル）を追加する場合

1. プラグインのインストール先フォルダの`voices`以下にデータを置く
2. `voices.json`を書き換える
3. 「YMM4 Open JTalk プラグインの声質を再読み込み」をして指定

#### `voices.json` 参考

```json:voices.json
{
    "Name": "女声: tohoku-f01",
    "Id": "tohoku-f01",
    "Terms": "https://github.com/icn-lab/htsvoice-tohoku-f01/blob/master/COPYRIGHT.txt",
    "FileSize": "",
    "Author": "東北大学 伊藤・能勢研究室(ICN Lab.)",
    "ContentId": "",
    "StylePaths": {
        "neutral" : "tohoku-f01-neutral.htsvoice",
        "happy" : "tohoku-f01-happy.htsvoice",
        "angry" : "tohoku-f01-angry.htsvoice",
        "sad" : "tohoku-f01-sad.htsvoice"
    }
},
```

- `"Id"`は`voices`以下のデータを置いたフォルダ名と一致させてください

#### ボイスライブラリ（音響モデル）

`htsvoice`形式の音響モデルがボイスライブラリとして利用できます。

- [なんかいろいろしてみます ダウンロード](http://akihiro0105.web.fc2.com/Downloads/Downloads-htsvoice.html)
- [音響モデルを配布していこうと思います（htsvoice） : kukulu合成音声（htsvoice)配付場所](https://ragolun.exblog.jp/22985257/)
- [【まほろば】Open JTalk の音響モデルを試す](https://petile.sakura.ne.jp/mahoroba/e1875.html)

## License

```
MIT License

Copyright (c) 2024 InuInu
```

- [Licenses](./licenses/)