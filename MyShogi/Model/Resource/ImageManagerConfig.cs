using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyShogi.Model.Resource
{
    /// <summary>
    /// ImageManagerで用いる設定集
    /// </summary>
    public class ImageManagerConfig
    {
        /// <summary>
        /// 盤面の画像番号。
        /// 例)3なら
        /// "board_v3_1920_1080.png"
        /// のようなファイル名になる。
        /// </summary>
        public int BoardImageNo { get; set; } = 1;

        /// <summary>
        /// 駒の画像番号
        /// 例) 3なら
        /// "piece_v3_1920_1080.png"
        /// のようなファイル名になる。
        /// </summary>
        public int PieceImageNo { get; set; } = 1;


    }
}
