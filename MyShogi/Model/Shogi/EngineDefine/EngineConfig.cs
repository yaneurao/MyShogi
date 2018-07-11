using System.Collections.Generic;
using System.Runtime.Serialization;
using MyShogi.Model.Common.Utility;

namespace MyShogi.Model.Shogi.EngineDefine
{
    /// <summary>
    /// エンジン別のオプション設定
    /// </summary>
    [DataContract]
    public class IndivisualEngineOptions
    {
        public IndivisualEngineOptions(string folderPath)
        {
            FolderPath = folderPath;
            SelectedPresetIndex = 1; // Presetの1番目を選ぶ。あるかどうかは知らん。
        }

        /// <summary>
        /// EngineDefineEx.FolderPathと同等。
        /// </summary>
        [DataMember]
        public string FolderPath;

        /// <summary>
        /// GUI上で前回選択されていたPresetsのIndex。
        /// PlayerSetting.SelectedEnginePresetの値。
        /// 0番目が「カスタム」なので1番目以降がEngineDefine.Presets[0...]と対応する。
        /// 選択がなければ-1もありうる。
        /// </summary>
        [DataMember]
        public int SelectedPresetIndex;

        /// <summary>
        /// このエンジンに対する個別設定。
        /// 共通設定をこれで上書きする。
        /// </summary>
        [DataMember]
        public EngineOptions Options;
    }

    /// <summary>
    /// エンジンの共通設定＋個別設定
    /// "MyShogi.engine.xml"としてMyShogi.exeの存在するフォルダにシリアライズして書き出す。
    /// </summary>
    [DataContract]
    public class EngineConfig
    {
        /// <summary>
        /// エンジンの共通設定
        /// </summary>
        [DataMember]
        public EngineOptions CommonOptions;

        /// <summary>
        /// 各エンジンの個別設定
        /// </summary>
        [DataMember]
        public List<IndivisualEngineOptions> IndivisualEngineOption;

        /// <summary>
        /// [UI Thread] : 保存する。例外投げるかも。
        /// 読み込む時は、EngineConfigUtility.GetEngineConfig()を用いる。
        /// </summary>
        public void Save()
        {
            EngineConfigUtility.SaveEngineConfig(this);
        }

        /// <summary>
        /// [UI Thread] : engine_pathに一致するエンジン設定を引っ張ってくる。
        /// なければ新規に要素を作って返す。
        /// </summary>
        public IndivisualEngineOptions Find(string engine_folder_path)
        {
            return EngineConfigUtility.Find(this, engine_folder_path);
        }
    }

    /// <summary>
    /// EngineConfigのシリアライズ/デシリアライズを行う補助クラス
    /// </summary>
    public static class EngineConfigUtility
    {
        /// <summary>
        /// [UI Thread] : EngineConfigのデシリアライズを行う。
        /// 例外投げない。
        /// </summary>
        /// <param name="filepath"></param>
        /// <returns></returns>
        public static EngineConfig ReadFile(string filepath)
        {
            var config = Serializer.Deserialize<EngineConfig>(filepath);

            // 読み込めなかったので新規に作成する。
            if (config == null)
                config = new EngineConfig();

            return config;
        }

        /// <summary>
        /// [UI Thread] : EngineConfigのシリアライズを行う。
        /// 例外投げるかも。
        /// </summary>
        /// <param name="filepath"></param>
        /// <param name="config"></param>
        public static void WriteFile(string filepath , EngineConfig config)
        {
            lock (config)
                Serializer.Serialize(filepath, config);
        }

        /// <summary>
        /// EngineConfigをシリアライズして保存しておくファイル名。
        /// これが実行ファイルと同じフォルダに生成される。
        /// </summary>
        public const string config_filepath = "MyShogi.engine.xml";

        /// <summary>
        /// [UI Thread] : "MyShogi.engine.xml"からEngineConfigをデシリアライズして返す。
        /// このファイルがなければ新規に生成して返す。例外は投げない。
        /// </summary>
        /// <returns></returns>
        public static EngineConfig GetEngineConfig()
        {
            return ReadFile(config_filepath);
        }

        /// <summary>
        /// [UI Thread] : "MyShogi.engine.xml"にEngineConfigをシリアライズする。
        /// 例外を投げるかも。
        /// </summary>
        /// <param name="config"></param>
        public static void SaveEngineConfig(EngineConfig config)
        {
            WriteFile(config_filepath, config);
        }

        /// <summary>
        /// [UI Thread] : engine_pathに一致するエンジン設定を引っ張ってくる。
        /// </summary>
        /// <param name="config"></param>
        /// <param name="engine_path"></param>
        /// <returns></returns>
        public static IndivisualEngineOptions Find(EngineConfig config, string engine_folder_path)
        {
            // 要素をinsertするかも知れないので一応lockしておく。
            // 呼び出すのはUI Threadのはずではあるが..
            lock(config)
            {
                // 初回、要素がなくてnullありうる。
                if (config.IndivisualEngineOption == null)
                    config.IndivisualEngineOption = new List<IndivisualEngineOptions>();

                var options = config.IndivisualEngineOption.Find(x => x.FolderPath == engine_folder_path);
                if (options == null)
                {
                    // ないので生成して返す。
                    options = new IndivisualEngineOptions(engine_folder_path);
                    config.IndivisualEngineOption.Add(options);
                }

                return options;
            }
        }
    }
}
