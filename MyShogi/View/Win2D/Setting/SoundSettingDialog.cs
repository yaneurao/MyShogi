using System;
using System.Windows.Forms;
using MyShogi.App;
using MyShogi.Model.Common.ObjectModel;

namespace MyShogi.View.Win2D.Setting
{
    public partial class SoundSettingDialog : Form
    {
        public SoundSettingDialog()
        {
            InitializeComponent();

            BindControl();
            Disposed += OnDisposed;

        }

        private void BindControl()
        {
            var config = TheApp.app.Config;
            binder.BindToInt(config, "EnableSound", checkBox1 , (e)=> groupBox1.Enabled = e);

            binder.BindToInt(config, "PieceSoundInTheGame", checkBox2);
            binder.BindToInt(config, "PieceSoundOffTheGame", checkBox3);

            // 棋譜読み上げは、商用版のみ選択可能

            if (config.CommercialVersion != 0)
            {
                binder.BindToInt(config, "ReadOutKifu", checkBox4 , (e) => groupBox2.Enabled = e);
                binder.BindToInt(config, "ReadOutGreeting", checkBox5);
                binder.BindToInt(config, "ReadOutSenteGoteEverytime", checkBox6);
                binder.BindToInt(config, "ReadOutCancelWhenGameEnd", checkBox7);

                binder.BindToInt(config, "ReadOutByoyomi", checkBox8);
            } else
            {
                groupBox2.Enabled = false;
                checkBox4.Enabled = false;
                checkBox5.Enabled = false;
                checkBox6.Enabled = false;
                checkBox7.Enabled = false;
                checkBox8.Enabled = false;
            }
        }

        private void OnDisposed(object sender, EventArgs e)
        {
            // data-bindしていたものすべてを解除する。
            binder.UnbindAll();
        }

        private ControlBinder binder = new ControlBinder();

    }
}
