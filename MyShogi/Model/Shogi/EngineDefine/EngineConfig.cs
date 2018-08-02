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
        /// (共通設定に倣わない項目のみ)
        /// </summary>
        [DataMember]
        public List<EngineOptionForIndivisual> Options;
    }

    /// <summary>
    /// 「通常対局用」「検討用」「詰将棋」用のいずれであるか。
    /// </summary>
    [DataContract]
    public enum EngineConfigType
    {
        [EnumMember]
        Normal ,        // 通常対局用

        [EnumMember]
        Consideration , // 検討用

        [EnumMember]
        Mate,           // 詰将棋用
    }

    /// <summary>
    /// エンジンの共通設定＋個別設定 
    /// </summary>
    [DataContract]
    public class EngineConfig
    {
        /// <summary>
        /// エンジンの共通設定
        /// </summary>
        [DataMember]
        public List<EngineOption> CommonOptions;

        /// <summary>
        /// 各エンジンの個別設定
        /// </summary>
        [DataMember]
        public List<IndivisualEngineOptions> IndivisualEnginesOptions;

        /// <summary>
        /// このインスタンスがs「通常対局用」「検討用」「詰将棋」用のいずれであるか。
        /// </summary>
        [DataMember]
        public EngineConfigType EngineConfigType;

        /// <summary>
        /// [UI Thread] : engine_pathに一致するエンジン設定を引っ張ってくる。
        /// なければ新規に要素を作って返す。
        /// </summary>
        public IndivisualEngineOptions Find(string engine_folder_path)
        {
            return EngineConfigUtility.Find(this, engine_folder_path);
        }

        /// <summary>
        /// 思考エンジンに渡すべきエンジンオプションの値を求める。
        /// nameで指定したoptionが存在しなければnullが返る。
        /// </summary>
        /// <param name="name">option名</param>
        /// <param name="commonSetting">共通設定</param>
        /// <param name="indSetting">個別設定</param>
        /// <param name="preset">プリセット</param>
        /// <returns></returns>
        public string GetOptionValue(string name , List<EngineOption> commonSetting ,
            IndivisualEngineOptions indSetting , List<EngineOption> preset)
        {
            string value = null;

            // 共通設定の反映
            if (commonSetting != null)
            {
                var opt = commonSetting.Find(x => x.Name == name);
                if (opt != null)
                    value = opt.Value;
            }

            // 個別設定の反映
            if (indSetting != null && indSetting.Options != null)
            {
                var opt = indSetting.Options.Find(x => x.Name == name);
                if (opt != null)
                {
                    if (!opt.FollowCommonSetting /* 共通設定に従わない */)
                        value = opt.Value;
                    else if (value == null)
                        // 共通設定、以前にあったこの項目が削除されている。この項目を共通設定に従うことは出来ない。ゆえに、ここで個別設定の値が代入されなければならない。
                        // 次回のエンジンの個別設定ダイアログを開いた時に、opt.FollowCommonSetting には falseが代入されるので辻褄は合う。
                        value = opt.Value;
                }
            }

            // プリセットの適用
            if (preset != null)
            {
                var opt = preset.Find(x => x.Name == name);
                if (opt != null)
                    value = opt.Value;
            }

            return value;
        }
    }


    /// <summary>
    /// エンジンの共通設定＋個別設定 、通常対局用と検討用と詰将棋用と…。
    /// "MyShogi.engine.xml"としてMyShogi.exeの存在するフォルダにシリアライズして書き出す。
    /// </summary>
    [DataContract]
    public class EngineConfigs
    {
        /// <summary>
        /// 通常対局用のEngineConfig
        /// </summary>
        [DataMember]
        public EngineConfig NormalConfig;

        /// <summary>
        /// 検討モード用のEngineConfig
        /// </summary>
        [DataMember]
        public EngineConfig ConsiderationConfig;

        /// <summary>
        /// 詰将棋モード用のEngineConfig
        /// </summary>
        [DataMember]
        public EngineConfig MateConfig;

        /// <summary>
        /// [UI Thread] : 保存する。例外投げるかも。
        /// 読み込む時は、EngineConfigUtility.GetEngineConfig()を用いる。
        /// </summary>
        public void Save()
        {
            EngineConfigUtility.SaveEngineConfig(this);
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
        public static EngineConfigs ReadFile(string filepath)
        {
            var config = Serializer.Deserialize<EngineConfigs>(filepath);

            // 読み込めなかったので新規に作成する。
            if (config == null)
                config = new EngineConfigs();

            // ぶら下がっているメンバがnullだといたるところでnull checkが必要になって
            // コードが書きにくいのでここでオブジェクトを生成して突っ込んでおく。
            // 空の要素をシリアライズしてデシリアライズするとnullになるので、ここで
            // 改めてnull checkが必要。

            if (config.NormalConfig == null)
                config.NormalConfig = new EngineConfig() { EngineConfigType = EngineConfigType.Normal };

            if (config.ConsiderationConfig == null)
                config.ConsiderationConfig = new EngineConfig() { EngineConfigType = EngineConfigType.Consideration };

            if (config.MateConfig == null)
                config.MateConfig = new EngineConfig() { EngineConfigType = EngineConfigType.Mate };

            void NullCheck(EngineConfig c)
            {
                // あとでMateEngine用の共通設定用のデフォルトオプションを別に用意するかも知れんが、
                // とりま、同じでいいような気もしている。
                if (c.CommonOptions == null || c.CommonOptions.Count == 0 /*前回保存時に設定忘れ*/)
                    c.CommonOptions = EngineCommonOptionsSample.CommonOptionDefault();
                else
                {
                    // この共通設定に現在存在しないオプションが混じっていると、その値によって個別設定が上書きされかねないので削除しておく。
                    var currentCommonOptions = EngineCommonOptionsSample.CommonOptionDefault();

                    var engineOptions = new List<EngineOption>();
                    foreach(var option in c.CommonOptions)
                    {
                        if (currentCommonOptions.Exists(x => x.Name == option.Name))
                            engineOptions.Add(option);
                    }
                    c.CommonOptions = engineOptions;
                }

                if (c.IndivisualEnginesOptions == null)
                    c.IndivisualEnginesOptions = new List<IndivisualEngineOptions>();
            }

            NullCheck(config.NormalConfig);
            NullCheck(config.ConsiderationConfig);
            NullCheck(config.MateConfig);

            return config;
        }

        /// <summary>
        /// [UI Thread] : EngineConfigのシリアライズを行う。
        /// 例外投げるかも。
        /// </summary>
        /// <param name="filepath"></param>
        /// <param name="config"></param>
        public static void WriteFile(string filepath , EngineConfigs config)
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
        public static EngineConfigs GetEngineConfig()
        {
            return ReadFile(config_filepath);
        }

        /// <summary>
        /// [UI Thread] : "MyShogi.engine.xml"にEngineConfigをシリアライズする。
        /// 例外を投げるかも。
        /// </summary>
        /// <param name="config"></param>
        public static void SaveEngineConfig(EngineConfigs config)
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
                var options = config.IndivisualEnginesOptions.Find(x => x.FolderPath == engine_folder_path);
                if (options == null)
                {
                    // ないので生成して返す。
                    options = new IndivisualEngineOptions(engine_folder_path);
                    config.IndivisualEnginesOptions.Add(options);
                }

                return options;
            }
        }

        /// <summary>
        /// EngineConfigTypeを日本語文字列化して返す。
        /// </summary>
        /// <param name="config"></param>
        /// <returns></returns>
        public static string Pretty(this EngineConfigType config)
        {
            switch(config)
            {
                case EngineConfigType.Normal: return "通常対局";
                case EngineConfigType.Consideration: return "検討";
                case EngineConfigType.Mate: return "詰将棋";
                default: return null;
            }
        }
    }
}
