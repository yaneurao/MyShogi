using System;
using System.Diagnostics;
using System.Windows.Forms;
using MyShogi.App;
using MyShogi.Model.Common.ObjectModel;
using MyShogi.Model.Common.Utility;
using MyShogi.Model.Shogi.Core;
using MyShogi.Model.Shogi.EngineDefine;
using MyShogi.View.Win2D.Common;
using SCore = MyShogi.Model.Shogi.Core;

namespace MyShogi.View.Win2D
{
    /// <summary>
    /// 対局設定ダイアログ
    ///
    /// 注意)
    /// Visual StudioのデザイナでこのDialogを編集するときは
    ///   AutoScale = Size(96F,96F)
    /// で編集しなければならない。
    /// 
    /// high dpi環境で編集して(192F,192F)とかになっていると、
    /// 解像度の低い実行環境でダイアログの大きさが小さくなってしまう。
    /// (.NET Frameworkのhigh dpiのバグ)
    /// 
    /// </summary>
    public partial class GameSettingDialog : Form
    {
        public GameSettingDialog(MainDialog mainDialog_)
        {
            InitializeComponent();

            mainDialog = mainDialog_;

            // ViewModelのハンドラの設定
            SetHandlers();

            // TheApp.app.config.GameSettingを、このFormのControlたちとデータバインドしておく。
            BindSetting();

            Disposed += OnDisposed;
        }

        // -- screen settings

        /// <summary>
        /// ViewModelのハンドラの設定
        /// </summary>
        private void SetHandlers()
        {
            var playerSettings = new[] { playerSettingControl1, playerSettingControl2 };
            var timeSettings = new[] { timeSettingControl1, timeSettingControl2 };

            foreach (var c in All.Colors())
            {
                var playerSetting = playerSettings[(int)c].ViewModel;
                // これ真っ先に設定しないと、間違えたほうの手番側に設定を保存してしまう。
                playerSetting.Color = c;
                playerSetting.AddPropertyChangedHandler("EngineSelectionButtonClicked", (args) =>
                {
                    CreateEngineSelectionDialog(c);
                });

                // GlobalConfigの値をPlayerSettingControl.ViewModelのEngineDefineに反映させる。
                // playerSetting.Colorを設定後に呼ぶ必要がある。
                set_engine_define(c);

                // 反対側のプレイヤーに詳細設定ボタンのEnable/Disableを伝達する。
                //var playerSetting2 = playerSettings[(int)c.Not()].ViewModel;
                //playerSetting.AddPropertyChangedHandler("SettingButton", (args) => playerSetting2.SettingButton = (bool)args.value);

                // 持ち時間設定も同様にColorを反映する。
                var timeSetting = timeSettings[(int)c].ViewModel;
                timeSetting.Color = c;
            }
        }

        /// <summary>
        /// GlobalConfigの値をPlayerSettingControl.ViewModelのEngineDefineに反映させる。
        /// </summary>
        /// <param name="c"></param>
        private void set_engine_define(Color c)
        {
            var playerSettings = new[] { playerSettingControl1, playerSettingControl2 };
            var setting = TheApp.app.Config.GameSetting;

            var path = setting.PlayerSetting(c).EngineDefineFolderPath;
            playerSettings[(int)c].ViewModel.EngineDefineFolderPath = path;
        }

        /// <summary>
        /// このダイアログのControlとGlobalConfig.Settingの一部をbindしておく。
        /// </summary>
        private void BindSetting()
        {
            SuspendLayout();

            var setting = TheApp.app.Config.GameSetting;

            // -- プレイヤーごとの設定
            var playerSettings = new[] { playerSettingControl1, playerSettingControl2 };
            var timeSettings = new[] { timeSettingControl1, timeSettingControl2 };
            foreach (var c in All.Colors())
            {
                // エンジンバナー、プリセットテキストなどの反映
                set_engine_define(c);

                // 対局者設定をbindする。
                playerSettings[(int)c].Bind(setting.PlayerSetting(c));

                // 対局時間設定をbindする
                timeSettings[(int)c].Bind(setting.KifuTimeSettings.RawPlayer(c));
            }

            // -- 開始局面

            // 手合割有効か
            binder.Bind(setting.BoardSetting, "BoardTypeEnable" , radioButton5 );
            binder.Bind(setting.BoardSetting, "BoardType" , comboBox2 );

            // 現在の局面から
            binder.Bind(setting.BoardSetting, "BoardTypeCurrent" , radioButton6);

            // -- 詳細設定であるか

            var misc = setting.MiscSettings;

            // -- 後手の対局時間設定を個別にするのか

            // このチェックボックスが無効だと、それに応じてgroupBox5が無効化されなくてはならない。
            binder.Bind(setting.KifuTimeSettings, "WhiteEnable", checkBox1 , (v)=>
            {
                timeSettingControl1.ViewModel.WhiteEnable = v;
                timeSettingControl2.ViewModel.WhiteEnable = v;
            });

            // 指定手数で引き分けにする
            binder.Bind(misc, "MaxMovesToDrawEnable", checkBox4 , (v)=>
            {
                misc.MaxMovesToDrawEnable = v;
                numericUpDown11.Enabled = v;
            });

            binder.Bind(misc , "MaxMovesToDraw" , numericUpDown11);

            // 入玉ルール設定
            binder.Bind(misc, "EnteringKingRule", comboBox1);

            ResumeLayout();
        }

