
# Mac、Linuxで動作させるには？


- MyShogi V1.32ではコードの修正なしにMonoでコンパイルでき、Mac/Linuxで動作するはずです。


- 参考記事(古い記事も含まれています)
  - MyShogiをMac環境で動かす
    - jnoryさんのfork : https://github.com/jnory/MyShogi
    - AI将棋ソフト『MyShogi』をMacBookProでビルド＆遊んでみた             : https://amado.hatenablog.com/entry/20180927/1537975944
    - AI将棋ソフト『MyShogi』をMacBookProでビルド＆遊んでみた２（本家版） : https://amado.hatenablog.com/entry/20181028/1540723499

  - MyShogiをLinux環境で動かす
    - やねうら王MyShogiをLinuxでビルドしてみた : http://hennohito.cocolog-nifty.com/blog/2018/06/myshogilinux-69.html
    - Ubuntuで『将棋神　やねうら王』 : http://fxst24.blog.fc2.com/blog-entry-545.html


## ビルドする上で注意点


- Monoのコンパイラ(mcs)が現状C#7.2までしか対応しておらず、本プロジェクトではC#7.3をターゲットとしているので、そのままだとコンパイルが通りません。
- MsBuildでビルドすると例外が出たときに行番号が表示されなくて困るのですが、mcsはまだC#7.2までしか対応していないのでビルドできません。
- mcsがC#7.3まで対応したら、mcsでビルドするようにすべきだと思います。


## 将棋神やねうら王をMacで


- Monoのコンパイラが動くようにする。(上の「AI将棋ソフト『MyShogi』をMacBookProでビルド＆遊んでみた」を参考に)

- git clone
  > git clone https://github.com/yaneurao/MyShogi.git

- ビルド方法
  > msbuild MyShogi.sln /p:Configuration=macOS


- 起動方法
  > mono --arch=32 MyShogi/bin/macOS/MyShogi.exe

- (V1.30) 表示設定のフォント選択ダイアログがフリーズするらしいです。(Linuxだとこの問題は起きない) Mac用のMonoのバグではないかと…。
- (V1.32) libsoundioを用いて音が鳴るようになりました。サウンド再生のために別途以下のライブラリが必要です。
    https://github.com/jnory/MyShogiSoundPlayer/releases/
- (V1.32) 棋譜ウインドウ、常に現在の局面が選択されている状態にしたいのですが、ListViewのOwnerDraw、Monoでバグるのでいまのところ実現できないです。
- (V1.32) 棋譜ウインドウ、メインウインドウに埋め込み状態からフロート状態に変更したときに再描画されません。
    - DockStyleの変更によってresizeイベント起きないというMonoのbugです。回避が難しいので、
      そのあと棋譜ウインドウをリサイズして再描画を促すか、再起動するかしてください。
    - 検討ウインドウ、メインウインドウに埋め込んでいると再描画されないことがあるようです。気になる人はフロート状態で使ってみてください。

- すべての機能を動作確認できているわけではありません。

- 参考)
  - 『将棋神やねうら王』のMac対応の作業報告 : http://yaneuraou.yaneu.com/2018/09/30/%E3%80%8E%E5%B0%86%E6%A3%8B%E7%A5%9E%E3%82%84%E3%81%AD%E3%81%86%E3%82%89%E7%8E%8B%E3%80%8F%E3%81%AEmac%E5%AF%BE%E5%BF%9C%E3%81%AE%E4%BD%9C%E6%A5%AD%E5%A0%B1%E5%91%8A/


## 将棋神やねうら王をLinuxで


- ビルド方法
  > msbuild MyShogi.sln /p:Configuration=LINUX

- 起動方法
  > MyShogi/bin/LINUX/MyShogi.exe

- その他、依存ライブラリ
  - 棋譜のクリップボードへのコピー、クリップボードからのペーストには、xclipが/usr/bin/xclipにインストールされている必要があります。
  - サウンド再生は、Mac用と同じく、libsoundioをwrapしたjnoryさんのライブラリ(実行ファイル)に依存します。
      https://github.com/jnory/MyShogiSoundPlayer/releases/tag/0.2.0  [2018/11/09 19:10]
    - このバージョンでUbuntu18.04で正常に音声が再生されることを確認しました。

- 詳細は上の「将棋神やねうら王をMacで」を参考にしてください。

