using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using MyShogi.App;
using MyShogi.Model.Common.ObjectModel;
using MyShogi.Model.Common.Utility;
using MyShogi.Model.Resource.Images;
using MyShogi.Model.Shogi.Core;

namespace MyShogi.View.Win2D.Setting
{
    public partial class RichSelector : UserControl
    {
        /// <summary>
        /// GroupBox内に複数個のRaidoButtonがあり、そこから選択するためのUserControl。
        /// 画像から選択できる。
        ///
        /// 画像は100×50pxで用意すること。ただしPictureBoxに境界線が設定されているため、実際に表示される領域は
        /// その内側の98×48pxである。
        /// </summary>
        public RichSelector()
        {
            InitializeComponent();

            // この2つのControlは位置決めのために配置しているだけ。非表示にして使う。
            radioButton1.Visible = false;
            pictureBox1.Visible = false;

            // Visual Studioのデザイナーからでも一応表示できたほうが便利なのだが…。
            if (TheApp.app.DesignMode)
            {
                // 簡単ではなさそう…。
                // cf. カスタムコントロールから、デザイン時にプロジェクトのパスを取得する方法 : http://www.atmarkit.co.jp/bbs/phpBB/viewtopic.php?topic=47369&forum=7&start=16

                // まあ、いいや。とりま、このControlを貼り付けたFormを編集したいなら、各自、この部分を自分の環境に合わせて一時的に書き換えるってことで(；ω；)
                ImageFolder = @"C:\Users\yaneu\Documents\Visual Studio 2017\project\MyShogi\MyShogi\bin\Debug\image\display_setting\";

                // レジストリとか環境変数とか使うのがスマートなのかな…。
                // どちらもあまり使いたくないのだが…。

            } else
            {
                ImageFolder = "image/display_setting/";
            }

            InitViewModel();

            Disposed += OnDispose;
        }

        public class RichSelectorViewModel : NotifyObject
        {
            /// <summary>
            /// 選択されているRadioButtonの番号。
            /// 変更されるとNotifyObjectなのでイベント通知が飛ぶので、それを捕捉するなりすれば良い。
            /// </summary>
            public int Selection
            {
                get { return GetValue<int>("Selection"); }
                set { SetValue<int>("Selection", value); }
            }

            /// <summary>
            /// Bind()したときに、ラジオボタンの1つ目を このSelectionOffsetの値だけ加算した値とみなしてSelectionとbindする。
            /// </summary>
            public int SelectionOffset { get; set; }

            /// <summary>
            /// SelectionとBind()でbindするのがbool型である。
            /// falseならint型とみなす。
            /// </summary>
            public bool SelectionTypeIsBool { get; set; }

            /// <summary>
            /// この設定が有効になるのは、再起動後ですの警告を出すか
            /// </summary>
            public bool WarningRestart { get; set; }
        }

        public RichSelectorViewModel ViewModel = new RichSelectorViewModel();

        private void InitViewModel()
        {
            ViewModel.AddPropertyChangedHandler("Selection", (args) =>
            {
                // 選択されているラジオボタンの番号
                var n = (int)args.value;
                if (radioButtons == null || n < 0 || radioButtons.Length <= n)
                    return;
                var r = radioButtons[n] as RadioButton;
                if (r != null)
                    r.Checked = true;
            });
        }

        /// <summary>
        /// ViewModel.Selectionと、特定のNotifyObjectのnameをOneWayでBindする。
        /// ViewModel.SelectionTypeIsBool == true ならNotifyObjectのnameをbool型とみなす。falseならint型とみなす。
        /// 
        /// 注意)
        /// ViewModel.SelectionOffsetの値が利いてくるので注意。
        /// Bind()を呼び出す前に、ViewModel.SelectionOffsetを適切な値に設定すること。
        /// </summary>
        /// <param name="notify"></param>
        /// <param name="name"></param>
        public void Bind(NotifyObject notify , string name)
        {
            if (ViewModel.SelectionTypeIsBool)
            {
                ViewModel.AddPropertyChangedHandler("Selection", (args) =>
                {
                    notify.SetValueAndRaisePropertyChanged(name, ((int)args.value + ViewModel.SelectionOffset) != 0);
                });
                ViewModel.Selection = (notify.GetValue<bool>(name) ? 1 : 0) - ViewModel.SelectionOffset; // いま即座に値を反映させておく。
            }
            else
            {
                ViewModel.AddPropertyChangedHandler("Selection", (args) =>
                {
                    notify.SetValueAndRaisePropertyChanged(name, (int)args.value + ViewModel.SelectionOffset);
                });
                ViewModel.Selection = notify.GetValue<int>(name) - ViewModel.SelectionOffset; // いま即座に値を反映させておく。
            }
        }


