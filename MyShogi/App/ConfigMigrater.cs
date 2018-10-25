using MyShogi.Model.Common.ObjectModel;
using MyShogi.Model.Common.String;

namespace MyShogi.App
{
    public partial class GlobalConfig : NotifyObject
    {
        #region データ構造のMigration

        /// <summary>
        /// データ構造が以前のバージョンから変化する場合、ここでそのマイグレーションを行う。
        /// phpのフレームワークとかでよくあるやつ。
        /// </summary>
        private void Migrate()
        {
            var version = Serializer.VersionStringToInt(Version);

            // Version 1.32以前
            if (version < 1320)
            {
                // 棋譜の記法に「打」が追加になったために、2番目以降を選択している場合、右に要素が一つずれる。
                if (KifuWindowKifuVersion >= 1)
                    ++KifuWindowKifuVersion;
                if (ConsiderationWindowKifuVersion >= 1)
                    ++ConsiderationWindowKifuVersion;
            }

            // 以下、if (version < XXX) { .. } をだらだら書いていく。
        }
        #endregion

    }
}
