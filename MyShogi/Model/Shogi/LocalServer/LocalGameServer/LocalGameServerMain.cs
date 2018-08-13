using System.Threading;
using MyShogi.App;
using MyShogi.Model.Common.ObjectModel;

namespace MyShogi.Model.Shogi.LocalServer
{
    /// <summary>
    /// 対局を管理するクラス
    /// 
    /// ・内部に棋譜管理クラス(KifuManager)を持つ
    /// ・思考エンジンへの参照を持つ
    /// ・プレイヤーからの入力を受け付けるコマンドインターフェースを持つ
    /// ・対局時間を管理している。
    /// 
    /// MainDialogにとってのViewModelの一部に相当すると考えられるが、MainDialogとは1:Nで対応するため、
    /// MainDialogViewModelとは切り離してある。
    /// 
    /// </summary>
    public partial class LocalGameServer : NotifyObject
    {
        #region 設計のガイドライン

        /*
         * 設計的には、このクラスのpropertyは、immutable objectになるようにClone()されたものをセットする。
         * このpropertyに対して、NotifyObjectの通知の仕組みを用いて、UI側はイベントを捕捉する。
         * UI側は、このpropertyを見ながら、画面を描画する。immutable objectなのでスレッド競合の問題は起きない。
         * 
         * UIからのコマンドを受け付けるのは、対局を監視するworker threadを一つ回してあり、このスレッドがコマンドを受理する。
         * このスレッドは1つだけであり、局面をDoMove()で進めるのも、このスレッドのみである。
         * ゆえに局面を進めている最中に他のスレッドがコマンドを受理してしまう、みたいなことは起こりえないし、
         * 対局時間が過ぎて、対局が終了しているのに、ユーザーからの指し手コマンドを受理してしまうというようなことも起こりえない。
         */

        /// <summary>
        /// LocalGameServerのコンストラクタ的な何か。
        /// LocalGameServerをnewした側で、このクラスのpropertyに対してAddPropertyChangedHandler()で
        /// ハンドラを設定したあとにこのStart()を呼び出すことで、worker threadが起動して、
        /// このクラスが使えるようになる。
        /// </summary>
        public void Start()
        {
            var config = TheApp.app.config;

            kifuManager.Tree.Bind("Position", this , DataBindWay.OneWay);
            kifuManager.Tree.Bind("KifuList", this , DataBindWay.OneWay);
            kifuManager.Tree.Bind("KifuListAdded"  , this , DataBindWay.OneWay);
            kifuManager.Tree.Bind("KifuListRemoved", this , DataBindWay.OneWay);

            AddPropertyChangedHandler("KifuListSelectedIndex", KifuListSelectedIndexChangedCommand);

            // 起動時に平手の初期局面が表示されるようにしておく。
            kifuManager.EnableKifuList = true;
            kifuManager.Init();
            // 開始時は、対局中ではないので、棋譜ウィンドウと同期EnableKifuListは解除しておく。
            // (現在表示されている局面より先の局面までの棋譜を棋譜ウィンドウに表示したいため)
            kifuManager.EnableKifuList = false;

            // ゲームの対局設定。GameStart()を呼び出すまでdefaultで何かを埋めておかなくてはならない。
            // 前回の対局時のものを描画するのもおかしいので、defaultのものを設定しておく。
            GameSetting = new GameSetting();

            // 開始時は、対局中でないことは保証されているのでユーザーが駒を動かせるはず。
            CanUserMove = true;

            // 初期化が終わった。この時点では棋譜は汚れていない扱いとする。
            KifuDirty = false;

            // 対局監視スレッドを起動して回しておく。
            if (!NoThread)
                new Thread(thread_worker).Start();
        }

        public void Dispose()
        {
            // 対局中であればエンジンなどを終了させる。
            Disconnect();

            // 対局監視用のworker threadを停止させる。
            workerStop = true;
        }

        #endregion


    }

}