        /// <summary>
        /// BindSetting()したものをすべて解除する。
        /// </summary>
        private void UnbindSetting()
        {
            binder.UnbindAll();

            var playerSettings = new[] { playerSettingControl1, playerSettingControl2 };
            var timeSettings = new[] { timeSettingControl1, timeSettingControl2 };
            foreach (var c in All.Colors())
            {
                playerSettings[(int)c].Unbind();
                timeSettings[(int)c].Unbind();
            }
        }

        /// <summary>
        /// 選択ダイアログの生成
        /// </summary>
        private void CreateEngineSelectionDialog(SCore.Color c)
        {
            // 詳細設定ボタンの無効化と、このエンジン選択ダイアログを閉じる時に詳細設定ボタンの再有効化。
            using (var engineSelectionDialog = new EngineSelectionDialog())
            {
                engineSelectionDialog.InitEngineDefines(true, false);

                var playerSettings = new[] { playerSettingControl1, playerSettingControl2 };

                // エンジンを選択し、「選択」ボタンが押された時のイベントハンドラ
                engineSelectionDialog.ViewModel.AddPropertyChangedHandler("ButtonClicked", (args) =>
                {
                    // これが選択された。
                    var engineDefine = (EngineDefineEx)args.value;

                    var vm = playerSettings[(int)c].ViewModel;

                    vm.EngineDefineFolderPath = engineDefine.FolderPath;
                    vm.RaisePropertyChanged("EngineSelected", null); // CPUのradio buttonを選択。先にEngineDefineを設定してからでないとエンジン選択ダイアログがまた出てしまう。

                    Debug.Assert(engineDefine.FolderPath != null);

                    var indivisualEngine = TheApp.app.EngineConfigs.NormalConfig.Find(engineDefine.FolderPath);
                    var preset = indivisualEngine.SelectedPresetIndex;
                    if (preset < 0) // 前回未選択だと-1がありうるので0に補整してやる。
                    preset = 0;

                    var setting = TheApp.app.Config.GameSetting;
                    //setting.PlayerSetting(c).SelectedEnginePreset = preset;
                    // TwoWayでbindingしているのでこれで値変わるはずだが同じ番号が選ばれるとイベントが発生しなくて…。
                    setting.PlayerSetting(c).SetValueAndRaisePropertyChanged("SelectedEnginePreset", preset);

                    // プレイヤー表示名の変更。
                    setting.PlayerSetting(c).PlayerName = engineDefine.EngineDefine.DisplayName;

                    engineSelectionDialog.Close();
                });

                // modal dialogとして出すべき。
                FormLocationUtility.CenteringToThisForm(engineSelectionDialog, this);
                engineSelectionDialog.ShowDialog(Parent);
            }
        }

        // -- handlers

        /// <summary>
        /// 「対局開始」ボタン
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button1_Click(object sender, EventArgs e)
        {
            var gameSetting = TheApp.app.Config.GameSetting;

            //　対局条件の正当性をチェックする。
            var error = gameSetting.IsValid();
            if (error != null)
            {
                TheApp.app.MessageShow(error , MessageShowType.Error);
                return;
            }

            var gameServer = mainDialog.gameServer;

            // 設定をClone()してから渡す。(immutableにしたいため)
            gameServer.GameStartCommand(gameSetting.Clone());

            // 対局が開始するのでこのダイアログを閉じる
            this.Close();
        }

        /// <summary>
        /// 「先後入替」ボタン
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button2_Click(object sender, EventArgs e)
        {
            // 対局者氏名、エンジン、持ち時間設定を入れ替える。
            // データバインドされているはずなので、DataSourceのほうで入替えて、
            // rebindすればいいような..
            
            UnbindSetting();
            TheApp.app.Config.GameSetting.SwapPlayer();
            BindSetting();
        }

        private void OnDisposed(object sender, EventArgs e)
        {
            // data-bindしていたものすべてを解除する。
            binder.UnbindAll();
        }

        // -- privates

        /// <summary>
        /// 親ウィンドウの何かを操作しないといけないことがあるので、
        /// コンストラクタでmainDialogの参照を受け取って、ここに保持しておく。
        /// </summary>
        private MainDialog mainDialog;

        /// <summary>
        /// エンジン選択ボタンが押された時にエンジンを選ぶダイアログ。
        /// </summary>
        public EngineSelectionDialog engineSelectionDialog;

        private ControlBinder binder = new ControlBinder();
    }
}
