using HLC.Properties;
using HLC.Services;
using HLC.Services.Impl;
using System;
using System.Media;
using System.Windows.Forms;

namespace HLC
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            HLCService process;
            string url = textBox1.Text;

            try
            {
                if (string.IsNullOrEmpty(url))
                {
                    process = new DailyService();
                }
                else
                {
                    process = new MatchStandaloneService(url);
                }
                process.Requester();
                SoundPlayer simpleSound = new SoundPlayer(Resources.ring);
                
                simpleSound.Play();
                MessageBox.Show("Success!");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ocorreu um erro, tente novamente.\n Erro: {ex.Message}");
            }
        }
    }
}