        #region コントロールとしてのProperty

        /// <summary>
        /// GroupBoxの上に表示するテキスト
        /// </summary>
        public string GroupBoxTitle
        {
            get { return groupBox1.Text; }
            set { groupBox1.Text = value; }
        }

        /// <summary>
        /// それぞれの選択肢のテキスト
        /// この数だけRadioButtonを生成する。
        ///
        /// "表示する,rank_0.png,ToolTip用の説明文"のように表示名と画像ファイル名を書く。
        /// </summary>
        public string [] SelectionTexts { get; set; }

        #endregion

        #region privates

        /// <summary>
        /// 画像が格納されているフォルダ
        /// コンストラクタで設定される。
        /// </summary>
        private string ImageFolder;

        private void ResizeRadioButtons(int n)
        {
            if (radioButtons != null && radioButtons.Length == n)
                return;

            if (radioButtons != null)
            {
                foreach (var i in All.Int(radioButtons.Length))
                {
                    var r = radioButtons[i];
                    groupBox1.Controls.Remove(r);
                    r.Dispose();

                    var p = pictureBoxes[i];
                    // PictureBoxは必ず持っているとは限らない。
                    if (p != null)
                    {
                        groupBox1.Controls.Remove(p);
                        p.Dispose();
                    }

                    var img = images[i];
                    if (img != null)
                        img.Dispose();
                }
            }

            radioButtons = new Control[n];
            pictureBoxes = new Control[n];
            images = new ImageLoader[n];

            foreach (var i in All.Int(n))
            {
                var texts = SelectionTexts[i].Split(',');
                if (texts.Length < 2)
                    continue;

                var r = new RadioButton();
                var x = (pictureBox1.Width + groupBox1.Margin.Left*2) * i + Margin.Left*5;
                var rx = x  /* + radioButton1.Location.X */;
                r.Location = new Point(rx , radioButton1.Location.Y);
                r.Text = texts[0];

                var j = i; // copy for lambda's capture
                r.CheckedChanged += (sender, args) => {
                    if (r.Checked)
                    {
                        // 再起動するように警告表示
                        if (ViewModel.WarningRestart && ViewModel.Selection != j)
                            TheApp.app.MessageShow("この変更が反映するのは次回起動時です。", MessageShowType.Confirmation);

                        ViewModel.Selection = j;
                    }
                };

                // 先にCheckを変更しないと、このあとのCheckedChangedのイベントハンドラが呼び出されてしまう。
                // →　先に変更しても無駄だった。そうか…。上のハンドラのなかに
                // " && ViewModel.Selection = j "を追加する。
                r.Checked = i == ViewModel.Selection;

                radioButtons[i] = r;
                groupBox1.Controls.Add(r);

                var p = new PictureBox();
                // 引き伸ばしておく。
                p.SizeMode = PictureBoxSizeMode.StretchImage;
                var x2 = x;
                p.Location = new Point(x2 , pictureBox1.Location.Y);
                p.Size = pictureBox1.Size; // サイズは固定しておいたほうが扱いやすい
                p.Click += (sender,args) => { r.Checked = true; /* RadioButtonがクリックされたのと同等の扱いをしてやる*/ };
                p.BorderStyle = BorderStyle.FixedSingle;
                pictureBoxes[i] = p;
                groupBox1.Controls.Add(p);

                var img = new ImageLoader();
                var path = Path.Combine(ImageFolder, texts[1]);
                img.Load(path);
                images[i] = img;
                p.Image = images[i].image;
                
                // ToolTipの追加。
                if (texts.Length >= 3)
                {
                    var tips = texts[2];
                    toolTip1.SetToolTip(r, tips);
                    toolTip1.SetToolTip(p, tips);
                }
            }

            Invalidate();
        }

        /// <summary>
        /// 保持しているRadioButtonとPictureBox。
        /// SelectionTexts.Lengthの数だけ生成する。
        /// これらはgroupBox1にぶら下げて使う。
        /// </summary>
        private Control[] radioButtons;
        private Control[] pictureBoxes;
        private ImageLoader[] images;
#endregion

#region handlers
        private void RichSelector_SizeChanged(object sender, System.EventArgs e)
        {
            // サイズが変更されたら、それに合わせたGroupBoxのサイズに変更する。

            groupBox1.Size = new Size(Width - Margin.Size.Width*2 , Height - Margin.Size.Height*2);

            var n = SelectionTexts == null ? 0 : SelectionTexts.Length;
            if (n == 0)
                return;

            ResizeRadioButtons(n);

        }

        private void OnDispose(object sneder , EventArgs e)
        {
            if (images != null)
                foreach (var img in images)
                    img?.Dispose();
        }
#endregion
    }
}
