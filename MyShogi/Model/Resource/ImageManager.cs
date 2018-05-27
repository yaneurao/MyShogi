using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyShogi.Model.Resource
{
    /// <summary>
    /// 画像を動的に読み込んで管理するためのクラス。
    /// 本GUI専用であって、汎用性はない。
    /// </summary>
    public class ImageManager
    {
        /// <summary>
        /// 画像ファイルの存在するフォルダ
        /// </summary>
        public static readonly string ImageFolder = "image";

        /// <summary>
        /// Configの設定に従い、画像を読み込む。
        /// </summary>
        public void Init()
        {
            BoardImg = Load($"board_v{Config.BoardImageNo}_1920_1080.png");
            PieceImg = Load($"piece_v{Config.PieceImageNo}_1920_1080.png");
        }

        /// <summary>
        /// 画像読み込みのための設定
        /// Init()を呼び出したときにこの設定に従い画像が読み込まれる。
        /// </summary>
        public ImageManagerConfig Config { set; get; }

        /// <summary>
        /// ファイル名を与えて、ImgFolderから画像を読み込む
        /// </summary>
        /// <param name="name"></param>
        private ImageLoader Load(string name)
        {
            var img = new ImageLoader();
            img.Load(Path.Combine(ImageFolder,name));
            return img;
        }

        // -- 以下、それぞれの画像

        /// <summary>
        /// 盤面
        /// </summary>
        public ImageLoader BoardImg { get; private set; }

        /// <summary>
        /// 駒画像
        /// </summary>
        public ImageLoader PieceImg { get; private set; }
    }
}
