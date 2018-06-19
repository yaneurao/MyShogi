using System.Text;
using MyShogi.Model.Shogi.Core;
using MyShogi.Model.Shogi.Converter;

namespace MyShogi.Model.Shogi.Kifu
{
    /// <summary>
    /// kif/kif2の読み書き
    /// </summary>
    public partial class KifuManager
    {

        // Kif/KI2形式の読み込み
        private string FromKifString(string[] lines, KifuFileType kf)
        {

            return string.Empty;
        }

        private string ToKifString()
        {
            var sb = new StringBuilder();
            sb.AppendFormat("先手：", KifuHeader.PlayerNameBlack).AppendLine();
            sb.AppendFormat("後手：", KifuHeader.PlayerNameWhite).AppendLine();
            switch (Tree.rootBoardType)
            {
                case BoardType.NoHandicap:
                    sb.AppendLine("手合割：平手");
                    break;
                default:
                    sb.AppendLine(Tree.position.ToBod().TrimEnd('\r', '\n'));
                    break;
            }
            // ToDo: ここに実装する
            return sb.ToString();
        }

        private string ToKi2String()
        {
            var sb = new StringBuilder();
            sb.AppendFormat("先手：", KifuHeader.PlayerNameBlack).AppendLine();
            sb.AppendFormat("後手：", KifuHeader.PlayerNameWhite).AppendLine();
            switch (Tree.rootBoardType)
            {
                case BoardType.NoHandicap:
                    sb.AppendLine("手合割：平手");
                    break;
                default:
                    sb.AppendLine(Tree.position.ToBod().TrimEnd('\r', '\n'));
                    break;
            }
            // ToDo: ここに実装する
            return sb.ToString();
        }
    }
}
