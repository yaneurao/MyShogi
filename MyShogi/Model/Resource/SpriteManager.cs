using System.Drawing;
using MyShogi.App;
using MyShogi.Model.Shogi.Core;

namespace MyShogi.Model.Resource
{
    /// <summary>
    /// スプライトを返すbuilder
    /// </summary>
    public static class SpriteManager
    {
        /// <summary>
        /// 盤面画像
        /// </summary>
        /// <returns></returns>
        public static Sprite Board()
        {
            var srcRect = new Rectangle(0, 0, board_img_width, board_img_height);
            var image = TheApp.app.imageManager.BoardImage.image;

            return new Sprite(image, srcRect);
        }

        /// <summary>
        /// 駒画像
        /// 
        /// 盤面に配置する用。
        /// 後手の駒なら、180度回転させた画像が返る。
        /// 
        /// pc == NO_PIECEのときはnullが返る。
        /// </summary>
        /// <param name="pc"></param>
        /// <returns></returns>
        public static Sprite Piece(Piece pc)
        {
            if (pc == Shogi.Core.Piece.NO_PIECE)
                return null;

            if (pc.PieceType()!= Shogi.Core.Piece.KING && pc.IsPromote())
            {
                if (TheApp.app.config.PromotePieceColorType == 1)
                    // 赤い成駒にする。
                    // これは駒画像素材の4,5段目に書かれているのでオフセット値を加算する。
                    pc += (pc.PieceColor() == Shogi.Core.Color.BLACK) ? 3*8 /*3段下から*/ : 2*8 /* 2段下から*/; 
            }

            var srcRect = new Rectangle(
                    ((int)pc % 8) * piece_img_width,
                    ((int)pc / 8) * piece_img_height,
                    piece_img_width, piece_img_height);

            var image = TheApp.app.imageManager.PieceImage.image;

            var sprite = new Sprite(image, srcRect);

            // 移動マーカーが必要ならそれを連結スプライトにして返す。
            if (TheApp.app.config.PieceAttackImageVersion != 0)
            {
                var image2 = TheApp.app.imageManager.PieceAttackImage.image;
                var sprite2 = new Sprite(image2, srcRect);
                sprite.next = sprite2;
            }

            return sprite;
        }

        /// <summary>
        /// 着手の移動先、移動元などのエフェクト画像の取得。
        /// ImageManager.PieceMoveImgに格納されている画像。
        /// pc = 左から何番目の画像か。
        /// </summary>
        /// <param name="pc"></param>
        /// <returns></returns>
        public static Sprite PieceMove(PieceMoveEffect pe , Piece pc = Shogi.Core.Piece.NO_PIECE)
        {
            // 効果を「オフ」にしているならnull spriteを返してやる。
            var config = TheApp.app.config;
            switch(pe)
            {
                case PieceMoveEffect.To:
                    if (config.LastMoveToColorType == 0) return null;
                    break;
                case PieceMoveEffect.From:
                    if (config.LastMoveFromColorType == 0) return null;
                    break;
                case PieceMoveEffect.PickedFrom:
                    if (config.PickedMoveFromColorType == 0) return null;
                    break;
                case PieceMoveEffect.PickedTo:
                    if (config.PickedMoveToColorType == 0) return null;
                    break;
            }

#if false
            // シャドウの表現
            if ((pe == PieceMoveEffect.From && config.LastMoveFromColorType == 4)
                || (pe == PieceMoveEffect.PickedTo && config.PickedMoveToColorType == 6))
            {
                // 盤面反転しているときは相手の駒にしないといけない。
                // if (reverse) pc = pc.Flip();

                var srcRect2 = new Rectangle(
                ((int)pc % 8) * piece_img_width,
                ((int)pc / 8) * piece_img_height,
                piece_img_width, piece_img_height);

                var image2 = TheApp.app.imageManager.PieceShadowImage.image;

                return new Sprite(image2, srcRect2);
            }
#endif

            var srcRect = new Rectangle(
            ((int)pe % 8) * piece_img_width,
            ((int)pe / 8) * piece_img_height,
            piece_img_width, piece_img_height);

            var image = TheApp.app.imageManager.PieceMoveImage.image;

            return new Sprite(image, srcRect);
        }

        /// <summary>
        /// 同一種の手駒の枚数のための数字
        /// 
        /// 例えば、count == 1なら「1」という数字が返る。
        /// </summary>
        /// <param name="count"></param>
        /// <returns></returns>
        public static Sprite HandNumber(int count)
        {
            var srcRect = new Rectangle(48 * (count - 1), 0, 48, 48);

            var image = TheApp.app.imageManager.HandNumberImage.image;

            return new Sprite(image, srcRect);
        }

