using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using MyShogi.App;
using MyShogi.Model.Common;
using MyShogi.Model.Common.Utility;
using MyShogi.Model.Shogi.Usi;

namespace MyShogi.Model.Shogi.EngineDefine
{
    /// <summary>
    /// EngineDefineのユーティリティ
    /// </summary>
    public static class EngineDefineUtility
    {
        /// <summary>
        /// "engine_define.xml"というファイルを読み込んで、デシリアライズする。
        /// </summary>
        /// <param name="filepath"></param>
        /// <returns></returns>
        public static EngineDefine ReadFile(string filepath)
        {
            var def = Serializer.Deserialize<EngineDefine>(filepath);

            // 読み込めなかったので新規に作成する。
            if (def == null)
                def = new EngineDefine();

            // バナーファイル名、実行ファイルのファイル名などをfull pathに変換しておく。

            var current = Path.GetDirectoryName(filepath);
            def.BannerFileName = Path.Combine(current, def.BannerFileName);
            def.EngineExeName = Path.Combine(current, def.EngineExeName);

            // presetの1つ目に「カスタム」を挿入。

            var custom_preset = new EnginePreset("カスタム", "カスタム・チューニングです。「詳細設定」の設定に従います。");
            def.Presets.Insert(0, custom_preset);

            return def;
        }

        /// <summary>
        /// ファイルにEngineDefineをシリアライズして書き出す。
        /// </summary>
        /// <param name="filepath"></param>
        /// <param name="engine_define"></param>
        public static void WriteFile(string filepath, EngineDefine engine_define)
        {
            Serializer.Serialize(filepath, engine_define);
        }

        /// <summary>
        /// 現在の環境に合わせた実行ファイル名が返る。
        /// 
        /// 例) "EngineExeName_avx2.exe"
        /// </summary>
        /// <param name="engine_define"></param>
        /// <returns></returns>
        public static string EngineExeFileName(this EngineDefine engine_define)
        {
            // 現在の環境を確定させる。
            var current_cpu = CpuUtil.GetCurrentCpu();

            // サポートしている実行ファイルのなかで、一番いいものにする。
            var cpu = CpuType.UNKNOWN;
            foreach (var c in engine_define.SupportedCpus)
                if (c <= current_cpu /* 現在のCPUで動作する*/ && cpu < c /* 一番ええやつ */)
                    cpu = c;

            return $"{ engine_define.EngineExeName }_{ cpu.ToSuffix()}.exe";
        }

        /// <summary>
        /// 実行ファイル配下のengineフォルダを調べて、"engine_define.xml"へのpathをすべて返す。
        /// </summary>
        /// <returns></returns>
        public static List<string> GetEngineDefineFiles()
        {
            var result = new List<string>();

            var current = Path.GetDirectoryName(Application.ExecutablePath);
            var engine_folder = Path.Combine(current, "engine");
            var folders = Directory.GetDirectories(engine_folder);
            foreach(var f in folders)
            {
                // このフォルダに"engine_define.xml"があれば、それを追加していく。
                var path = Path.Combine(f, "engine_define.xml");
                if (File.Exists(path))
                    result.Add(path);
            }
            return result;
        }

        /// <summary>
        /// 実行ファイル配下のengineフォルダを調べて、それぞれの"engine_define.xml"を読み込んで
        /// EngineDefineのListを返す。
        /// </summary>
        /// <returns></returns>
        public static List<EngineDefineEx> GetEngineDefines()
        {
            // 実行ファイル配下のengine/フォルダ配下にある"engine_define.xml"を列挙する。
            var def_files = GetEngineDefineFiles();

            // MyShogiの実行フォルダ。これをfilenameから削って、相対pathを得る。
            var current_path = Path.GetDirectoryName(Application.ExecutablePath);

            var list = new List<EngineDefineEx>();
            foreach (var filename in def_files)
            {
                try
                {
                    var engine_define = ReadFile(filename);
                    var relative_path = Path.GetDirectoryName(filename).Substring(current_path.Length);

                    var engine_define_ex = new EngineDefineEx()
                    {
                        EngineDefine = engine_define,
                        FolderPath = relative_path,
                    };
                    list.Add(engine_define_ex);
                } catch (Exception ex)
                {
                    TheApp.app.MessageShow($"{filename}の解析に失敗しました。\n例外名" + ex.Message , MessageShowType.Error);
                }
            }

            // EngineDefine.EngineOrderの順で並び替える。
            list.Sort((x ,y) => y.EngineDefine.DisplayOrder - x.EngineDefine.DisplayOrder);

            return list;
        }

