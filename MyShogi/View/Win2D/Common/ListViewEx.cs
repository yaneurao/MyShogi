using System;
using System.Drawing;
using System.Windows.Forms;

namespace MyShogi.View.Win2D
{
    /// <summary>
    /// 1) DoubleBuffer = trueなListView
    /// 2) focusを失うと選択行がわからなくなるのでLeaveに対して選択行の背景を青にするコードを入れたもの。
    /// </summary>
    public class ListViewEx : ListView
    {
        public ListViewEx()
        {
            // このフラグはprotectedなので変更するにはListView派生クラスを作るしかない。
            DoubleBuffered = true;

            // HideSelection = trueにして、
            // focusを移動したときに自前で背景色を変更するの、EnterとLeaveをハンドルするだけでは、
            // DockWindowのようなときに、EnterもLeaveも飛んでこない。
            // 結論的には、OwnerDrawにする必要がありそう。

            // cf. ListView OwnerDrawの既定の実装 : https://stackoverrun.com/ja/q/2363016
            // cf. オーナー描画	: http://jsworld.jp/surasura/mgh?contentid=prg003p004

            OwnerDraw = true;

            // OwnerDrawなので関係ない(はず)
            HideSelection = false;

            DrawColumnHeader += MyListView_DrawColumnHeader;
            DrawSubItem += MyListView_DrawSubItem;
            // DrawItemとDrawSubItemは描画範囲が重複しているので片側のみで良い。

        }

        private void MyListView_DrawColumnHeader(object sender, DrawListViewColumnHeaderEventArgs e)
        {
            // OwnerDraw時でもデフォルトの描画でOkな場合、こうしておくだけで良い。
            e.DrawDefault = true;
        }

        private void MyListView_DrawSubItem(object sender, DrawListViewSubItemEventArgs e)
        {
            // 描画しようとしている項目が選択されているのかどうかにより、描画色を変更する。
            var item = e.Item;
            var subItem = e.SubItem;
            if (item.Selected)
            {
                // 選択されているなら背景色を青に
                using (var brush = new SolidBrush(SystemColors.Highlight))
                {
                    e.Graphics.FillRectangle(brush, e.Bounds);
                }
                // 描画文字色を白に
                subItem.ForeColor = SystemColors.HighlightText;
            }
            else
            {
                // 通常の色で問題ない
                subItem.ForeColor = Color.Empty;
            }

            // 列ヘッダで指定されているalignを反映させる必要がある。

            var align = Columns[e.ColumnIndex].TextAlign;

#if false
            // NoPaddingを指定しないと右に1文字分ほどずれる。
            // cf. DrawString()で描画すると、TextOut()に比べて開始位置が右にずれる。
            // https://social.msdn.microsoft.com/Forums/vstudio/ja-JP/a9f5c0cc-6158-4dbe-96c6-5c09852cbb67/drawstring12391255513001112377124271239212289textout123952760412409?forum=csharpgeneralja
            var format =
                align == HorizontalAlignment.Left ? (TextFormatFlags.Left | TextFormatFlags.NoPadding) :
                align == HorizontalAlignment.Center ? (TextFormatFlags.HorizontalCenter | TextFormatFlags.NoPadding) :
                /*align == HorizontalAlignment.Right ? */ (TextFormatFlags.Right | TextFormatFlags.NoPadding);

            // Centerは、列の幅が足りないときにあぶれてしまうので、なるべくなら使わないほうが良いと思う。

            e.DrawText(format);
#endif
            // →　どうやっても1文字分ほどずれる
            // DrawStringで描画するしかない。

            var format =
                align == HorizontalAlignment.Left ? StringAlignment.Near :
                align == HorizontalAlignment.Center ? StringAlignment.Center :
                /*align == HorizontalAlignment.Left ? */ StringAlignment.Far;

            var stringFormat = new StringFormat();
            stringFormat.Alignment = format;
            stringFormat.LineAlignment = StringAlignment.Center;
            stringFormat.FormatFlags = StringFormatFlags.NoWrap; // 折り返されるとListViewの1行に収まらなくて迷惑
            using (var brush = new SolidBrush(subItem.ForeColor))
            {
                e.Graphics.DrawString(subItem.Text, Font, brush, e.Bounds, stringFormat);
            }
        }
    }
}