        /// <summary>
        /// 盤面の上に表示する「筋」を表現する数字画像
        /// </summary>
        /// <param name="reverse"></param>
        /// <returns></returns>
        public static Sprite BoardNumberFile(bool reverse)
        {
            var img = TheApp.app.imageManager;
            var file_img = (!reverse) ? img.BoardNumberImageFile.image : img.BoardNumberImageRevFile.image;
            var srcRect = new Rectangle(0, 0, file_img.Width, file_img.Height); // 画像丸ごとなので大きさのことは知らん。

            return new Sprite(file_img, srcRect);
        }

        /// <summary>
        /// 盤面の上に表示する「段」を表現する数字画像
        /// </summary>
        /// <param name="reverse"></param>
        /// <returns></returns>
        public static Sprite BoardNumberRank(bool reverse)
        {
            var img = TheApp.app.imageManager;
            var rank_img = (!reverse) ? img.BoardNumberImageRank.image : img.BoardNumberImageRevRank.image;
            var srcRect = new Rectangle(0, 0, rank_img.Width, rank_img.Height); // 画像丸ごとなので大きさのことは知らん。

            return new Sprite(rank_img, srcRect);
        }

        /// <summary>
        /// PromoteDialogを連結スプライトにして返す。(駒も含めて)
        /// pc = 成っていない駒。
        /// </summary>
        /// <param name="select"></param>
        /// <returns></returns>
        public static Sprite PromoteDialog(PromoteDialogSelectionEnum select , Piece pc)
        {
            var img = TheApp.app.imageManager;

            // その素材にhoverされているときはx座標にこのオフセット値を加算する
            var hover_offset = promote_dialog_cancel_rect.Width;

            // 駒を画像に対してセンタリングするためのmargin
            var piece_margin = new Size(
                (promote_dialog_promote_rect.Width - piece_img_width) / 2 ,
                (promote_dialog_promote_rect.Height - piece_img_height) / 2

                );


            // -- 成り
            var rect = promote_dialog_promote_rect;
            if (select == PromoteDialogSelectionEnum.PROMOTE)
                rect.X += hover_offset;
            var sprite = new Sprite(img.PromoteDialogImage.image, rect);
            var first_sprite = sprite; // 最後にこれをreturnで返す
            sprite.next = new Sprite(Piece(Util.MakePiecePromote(Shogi.Core.Color.BLACK, pc)), piece_margin);
            if (sprite.next != null)
                sprite = sprite.next;

            // -- 不成
            rect = promote_dialog_unpromote_rect;
            var dstOffset = new Size(rect.X,rect.Y);
            if (select == PromoteDialogSelectionEnum.UNPROMOTE)
                rect.X += hover_offset;

            sprite.next = new Sprite(img.PromoteDialogImage.image, rect , dstOffset);
            sprite = sprite.next;
            sprite.next = new Sprite(Piece(pc), dstOffset + piece_margin);
            if (sprite.next != null)
                sprite = sprite.next;

            // -- キャンセルボタン

            rect = promote_dialog_cancel_rect;
            dstOffset = new Size(rect.X, rect.Y);
            if (select == PromoteDialogSelectionEnum.CANCEL)
                rect.X += hover_offset;
            sprite.next = new Sprite(img.PromoteDialogImage.image, rect , dstOffset);

            return first_sprite;
        }

        /// <summary>
        /// 成り・不成のダイアログを(0,0)に描画したとして、そのときに座標pが
        /// どこのパーツに属するかを返す
        /// </summary>
        /// <param name="p"></param>
        /// <returns></returns>
        public static PromoteDialogSelectionEnum IsHoverPromoteDialog(Point p)
        {
            if (promote_dialog_promote_rect.Contains(p))
                return PromoteDialogSelectionEnum.PROMOTE;
            if (promote_dialog_unpromote_rect.Contains(p))
                return PromoteDialogSelectionEnum.UNPROMOTE;
            if (promote_dialog_cancel_rect.Contains(p))
                return PromoteDialogSelectionEnum.CANCEL;
            return PromoteDialogSelectionEnum.NO_SELECT;
        }

        /// <summary>
        /// 通常の駒台時の手番画像
        /// </summary>
        /// <returns></returns>
        public static Sprite TurnNormal()
        {
            // 非表示ならnull spriteを返す。
            if (TheApp.app.config.TurnDisplay == 0)
                return null;

            var image = TheApp.app.imageManager.TurnNormalImage.image;
            var sprite = new Sprite(image);
            return sprite;
        }

