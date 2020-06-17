using System;
using System.Drawing;
using System.Media;
using System.Windows.Forms;

namespace postListen
{
    public partial class Bildirim : Form
    {
        SoundPlayer sp;
        public Bildirim(string yazar, string zaman, string konuAdi, bool sesli_mi, int gozukme_Suresi)
        {
            InitializeComponent();
            timer1.Interval = gozukme_Suresi * 1000;
            if (sesli_mi == true) { sp = new SoundPlayer("notify.wav"); sp.Play(); }
            Screen ekran = Screen.FromPoint(Location);
            Location = new Point(ekran.WorkingArea.Right - Width, ekran.WorkingArea.Bottom - Height - Form1.topOf);
            label2.Text += " " + yazar;
            label3.Text += " " + zaman;
            label4.Text += " " + konuAdi;
        }

        private void timer1_Tick(object sender, EventArgs e)
        {            
            Close();
        }

        private void Bildirim_FormClosing(object sender, FormClosingEventArgs e)
        {
            Form1.topOf -= 250;
            if (sp != null) { sp.Stop(); sp.Dispose(); }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Close();
        }
    }
}