- Ubuntuで『将棋神　やねうら王』 : http://fxst24.blog.fc2.com/blog-entry-545.html
  - (V1.32) この手順でHyper-V + Ubuntu18.04で動作することを確認しました。[2018/10/23 14:30]


## Monoのバグ


- Mono(Mac/Linux共通)
  - ListViewのEnsureVisibleで例外が出る。Visible=falseのときにスクローバーの高さの計算を間違うようだ。
    - Visible=falseのときはEnsureVisibleを呼び出さないようにして回避。
  - GDI+でalpha channelのある画像の転送でゴミが出る。(alpha = 0の部分の画像の転送がおかしい)
    - 自前で転送するコードを書いて回避。
  - Graphics.DrawImage()で転送元が半透明かつ、転送先がCreateBitmap()したBitmapだと転送元のalphaが無視される
    - 転送先が24bppRgbだとこの問題が出るようなので、Monoの時だけ転送先を32bppArgbに変更して回避。

- Mono(Mac)
  - ファイルダイアログを出すところでフリーズ。回避しようがないのであぼーん。

- Mono(Linux)
  - ListViewExでOwnerDrawにすると無限再帰になってメッセージの処理ができなくなる。これもMono(Linux)のバグくさい。Macではどうだかわからない。
    - DrawSubItemのイベントで描画したときに画面が汚れた判定になっていて、再描画イベントがまた飛んでくるのがおかしい。
    - これのせいでメッセージを処理できなくなり、メニューなどが描画されない。
    - MonoのときだけListViewのOwnerDrawで描画するのをやめる。
  - メニューの「設定」の項目から右側の項目、文字フォントの変更が反映していない。
    - MenuStripにぶらさげているToolStripMenuItemの、ambient propertyになっているFont、親のFontが反映しないバグ。
    - 自前ですべてにフォントを設定することで回避。
  - 棋譜ウインドウをフロートに変更したときに再描画されない。
    - DockStyleの変更によってresizeイベント起きないというMonoのbugのため。回避が難しいので、とりま放置。
      - Controls.Add()の直後にイベントが来てないっぽい。await Task.Delay(0)とかで回避できるかも。


## 音声・画像素材について


- 音声・画像素材など足りない素材は『将棋神やねうら王』のインストール先フォルダからコピーしてきてください。(そのうち整理して公開する予定です)

- フリーの画面素材 : https://github.com/jnory/MyShogi
- フリーの音声素材 : https://github.com/matarillo/MyShogiSound cf. https://twitter.com/matarillo/status/1053131190163533825
- その他、足りない素材は、『将棋神やねうら王』のUpdaterのなかに入っているかも。


## 思考エンジンについて


- 『将棋神やねうら王』の各思考エンジンをMac/Linux上でコンパイルする必要があります。
  (Windows向けの実行ファイルをそのまま動かすことは出来ません)

  - 『tanuki-2018』 → NNUE型
  - 『tanuki- SDT5』,『Qhapaq 2018』『読み太 2018』→ KPPT型
  - 『やねうら王 2018』→ KKPT型
  - 『tanuki-詰将棋』→ MATE_ENGINE
  の4種類あります。


### 思考エンジンのコンパイル手順


- やねうら王のmakefileを用いてclangでコンパイルします。

```CMake
mingw32-make clean YANEURAOU_EDITION=YANEURAOU_2018_TNK_ENGINE
mingw32-make -j8 avx2 COMPILER=clang++ YANEURAOU_EDITION=YANEURAOU_2018_TNK_ENGINE
```

    YaneuraOu-by-gcc.exeというファイルが出来ますので、これをMyShogi.exe配下の以下のところにコピーします。
      engine/tanuki2018/YaneuraOu2018NNUE_avx2.exe

    以下、各CPUのファイルをビルドする例です。

```CMake
mingw32-make clean YANEURAOU_EDITION=YANEURAOU_2018_TNK_ENGINE
mingw32-make -j8 avx2 COMPILER=clang++ YANEURAOU_EDITION=YANEURAOU_2018_TNK_ENGINE
cp YaneuraOu-by-gcc.exe XXX/engine/tanuki2018/YaneuraOu2018NNUE_avx2.exe
```