        /// <summary>
        /// 細長いの駒台時の手番画像
        /// </summary>
        /// <returns></returns>
        public static Sprite TurnSlim(Shogi.Core.Color sideToMove,bool reverse)
        {
            // 非表示ならnull spriteを返す。
            if (TheApp.app.config.TurnDisplay == 0)
                return null;

            // 「後」　　　　「先」
            // みたいなスプライトを返す。
            // ・手番のないほうを薄暗い素材に。
            // ・reverseがtrueなら先後入れ替え。

            var image = TheApp.app.imageManager.TurnSlimImage.image;
            int w = 60;
            var srcRect1 = new Rectangle(0, 0, w, w);                   // 後手素材
            var srcRect2 = new Rectangle(turn_slim_width - w, 0, w, w); // 先手素材
            if (sideToMove == Shogi.Core.Color.BLACK)
                srcRect1.Y = turn_slim_height - w; // 後手側を手番がない素材に変更
            else
                srcRect2.Y = turn_slim_height - w; // 先手側を手番がない素材に変更

            if (reverse)
                swap(ref srcRect1, ref srcRect2);

            var sprite1 = new Sprite(image, srcRect1);
            var sprite2 = new Sprite(image, srcRect2, new Size(turn_slim_width - w, 0));
            sprite1.next = sprite2;

            return sprite1;
        }

        /// <summary>
        /// 細長い駒台時のネームプレート素材
        /// </summary>
        /// <param name="sideToMove"></param>
        /// <param name="reverse"></param>
        /// <returns></returns>
        public static Sprite NamePlateSlim(Shogi.Core.Color sideToMove, bool reverse)
        {
            // TurnSlim()と同様

            var image = TheApp.app.imageManager.TurnSlimImage.image;
            int w = 60;
            int center = turn_slim_width / 2;
            int width = center - w; // 横幅

            var srcRect1 = new Rectangle(w      , 0, width , w); // 後手素材
            var srcRect2 = new Rectangle(center , 0, width , w); // 先手素材

            if (reverse)
                sideToMove = sideToMove.Not();

            if (sideToMove == Shogi.Core.Color.BLACK)
                srcRect1.Y = turn_slim_height - w; // 後手側を手番がない素材に変更
            else
                srcRect2.Y = turn_slim_height - w; // 先手側を手番がない素材に変更

            var sprite1 = new Sprite(image, srcRect1, new Size(w     , 0));
            var sprite2 = new Sprite(image, srcRect2, new Size(center, 0));
            sprite1.next = sprite2;

            return sprite1;

        }

        /// <summary>
        /// エンジンの初期化時のダイアログ
        /// </summary>
        /// <returns></returns>
        public static Sprite EngineInit()
        {
            var image = TheApp.app.imageManager.EngineInitImage.image;
            return new Sprite(image);
        }

        /// <summary>
        /// a,bの入れ替え
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="a"></param>
        /// <param name="b"></param>
        private static void swap<T> (ref T a , ref T b)
        {
            T t = a;
            a = b;
            b = t;
        }

        /// <summary>
        /// 成り・不成のダイアログの成りのほうのrect
        /// </summary>
        private static Rectangle promote_dialog_promote_rect = new Rectangle(0 , 0, 103, 124);

        /// <summary>
        /// 成り・不成のダイアログの不成のほうのrect
        /// </summary>
        private static Rectangle promote_dialog_unpromote_rect = new Rectangle(103, 0, 102, 124);

        /// <summary>
        /// 成り・不成のダイアログのキャンセルのrect
        /// </summary>
        private static Rectangle promote_dialog_cancel_rect = new Rectangle(0, 124, 205, 39);


        // -- 以下、画像絡みの定数。
        // これはMainDialogConst.csのほうにも同様の定義がある。

        // 盤面素材の画像サイズ
        public static readonly int board_img_width = 1920;
        public static readonly int board_img_height = 1080;

        // 駒素材の画像サイズ(駒1つ分)
        // これが横に8つ、縦に4つ、計32個並んでいる。
        public static readonly int piece_img_width = 97;
        public static readonly int piece_img_height = 106;

        /// <summary>
        /// 手番素材、駒台が細長い用
        /// </summary>
        public static readonly int turn_slim_width = 1057 ;
        public static readonly int turn_slim_height = 157 ;

    }
}
