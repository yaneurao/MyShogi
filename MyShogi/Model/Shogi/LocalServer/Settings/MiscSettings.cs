using System.Runtime.Serialization;
using MyShogi.Model.Common.ObjectModel;

namespace MyShogi.Model.Shogi.LocalServer
{
    /// <summary>
    /// 対局設定のうちのその他の設定
    /// </summary>
    public class MiscSettings : NotifyObject
    {
        // -- DataMembers

        /// <summary>
        /// 最大手数で引き分けが有効であるか
        /// これがtrueだとMaxMovesToDrawの値が有効になる。
        /// </summary>
        [DataMember]
        public bool MaxMovesToDrawEnable
        {
            get { return GetValue<bool>("MaxMovesToDrawEnable"); }
            set { SetValue("MaxMovesToDrawEnable", value); }
        }

        /// <summary>
        /// 引き分けになる手数。この手数に達した時に引き分けになる。
        /// </summary>
        [DataMember]
        public int MaxMovesToDraw
        {
            get { return GetValue<int>("MaxMovesToDraw"); }
            set { SetValue("MaxMovesToDraw", value); }
        }

        /// <summary>
        /// 入玉ルールの設定
        ///
        /// 0 : 入玉ルールなし
        /// 1 : 24点法(CSAルール)
        /// 2 : 27点法(CSAルール)
        /// 3 : トライルール
        ///
        /// EnteringKingRule enumと同じ値。
        /// </summary>
        [DataMember]
        public int EnteringKingRule
        {
            get { return GetValue<int>("EnteringKingRule"); }
            set { SetValue("EnteringKingRule", value); }
        }

        /// <summary>
        /// 連続対局を有効にするのか
        /// </summary>
        [DataMember]
        public bool ContinuousGameEnable
        {
            get { return GetValue<bool>("ContinuousGameEnable"); }
            set { SetValue("ContinuousGameEnable", value); }
        }

        /// <summary>
        /// 連続対局の回数。(ContinuousGameEnable == trueのときのみ有効)
        /// </summary>
        public int ContinuousGame
        {
            get { return GetValue<int>("ContinuousGame"); }
            set { SetValue<int>("ContinuousGame", value); }
        }

        /// <summary>
        /// 「連続対局のときに手番を入れ替えない」
        /// </summary>
        public bool ContinuousGameNoSwapPlayer
        {
            get { return GetValue<bool>("ContinuousGameNoSwapPlayer"); }
            set { SetValue<bool>("ContinuousGameNoSwapPlayer", value); }
        }

        /// <summary>
        /// BestMoveIgnoreTimeForEngineの有効/無効の設定。
        /// </summary>
        public bool EnableBestMoveIgnoreTimeForEngine
        {
            get { return GetValue<bool>("EnableBestMoveIgnoreTimeForEngine"); }
            set { SetValue<bool>("EnableBestMoveIgnoreTimeForEngine", value); }
        }

        /// <summary>
        /// 動機)
        /// コンピューター側、指し手が速すぎて追いかけられないので、1手に必ず1秒使うようなモードが欲しい。
        /// 
        /// 解決法)
        /// コンピューターが返したbest moveに対して、x[ms] になるまでGUI側が指し手を無視する機能を用意することにした。
        /// 対局設定ダイアログの「コンピューターは1手に必ずこれだけ使う」
        ///
        /// そのパラメーターがこの変数。
        /// 単位は[ms]。0なら効果なし。負の値は許容しない。
        /// </summary>
        public int BestMoveIgnoreTimeForEngine
        {
            get { return GetValue<int>("BestMoveIgnoreTimeForEngine"); }
            set { SetValue<int>("BestMoveIgnoreTimeForEngine", value); }
        }


        // -- public members

        public MiscSettings()
        {
            MaxMovesToDrawEnable = false;
            MaxMovesToDraw = 256;
            EnteringKingRule = 2; // デフォルトでは27点法

            // デフォルトでは連続対局は無効
            ContinuousGameEnable = false;
            ContinuousGame = 100;

            EnableBestMoveIgnoreTimeForEngine = true;
            BestMoveIgnoreTimeForEngine = 0;
        }

        public MiscSettings Clone()
        {
            var clone = (MiscSettings)this.MemberwiseClone();

            return clone;
        }

#if false
        public MiscSettingsMin ToMiscSettingsMin()
        {
            return new MiscSettingsMin()
            {
                MaxMovesToDrawEnable = MaxMovesToDrawEnable,
                MaxMovesToDraw = MaxMovesToDraw,
                EnteringKingRule = EnteringKingRule,
            };
        }

        public static MiscSettings FromMiscSettingsMin(MiscSettingsMin min)
        {
            return new MiscSettings()
            {
                MaxMovesToDrawEnable = min.MaxMovesToDrawEnable,
                MaxMovesToDraw = min.MaxMovesToDraw,
                EnteringKingRule = min.EnteringKingRule,
            };
        }
#endif
    }

#if false
    [DataContract]
    public class MiscSettingsMin
    {
        [DataMember] public bool MaxMovesToDrawEnable;
        [DataMember] public int MaxMovesToDraw;
        [DataMember] public int EnteringKingRule;
    }
#endif

}