```CMake
mingw32-make clean YANEURAOU_EDITION=YANEURAOU_2018_TNK_ENGINE
mingw32-make -j8 sse42 COMPILER=clang++ YANEURAOU_EDITION=YANEURAOU_2018_TNK_ENGINE
cp YaneuraOu-by-gcc.exe XXX/engine/tanuki2018/YaneuraOu2018NNUE_sse42.exe
```

```CMake
mingw32-make clean YANEURAOU_EDITION=YANEURAOU_2018_TNK_ENGINE
mingw32-make -j8 sse41 COMPILER=clang++ YANEURAOU_EDITION=YANEURAOU_2018_TNK_ENGINE
cp YaneuraOu-by-gcc.exe XXX/engine/tanuki2018/YaneuraOu2018NNUE_sse41.exe
```

```CMake
mingw32-make clean YANEURAOU_EDITION=YANEURAOU_2018_TNK_ENGINE
mingw32-make -j8 sse2 COMPILER=clang++ YANEURAOU_EDITION=YANEURAOU_2018_TNK_ENGINE
cp YaneuraOu-by-gcc.exe XXX/engine/tanuki2018/YaneuraOu2018NNUE_sse2.exe
```

```CMake
mingw32-make clean YANEURAOU_EDITION=YANEURAOU_2018_TNK_ENGINE
mingw32-make -j8 COMPILER=clang++ tournament YANEURAOU_EDITION=YANEURAOU_2018_TNK_ENGINE
cp YaneuraOu-by-gcc.exe XXX/engine/tanuki2018/YaneuraOu2018NNUE_tournament.exe
```

```CMake
mingw32-make clean YANEURAOU_EDITION=YANEURAOU_2018_TNK_ENGINE
mingw32-make -j8 evallearn COMPILER=g++ YANEURAOU_EDITION=YANEURAOU_2018_TNK_ENGINE
cp YaneuraOu-by-gcc.exe XXX/engine/tanuki2018/YaneuraOu2018NNUE_learn_avx2.exe
```

```CMake
mingw32-make clean YANEURAOU_EDITION=YANEURAOU_2018_TNK_ENGINE
mingw32-make -j8 evallearn-sse42 COMPILER=g++ YANEURAOU_EDITION=YANEURAOU_2018_TNK_ENGINE
cp YaneuraOu-by-gcc.exe XXX/engine/tanuki2018/YaneuraOu2018NNUE_learn_sse42.exe
```

// 32bit環境向け

```CMake
mingw32-make clean YANEURAOU_EDITION=YANEURAOU_2018_TNK_ENGINE
mingw32-make -j8 nosse COMPILER=clang++ YANEURAOU_EDITION=YANEURAOU_2018_TNK_ENGINE
cp YaneuraOu-by-gcc.exe XXX/engine/tanuki2018/YaneuraOu2018NNUE_nosse.exe
```

// KPPT型、AVX2用にコンパイルする例

```CMake 
mingw32-make clean YANEURAOU_EDITION=YANEURAOU_2018_OTAFUKU_ENGINE_KPPT
mingw32-make -j8 avx2 COMPILER=clang++ YANEURAOU_EDITION=YANEURAOU_2018_OTAFUKU_ENGINE_KPPT
cp YaneuraOu-by-gcc.exe XXX/engine/yomita2018/YaneuraOu2018KPPT_avx2.exe
```

// KKPT型、AVX2用にコンパイルする例

```CMake
mingw32-make clean YANEURAOU_EDITION=YANEURAOU_2018_OTAFUKU_ENGINE_KPP_KKPT
mingw32-make -j8 avx2 COMPILER=clang++ YANEURAOU_EDITION=YANEURAOU_2018_OTAFUKU_ENGINE_KPP_KKPT
cp YaneuraOu-by-gcc.exe XXX/engine/yaneuraou2018/Yaneuraou2018_kpp_kkpt_avx2.exe
```

// tanuki-詰将棋エンジンをAVX2用にコンパイルする例

```CMake
mingw32-make clean YANEURAOU_EDITION=MATE_ENGINE
mingw32-make -j8 avx2 COMPILER=clang++ YANEURAOU_EDITION=MATE_ENGINE
cp YaneuraOu-by-gcc.exe XXX/engine/tanuki_mate/2018-Otafuku/tanuki_mate_avx2.exe
```
