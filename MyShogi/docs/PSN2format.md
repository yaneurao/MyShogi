
# PSN 2 fotmat

PSNフォーマットを改良したPSN 2 フォーマットを提案します。

## PSNフォーマットの原案

PSNフォーマットの原案は以下にある。

[PORTABLE SHOGI NOTATION (PSN) SPECIFICATION](http://genedavis.com/articles/2014/05/09/psn/)

## PSNフォーマットに書かれていない表現

将棋所の書き出すPSN形式のファイルでは終局の時の表現として次のようなものがある。

- Interrupt : 中断
- Mate : 詰み
- Sennichite : 千日手(引き分け / 相手の連続王手による勝ちのとき)
- Resigns : 投了
- Jishogi : 持将棋(手数が最大手数に達したとき / 入玉宣言勝ちのとき)
- Timeup : 時間切れ


## PSNフォーマットの問題点

- ファイルのエンコードがわからない。→　将棋所ではsjisで書き出されるが、対局者名にunicodeを使いたいので、ファイルはBOMつきutf-8に固定する。
- 消費時間のところがmm:ssなので59:59までしか表現できない。これを23:59:59まで使えるように拡張する。
- 移動させる駒を出力しないといけないので面倒くさい。→　打駒以外出力しないことにする
- 駒を捕獲する指し手のときに"x"を出力しないといけなくて面倒くさい。→　出力しないことにする。
- 指し手の移動元と移動先の間に"-"を出力しないといけなくて面倒くさい。→　USIプロトコルの指し手文字列をそのまま指し手表現として使うことにする。
- 千日手引き分けと、千日手勝ち(相手による連続王手の千日手による勝ち)の区別がつかない。→　前者はRepetitionDraw,後者はRepetitionWinに変更する
- 「手数(256手)による引き分け」も宣言勝ちもどちらもJishogiになってしまう。→　前者をMaxMovesDraw、後者をDeclarationWinに変更する。
- 駒落ちの局面など局面図を指定できるマジックナンバーみたいなのが多すぎる。→　必ず初期盤面として、sfenで指定することを強制し、平手の初期局面であってもsfenで出力するものとする。
- 将棋所だと[Sente "..."]、2行目が[Gote "..."]となっているが、これを保証しないと、他のフォーマットと1行目を見て判別できない。
	- また、フォーマット内に日本語での用語を排したいので、1行目は[Black "..."]、2行目は[White "..."]と強制する。
	- 1行目に[Black "..."]とあれば、このフォーマットであると判定できる。
- token文字列、読み込み時にはcase insentive(大文字・小文字どちらでもOk)にする。(書き出し時の大文字・小文字はPSN原案に準拠)

## 持ち時間設定の読み書きの拡張

- 持ち時間設定を次のように書き出す
	- 先手の設定 → [BlackTimeSetting "{Hour},{Minute},{Second},{ByoyomiEnable},{Byoyomi},{incTimeEnable},{IncTime},{ignoreTime},{timeLimitless}"]
	- 後手の設定 → [WhiteTimeSetting "{Hour},{Minute},{Second},{ByoyomiEnable},{Byoyomi},{incTimeEnable},{IncTime},{ignoreTime},{timeLimitless}"]
	- 対局設定の持ち時間のところのそれぞれの値をそのままカンマで区切って出力。9要素。boolではなく0,1で数値として出力すること。

