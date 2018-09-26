# Mac、Linuxで動作させるには？

- 参考記事
  - Mac環境
    - jnoryさんのfork : https://github.com/jnory/MyShogi
  - Linux環境
    - やねうら王MyShogiをLinuxでビルドしてみた : http://hennohito.cocolog-nifty.com/blog/2018/06/myshogilinux-69.html

- コンパイルにはmono環境が必要。
  ソースコード上に if !MONO などと書いてあるので、ソースコード全体を「MONO」で検索して適宜修正すれば他の環境でも動くかも。


## Macについて

- ビルド方法
  > msbuild ../MyShogi.sln /p:Configuration=Debug

  参考)
    https://twitter.com/arrow_elpis/status/1044909606697283585
     msbuildはmono入れたら入ってきますよ。あと、MyShogiをビルドするには MyShogiディレクトリ内で
    "nuget install Microsoft.Net.Compilers" を実行してmonoではないコンパイラを落としてくる必要があります。


- 起動方法
  > mono --arch=32 bin/Debug/MyShogi.exe

- 音は鳴りません。(mono未対応のライブラリが使われているため)
- すべての機能を動作確認できているわけではありません。

## 画像集について

- 画像素材は『将棋神やねうら王』のほうからコピーしてきてください。(そのうち整理して公開する予定です)
