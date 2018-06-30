using System.Collections.Generic;

/*
    【形勢グラフControlの仕様】
 
    形勢グラフControlは、
    public void UpdateGraphData(PropertyChangedEventArgs args)
    というイベントハンドラを持ち、
    var data = args.value as EvaluationGraphData;
    として、この値に基づいて形勢グラフを描画する。
 
    このイベントハンドラはUIスレッドからしか呼び出されないものとする。
 
    このハンドラが呼び出された時、再描画され、そのとき、
    EvaluationGraphData.selectedIndexの場所が画面内に
    収まっていないといけないものとする。
    (そのあとユーザーが水平スクロールバーで左右にスクロールさせることはありうるが)

    あと、EvaluationGraphData.type == EvaluationGraphType.Normalであっても、
    selectedIndexの局面での評価値を勝率に変換した数字が
    左上と右上などに表示されているとわかりやすいかも知れない。

    このとき評価値から勝率への変換式は、
        勝率 = 1 / (1 + exp(-評価値/600))
    を用いるものとする。
*/

namespace MyShogi.Model.Shogi.EvaluationGraph
{
    /// <summary>
    /// 1局を通じた評価値
    /// 片側プレイヤー分
    /// </summary>
    public class GameEvalationData
    {
        /// <summary>
        /// 各局面での評価値。
        /// 値が記録されていないところは、int.MinValueになっているのでそれは描画してはならない。
        /// </summary>
        public List<int> values;
    }

    /// <summary>
    ///  評価値グラフ用に渡されるデータ
    /// </summary>
    public class EvaluationGraphData
    {
        /// <summary>
        /// data_array[0] : 先手のエンジンから返ってきた各局面の評価値
        /// data_array[1] : 後手のエンジンから返ってきた各局面の評価値
        /// 
        /// data_array[2] : 別のエンジンで検討させた時の各局面の評価値。(この要素があるとは限らない)
        /// data_array[3] : 別のエンジンで検討させた時の各局面の評価値。(この要素があるとは限らない)
        /// …
        /// 
        /// data_array.Length >= 2は保証されているものとする。
        /// </summary>
        public GameEvalationData[] data_array;

        /// <summary>
        /// 棋譜ウィンドウで現在選択されている局面(rootなら0、選ばれていないなら-1) 
        /// ここが選ばれているように見えるように描画する。(赤い縦線を入れるなど)
        /// </summary>
        public int selectedIndex;

        /// <summary>
        /// 棋譜ウィンドウの指し手の数
        /// -1 <= selectedIndex && selectedIndex < maxIndex
        /// それぞれのxに対して data_array[x].Count == maxIndex
        /// であることは保証されている。
        /// 
        /// maxIndexが大きくて画面に収まりきらない場合、水平スクロールバーを出して
        /// スクロールさせること。
        /// </summary>
        public int maxIndex;

        /// <summary>
        /// 形勢グラフをどう描画するか
        /// </summary>
        public EvaluationGraphType type;
    }

    /// <summary>
    /// 形勢グラフの描画スタイル
    /// </summary>
    public enum EvaluationGraphType
    {
        Normal,       // 普通の
        WinningRate , // 勝率での表示
        // 他、気が向いたら追加
	}
}
