using System.Threading;
using System.Diagnostics;
using MyShogi.App;
using MyShogi.Model.Shogi.EngineDefine;
using MyShogi.Model.Shogi.Player;
using System.Collections.Generic;

namespace MyShogi.View.Win2D.Setting
{
    /// <summary>
    /// EngineOptionSettingDialogに渡すパラメーターがあまりにも複雑なので、パラメーターを構築する部分を分離してある。
    /// 対局設定ダイアログ、検討エンジン設定ダイアログ、詰将棋エンジン設定ダイアログから呼び出されることを想定している。
    /// </summary>
    public static class EngineOptionSettingDialogBuilder
    {
        public static EngineOptionSettingDialog Build(

            // 共通設定のために必要なもの
            EngineOptionsForSetting commonSettings,
            EngineConfig config,

            // 個別設定のために必要なもの
            EngineDefineEx engineDefineEx
            )
        {

            // -- エンジン共通設定

            commonSettings.IndivisualSetting = false; // エンジン共通設定
            commonSettings.BuildOptionsFromDescriptions(); // OptionsをDescriptionsから構築する。

            // エンジン共通設定の、ユーザーの選択をシリアライズしたものがあるなら、そのValueを上書きする。
            var commonOptions = config.CommonOptions;
            if (commonOptions != null)
                commonSettings.OverwriteEngineOptions(commonOptions);


            // -- エンジン個別設定

            // -- エンジン個別設定

            // 思考エンジンの個別ダイアログのための項目を、実際に思考エンジンを起動して取得。
            // 一瞬で起動～終了するはずなので、UIスレッドでやっちゃっていいや…。
            var engineDefine = engineDefineEx.EngineDefine;
            var exefilename = engineDefine.EngineExeFileName();

            var engine = new UsiEnginePlayer();
            try
            {
                engine.Engine.EngineSetting = true;
                engine.Start(exefilename);

                while (engine.Initializing)
                {
                    engine.OnIdle();
                    Thread.Sleep(100);
                    var ex = engine.Engine.Exception;
                    if (ex != null)
                    {
                        // time outも例外が飛んでくる…ようにすべき…。
                        // 現状の思考エンジンでここでタイムアウトにならないから、まあいいや…。
                        TheApp.app.MessageShow(ex.ToString());
                        return null;
                    }
                }
            }
            finally
            {
                engine.Dispose();
            }

            // エンジンからこれが取得出来ているはずなのだが。
            Debug.Assert(engine.Engine.OptionList != null);

            // エンジンからUsiOption文字列を取得

            var useHashCommand = engineDefine.IsSupported(ExtendedProtocol.UseHashCommandExtension);

            var ind_options = new List<EngineOptionForSetting>();
            foreach (var option in engine.Engine.OptionList)
            {
                //Console.WriteLine(option.CreateOptionCommandString());

                // "USI_Ponder"は無視する。
                if (option.Name == "USI_Ponder")
                    continue;

                // "USI_Hash","Hash"は統合する。
                else if (option.Name == "USI_Hash")
                {
                    // USI_Hash使わないエンジンなので無視する。
                    if (useHashCommand)
                        continue;

                    option.SetName("Hash_"); // これにしておけばあとで置換される。
                }
                else if (option.Name == "Hash")
                {
                    //Debug.Assert(useHashCommand);

                    option.SetName("Hash_"); // これにしておけばあとで置換される。
                }

                var opt = new EngineOptionForSetting(option.Name, option.CreateOptionCommandString());
                opt.Value = option.GetDefault();
                ind_options.Add(opt);
            }

            var ind_descriptions = engineDefine.EngineOptionDescriptions; // nullありうる
            if (ind_descriptions == null)
            {
                // この時は仕方ないので、Optionsの内容そのまま出しておかないと仕方ないのでは…。
                ind_descriptions = new List<EngineOptionDescription>();

                foreach (var option in ind_options)
                    ind_descriptions.Add(new EngineOptionDescription(option.Name, option.Name, null, null, option.UsiBuildString));
            }
            else
            {
                // Descriptionsに欠落している項目を追加する。(Optionsにだけある項目を追加する。)
                foreach (var option in ind_options)
                {
                    if (!ind_descriptions.Exists(x => x.Name == option.Name))
                        ind_descriptions.Add(new EngineOptionDescription(option.Name, option.Name, null, null, option.UsiBuildString));
                }

                // DescriptionにあってOptionsにない項目をOptionsに追加する。
                foreach (var desc in ind_descriptions)
                {
                    if (!ind_options.Exists(x => x.Name == desc.Name))
                        ind_options.Add(new EngineOptionForSetting(desc.Name, desc.UsiBuildString));
                }
            }

            // エンジン個別設定でシリアライズしていた値で上書きしてやる。
            var indivisualConfig = config.IndivisualEnginesOptions.Find(x => x.FolderPath == engineDefineEx.FolderPath);
            if (indivisualConfig == null)
            {
                indivisualConfig = new IndivisualEngineOptions(engineDefineEx.FolderPath);
                config.IndivisualEnginesOptions.Add(indivisualConfig);
            }
            if (indivisualConfig.Options == null)
                indivisualConfig.Options = new EngineOptionsForIndivisual();

            var ind_setting = new EngineOptionsForSetting()
            {
                Options = ind_options,
                Descriptions = ind_descriptions,
                IndivisualSetting = true, // エンジン個別設定
            };

            // エンジン共通設定の、ユーザーの選択をシリアライズしたものがあるなら、そのValueを上書きする。
            if (ind_options != null)
                ind_setting.OverwriteEngineOptions(indivisualConfig.Options , commonSettings);

            // -- ダイアログの構築

            var dialog = new EngineOptionSettingDialog();
            dialog.SettingControls(0).ViewModel.Setting = commonSettings;
            dialog.SettingControls(0).ViewModel.AddPropertyChangedHandler("ValueChanged", (args) =>
            {
                config.CommonOptions = commonSettings.ToEngineOptions();
                // 値が変わるごとに保存しておく。
            });

            dialog.SettingControls(1).ViewModel.Setting = ind_setting;
            dialog.SettingControls(1).ViewModel.AddPropertyChangedHandler("ValueChanged", (args) =>
            {
                indivisualConfig.Options = ind_setting.ToEngineOptionsForIndivisual();
            });

            return dialog;
        }
    }
}
