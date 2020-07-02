using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Forms;

namespace postListen
{
    public partial class Form1 : Form  //SagoistCoding//
    {
        public Form1()
        {
            InitializeComponent();
        }
        Dictionary<string, Thread> openedThread = new Dictionary<string, Thread>();
        public static int topOf = 0;
        private string clear(string input)
        {
            //string doubleQuote = '"'.ToString();
            string charList = File.ReadAllText("regular chars.txt"); //Kullanıcının sonradan editleyebilmesi için dosyadan okuyoruz.
            //@"'<>!£#^+$.%½&:/{([)]=}?*\-_|@é~;,`qwertyuıopğüasdfghjklşizxcvbnmöç 1234567890" + doubleQuote;
            foreach (char c in input) //başlıktaki emojileri temizliyoruz. Temizleme işlemini html dökümanında da yapıyoruz aşağıda.
            {
                if (charList.ToArray().Contains(Convert.ToChar(c.ToString().ToLower())) == false)
                {
                    input = input.Replace(" " + c.ToString() + " ", " ");
                    input = input.Replace(" " + c.ToString(), " ");
                    input = input.Replace(c.ToString(), "");
                }
            }
             return input.Trim();
        }
        private void konuListen(string baslik, string kategoriLinki)
        {
            /*
            GeckoFx'den daha fazla uğraştırıyor ama proje boyutu da küçük oluyor, bu yüzden htmlagilitypack
            kullanıyorum bazı projelerde. Gecko kullansaydım daha az uğraşırdım ama proje boyutu artardı.
            */
            baslik = clear(baslik);
            string istatistikler = "";
            string require = "";
            bool bulundu = false;
            string url = kategoriLinki;
            string yazar = "";
            string zaman = "";           
            HtmlWeb web = new HtmlWeb();
            web.OverrideEncoding = Encoding.Default;
            while (true)
            {
                istatistikler = ""; require = ""; bulundu = false;
                HtmlAgilityPack.HtmlDocument doc = web.Load(url);
                foreach (HtmlNode node in doc.DocumentNode.SelectNodes("//tr"))
                {
                    try
                    {
                        foreach (var cocuk in node.ChildNodes)
                        {
                            ListViewItem lvi = listView1.Items.Cast<ListViewItem>().Where(items => clear(items.Text) == baslik).First();
                            require = cocuk.GetAttributeValue("id", "").ToString();
                            if (string.IsNullOrWhiteSpace(require) == false)
                            {
                                if (require.Contains("td_threadtitle"))
                                {
                                    for (int i = 0; i < cocuk.ChildNodes.Count; i++)
                                    {
                                        foreach (var h in cocuk.ChildNodes[i].ChildNodes)
                                        {
                                            string innereText = h.InnerText;
                                            innereText = Regex.Replace(innereText, " &#[0-9]+; ", " ").Trim();
                                            innereText = Regex.Replace(innereText, " &#[0-9]+;", " ").Trim();
                                            innereText = Regex.Replace(innereText, "&#[0-9]+;", "").Trim();
                                            if (baslik == innereText) //Emoji kodlarını replace ediyoruz.
                                            {
                                                bulundu = true;
                                                break;
                                            }
                                        }
                                        if (bulundu == true) { break; }
                                    }

                                }
                            }
                            else
                            {
                                istatistikler = cocuk.GetAttributeValue("title", "").ToString();
                                if (string.IsNullOrWhiteSpace(istatistikler) == false)
                                {
                                    if (bulundu == true)
                                    {
                                        yazar = cocuk.ChildNodes[1].ChildNodes[3].InnerText.Trim();
                                        zaman = cocuk.ChildNodes[1].ChildNodes[0].InnerText.Trim();
                                        lvi.SubItems[4].Text = yazar + " & " + zaman;
                                        string[] cimbiz = istatistikler.Split(',');
                                        string comments = cimbiz[0].Replace(" ", "").Replace("Cevaplar:", "").Trim();
                                        string goruntulenme = cimbiz[1].Replace(" ", "").Replace("Görüntüleme:", "").Trim();
                                        if (lvi.SubItems[2].Text != "")
                                        {
                                            if (int.Parse(comments) > int.Parse(lvi.SubItems[2].Text))
                                            {
                                                if (checkBox3.Checked == false)
                                                {
                                                    Invoke((MethodInvoker)delegate
                                                    {
                                                      new Bildirim(yazar, zaman, lvi.Text, checkBox2.Checked, (int)numericUpDown2.Value).Show();                                                       
                                                      topOf += 250;
                                                    });
                                                }
                                                else
                                                {
                                                    if(yazar != textBox3.Text)
                                                    {
                                                        Invoke((MethodInvoker)delegate
                                                        {
                                                            new Bildirim(yazar, zaman, lvi.Text, checkBox2.Checked, (int)numericUpDown2.Value).Show();
                                                            topOf += 250;
                                                        });
                                                    }
                                                }
                                            }                                          
                                        }                                  
                                        lvi.SubItems[2].Text = comments;                                        
                                        lvi.SubItems[3].Text = goruntulenme;
                                        break;
                                    }
                                }
                            }
                        }
                    }
                    catch (Exception) { }
                    if (bulundu == true) { break; } 
                    //işimize yarayanı aldıktan sonra tüm döngülere break ekledim. Boşuna ilerlemesin döngüler.
                }
                Thread.Sleep(((int)numericUpDown1.Value) * 1000);
            }
        }
        private void button1_Click(object sender, EventArgs e)
        {
            if (checkBox1.Checked == false)
            {
                ListViewItem lvi = new ListViewItem(textBox1.Text.Trim());
                if (listView1.Items.Cast<ListViewItem>().Where(items => items.Text == lvi.Text).Count() == 0)
                {
                lvi.SubItems.Add(textBox2.Text);
                lvi.SubItems.Add("");
                lvi.SubItems.Add("");
                lvi.SubItems.Add("");           
                listView1.Items.Add(lvi);                            
                if(button2.Enabled == false)
                {
                        the = new Thread(() => konuListen(lvi.Text, lvi.SubItems[1].Text));
                        the.Start();
                        openedThread.Add(lvi.Text, the);
               }
                label1.Text = "Toplam: " + listView1.Items.Count.ToString();
               }
            }
            else
            {
                if(listView1.SelectedItems.Count > 0)
                {
                    listView1.SelectedItems[0].Text = textBox1.Text;
                    listView1.SelectedItems[0].SubItems[1].Text = textBox2.Text;
                }
                else
                {
                    MessageBox.Show("Düzenlemek için bi konu seç", "SagoistCoding");
                }
            }
        }
        Thread the;
        private void button2_Click(object sender, EventArgs e)
        {
            if (listView1.Items.Count > 0)
            {
                button2.Enabled = false;
                button3.Enabled = true;
                foreach (ListViewItem konu in listView1.Items)
                {
                    the = new Thread(() => konuListen(konu.Text, konu.SubItems[1].Text));
                    the.Start();
                    openedThread.Add(konu.Text, the);
                }
            }
            else { MessageBox.Show("Bi konu ekle","SagoistCoding"); }
        }

        private void çıkışToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Visible = !Visible;
        }

        private void çıkışToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            Environment.Exit(0);
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            Environment.Exit(0);
        }

        private void kaldırToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if(listView1.SelectedItems.Count > 0)
            {
                try
                {
                    openedThread[listView1.SelectedItems[0].Text].Abort();
                    openedThread.Remove(listView1.SelectedItems[0].Text);
                }
                catch (Exception) { }
                listView1.SelectedItems[0].Remove();
                label1.Text = "Toplam: " + listView1.Items.Count.ToString();
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            button2.Enabled = true;
            button3.Enabled = false;
            foreach (KeyValuePair<string,Thread> t in openedThread)
            {
                t.Value.Abort();
            }
            openedThread.Clear();
        }
    }
}
