<a id="markdown-外部エンジンを将棋神やねうら王myshogiで使う方法" name="外部エンジンを将棋神やねうら王myshogiで使う方法"></a>
# 外部エンジンを将棋神やねうら王/MyShogiで使う方法

<!-- TOC -->

1. [外部エンジンを将棋神やねうら王/MyShogiで使う方法](#外部エンジンを将棋神やねうら王myshogiで使う方法)
    1. [Step 1. 読み込みたいエンジンに一番近い思考エンジンを選びます。](#step-1-読み込みたいエンジンに一番近い思考エンジンを選びます)
        1. [通常対局エンジンの場合](#通常対局エンジンの場合)
            1. [通常対局エンジン - やねうら王系](#通常対局エンジン---やねうら王系)
            2. [通常対局エンジン - Apery系](#通常対局エンジン---apery系)
            3. [通常対局エンジン - 技巧系](#通常対局エンジン---技巧系)
            4. [通常対局エンジン - それ以外](#通常対局エンジン---それ以外)
        2. [詰将棋エンジンの場合](#詰将棋エンジンの場合)
            1. [詰将棋エンジン - やねうら王系](#詰将棋エンジン---やねうら王系)
            2. [詰将棋エンジン - 脊尾詰](#詰将棋エンジン---脊尾詰)
            3. [詰将棋エンジン - なのは詰め](#詰将棋エンジン---なのは詰め)
            4. [詰将棋エンジン - その他の詰将棋用エンジン](#詰将棋エンジン---その他の詰将棋用エンジン)
    2. [Step 2. 入力欄を適宜、書き換えます。](#step-2-入力欄を適宜書き換えます)
        1. [Step 2.の「エンジンファイル名」](#step-2のエンジンファイル名)
    3. [Step 3. 「エンジン定義ファイルの書き出し」を行います。](#step-3-エンジン定義ファイルの書き出しを行います)
    4. [Step 4. 思考エンジンの入ったフォルダを配置します。](#step-4-思考エンジンの入ったフォルダを配置します)
        1. [思考エンジンのフォルダの配置先のフォルダ](#思考エンジンのフォルダの配置先のフォルダ)

<!-- /TOC -->

<a id="markdown-step-1-読み込みたいエンジンに一番近い思考エンジンを選びます" name="step-1-読み込みたいエンジンに一番近い思考エンジンを選びます"></a>
## Step 1. 読み込みたいエンジンに一番近い思考エンジンを選びます。

読み込みたいエンジンに一番近いと思われる思考エンジンを選びます。
思考エンジンには、通常対局用と詰将棋エンジンとの二種類が存在します。(両方を兼ねているものもありえますが、本ソフトではそれらは別個のものとして扱います。)

<a id="markdown-通常対局エンジンの場合" name="通常対局エンジンの場合"></a>
### 通常対局エンジンの場合 

<a id="markdown-通常対局エンジン---やねうら王系" name="通常対局エンジン---やねうら王系"></a>
#### 通常対局エンジン - やねうら王系

やねうら王を改造して作られているものは、Step 1.で「やねうら系」を選びましょう。

やねうら王系の代表的なバリエーションとして以下の思考エンジンがあります。

- [elmo](https://mk-takizawa.github.io/elmo/howtouse_elmo.html) : WCSC27優勝
- [Kristallweizen](https://github.com/Tama4649/Kristallweizen/) : WCSC28優勝 , WCSC29準優勝
- [tanuki-](https://github.com/nodchip/tanuki-) : WCSC29 3位
- [dolphin](https://twitter.com/_illqha) : 草の根で人気
- [水匠](https://twitter.com/tayayan_ts/status/1258188718759768065) : WOCSC30優勝
- [Qhapaq](https://github.com/qhapaq-49/qhapaq-bin/releases) : WCSC29 5位

<a id="markdown-通常対局エンジン---apery系" name="通常対局エンジン---apery系"></a>
#### 通常対局エンジン - Apery系

『Apery』か、『Apery』を改造して作られているものは、Step 1.で「Apery系」を選びましょう。

- [Apery](https://hiraokatakuya.github.io/apery/) : WCSC28 3位

<a id="markdown-通常対局エンジン---技巧系" name="通常対局エンジン---技巧系"></a>
#### 通常対局エンジン - 技巧系

『技巧』か、『技巧』を改造して作られているものは、Step 1.で「技巧系」を選びましょう。

- [技巧](https://github.com/gikou-official/Gikou/) : WCSC26 2位 , WCSC27 3位

<a id="markdown-通常対局エンジン---それ以外" name="通常対局エンジン---それ以外"></a>
#### 通常対局エンジン - それ以外

上記以外の通常対局エンジンの場合、Step 1.で「その他の通常対局用エンジン」を選びましょう。

本ソフトから用いる思考エンジンは、USIプロトコルに対応している必要があります。
(有志が作って公開している将棋の思考エンジンはたいていUSIプロトコルに対応しています)

有名なところでは、以下のような通常対局用エンジンがあります。

- [GPSshogi](https://gps.tanaka.ecc.u-tokyo.ac.jp/gpsshogi/) : WCSC22 優勝

<a id="markdown-詰将棋エンジンの場合" name="詰将棋エンジンの場合"></a>
### 詰将棋エンジンの場合

<a id="markdown-詰将棋エンジン---やねうら王系" name="詰将棋エンジン---やねうら王系"></a>
#### 詰将棋エンジン - やねうら王系

やねうら王系の詰将棋エンジンの場合は、Step 1.で「やねうら王詰め」を選びましょう。
例えば、『tanuki-詰将棋エンジン』もやねうら王系の詰将棋エンジンで、やねうら王のGitHubからダウンロードできます。

- [やねうら王](https://github.com/yaneurao/YaneuraOu)

<a id="markdown-詰将棋エンジン---脊尾詰" name="詰将棋エンジン---脊尾詰"></a>
#### 詰将棋エンジン - 脊尾詰

詰将棋エンジンとして昔から有名なのは『脊尾詰』でしょう。以下のところからダウンロードできます。
『脊尾詰』を使う場合は、Step 1.で「脊尾詰」を選びましょう。

- [脊尾詰](http://panashogi.web.fc2.com/seotsume.html)

<a id="markdown-詰将棋エンジン---なのは詰め" name="詰将棋エンジン---なのは詰め"></a>
#### 詰将棋エンジン - なのは詰め

長手数の詰将棋が解ける詰将棋エンジンとしては、『なのは詰め』も有名でしょう。『なのは詰め』を使う場合は、Step 1.で「脊尾詰」を選びましょう。

- [なのは詰め](http://vivio.blog.shinobi.jp/%E3%82%B3%E3%83%B3%E3%83%94%E3%83%A5%E3%83%BC%E3%82%BF%E5%B0%86%E6%A3%8B/%E3%81%AA%E3%81%AE%E3%81%AF%E8%A9%B0%E3%82%8164bit%E7%89%88)

<a id="markdown-詰将棋エンジン---その他の詰将棋用エンジン" name="詰将棋エンジン---その他の詰将棋用エンジン"></a>
#### 詰将棋エンジン - その他の詰将棋用エンジン

その他の詰将棋エンジンの場合、Step 1.で「その他の詰将棋用エンジン」を選びましょう。

<a id="markdown-step-2-入力欄を適宜書き換えます" name="step-2-入力欄を適宜書き換えます"></a>
## Step 2. 入力欄を適宜、書き換えます。

Step 1.で近い思考エンジンを選択すれば、Step 2.の空欄(テキストボックスなど)に自動的に記入されます。これを適宜書き換えます。

<a id="markdown-step-2のエンジンファイル名" name="step-2のエンジンファイル名"></a>
### Step 2.の「エンジンファイル名」

「エンジンファイル名」のところには、エンジンのファイル名を書きます。
ただし、
- 拡張子は書きません。
- 各CPU用のファイル名は
でなければなりません。


<a id="markdown-step-3-エンジン定義ファイルの書き出しを行います" name="step-3-エンジン定義ファイルの書き出しを行います"></a>
## Step 3. 「エンジン定義ファイルの書き出し」を行います。

「エンジン定義ファイルの書き出し」のボタンを押して、エンジン定義ファイルを書き出します。
エンジン定義ファイルは、"engine_define.xml"というファイル名固定です。このファイル名を変更しないでください。

ここで書き出されたファイルは、思考エンジンの入っているフォルダに配置します。

<a id="markdown-step-4-思考エンジンの入ったフォルダを配置します" name="step-4-思考エンジンの入ったフォルダを配置します"></a>
## Step 4. 思考エンジンの入ったフォルダを配置します。

思考エンジン本体は、ドキュメントフォルダに
myshogi-engines/
というフォルダを作成し、そこにさらに思考エンジン名でフォルダを作成します。

<a id="markdown-思考エンジンのフォルダの配置先のフォルダ" name="思考エンジンのフォルダの配置先のフォルダ"></a>
### 思考エンジンのフォルダの配置先のフォルダ

"Document/myshogi-engines"の配下に思考エンジンのフォルダを配置します

Step 3.で書き出されたエンジン定義ファイルを思考エンジン本体があるフォルダに移動させます。このファイル名は"engine_define.txt"固定です。変更しないでください。

例えば、Aperyであるなら、
myshogi-engines/Apery/
のようにフォルダを作成します。

※　ドキュメントフォルダは、Windows 10では、普通は、"C:\Users\ユーザー名\Documents"です。

そのフォルダのなかに各CPU向けの実行ファイルを配置します。
配置するときにファイル名をどのCPUに対応しているかをつけた名前に適宜変更します。

以下に代表的なソフトでどのように名前を変更すれば良いか書いておきます。

例) やねうら王系の場合。例として、ilqhaというソフトだとします。最初から以下のようなファイル名で配布されていることが多いです。その場合は、ファイル名の変更は必要なく、Step 2.のエンジンファイル名のところを書き換えるだけで良いです。
myshogi-engines/ilqha/ilqha_avx2.exe       ← 64bit版 AVX2以上用
myshogi-engines/ilqha/ilqha_sse4_2.exe    ← 64bit版 SSE4.2以上用
myshogi-engines/ilqha/ilqha_sse4_1.exe    ← 64bit版 SSE4.1以上用
myshogi-engines/ilqha/ilqha_sse2.exe        ← 64bit版 SSE2以上用
myshogi-engines/ilqha/ilqha_no_sse.exe    ← 32bit版

例) Aperyの場合
myshogi-engines/apery/apery_avx2.exe       ← 64bit版 AVX2以上用
myshogi-engines/apery/apery_sse4_2.exe    ← 64bit版 SSE4.2以上用
myshogi-engines/apery/apery_sse4_1.exe    ← 64bit版 SSE4.1以上用
myshogi-engines/apery/apery_sse2.exe        ← 64bit版 SSE2以上用
myshogi-engines/apery/apery_no_sse.exe    ← 32bit版

例) 技巧２の場合
myshogi-engines/gikou2/gikou2_sse4_2.exe ← 64bit版 SSE4.2以上用 : 技巧２は、SSE4.2以上用しか配布されていないので、この名前に変更すると良いです。

例) 脊尾詰めの場合
myshogi-engines/seotsume/seotsume_nosse.exe ← 32bit版 : 脊尾詰めは、32bit版しか配布されていないのでこの名前に変更すると良いです。

例) なのは詰めの場合
myshogi-engines/NanohaTsume/NanohaTsumeUSI_sse4_1.exe ← 64bit版SSE4.1以上用 : なのは詰め、64bit版(元ファイル名 "NanohaTsumeUSIx64_popcnt.exe")
myshogi-engines/NanohaTsume/NanohaTsumeUSI_sse2.exe ← 64bit版SSE2以上用 : なのは詰め、64bit版(元ファイル名 "NanohaTsumeUSIx64.exe")
myshogi-engines/NanohaTsume/NanohaTsumeUSI_nosse.exe ← 32bit版 : なのは詰め、32bit版(元ファイル名 "NanohaTsumeUSI.exe")

例) それ以外の場合、例えば、64bit版のSSE4.1用の実行ファイルしか配布されていない場合
myshogi-engines/usi_engine/usi_engine_sse4_1.exe ← 64bit版SSE4.1以上用 : このように、ファイル名の末尾に「sse4_1」とつけて、かつ、Step 2.のチェックボックスでSSE4.2にだけチェックを入れてください。
なお、この実行ファイルは、SSE4.2命令をサポートしたCPUでしか動作しません。

集合の関係で言うと、以下のようになっており、例えば、AVX2命令をサポートしているCPUは、SSE2, SSE4.1, SSE4.2の命令もサポートしています。
　　no_sse ⊂ SSE2 ⊂ SSE4.1 ⊂ SSE4.2 ⊂ AVX2 ⊂ AVX512


Step 3.で書き出したファイルも同じフォルダに配置します。
myshogi-engines/apery/engine_define.xml

あと、バナー画像も用意してあげると、思考エンジンを選択するときにそれが表示されます。(なければ"NO BANNER"と表示されるだけです。なくても動作には問題ありません。)

バナー画像のファイル名は"banner.png"固定です。画像サイズは、横512px × 縦160pxでPNG形式で用意します。
myshogi-engines/apery/banner.png

思考エンジンが用いる評価関数は、その思考エンジンが読み込めるように配置します。例えば、Aperyであれば、同じフォルダのなかに"eval"というフォルダを作成してそこに配置するはずです。(各思考エンジンに合わせた配置の仕方をしてください。)
myshogi-engines/apery/eval/…

将棋神やねうら王/MyShogiでは、起動時にこのmyshogi-enginesというフォルダ配下のengine_define.xmlを探して、このファイルがあれば、そのフォルダを思考エンジンが格納されているフォルダとみなします。
なので、以上のように配置し、将棋神やねうら王を起動すれば思考エンジン選択の画面に表示されるはずです。