using MyShogi.Model.Common.Math;

namespace MyShogi.View.Win2D
{
    /// <summary>
    /// 1つのMainDialogに対して、N個(複数)のMainDialogViewModelとbindするときに
    /// 複数個必要となるもの。これをViewModelの数だけ生成する。
    /// </summary>
    public partial class GameScreenControlViewModel
    {
        /// <summary>
        /// 元画像から画面に描画するときに横・縦方向の縮小率とオフセット値(affine変換の係数)
        /// Draw()で描画するときに用いる。
        /// </summary>
        public AffineMatrix AffineMatrix;

        /// <summary>
        /// ユーザー操作に対して、このViewがどういう状態にあるかを表現する変数
        /// 駒を持ち上げている状態であるだとか、王手を回避していない警告ダイアログを出すだとか
        /// </summary>
        public GameScreenControlViewState viewState { get; private set; } = new GameScreenControlViewState();

        /// <summary>
        /// 画面が汚れているか(OnDraw()を呼び出さないといけないか)のフラグ
        /// </summary>
        public bool dirty { get; set; }

        /// <summary>
        /// 残り持ち時間だけが更新されたので部分的に描画して欲しいフラグ
        /// (未実装)　あとで考える。
        /// </summary>
        //public bool dirtyRestTime { get; set; }
    }
}
