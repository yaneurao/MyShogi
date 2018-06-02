using MyShogi.ViewModel;
using MyShogi.Model.Math;
using System.Windows.Forms;

namespace MyShogi.View.Win2D
{
    /// <summary>
    /// 1つのMainDialogに対して、N個(複数)のMainDialogViewModelとbindするときに
    /// 複数個必要となるもの。これをViewModelの数だけ生成する。
    /// </summary>
    public partial class MainDialogViewInstance
    {
        /// <summary>
        /// 初期化
        /// 親フォームにこのインスタンスの持つControlを関連付けておく。
        /// </summary>
        /// <param name="parent"></param>
        public void Init(Form parent)
        {
            kifuControl = new KifuControl();
            parent.Controls.Add(kifuControl);
        }

        /// <summary>
        /// 元画像から画面に描画するときに横・縦方向の縮小率とオフセット値(affine変換の係数)
        /// Draw()で描画するときに用いる。
        /// 
        /// 盤面のN画面対応をするときにはNセット必要…。
        /// </summary>
        public AffineMatrix AffineMatrix;

        /// <summary>
        /// このViewに対応するViewModel
        /// このクラスをnewした時にViewModelのインスタンスと関連付ける。
        /// </summary>
        public MainDialogViewModel ViewModel;

        /// <summary>
        /// 棋譜ウィンドウ(埋め込み)
        /// </summary>
        public KifuControl kifuControl;
    }
}
