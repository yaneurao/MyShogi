# Mac、Linuxで動作させるには？

- 参考記事
  - MyShogiをMac環境で動かす
    - jnoryさんのfork : https://github.com/jnory/MyShogi
    - AI将棋ソフト『MyShogi』をMacBookProでビルド＆遊んでみた : http://amado.hatenablog.com/entry/20180927/1537975944
  - MyShogiをLinux環境で動かす
    - やねうら王MyShogiをLinuxでビルドしてみた : http://hennohito.cocolog-nifty.com/blog/2018/06/myshogilinux-69.html


## 将棋神やねうら王をMacで

// 書きかけ

- ビルド方法
  > msbuild ../MyShogi.sln /p:Configuration=macOS

  参考)
    https://twitter.com/arrow_elpis/status/1044909606697283585
     msbuildはmono入れたら入ってきますよ。あと、MyShogiをビルドするには MyShogiディレクトリ内で
    "nuget install Microsoft.Net.Compilers" を実行してmonoではないコンパイラを落としてくる必要があります。


- 起動方法
  > mono --arch=32 bin/macOS/MyShogi.exe

- 音は鳴りません。(mono未対応のライブラリが使われているため)
- すべての機能を動作確認できているわけではありません。


## 画像集について

- 画像素材は『将棋神やねうら王』のほうからコピーしてきてください。(そのうち整理して公開する予定です)
