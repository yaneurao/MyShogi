
# Mac、Linuxで動作させるには？


- MyShogi V1.31ではコードの修正なしにMonoでコンパイルでき、Mac/Linuxで動作するはずです。


- 参考記事(MyShogiの少し古いバージョンの話なので参考程度にどうぞ)
  - MyShogiをMac環境で動かす
    - jnoryさんのfork : https://github.com/jnory/MyShogi
    - AI将棋ソフト『MyShogi』をMacBookProでビルド＆遊んでみた : http://amado.hatenablog.com/entry/20180927/1537975944
  - MyShogiをLinux環境で動かす
    - やねうら王MyShogiをLinuxでビルドしてみた : http://hennohito.cocolog-nifty.com/blog/2018/06/myshogilinux-69.html



## 将棋神やねうら王をMacで


- Monoのコンパイラが動くようにする。(上の「AI将棋ソフト『MyShogi』をMacBookProでビルド＆遊んでみた」を参考に)

  参考)
    https://twitter.com/arrow_elpis/status/1044909606697283585
     msbuildはmono入れたら入ってきますよ。あと、MyShogiをビルドするには MyShogiディレクトリ内で
    "nuget install Microsoft.Net.Compilers" を実行してmonoではないコンパイラを落としてくる必要があります。

- git clone
  > git clone https://github.com/yaneurao/MyShogi.git

- ビルド方法
  > msbuild MyShogi.sln /p:Configuration=macOS

- 起動方法
  > mono --arch=32 MyShogi/bin/macOS/MyShogi.exe

- (V1.30) 公式のGitHubのソースコードで、修正なしにビルドが通って実行自体は出来るところまできました。
- (V1.30) 表示設定のフォント選択ダイアログがフリーズするらしいです。(Linuxだとこの問題は起きない) Mac用のMonoのバグではないかと…。
- 音は鳴りません。(mono未対応のライブラリが使われているため) → libsoundioでどうにか出来るか検討中です
- すべての機能を動作確認できているわけではありません。

- 参考)
  - 『将棋神やねうら王』のMac対応の作業報告 : http://yaneuraou.yaneu.com/2018/09/30/%E3%80%8E%E5%B0%86%E6%A3%8B%E7%A5%9E%E3%82%84%E3%81%AD%E3%81%86%E3%82%89%E7%8E%8B%E3%80%8F%E3%81%AEmac%E5%AF%BE%E5%BF%9C%E3%81%AE%E4%BD%9C%E6%A5%AD%E5%A0%B1%E5%91%8A/


## 将棋神やねうら王をLinuxで


- ビルド方法
  > msbuild MyShogi.sln /p:Configuration=LINUX

- 起動方法
  > mono --arch=32 MyShogi/bin/LINUX/MyShogi.exe

- 詳細は上の「将棋神やねうら王をMacで」を参考にしてください。


## 画像集について


