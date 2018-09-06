using System.IO;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Diagnostics;

namespace MyShogi.Model.Common.Tool
{
    /// <summary>
    /// 最近使ったファイルの管理用
    /// </summary>
    [DataContract] // Listを使うので、これをつけておかねばならない。
    public class MostRecentUsedFiles
    {
        /// <summary>
        /// 保持しているファイルの数。
        /// DataContractをつけているので、この値、メンバに持たせるとややこしいことになる。
        /// </summary>
        const int file_num = 40;

        /// <summary>
        /// このファイルを使ったので、保持しているFilesを並び替える。
        /// Files[0]に来る。
        /// 
        /// path : full pathで指定。
        ///
        /// 並び替えが発生して、メニューの更新が必要ならtrueが返る。
        /// </summary>
        /// <param name="path"></param>
        public bool UseFile(string path)
        {
            Debug.Assert(Files != null);

            var index = Files.FindIndex(_ => _ == path);
            if (index == 0)
                return false; // 先頭にあるから挿入も並び替え不要

            if (index == -1)
            {
                // 見つからなかったのでFiles[0]に追加。
                Files.Insert(0, path);
                if (Files.Count > file_num)
                    Files.RemoveAt(file_num - 1);

            } else {

                // 見つかったので、それを先頭に持ってくる。
                Files.RemoveAt(index);
                Files.Insert(0, path);

            }

            return true;
        }

        /// <summary>
        /// メニューで表示する用のファイル名を取得する。
        ///
        /// 存在しない要素、範囲外の要素にアクセスしたらnullが返る。
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public string GetDisplayFileName(int index)
        {
            Debug.Assert(Files != null);

            if (Files.Count <= index)
                return null;

            var path = Files[index];
            if (path == null)
                return null;

            // "c:/.../親Directory/ファイル名"ぐらいの表示で良いと思う。
            // 最初の"/"までをまず表示。
            // network driveのときに"\\pc\..."の"\\pc\"は欲しい気もする。
            // → 2文字目以降の最初に見つかった"\"までを取得。
            var sep = Path.DirectorySeparatorChar;
            if (path.Length < 2)
                return path;

            var i = path.IndexOf(sep,2 /* 2文字目以降で*/);
            if (i == -1)
                return path; // あかん。どうなっとんねん。

            // drive letter
            var drive = path.Substring(0, i +1 /* "\"まで含めて */);

            var j = path.LastIndexOf(sep);
            if (j < 2)
                return path;
            var k = path.LastIndexOf(sep, j - 1 /* jの位置にあるsepを除いて*/);
            if (j == i || k == i || k < 2 || k == -1)
                return path; // "c:/a/1.kif" , "c:/1.kif" みたいなの。

            var parent = path.Substring(k, j - k);
            var file = Path.GetFileName(path);

            return $"{drive}..{parent}{sep}{file}";
        }

        /// <summary>
        /// Deserializeしたときにnullが突っ込まれるパターンがあるのでList使うの注意。
        /// Deserializeしたときにこのメソッドを呼び出すこと。
        /// </summary>
        public void OnDeserialize()
        {
            if (Files == null)
                Files = new List<string>(file_num);
        }

        /// <summary>
        /// 保存されているファイル
        ///
        /// DeserializeしたときにOnLoad()を呼び出すことを強制してあるので、nullにはならない。
        /// </summary>
        [DataMember]
        public List<string> Files;

    }
}
