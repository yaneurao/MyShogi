using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyShogi.Model.Resource
{
    /// <summary>
    /// SpriteManagerでPromoteDialogのために使うenum
    /// </summary>
    public enum PromoteDialogSelectionEnum : int
    {
        NO_SELECT,  // 何も選択されていない
        PROMOTE,    // 成り選択(hover)中
        UNPROMOTE , // 不成選択(hover)中
        CANCEL ,    // キャンセル選択(hover)中
    }
}