- 画像素材は『将棋神やねうら王』のインストール先フォルダからコピーしてきてください。(そのうち整理して公開する予定です)


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

    > mingw32-make clean YANEURAOU_EDITION=YANEURAOU_2018_TNK_ENGINE
    > mingw32-make -j8 avx2 COMPILER=clang++ YANEURAOU_EDITION=YANEURAOU_2018_TNK_ENGINE

    YaneuraOu-by-gcc.exeというファイルが出来ますので、これをMyShogi.exe配下の以下のところにコピーします。
      engine/tanuki2018/YaneuraOu2018NNUE_avx2.exe

    以下、各CPUのファイルをビルドする例です。

    > mingw32-make clean YANEURAOU_EDITION=YANEURAOU_2018_TNK_ENGINE
    > mingw32-make -j8 avx2 COMPILER=clang++ YANEURAOU_EDITION=YANEURAOU_2018_TNK_ENGINE
    > cp YaneuraOu-by-gcc.exe XXX/engine/tanuki2018/YaneuraOu2018NNUE_avx2.exe

    > mingw32-make clean YANEURAOU_EDITION=YANEURAOU_2018_TNK_ENGINE
    > mingw32-make -j8 sse42 COMPILER=clang++ YANEURAOU_EDITION=YANEURAOU_2018_TNK_ENGINE
    > cp YaneuraOu-by-gcc.exe XXX/engine/tanuki2018/YaneuraOu2018NNUE_sse42.exe

    > mingw32-make clean YANEURAOU_EDITION=YANEURAOU_2018_TNK_ENGINE
    > mingw32-make -j8 sse41 COMPILER=clang++ YANEURAOU_EDITION=YANEURAOU_2018_TNK_ENGINE
    > cp YaneuraOu-by-gcc.exe XXX/engine/tanuki2018/YaneuraOu2018NNUE_sse41.exe

    > mingw32-make clean YANEURAOU_EDITION=YANEURAOU_2018_TNK_ENGINE
    > mingw32-make -j8 sse2 COMPILER=clang++ YANEURAOU_EDITION=YANEURAOU_2018_TNK_ENGINE
    > cp YaneuraOu-by-gcc.exe XXX/engine/tanuki2018/YaneuraOu2018NNUE_sse2.exe

    > mingw32-make clean YANEURAOU_EDITION=YANEURAOU_2018_TNK_ENGINE
    > mingw32-make -j8 COMPILER=clang++ tournament YANEURAOU_EDITION=YANEURAOU_2018_TNK_ENGINE
    > cp YaneuraOu-by-gcc.exe XXX/engine/tanuki2018/YaneuraOu2018NNUE_tournament.exe

    > mingw32-make clean YANEURAOU_EDITION=YANEURAOU_2018_TNK_ENGINE
    > mingw32-make -j8 evallearn COMPILER=g++ YANEURAOU_EDITION=YANEURAOU_2018_TNK_ENGINE
    > cp YaneuraOu-by-gcc.exe XXX/engine/tanuki2018/YaneuraOu2018NNUE_learn_avx2.exe

    > mingw32-make clean YANEURAOU_EDITION=YANEURAOU_2018_TNK_ENGINE
    > mingw32-make -j8 evallearn-sse42 COMPILER=g++ YANEURAOU_EDITION=YANEURAOU_2018_TNK_ENGINE
    > cp YaneuraOu-by-gcc.exe XXX/engine/tanuki2018/YaneuraOu2018NNUE_learn_sse42.exe

    // 32bit環境向け

    > mingw32-make clean YANEURAOU_EDITION=YANEURAOU_2018_TNK_ENGINE
    > mingw32-make -j8 nosse COMPILER=clang++ YANEURAOU_EDITION=YANEURAOU_2018_TNK_ENGINE
    > cp YaneuraOu-by-gcc.exe XXX/engine/tanuki2018/YaneuraOu2018NNUE_nosse.exe

    // KPPT型、AVX2用にコンパイルする例
  
    > mingw32-make clean YANEURAOU_EDITION=YANEURAOU_2018_OTAFUKU_ENGINE_KPPT
    > mingw32-make -j8 avx2 COMPILER=clang++ YANEURAOU_EDITION=YANEURAOU_2018_OTAFUKU_ENGINE_KPPT
    > cp YaneuraOu-by-gcc.exe XXX/engine/yomita2018/YaneuraOu2018KPPT_avx2.exe

    // KKPT型、AVX2用にコンパイルする例

    > mingw32-make clean YANEURAOU_EDITION=YANEURAOU_2018_OTAFUKU_ENGINE_KPP_KKPT
    > mingw32-make -j8 avx2 COMPILER=clang++ YANEURAOU_EDITION=YANEURAOU_2018_OTAFUKU_ENGINE_KPP_KKPT
    > cp YaneuraOu-by-gcc.exe XXX/engine/yaneuraou2018/Yaneuraou2018_kpp_kkpt_avx2.exe

    // tanuki-詰将棋エンジンをAVX2用にコンパイルする例

    > mingw32-make clean YANEURAOU_EDITION=MATE_ENGINE
    > mingw32-make -j8 avx2 COMPILER=clang++ YANEURAOU_EDITION=MATE_ENGINE
    > cp YaneuraOu-by-gcc.exe XXX/engine/tanuki_mate/2018-Otafuku/tanuki_mate_avx2.exe
