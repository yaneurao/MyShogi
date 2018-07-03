using MyShogi.Model.Shogi.Core;
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

namespace MyShogi.Model.Shogi.Data
{
    /// <summary>
    /// 1局を通じた評価値
    /// 片側プレイヤー分
    /// </summary>
    public class GameEvaluationData
    {
        /// <summary>
        /// 各局面での評価値。
        /// 値が記録されていないところは、int.MinValueになっているのでそれは描画してはならない。
        /// </summary>
        public List<EvalValue> values;
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
        public GameEvaluationData[] data_array;

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

        /// <summary>
        /// 形勢グラフを上下反転するか
        /// </summary>
        public bool reverse;

        /// <summary>
        /// 評価値から[-1,+1]のy軸値に変換する関数の取得
        /// EvalValue.NoValue の時は float.NaN を返す
        /// </summary>
        public static System.Func<EvalValue, float> MakeEval2VertFunc(EvaluationGraphType type, bool reverse)
        {
            switch (type)
            {
                case EvaluationGraphType.Normal:
                    // 普通の
                    // reverse == false, 返り値が { -1, -0.5, 0, +0.5, +1 } の場合、それぞれ評価値 { -3000以下, -1500, 0, +1500, +3000以上 } に相当
                    return (EvalValue v) => v == EvalValue.NoValue ? float.NaN :
                        (reverse ? -1 : +1) * System.Math.Min(System.Math.Max((float)v / 3000f, -1f), +1f);
                case EvaluationGraphType.TrigonometricSigmoid:
                    // ShogiGUIの非線形
                    // 途中の定数は ±1000 の入力で ±0.5 を返す値
                    // reverse == false, 返り値が { -1, -0.5, 0, +0.5, +1 } の場合、それぞれ評価値 { -∞, -1000, 0, +1000, +∞ } に相当
                    return (EvalValue v) => v == EvalValue.NoValue ? float.NaN :
                        (reverse ? -1 : +1) * (float)(System.Math.Asin(System.Math.Atan((double)v * 0.00201798867190979486291580478906) * 2 / System.Math.PI) * 2 / System.Math.PI);
                case EvaluationGraphType.WinningRate:
                    // 勝率
                    // reverse == false, 返り値が { -1, -0.5, 0, +0.5, +1 } の場合、それぞれ勝率 { 0%, 25%, 50%, 75%, 100% } に相当
                    return (EvalValue v) => v == EvalValue.NoValue ? float.NaN :
                        (reverse ? -1 : +1) * (float)(System.Math.Tanh((double)v / 1200.0));
                default:
                    // 未定義
                    // とりあえず 0 か NaN を返す
                    return (EvalValue v) => v == EvalValue.NoValue ? float.NaN : 0f;
            }
        }

        /// <summary>
        /// 評価値から[-1,+1]のy軸値に変換する関数の取得
        /// </summary>
        public System.Func<EvalValue, float> eval2VertFunc { get => MakeEval2VertFunc(type, reverse); }
    }

    /// <summary>
    /// 形勢グラフの描画スタイル
    /// </summary>
    public enum EvaluationGraphType
    {
        Normal,       // 普通の
        WinningRate,  // 勝率での表示
        TrigonometricSigmoid, // ShogiGUIの非線形描画グラフ
        // 他、気が向いたら追加
	}
}