        /// <summary>
        /// このエンジンが、あるExtenedProtocolをサポートしているかを判定する。
        /// </summary>
        /// <param name="engineDefine"></param>
        /// <param name="protocol"></param>
        /// <returns></returns>
        public static bool IsSupported(this EngineDefine engineDefine , ExtendedProtocol protocol)
        {
            return engineDefine.SupportedExtendedProtocol == null ? false :
                engineDefine.SupportedExtendedProtocol.Contains(protocol);
        }

        /// <summary>
        /// UsiEngineに渡すEngineOptionsを生成する。
        /// 
        /// ・エンジン共通設定
        /// ・エンジン個別設定
        /// 
        /// ・エンジン側から送られてきた"option"の列
        /// →　これは送られて来る前に、OptionListを設定しないといけないので使えない？
        /// →　エンジン共通設定にあるものしか設定できないので、エンジンからオプションリストをもらわないと
        /// どうにもならないのでは…。
        /// 
        /// </summary>
        /// <param name="optionList">これが改変される</param>
        /// <param name="engineDefineEx"></param>
        /// <param name="selectedPresetIndex">プリセットの番号</param>
        /// <param name="commonSetting">共通設定</param>
        /// <param name="HashSize">hashサイズ[MB] 0を指定するとoption設定に従う。AutoHashの時に呼び出し元のほうで設定する。</param>
        /// <param name="threads">スレッド数。エンジンオプションのThreadsの値は、この値で設定される。</param>
        /// <returns></returns>
        public static void SetDefaultOption(List<UsiOption> optionList, EngineDefineEx engineDefineEx, int selectedPresetIndex ,
            EngineConfig config , long hashSize , int threads , bool ponder)
        {
            var engineDefine = engineDefineEx.EngineDefine;
            var folderPath = engineDefineEx.FolderPath;

            // EnginePreset
            var index = selectedPresetIndex /* - 1 */; // EngineDefineのデシリアライズ時に0番目に「カスタム」を自動挿入している。ゆえに、このまま対応する。
            List<EngineOption> preset = null;
            if (0 <= index && index < engineDefine.Presets.Count)
            {
                preset = engineDefine.Presets[index].Options;
            }

            // 共通設定
            var commonSetting = config.CommonOptions;
            // 個別設定
            var indSetting = config.IndivisualEnginesOptions.Find(x => x.FolderPath == folderPath);

            foreach (var option in optionList)
            {
                var value = config.GetOptionValue(option.Name , commonSetting , indSetting , preset);

                // 値を変更したい場合は、この変数valueを上書きする。(最後にSetDefault(value)しているので)

                // Hashサイズの自動マネージメント
                if (option.Name == "Hash" || option.Name == "USI_Hash")
                {
                    if (hashSize != 0)
                        value = hashSize.ToString();

                    // option名を適切なoption名にrename
                    // デフォルトでは"USI_Hash" , extentionで指定されていれば"Hash"

                    var option_name = engineDefine.IsSupported(ExtendedProtocol.UseHashCommandExtension) ? "Hash" : "USI_Hash";
                    option.SetName(option_name);
                }
                // Threadsの自動マネージメント
                else if (option.Name == "Threads")
                {
                    value = threads.ToString();
                }
                // Ponder設定の反映。
                else if (option.Name == "USI_Ponder")
                {
                    value = ponder ? "true" : "false";
                }

                if (value != null)
                    option.SetDefault(value);
            }

            // スレッド数の自動マネージメントについて..
            // ponderの自動マネージメントについて..

        }

    }
}
