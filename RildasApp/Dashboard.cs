using MetroFramework;
using MetroFramework.Controls;
using MetroFramework.Forms;
using RildasApp.HelpObject;
using RildasApp.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RildasApp
{
    public partial class Dashboard : MetroForm
    {
        public static Dashboard instance;
        private string filename1, filename2, safeName1, safeName2, finalName1, finalName2;
        int animeId;
        int mouseX, mouseY;
        int selectedPublish;
        EpisodeVersion selectedVersion;
        Timetable.Timetable table;
        MyPanel myPanel;
        MyPanel myPanel2;
        EventControler econtrol;
        MetroFramework.Controls.MetroButton leftButton;
        MetroFramework.Controls.MetroButton rightButton;
        Timetable.Event moveEvent;
        string[,] animes; 
        string[] selectedAnime;
        int month;
        int year;
        public Dashboard()
        {
            
            InitializeComponent();
            mouseX = 0;
            mouseY = 0;
            selectedPublish = -1;
            year = DateTime.Today.Year;
            month = DateTime.Today.Month;
            /*Testovaci část*/
            myPanel = new MyPanel();
            myPanel.Location = new Point(5, 5);
            myPanel.Name = "panel1";
            myPanel.Size = new Size(650, 600);
            myPanel.TabIndex = 3;
            myPanel.Paint += new PaintEventHandler(this.metroPanel1_Paint);
            myPanel.MouseMove += new MouseEventHandler(this.metroPanel1_MouseMove);
            myPanel.MouseClick += new MouseEventHandler(myPanel_Click);
            myPanel.MouseDown += new MouseEventHandler(myPanel_Down);
            myPanel.MouseUp += new MouseEventHandler(myPanel_Up);
            myPanel.Cursor = Cursors.Hand;
            _calendar.Controls.Add(myPanel);
            table = new Timetable.Timetable(myPanel, toolTip1, myPanel.Width - 1, myPanel.Height - 20);
            /*--------------*/
            /*Vytvořeno Danem*/
            /*Panel pro zveřejnění*/
            myPanel2 = new MyPanel();
            myPanel2.Location = new Point(19, 99);
            myPanel2.Name = "publish_panelAnime";
            myPanel2.Size = new Size(305, 460);
            myPanel2.BackColor = Color.FromArgb(17, 17, 17);
            myPanel2.TabIndex = 4;
            myPanel2.Paint += new PaintEventHandler(this.publish_panelAnime_Paint);
            myPanel2.MouseMove += new MouseEventHandler(publishPanel_MouseMove);
            myPanel2.MouseClick += new MouseEventHandler(publishPanel_MouseClick);
            myPanel2.MouseLeave += new EventHandler(publishPanel_MouseLeave);
            myPanel2.Cursor = Cursors.Hand;
            _publish.Controls.Add(myPanel2);
            /*--------------*/
            this.News.MouseWheel += News_MouseWheel;
            this.chatPanelPrivate.MouseWheel += ChatPanelPrivate_MouseWheel;
            metroScrollBar1.Scroll += MetroScrollBar1_Scroll;
            instance = this;
            for(int i = 0; i < 23; i++)
            {
                publish_cbHours.Items.Add(i);
            }
            for(int i = 0; i < 59; i++)
            {
                publish_cbMinutes.Items.Add(i);
            }


        }

        private void publishPanel_MouseLeave(object sender, EventArgs e)
        {
            mouseX = 0;
            mouseY = 0;
            myPanel2.Refresh();
        }

        public void _publishSetLabels(EpisodeVersion ep)
        {
            if (ep == null)
            {
                publish_animelabel.Text = "Anime: ";
                publish_episodelabel.Text = "Epizoda: ";
                publish_fromlabel.Text = "Připraveno ke zveřejnění od: ";
                publish_translatorlabel.Text = "Přeložil: ";
                publish_confirmerlabel.Text = "Schválil: ";
            }
            else
            {
                publish_animelabel.Text = "Anime: " + ((Global.GetAnime(ep.animeId) != null) ? Global.GetAnime(ep.animeId).name : "");
                publish_episodelabel.Text = "Epizoda: " + ep.episode;
                publish_fromlabel.Text = "Připraveno ke zveřejnění od: ";
                publish_translatorlabel.Text = "Přeložil: " + ((Global.GetUser(ep.addedBy) != null) ? Global.GetUser(ep.addedBy).username : "");
                publish_confirmerlabel.Text = "Schválil: " + ((Global.GetUser(ep.reservedBy) != null) ? Global.GetUser(ep.reservedBy).username : "");
            }
        }

        public void _publishSelectVersion()
        {
            int animId = Global.GetAnime(publish_AnimeComboBox.SelectedItem.ToString()).id;
            List<EpisodeVersion> versions = Global.GetEpisodeVersions();
            int counter = 0;
            foreach (EpisodeVersion ep in versions)
            {
                if (ep.animeId == animId && ep.state == 2)
                {
                    if (ep.animeId == selectedVersion.animeId && ep.episode == selectedVersion.episode)
                        selectedPublish = counter;
                    counter++;
                }
            }
        }

        private void publishPanel_MouseClick(object sender, MouseEventArgs e)
        {
            selectedPublish = mouseY / 45;
            int animId = Global.GetAnime(publish_AnimeComboBox.SelectedItem.ToString()).id;
            List<EpisodeVersion> versions = Global.GetEpisodeVersions();
            int counter = 0;
            foreach (EpisodeVersion ep in versions)
            {
                if (ep.animeId == animId && ep.state == 2)
                {
                    if (counter == selectedPublish)
                    {
                        _publishSetLabels(ep);
                        selectedVersion = ep;
                    }
                    counter++;
                }
            }
        }

        private void publishPanel_MouseMove(object sender, MouseEventArgs e)
        {
            mouseX = e.X;
            mouseY = e.Y;
            myPanel2.Refresh();
        }

        private void ChatPanelPrivate_MouseWheel(object sender, MouseEventArgs e)
        {
            int value;
            if (e.Delta > 0)
            {

                if (metroScrollBar2.Value - 4 >= metroScrollBar2.Minimum)
                    metroScrollBar2.Value -= 4;
                else
                    metroScrollBar2.Value = metroScrollBar2.Minimum;
            }
            else
            {
                if (metroScrollBar2.Value + 4 <= metroScrollBar2.Maximum)
                    metroScrollBar2.Value += 4;
                else
                    metroScrollBar2.Value = metroScrollBar2.Maximum;
            }
            metroScrollBar2_Scroll(this, new ScrollEventArgs(ScrollEventType.ThumbTrack, metroScrollBar2.Value));
        }

        private void Dashboard_Load(object sender, EventArgs e)
        {
            metroComboBox1.SelectedIndex = 0;
            Global.EpisodeVersionListUpdated += Global_EpisodeVersionListUpdated;
            LoadTeamMembers();
        }

        private void MetroScrollBar1_Scroll(object sender, ScrollEventArgs e)
        {
            News.VerticalScroll.Value = (News.PreferredSize.Height - News.Size.Height) < 0 ? 0 : ((News.PreferredSize.Height - News.Size.Height) * metroScrollBar1.Value) / 100;
        }

        private void News_MouseWheel(object sender, MouseEventArgs e)
        {
            int value;
            if (e.Delta > 0)
            {

                if (metroScrollBar1.Value - 4 >= metroScrollBar1.Minimum)
                    metroScrollBar1.Value -= 4;
                else
                    metroScrollBar1.Value = metroScrollBar1.Minimum;
            }
            else
            {
                if (metroScrollBar1.Value + 4 <= metroScrollBar1.Maximum)
                    metroScrollBar1.Value += 4;
                else
                    metroScrollBar1.Value = metroScrollBar1.Maximum;
            }
            MetroScrollBar1_Scroll(this, new ScrollEventArgs(ScrollEventType.ThumbTrack, metroScrollBar1.Value));
        }

        private void Dashboard_Resize(object sender, EventArgs e)
        {
            
            if (this.WindowState == FormWindowState.Minimized)
            {
                notifyIcon1.Visible = true;
                notifyIcon1.ShowBalloonTip(500);
                this.ShowInTaskbar = false;
            }
            else if (FormWindowState.Normal == this.WindowState)
            {
                notifyIcon1.Visible = false;
            }
        }
        private void notifyIcon_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            this.WindowState = FormWindowState.Normal;
            this.ShowInTaskbar = true;
            notifyIcon1.Visible = false;
        }
        private void Dashboard_FormClosing(object sender, FormClosingEventArgs e)
        {


        }


        private void metroButton1_Click(object sender, EventArgs e)
        {
            int size = -1;
            openFileDialog1.Title = "Vyberte titulky k nahrání.";
            openFileDialog1.Filter = "Titulky|*.ass|Titulky|*.srt|Vše|*.*";
            openFileDialog1.CheckFileExists = true;
            DialogResult result = openFileDialog1.ShowDialog(); // Show the dialog.
            if (result == DialogResult.OK) // Test result.
            {
                filename1 = openFileDialog1.FileName;
                string file = openFileDialog1.FileName;
                safeName1 = openFileDialog1.SafeFileName;
                labelFile.Text = openFileDialog1.SafeFileName;
                try
                {
                    string text = File.ReadAllText(file);
                    size = text.Length;
                }
                catch (IOException)
                {
                }
            }
        }
        private void metroButton1AJ_Click(object sender, EventArgs e)
        {
            int size = -1;
            openFileDialog1.Title = "Vyberte titulky k nahrání.";
            openFileDialog1.Filter = "Titulky|*.ass|Titulky|*.srt|Vše|*.*";
            openFileDialog1.CheckFileExists = true;
            DialogResult result = openFileDialog1.ShowDialog(); // Show the dialog.
            if (result == DialogResult.OK) // Test result.
            {
                filename2 = openFileDialog1.FileName;
                string file = openFileDialog1.FileName;
                safeName2 = openFileDialog1.SafeFileName;
                lbAJ2.Text = openFileDialog1.SafeFileName;
                try
                {
                    string text = File.ReadAllText(file);
                    size = text.Length;
                }
                catch (IOException)
                {
                }
            }
        }

        private void bSend_Click(object sender, EventArgs e)
        {
            if (cbAnime.SelectedItem == null || cbEpisode.SelectedItem == null || filename1 == null)
                MetroMessageBox.Show(this, "Nevyplnili jste všechny povinné údaje.", "Upozornění", MessageBoxButtons.OK, MessageBoxIcon.Information);
            else
            {
                
                int hash = (int)(DateTime.UtcNow.Subtract(new DateTime(2016, 1, 1))).TotalMilliseconds + Environment.TickCount;
                byte[] file1bytes = File.ReadAllBytes(filename1);
                RildasServerAPI.UploadFile(0, hash.ToString("X"), Encoding.UTF8.GetString(file1bytes));
                if (filename2 != null)
                {
                    byte[] file2bytes = File.ReadAllBytes(filename2);
                    RildasServerAPI.UploadFile(1, "en_" + hash.ToString("X"), Encoding.UTF8.GetString(file2bytes));
                }
                EpisodeVersion version = new EpisodeVersion()
                {
                    type = rbPreklad.Checked ? EpisodeVersion.Type.PŘEKLAD : EpisodeVersion.Type.KOREKCE,
                    animeId = animeId,
                    episode = int.Parse(cbEpisode.SelectedItem.ToString()),
                    state = cbReady.Checked ? -3 : 0,
                    title = finalName1,
                    titleEN = finalName2,
                    name = safeName1,
                    nameEN = safeName2,
                    comment = tbComment.Text,
                    timeOn = tbTime.Text,
                    addedBy = Global.loggedUser.id,
                    _hash = hash.ToString("X"),
                };
                RildasServerAPI.AddVersion(version);
                metroTabControl1.SelectedIndex = 0;
            }
        }

        private void AnimeCheck(object sender, EventArgs e)
        {
            panel3.Visible = false;
            cb2Episodes.Items.Clear();
            cbEpisode.Refresh();
            cb2Version.Items.Clear();
            cb2Version.Refresh();
            Anime selectedAnime = Global.GetAnime(cb2Anime.Text);
            if (selectedAnime == null) return;
            animeId = selectedAnime.id;
            Episode[] episodes = Global.GetEpisodes(selectedAnime.id);
            for (int i = 0; i < episodes.Length; i++)
            {
                ComboboxItem item = new ComboboxItem();
                //item.Text = currName;
                item.Value = episodes[i];           
                if (episodes[i].epState == state.Not_ready) continue;
                if (episodes[i].special) item.Text = "SP" + episodes[i].ep_number; //cb2Episodes.Items.Add("SP" + publishedEps[i].ep_number);
                else item.Text = episodes[i].ep_number.ToString();
                cb2Episodes.Items.Add(item);
            }
        }

        private void cb2Episodes_SelectedIndexChanged(object sender, EventArgs e)
        {
            cb2Version.Items.Clear();
            cb2Version.Refresh();
            panel3.Visible = false;
            Episode ep = (cb2Episodes.SelectedItem as ComboboxItem).Value as Episode;
            EpisodeVersion[] episodeVersions = Global.GetEpisodeVersions(ep.animeid, ep.ep_number, ep.special, true);

            for (int i = 0; i < episodeVersions.Length; i++)
            {
                string currName = episodeVersions[i].type + " od " + Global.GetUser(episodeVersions[i].addedBy).username + " z " + episodeVersions[i].added;
                if (episodeVersions[i].timeOn != "") currName += " [" + episodeVersions[i].timeOn + "]";
                ComboboxItem item = new ComboboxItem();
                item.Text = currName;
                item.Value = episodeVersions[i];
                cb2Version.Items.Add(item);
            }
        }
        public void DisableForm()
        {
            foreach (Control c in this.Controls)
            {
                c.Invoke(new MethodInvoker(delegate { c.Enabled = false; }));
            }

            metroLabel13.Invoke(new MethodInvoker(delegate { metroLabel13.Visible = true; }));
            metroLabel13.Invoke(new MethodInvoker(delegate { metroLabel13.Enabled = true; }));
        }
        public void EnableForm()
        {
            foreach (Control c in this.Controls)
            {
                c.Invoke(new MethodInvoker(delegate { c.Enabled = true; }));
            }
            metroLabel13.Invoke(new MethodInvoker(delegate { metroLabel13.Visible = false; }));
        }

        private void cb2Version_SelectedIndexChanged(object sender, EventArgs e)
        {
            panel3.Visible = true;
            EpisodeVersion version = (cb2Version.SelectedItem as ComboboxItem).Value as EpisodeVersion;
            tb3Comment.Text = version.comment;
            tb3Download.Tag = version;
            tb3DownloadAJ.Tag = version;
            if (version.titleEN == "") tb3DownloadAJ.Visible = false;
            else tb3DownloadAJ.Visible = true;
        }

        private void DownloadMyTag(object sender, EventArgs e)
        {
            EpisodeVersion version = (sender as MetroFramework.Controls.MetroButton).Tag as EpisodeVersion;
            SaveFileDialog saveFileDialog1 = new SaveFileDialog();
            saveFileDialog1.FileName = "[Rildas]" + Global.GetAnime(version.animeId).name.Replace(":","-") + "-" +version.episode;
            if (version.state == 1) saveFileDialog1.FileName += "[DONE]";
            saveFileDialog1.FileName += ".ass";
            saveFileDialog1.Title = "Save titles";


            // If the file name is not an empty string open it for saving.
            if (saveFileDialog1.ShowDialog() == DialogResult.OK)
            {
                if (saveFileDialog1.FileName != "")
                {
                    RildasServerAPI.DownloadFile(version, saveFileDialog1.FileName, false);
                }
            }
        }

        private void DownloadMyTagAJ(object sender, EventArgs e)
        {
            EpisodeVersion version = (sender as MetroFramework.Controls.MetroButton).Tag as EpisodeVersion;
            SaveFileDialog saveFileDialog1 = new SaveFileDialog();
            saveFileDialog1.FileName = "[Rildas]En_ " + Global.GetAnime(version.animeId).name.Replace(":", "-") + "-" + version.episode;
            if (version.state == 1) saveFileDialog1.FileName += "[ORIGINAL]";
            saveFileDialog1.FileName += ".ass";
            saveFileDialog1.Title = "Save titles";

            // If the file name is not an empty string open it for saving.
            if (saveFileDialog1.ShowDialog() == DialogResult.OK)
            {
                if (saveFileDialog1.FileName != "")
                {
                    RildasServerAPI.DownloadFile(version, saveFileDialog1.FileName, true);
                }
            }
        }

        public void MakeStates()
        {
            metroTabControl1.SelectedTab.Controls.Clear();
            const int imageSize = 204;
            const int rightPadding = 20;
            const int downPadding = 70;
            int animesInRow = (metroTabControl1.SelectedTab.Width / (imageSize + rightPadding));
            List<Anime> animeList = Global.GetAnimes();

            Panel panel = new Panel();
            panel.BackColor = Color.FromArgb(17, 17, 17);
            panel.Size = new Size(1400, 600);
            panel.AutoScroll = true;
            panel.Location = new Point(0, 0);

            metroTabControl1.SelectedTab.Controls.Add(panel);


            for (int i = 0; i < animeList.Count; ++i)
            {
                
                int positionY = (((imageSize + rightPadding) * i) / panel.Width) * (imageSize + downPadding);
                int positionX = (i % animesInRow) * (imageSize + rightPadding);
                PictureBox picture = new PictureBox();
                Label label = new Label();

                picture.Size = new Size(imageSize, imageSize);
                picture.Location = new Point(positionX, positionY);
                picture.Load("http://anime.rildas.cz/" + animeList[i].animelist_img);
                panel.Controls.Add(picture);

                label.Text = animeList[i].name + "\r\n" + "0" + "/" + animeList[i].ep_count;
                label.Location = new Point(picture.Location.X, picture.Location.Y + picture.Height + 5);
                label.Size = new Size(picture.Width, 60);
                label.ForeColor = Color.White;
                panel.Controls.Add(label);
            }
        }

        private void metroTabControl1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (metroTabControl1.SelectedIndex == 2)
            {
                cb2Anime.Items.Clear();
                List<Anime> animes = Global.GetAnimes();
                foreach (Anime anime in animes)
                {
                    if (!cb2DoneAnime.Checked && anime.status != Anime.Status.PŘEKLÁDÁ_SE) continue;
                    cb2Anime.Items.Add(anime.name);
                }
            }
            if (metroTabControl1.SelectedTab.Name == "_publish")
            {
                publish_AnimeComboBox.Items.Clear();
                foreach (Anime anime in Global.GetAnimes())
                {
                    if (anime.status != Anime.Status.PŘELOŽENO)
                    {
                        publish_AnimeComboBox.Items.Add(anime.name);
                    }
                }
            }
            if (metroTabControl1.SelectedTab.Name == "_states")
            {
                MakeStates();
            }
        }

        private void CheckAnimeList(object sender, EventArgs e)
        {
            cbAnime.Items.Clear();
            cbEpisode.Items.Clear();
            if (rbPreklad.Checked)
            {
                Anime[] animes = Global.GetAnimesOfUser(Global.loggedUser);
                //var animes = cbDoneAnime.Checked ? Global.Query("SELECT name FROM anime WHERE translator_id='" + Global.loggedUser.id + "'") : Global.Query("SELECT name FROM anime WHERE translator_id='" + Global.loggedUser.id + "' AND status='PŘEKLÁDÁ SE'");
                for (int i = 0; i < animes.Length; i++)
                {
                    if(cbDoneAnime.Checked || animes[i].status == Anime.Status.PŘEKLÁDÁ_SE) cbAnime.Items.Add(animes[i].name);
                }
                lbAJ.Visible = true;
                lbAJ2.Visible = true;
                btnAJ.Visible = true;
            }
            if (rbKorekce.Checked)
            {
                List<Anime> animes = Global.GetAnimes();// cbDoneAnime.Checked ? Global.Query("SELECT name FROM anime") : Global.Query("SELECT name FROM anime WHERE status='PŘEKLÁDÁ SE'");
                foreach(Anime anime in animes)
                {
                    if(cbDoneAnime.Checked || anime.status == Anime.Status.PŘEKLÁDÁ_SE) cbAnime.Items.Add(anime.name);
                }
                lbAJ.Visible = false;
                lbAJ2.Visible = false;
                btnAJ.Visible = false;
            }
        }

        private void cbEpisode_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void cbReady_CheckedChanged(object sender, EventArgs e)
        {
            if(cbReady.Checked == true)
            {
                DialogResult dr = MetroMessageBox.Show(this, "Při zaškrtnutí této možnosti bude překlad bez další kontroly odeslán na enkód. Jste si tím jisti?", "Enkód?", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (dr == DialogResult.No) cbReady.Checked = false;
            }
        }

        private void CheckEpisodeCount(object sender, EventArgs e)
        {
            cbEpisode.Items.Clear();
            cbEpisode.Refresh();
            Anime res = Global.GetAnime(cbAnime.Text);
            if (res == null) return;
            animeId = res.id;
            int epCount = res.ep_count;
            Episode[] episodes = Global.GetEpisodes(animeId);
            if (cbAllEpisodes.Checked)
            {
                
                for (int i = 1; i <= epCount; i++)
                {
                    cbEpisode.Items.Add(i);
                }

            }
            else
            {

                if (rbKorekce.Checked)
                {
                    foreach (Episode ep in episodes)
                    {
                        ComboboxItem item = new ComboboxItem();
                        item.Value = ep;
                        if (ep.epState == state.Not_ready) continue;
                        if (ep.special) item.Text = "SP" + ep.ep_number; 
                        else item.Text = ep.ep_number.ToString();
                        cbEpisode.Items.Add(item);
                    }

                }
                else
                {
                    foreach (Episode ep in episodes)
                    {
                        ComboboxItem item = new ComboboxItem();
                        //item.Text = currName;
                        item.Value = ep;
                        if (ep.epState != state.Not_ready) continue;
                        if (ep.special) item.Text = "SP" + ep.ep_number; 
                        else item.Text = ep.ep_number.ToString();
                        cbEpisode.Items.Add(item);
                    }
                }
            }

        }
        private void LoadNews()
        {
            int lastStart = 0;
            List<EpisodeVersion> filteredVersions = new List<EpisodeVersion>();
            System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
            sw.Start();
            while (filteredVersions.Count < 20)
            {
                EpisodeVersion[] epVersions = Global.GetLastEpisodeVersions(lastStart, 20);
                lastStart += 20;
                
                foreach (EpisodeVersion epver in epVersions)
                {
                    string text = "";
                    metroComboBox1.Invoke(new MethodInvoker(delegate { text = metroComboBox1.SelectedItem.ToString(); }));
                    switch (text)
                    {
                        case "Vše": filteredVersions.Add(epver); break;
                        case "Bez přidělené korekce": if (epver.state == 0) { filteredVersions.Add(epver); } break;
                        case "S přidělenou korekcí": if (epver.state == -1) { filteredVersions.Add(epver); } break;
                        case "Moje soubory": if (epver.addedBy == Global.loggedUser.id) { filteredVersions.Add(epver); } break;
                        case "Moje anime": if(Global.GetAnimes().Any(x => x.translatorid == Global.loggedUser.id && x.id == epver.animeId)) { filteredVersions.Add(epver); } break;
                        case "Připraveno na enkód": if (epver.state == 1) { filteredVersions.Add(epver); } break;
                        case "Připraveno ke zveřejnění": if (epver.state == 2) { filteredVersions.Add(epver); } break;
                    }
                }
                
                if (epVersions.Length != 20) break;                
            }
            sw.Stop();
            System.Diagnostics.Debug.WriteLine("Elapsed: " + sw.ElapsedMilliseconds);
            List<Anime> animes = Global.GetAnimes();
            MetroLink metroLink = new MetroLink();
            metroLink.Location = new System.Drawing.Point(179, 188);
            metroLink.Name = "metroProgressr1";
            metroLink.AutoSize = true;
            metroLink.Text = "Načítání";
            metroLink.Style = MetroFramework.MetroColorStyle.Blue;
            metroLink.TabIndex = 2;
            metroLink.Theme = MetroFramework.MetroThemeStyle.Dark;
            metroLink.UseSelectable = true;
            News.Invoke(new MethodInvoker(delegate
            {

                News.Controls.Clear();
                News.Refresh();

                News.Controls.Add(metroLink);
                metroLink.Visible = true; 
                News.Refresh();
            }));

            int positionIterator = 0;
            List<Panel> panels = new List<Panel>();
            this.Invoke((MethodInvoker)delegate
            {

                foreach (EpisodeVersion epver in filteredVersions)
                {
                    Anime anime = animes.FirstOrDefault(x => x.id == epver.animeId);

                    if (anime == null) continue;
                    MetroLabel label2 = new MetroLabel();
                    MetroLabel label1 = new MetroLabel();
                    MetroLabel labelTime = new MetroLabel();
                    MetroLabel labelDone = new MetroLabel();
                    MetroLink name = new MetroLink();
                    MetroPanel panel = new MetroPanel();
                    PictureBox picture = new PictureBox();


                    panel.Location = new System.Drawing.Point(0, positionIterator * 100);
                    panel.Name = "metroPanel1" + epver.id;
                    panel.Size = new System.Drawing.Size(475, 100);
                    panel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
                    panel.TabIndex = 2;
                    panel.Theme = MetroThemeStyle.Dark;


                    picture.Location = new System.Drawing.Point(3, 3);
                    picture.Name = "pictureBox1";
                    picture.Size = new System.Drawing.Size(99, 93);
                    picture.TabIndex = 2;
                    picture.TabStop = false;
                    picture.Load("http://anime.rildas.cz/" + anime.animelist_img);
                    picture.SizeMode = PictureBoxSizeMode.StretchImage;
                    // 
                    // metroLink1
                    // 
                    name.FontSize = MetroFramework.MetroLinkSize.Medium;
                    name.Location = new System.Drawing.Point(108, 13);
                    name.Name = "nameInNews" + epver.id;
                    name.Size = new System.Drawing.Size(350, 23);
                    name.TabIndex = 3;
                    name.Text = (epver.type == EpisodeVersion.Type.KOREKCE ? "Korekce" : "Překlad") + " " + anime.name + " " + epver.episode;
                    System.Windows.Forms.ToolTip ToolTip1 = new System.Windows.Forms.ToolTip();
                    ToolTip1.SetToolTip(name, (epver.type == EpisodeVersion.Type.KOREKCE ? "Korekce" : "Překlad") + " " + anime.name + " " + epver.episode);
                    name.Theme = MetroFramework.MetroThemeStyle.Dark;
                    name.UseSelectable = true;
                    name.Tag = epver;
                    name.Click += Name_Click;
                    name.TextAlign = ContentAlignment.TopLeft;
                    // 
                    // metroLabel15
                    // 
                    label1.AutoSize = true;
                    label1.Location = new System.Drawing.Point(108, 35);
                    label1.Name = "labelInNews1" + epver.id;
                    label1.Size = new System.Drawing.Size(88, 19);
                    label1.Theme = MetroThemeStyle.Dark;

                    label1.TabIndex = 4;
                    label1.Text = "Soubor nahrál: " + Global.GetUser(epver.addedBy).username;
                    // 
                    // metroLabel16
                    // 
                    label2.AutoSize = true;
                    label2.Location = new System.Drawing.Point(108, 55);
                    label2.Name = "labelInNews2" + epver.id;
                    label2.Size = new System.Drawing.Size(88, 19);
                    label2.TabIndex = 5;
                    label2.Theme = MetroThemeStyle.Dark;

                    label2.Text = "Komentář: " + epver.comment;

                    // 
                    // labelTime
                    // 
                    labelTime.AutoSize = true;

                    labelTime.Location = new System.Drawing.Point(350, 75);
                    labelTime.Name = "labelInNews3" + epver.id;
                    labelTime.Size = new System.Drawing.Size(200, 19);
                    labelTime.TabIndex = 6;
                    labelTime.Theme = MetroThemeStyle.Dark;
                    labelTime.TextAlign = ContentAlignment.BottomRight;
                    labelTime.Text = "Přidáno: " + epver.added.ToString("dd.MM.yyyy");
                    // 
                    // labelDone
                    // 
                    labelDone.AutoSize = true;


                    labelDone.Location = new System.Drawing.Point(108, 75);
                    labelDone.Name = "labelInNews3" + epver.id;
                    labelDone.Size = new System.Drawing.Size(140, 19);
                    labelDone.TabIndex = 6;
                    labelDone.Theme = MetroThemeStyle.Dark;
                    labelDone.TextAlign = ContentAlignment.BottomRight;
                    labelDone.Text = (epver.state == 1) ? "Připraveno na enkód" : "Vyžaduje korekci";
                    switch (epver.state)
                    {
                        case -3: labelDone.Text = "Čeká se na schválení překladatelem"; break;
                        case -2: labelDone.Text = "Existuje novější verze souboru"; break;
                        case -1: labelDone.Text = "Korekce zamluvena: " + Global.GetUser(epver.reservedBy).username; break;
                        case 0: labelDone.Text = "Vyžaduje korekci"; break;
                        case 1: labelDone.Text = "Připraveno na enkód"; break;
                        case 2: labelDone.Text = "Připraveno ke zveřejnění"; break;
                        case 3: labelDone.Text = "Zveřejněno"; break;
                    }
                    //
                    // Button
                    //

                    MetroFramework.Controls.MetroButton button = new MetroFramework.Controls.MetroButton();
                    button.Location = new System.Drawing.Point(350, 35);
                    button.Name = "newsButton" + epver.id;
                    button.Size = new System.Drawing.Size(125, 23);
                    //button.AutoSize = true;
                    button.Theme = MetroThemeStyle.Dark;
                    button.TabIndex = 9;
                    button.UseSelectable = true;
                    button.Click += new System.EventHandler(NewsButton_Click);
                    button.Tag = epver;
                    if (epver.state == -3 && (Global.loggedUser.access > 5 || anime.translatorid == Global.loggedUser.id))
                    {
                        button.Text = "Schválit";
                        panel.Controls.Add(button);
                    }
                    if (epver.state == -1 && epver.reservedBy == Global.loggedUser.id)
                    {
                        button.Text = "Zřeknout se korekce";
                        panel.Controls.Add(button);
                    }
                    if (epver.state == 0)
                    {
                        button.Text = "Zamluvit korekci";
                        panel.Controls.Add(button);
                    }
                    if (epver.state == 1 && Global.loggedUser.access > 5)
                    {
                        button.Text = "Enkódovat";
                        panel.Controls.Add(button);
                    }
                    if (epver.state == 2 && (Global.loggedUser.access > 5 || anime.translatorid == Global.loggedUser.id))
                    {
                        button.Text = "Zveřejnit";
                        panel.Controls.Add(button);
                    }

                    panel.Controls.Add(labelDone);
                    panel.Controls.Add(label1);
                    panel.Controls.Add(label2);
                    panel.Controls.Add(labelTime);

                    panel.Controls.Add(name);
                    panel.Controls.Add(picture);

                    panels.Add(panel);
                    positionIterator++;
                }
            });
            News.Invoke(new MethodInvoker(delegate
            {
                panels.ForEach(x=>News.Controls.Add(x));
                metroLink.Visible = false;
                News.Refresh();
            }));

        }
        private void LoadTeamMembers()
        {
            IEnumerable<User> users = Global.GetUsers().Where(x => x.access > 1);
            users = users.OrderBy(x => x.username);


            int positionIterator = 0;

            foreach (User user in users)
            {
                MetroLink name = new MetroLink();
                MetroPanel panel = new MetroPanel();


                panel.Location = new System.Drawing.Point(0, positionIterator * 25);
                panel.Name = "privateChat" + user.username;
                panel.Size = new System.Drawing.Size(150, 25);
                panel.BorderStyle = System.Windows.Forms.BorderStyle.None;
                panel.Theme = MetroThemeStyle.Dark;

                name.FontSize = MetroFramework.MetroLinkSize.Medium;
                name.Location = new System.Drawing.Point(0, 1);
                name.Name = "namePrivateChat" + user.username;
                name.Size = new System.Drawing.Size(150, 23);
                name.TabIndex = 3;
                name.Text =user.username;
                name.Theme = MetroFramework.MetroThemeStyle.Dark;
                name.UseSelectable = true;
                name.Tag = user;
                name.Click += User_Click;
                name.TextAlign = ContentAlignment.TopLeft;
                panel.Controls.Add(name);
                positionIterator++;
                chatPanelPrivate.Controls.Add(panel);

            }
            chatPanelPrivate.Refresh();
        }

        private void User_Click(object sender, EventArgs e)
        {
            User user = (sender as MetroLink).Tag as User;
            Global.OpenIfNeeded(user);
        }
      
  

        private void Name_Click(object sender, EventArgs e)
        {
            metroTabControl1.SelectedIndex = 2;
            EpisodeVersion epver = (sender as MetroFramework.Controls.MetroLink).Tag as EpisodeVersion;
            cb2Anime.Text = Global.GetAnime(epver.animeId).name;
            cb2Episodes.Text = epver.episode.ToString();
            string epVerText = epver.type + " od " + Global.GetUser(epver.addedBy).username + " z " + epver.added;
            if (epver.timeOn != "") epVerText += " [" + epver.timeOn + "]";
            cb2Version.Text = epVerText;
            panel3.Visible = true;
            tb3Comment.Text = epver.comment;
            tb3Download.Tag = epver;
            tb3DownloadAJ.Tag = epver;
            if (epver.titleEN == "") tb3DownloadAJ.Visible = false;
            else tb3DownloadAJ.Visible = true;
        }

        private void NewsButton_Click(object sender, EventArgs e)
        {
            string buttonText = (sender as MetroButton).Text;
            EpisodeVersion epVersion = (EpisodeVersion)((sender as MetroButton).Tag);
             if (buttonText == "Schválit")
            {
                epVersion.state = 1;
            }
            if ((sender as Button).Text == "Zřeknout se korekce")
            {
                epVersion.state = 0;
            }
            if ((sender as Button).Text == "Zamluvit korekci")
            {
                epVersion.state = -1;
                epVersion.reservedBy = Global.loggedUser.id;
            }
            if ((sender as Button).Text == "Enkódovat")
            {
                epVersion.state = 2;
            }
            if ((sender as Button).Text == "Zveřejnit")
            {
                foreach (TabPage page in metroTabControl1.TabPages)
                {
                    if (page.Name == "_publish")
                    {
                        metroTabControl1.SelectedTab = page;
                        string animeName = ((Global.GetAnime(epVersion.animeId) != null)?Global.GetAnime(epVersion.animeId).name:"");
                        for (int i = 0; i < publish_AnimeComboBox.Items.Count; ++i)
                        {
                            if (publish_AnimeComboBox.Items[i].ToString() == animeName)
                            {
                                publish_AnimeComboBox.SelectedIndex = i;
                                selectedVersion = epVersion;
                                _publishSelectVersion();
                                myPanel2.Refresh();
                                break;
                            }
                        }                        
                        _publishSetLabels(epVersion);
                    }
                }
            }
            RildasServerAPI.UpdateEpisodeVersion(epVersion);
            //LoadNews();

        }


        private void Global_EpisodeVersionListUpdated()
        {
            LoadNews();
        }

        private void _translate_Scroll(object sender, ScrollEventArgs e)
        {

        }


        private void metroComboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            LoadNews();
        }

        private void Přehrávač_Click(object sender, EventArgs e)
        {
        //    playerControl1.OnLoad(sender, e);
            nVLC_Demo_WinForms.Form1 form = new nVLC_Demo_WinForms.Form1();
            form.Visible = true;
            form.Show();
            form.Activate();
        }

        private void News_Scroll(object sender, ScrollEventArgs e)
        {

        }

        private void metroScrollBar1_Scroll_1(object sender, ScrollEventArgs e)
        {

        }

        private void metroScrollBar2_Scroll(object sender, ScrollEventArgs e)
        {
            chatPanelPrivate.VerticalScroll.Value = (chatPanelPrivate.PreferredSize.Height - chatPanelPrivate.Size.Height) < 0 ? 0 : ((chatPanelPrivate.PreferredSize.Height - chatPanelPrivate.Size.Height) * metroScrollBar2.Value) / 100;
        }

        private void chatPanelPrivate_Scroll(object sender, ScrollEventArgs e)
        {

        }

        private void publish_AnimeComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            Anime an = Global.GetAnime(publish_AnimeComboBox.SelectedItem.ToString());
            selectedPublish = -1;
            EpisodeVersion ver = new EpisodeVersion();
            ver.animeId = an.id;
            _publishSetLabels(ver);
            if(an != null)
            publish_animePicturebox.Load("http://anime.rildas.cz/" + an.post_img);
            myPanel2.Refresh();
        }

        private void publish_panelAnime_Paint(object sender, PaintEventArgs e)
        {
            StringFormat stringFormat = new StringFormat();
            stringFormat.Alignment = StringAlignment.Center;
            stringFormat.LineAlignment = StringAlignment.Center;
            List<Anime> animes = Global.GetAnimes();
            int animId = 0;
            if (publish_AnimeComboBox.SelectedItem == null) return;
            animId = Global.GetAnime(publish_AnimeComboBox.SelectedItem.ToString()).id;

            List<EpisodeVersion> versions = Global.GetEpisodeVersions();
            int counter = 0;
            SolidBrush brush;
            foreach (EpisodeVersion ep in versions)
            {
                if (ep.animeId == animId && ep.state == 2)
                {
                    brush = null;
                    Rectangle rec = new Rectangle(0, counter * 45, myPanel2.Width - 2, 45);
                    if (counter == selectedPublish) brush = new SolidBrush(Color.FromArgb(35, 35, 40));
                    if (mouseY > rec.Top && mouseY < rec.Bottom) brush = new SolidBrush(Color.FromArgb(50, 50, 50));
                    if (brush != null)
                        e.Graphics.FillRectangle(brush, rec);
                    e.Graphics.DrawRectangle(new Pen(Brushes.White), rec);
                    e.Graphics.DrawString(publish_AnimeComboBox.SelectedItem.ToString() + "_" + ep.episode, new Font("Arial", 13), Brushes.White, rec, stringFormat);
                    counter++;
                }
            }
        }

        private void btn_publishNow_Click(object sender, EventArgs e)
        {

        }

        private void publish_publishPlan_Click(object sender, EventArgs e)
        {

        }

        private void timetable_panel_Paint(object sender, PaintEventArgs e)
        {
            var p = sender as Panel;
            var g = e.Graphics;

            int users = 10; //Počet lidí, kteří jsou v týmu

            Point[] points = new Point[(users + 1) * 2];
            Pen pen = new Pen(Color.Black);

            for (int i = 1; i < (points.Length - 1); i = i + 2)
            {
                g.DrawLine(pen, new Point(10, i * ((p.Height / 2) / (users + 2))), new Point(p.Width - 10, i * ((p.Height / 2) / (users + 2))));
            }
        }

        private void metroPanel1_Paint(object sender, PaintEventArgs e)
        {
            e.Graphics.Clear(Color.FromArgb(17, 17, 17));
            table.MakeTable(e.Graphics, mouseX, mouseY);
            e.Graphics.Flush();
        }


        private void metroPanel1_MouseMove(object sender, MouseEventArgs e)
        {
            mouseX = e.X;
            mouseY = e.Y;
            myPanel.Refresh();
        }
        private void myPanel_Down(object sender, MouseEventArgs e)
        {
            if (table.GetEvent(e.X, e.Y).Count == 1)
                moveEvent = table.GetEvent(e.X, e.Y)[0];
            else
                moveEvent = null;
        }
        private void myPanel_Up(object sender, MouseEventArgs e)
        {
            /*if (moveEvent != null)
            {
                if (moveEvent.date.CompareTo(table.GetEvent(e.X, e.Y)[0].date) != 0)
                    table.MoveEvent(moveEvent, table.GetEvent(e.X, e.Y)[0]);
            }*/
        }

        private void myPanel_Click(object sender, MouseEventArgs e)
        {
            //Provizorní řešení, později budeme reagovat jinak
            /* List<Timetable.Event> ev = table.GetEvent(mouseX, mouseY);
             if (moveEvent == null || moveEvent.date.CompareTo(ev[0].date) == 0)
             {
                 string text = "";
                 foreach (Timetable.Event tmpe in ev)
                 {
                     text += tmpe.text + "\r\n\r\n";
                 }
                 MessageBox.Show((text != "\r\n\r\n") ? text : "V tento den se nekoná žádná akce!");
             }*/
            Timetable.Day day = table.GetDay(mouseX, mouseY);
            if (econtrol == null || econtrol.IsDisposed)
            {
                econtrol = new EventControler(ref day, table);
                econtrol.Show();
            }
        }
    }
    public class ComboboxItem
    {
        public string Text { get; set; }
        public object Value { get; set; }

        public override string ToString()
        {
            return Text;
        }
    }
}
