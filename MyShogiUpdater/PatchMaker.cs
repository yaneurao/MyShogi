using System;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace MyShogiUpdater
{
    public static class PatchMaker
    {
        /// <summary>
        /// フォルダ2つを比較して、1つ目のフォルダになくて、
        /// 2つ目のフォルダにあるファイルのみをpatchPathにコピーしていく。
        ///
        /// 使用例)
        /// V100とV108との差分を生成する。
        ///   PatchMaker.MakePatch("V100", "V108");
        /// "V100toV108"というフォルダが生成される。
        /// </summary>
        /// <param name="sourcePath1">1つ目のフォルダ</param>
        /// <param name="sourcePath2">2つ目のフォルダ</param>
        public static void MakePatch(string sourcePath1 , string sourcePath2)
        {
            var patchPath = $"{sourcePath1}to{sourcePath2}";

            sourcePath1 = Path.GetFullPath(sourcePath1);
            sourcePath2 = Path.GetFullPath(sourcePath2);
            patchPath = Path.GetFullPath(patchPath);

            var source2 = Directory.GetFiles(sourcePath2 , "*" , SearchOption.AllDirectories);
            Directory.CreateDirectory(patchPath);

            foreach(var target2 in source2)
            {
                // このファイル、source1にあるのか？

                // 相対Pathに変換
                var relative_target2 = target2.Substring(sourcePath2.Length + 1 /* PathSeparator.Length */);
                var target1 = Path.Combine(sourcePath1, relative_target2);
                var target3 = Path.Combine(patchPath, relative_target2);

                // 同一であるかのフラグ。これがfalseならファイルをコピーする。
                bool the_same = true;
                if (!File.Exists(target1))
                {
                    // ファイルがtarget1に存在しないので新規ファイルだからtarget2からコピーしなくてはならない。
                    the_same = false;
                }
                else
                {
                    // target2が元のファイル(target1)とバイナリレベルで同一か確認する。
                    var bin1 = File.ReadAllBytes(target1);
                    var bin2 = File.ReadAllBytes(target2);

                    if (!bin1.SequenceEqual( bin2) )
                        the_same = false;
                }

                if (!the_same)
                {
                    Console.WriteLine($"copy {target2} to {target3}");

                    var dir = Path.GetDirectoryName(target3);
                    if (!Directory.Exists(dir))
                        Directory.CreateDirectory(dir);

                    File.Copy(target2, target3 , true);
                }

            }

            Console.WriteLine("done!");
        }

        /// <summary>
        /// sourceフォルダにあるファイル群をすべてtargetフォルダに上書きコピーする。
        /// </summary>
        /// <param name="source"></param>
        /// <param name="target"></param>
        public static void FolderCopy(string sourcePath , string targetPath , Action<string> copyed_file)
        {
            sourcePath = Path.GetFullPath(sourcePath);
            // target側はfull pathで取得できているはず。

            var sources = Directory.GetFiles(sourcePath, "*", SearchOption.AllDirectories);
            foreach(var s in sources)
            {
                var relative_source_path = s.Substring(sourcePath.Length + 1 /* PathSeparator.Length */);
                var target = Path.Combine(targetPath, relative_source_path);

                var dir = Path.GetDirectoryName(target);
                if (!Directory.Exists(dir))
                    Directory.CreateDirectory(dir);

                File.Copy(s, target, true);

                copyed_file(target); // コピーされたファイル。
            }

            copyed_file("Updateが完了しました!");
        }

    }
}
