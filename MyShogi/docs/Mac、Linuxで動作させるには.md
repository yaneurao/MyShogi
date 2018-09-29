# Mac、Linuxで動作させるには？

- 参考記事
  - MyShogiをMac環境で動かす
    - jnoryさんのfork : https://github.com/jnory/MyShogi
    - AI将棋ソフト『MyShogi』をMacBookProでビルド＆遊んでみた : http://amado.hatenablog.com/entry/20180927/1537975944
  - MyShogiをLinux環境で動かす
    - やねうら王MyShogiをLinuxでビルドしてみた : http://hennohito.cocolog-nifty.com/blog/2018/06/myshogilinux-69.html

  参考)
    https://twitter.com/arrow_elpis/status/1044909606697283585
     msbuildはmono入れたら入ってきますよ。あと、MyShogiをビルドするには MyShogiディレクトリ内で
    "nuget install Microsoft.Net.Compilers" を実行してmonoではないコンパイラを落としてくる必要があります。


## 将棋神やねうら王をMacで

- Monoのコンパイラが動くようにする。(上の「AI将棋ソフト『MyShogi』をMacBookProでビルド＆遊んでみた」を参考に)

- git clone
  > git clone https://github.com/yaneurao/MyShogi.git

- ビルド方法
  > msbuild MyShogi.sln /p:Configuration=macOS

- 起動方法
  > mono --arch=32 MyShogi/bin/macOS/MyShogi.exe

- 音は鳴りません。(mono未対応のライブラリが使われているため)
- すべての機能を動作確認できているわけではありません。
- フォントがおかしいところに関しては作業中。
- フォント生成で落ちるようであれば、ソース上の以下の文字列を"Hiragino Kaku Gothic Pro W3"とかに変更してください。

                "MS Gothic"
                "MS UI Gothic"
                "ＭＳ ゴシック"
                "MSPゴシック"
                "Yu Gothic UI"
                "Microsoft Sans Serif"

- フォントに関してはもう少し良い仕組みを今後のUpdateで用意します。

- 参考)
  - 『将棋神やねうら王』のMac対応の作業報告 : http://yaneuraou.yaneu.com/2018/09/30/%E3%80%8E%E5%B0%86%E6%A3%8B%E7%A5%9E%E3%82%84%E3%81%AD%E3%81%86%E3%82%89%E7%8E%8B%E3%80%8F%E3%81%AEmac%E5%AF%BE%E5%BF%9C%E3%81%AE%E4%BD%9C%E6%A5%AD%E5%A0%B1%E5%91%8A/


## 将棋神やねうら王をLinuxで

// 機種依存コードを移植してないと動きません。
//  → MyShogi.Model.Dependent.Mono/MonoAPI.csにLinux用のコードを書き足してください。

- ビルド方法
  > msbuild MyShogi.sln /p:Configuration=LINUX

- 起動方法
  > mono --arch=32 MyShogi/bin/LINUX/MyShogi.exe

- 詳細は上の「将棋神やねうら王をMacで」を参考にしてください。


## 画像集について

- 画像素材は『将棋神やねうら王』のインストール先フォルダからコピーしてきてください。(そのうち整理して公開する予定です)
