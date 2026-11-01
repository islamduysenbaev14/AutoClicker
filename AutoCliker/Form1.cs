using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Newtonsoft.Json;

namespace AutoCliker
{
    public partial class Form1 : Form
    {
        // Qaysi guruhda turganini eslab qolish uchun (Davom etish uchun kerak)
        private Guruh currentGuruh = null;
        public static string fayl1;
        public static string fayl2;
        private string tanlanganFaylYoli = "";

        public Form1()
        {
            InitializeComponent();
            //this.StartPosition = FormStartPosition.
            // 1. Oynaning joylashuvini qo'lda sozlashga ruxsat berish
            this.StartPosition = FormStartPosition.Manual;

            // 2. Ekranning o'lchamini aniqlash (pastdagi pusk panellarini hisobga olgan holda)
            var workingArea = Screen.PrimaryScreen.WorkingArea;

            // 3. Oynaning koordinatasini hisoblash:
            // X o'qi (chap/o'ng): Ekranning eng o'ng chetidan oynaning kengligini ayiramiz
            int x = workingArea.Right - this.Width;

            // Y o'qi (tepa/past): Ekranning eng tepasiga joylashtiramiz
            int y = workingArea.Top;

            // 4. Oynani hisoblangan joyga ko'chirish
            this.Location = new Point(x, y+300);
        
        f8Timer = new Timer();
            f8Timer.Interval = 50;
            f8Timer.Tick += F8Timer_Tick;
            numericUpDown1.Minimum = 1;
            numericUpDown1.Value = 1;
        }

        // --- 1. FAYLGA SAQLASH FUNKSIYASI ---
        private void GuruhlarniSaqlash()
        {
            if (string.IsNullOrEmpty(tanlanganFaylYoli)) return;

            try
            {
                var guruhlar = Guruh.Olish();
                string jsonString = JsonConvert.SerializeObject(guruhlar, Formatting.Indented);
                File.WriteAllText(tanlanganFaylYoli, jsonString);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Saqlashda xatolik: {ex.Message}", "Xatolik", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // --- 2. FAYLDAN YUKLAB OLISH FUNKSIYASI ---
        private void GuruhlarniYuklash()
        {
            try
            {
                if (!string.IsNullOrEmpty(tanlanganFaylYoli) && File.Exists(tanlanganFaylYoli))
                {
                    string jsonString = File.ReadAllText(tanlanganFaylYoli);
                    List<Guruh> yuklanganGuruhlar = JsonConvert.DeserializeObject<List<Guruh>>(jsonString);

                    if (yuklanganGuruhlar != null)
                    {
                        Guruh.Tozalash();
                        Kordinata.Tozalash();

                        foreach (var g in yuklanganGuruhlar)
                        {
                            Guruh.Olish().Add(g);

                            foreach (var gk in g.grkordinat)
                            {
                                if (!Kordinata.Olish().Any(k => k.Nomi == gk.Nomi))
                                {
                                    new Kordinata(gk.Nomi, gk.X, gk.Y, gk.ksec);
                                }
                            }
                        }

                        GuruhlarniChiqarish();
                        KordinatlarniChiqarish();
                        int maxNum = 0;
                        foreach (var k in Kordinata.Olish())
                        {
                            if (k.Nomi.StartsWith("k") && int.TryParse(k.Nomi.Substring(1), out int num))
                            {
                                if (num > maxNum) maxNum = num;
                            }
                        }
                        kSoni = maxNum;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Yuklashda xatolik: {ex.Message}", "Xatolik", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // --- 3. AVTOMATIK SAQLASH ---
        private void AvtomatikSaqlash()
        {
            if (!string.IsNullOrEmpty(tanlanganFaylYoli))
            {
                try
                {
                    var guruhlar = Guruh.Olish();
                    string jsonString = JsonConvert.SerializeObject(guruhlar, Formatting.Indented);
                    File.WriteAllText(tanlanganFaylYoli, jsonString);
                }
                catch (Exception ex)
                {
                    // Xatolikni yashirin qoldiramiz
                }
            }
        }

        private void KordinatlarniChiqarish()
        {
            flowLayoutPanelKordinatlar.Controls.Clear();
            flowLayoutPanelKordinatlar.FlowDirection = FlowDirection.TopDown;
            flowLayoutPanelKordinatlar.WrapContents = false;

            foreach (var k in Kordinata.Olish())
            {
                Panel rowPanel = new Panel();
                rowPanel.Width = 53;
                rowPanel.Height = 37;
                rowPanel.BorderStyle = BorderStyle.FixedSingle;
                rowPanel.BackColor = Color.WhiteSmoke;

                Label lblInfo = new Label();
                lblInfo.Text = $"{k.Nomi} :";
                lblInfo.Location = new Point(2, 2);
                lblInfo.Width = 50;
                lblInfo.Height = 13;
                lblInfo.BackColor = Color.Blue;
                lblInfo.Font = new Font(Font, FontStyle.Regular);

                NumericUpDown numSec = new NumericUpDown();
                numSec.Location = new Point(2, 16);
                numSec.Width = 50;
                numSec.Minimum = 0;
                numSec.Maximum = 1000;
                numSec.Value = k.S;

                numSec.ValueChanged += (s, e) =>
                {
                    k.S = (int)numSec.Value;

                    foreach (var guruh in Guruh.Olish())
                    {
                        foreach (var gk in guruh.grkordinat)
                        {
                            if (gk.Nomi == k.Nomi)
                            {
                                gk.ksec = k.S;
                            }
                        }
                    }

                    AvtomatikSaqlash();
                };

                rowPanel.Controls.Add(lblInfo);
                rowPanel.Controls.Add(numSec);

                flowLayoutPanelKordinatlar.Controls.Add(rowPanel);
            }
        }

        private bool f8OldinBosilgan = false;
        private Timer f8Timer;

        [DllImport("user32.dll")]
        public static extern short GetAsyncKeyState(int vKey);

        private void Form1_Load(object sender, EventArgs e)
        {
            KordinatlarniChiqarish();
        }

        private int kSoni = 0;
        private const int VK_F8 = 0x77;
        private void F8Timer_Tick(object sender, EventArgs e)
        {
            bool f8Bosildi = (GetAsyncKeyState(VK_F8) & 0x8000) != 0;

            if (f8Bosildi && !f8OldinBosilgan)
            {
                kSoni++;
                Point joy = Cursor.Position;
                Kordinata newkordinat = new Kordinata("k" + kSoni, joy.X, joy.Y, 0);

                KordinatlarniChiqarish();

                this.Text = $"K{kSoni} saqlandi: X={joy.X}, Y={joy.Y}";
            }

            f8OldinBosilgan = f8Bosildi;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            KordinatlarniChiqarish();

            f8OldinBosilgan = false;
            f8Timer.Start();
            this.Text = "Koordinata yig'ish boshlandi. F8 ni bosing.";
        }

        public int id = 0;
        private void gruh_Click(object sender, EventArgs e)
        {
            f8Timer.Stop();
            this.Text = "Koordinata yig'ish to'xtatildi.";
            id++;
            int guruhid = id;

            Guruh yangiGuruh = new Guruh(guruhid, "Click", 1, "", "");

            GuruhlarniChiqarish();
            AvtomatikSaqlash();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            Kordinata.Tozalash();
            KordinatlarniChiqarish();
            kSoni = 0;
        }

        private void button3_Click(object sender, EventArgs e)
        {
            Guruh.Tozalash();
            GuruhlarniChiqarish();
            AvtomatikSaqlash();
        }

        private void GuruhlarniChiqarish()
        {
            flowLayoutPanel1.Controls.Clear();
            flowLayoutPanel1.FlowDirection = FlowDirection.TopDown;
            flowLayoutPanel1.WrapContents = false;

            int guruhRaqami = 0;
            foreach (var g in Guruh.Olish())
            {
                guruhRaqami++;

                Panel rowPanel = new Panel();
                rowPanel.Width = 367;
                rowPanel.Height = 45;
                rowPanel.BorderStyle = BorderStyle.FixedSingle;
                rowPanel.BackColor = Color.WhiteSmoke;

                Label lblNo = new Label();
                lblNo.Text = guruhRaqami.ToString();
                lblNo.Location = new Point(3, 12);
                lblNo.Width = 29;
                lblNo.Font = new Font(Font, FontStyle.Bold);

                ComboBox cmbTipi = new ComboBox();
                cmbTipi.Location = new Point(35, 10);
                cmbTipi.Width = 70;
                cmbTipi.DropDownStyle = ComboBoxStyle.DropDownList;
                cmbTipi.Items.Add("Click");
                cmbTipi.Items.Add("Copy");
                cmbTipi.Items.Add("Input text");
                cmbTipi.Items.Add("bool(color)");
                cmbTipi.Items.Add("Kombinatsiya");
                cmbTipi.Items.Add("Bool");
                cmbTipi.SelectedItem = g.tipi;

                TextBox txtsec = new TextBox();
                txtsec.Location = new Point(106, 10);
                txtsec.Width = 25;
                txtsec.Text = g.grsec.ToString();

                txtsec.TextChanged += (s, e) =>
                {
                    if (int.TryParse(txtsec.Text, out int natija))
                    {
                        g.grsec = natija;
                        AvtomatikSaqlash();
                    }
                };

                TextBox txtKordinatlar = new TextBox();
                txtKordinatlar.Location = new Point(132, 10);
                txtKordinatlar.Width = 120;
                txtKordinatlar.Text = g.KordinatMatni;
                txtKordinatlar.TextChanged += (s, e) =>
                {
                    g.KordinatMatni = txtKordinatlar.Text;
                    g.grkordinat.Clear();

                    var qismlar = txtKordinatlar.Text.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                    foreach (var qism in qismlar)
                    {
                        string nom = qism.Trim();
                        if (!string.IsNullOrEmpty(nom))
                        {
                            foreach (var n in Kordinata.Olish())
                            {
                                if (n.Nomi == nom)
                                {
                                    g.grkordinat.Add(new GuruhKordinata
                                    {
                                        Nomi = n.Nomi,
                                        ksec = n.S,
                                        X = n.X,
                                        Y = n.Y
                                    });
                                }
                            }
                        }
                    }
                    AvtomatikSaqlash();
                };

                TextBox txtFalse = new TextBox();
                txtFalse.Location = new Point(253, 10);
                txtFalse.Width = 30;
                txtFalse.Text = g.fl;
                txtFalse.TextChanged += (s, e) =>
                {
                    g.fl = txtFalse.Text;
                    AvtomatikSaqlash();
                };

                TextBox txtTrue = new TextBox();
                txtTrue.Location = new Point(284, 10);
                txtTrue.Width = 30;
                txtTrue.Text = g.tr;
                txtTrue.TextChanged += (s, e) =>
                {
                    g.tr = txtTrue.Text;
                    AvtomatikSaqlash();
                };

                // 1. Parametrlar uchun NumericUpDown (Copy, Kombinatsiya va Bool->vaqt uchun)
                NumericUpDown numParam = new NumericUpDown();
                numParam.Location = new Point(315, 22);
                numParam.Width = 50;
                numParam.Minimum = 0;
                numParam.Maximum = 100000;

                decimal comVal = 1;
                if (g.tipi == "Copy") comVal = g.tab;
                else if (g.tipi == "Kombinatsiya") comVal = g.comsoni;
                else if (g.tipi == "Bool" && g.booltekshir == "vaqt") comVal = g.vaqtsoat > 0 ? g.vaqtsoat : 600;

                numParam.Value = comVal;
                numParam.Visible = (g.tipi == "Kombinatsiya" || g.tipi == "Copy" || (g.tipi == "Bool" && g.booltekshir == "vaqt"));

                numParam.ValueChanged += (s, e) =>
                {
                    int natija = (int)numParam.Value;
                    if (g.tipi == "Copy")
                    {
                        g.tab = natija;
                    }
                    else if (g.tipi == "Kombinatsiya")
                    {
                        g.comsoni = natija;
                    }
                    else if (g.tipi == "Bool" && g.booltekshir == "vaqt")
                    {
                        g.vaqtsoat = natija;
                    }
                    AvtomatikSaqlash();
                };

                // 2. Bool tipi uchun ComboBox
                ComboBox cmbBoolParam = new ComboBox();
                cmbBoolParam.Location = new Point(315, 1);
                cmbBoolParam.Width = 50;
                cmbBoolParam.DropDownStyle = ComboBoxStyle.DropDownList;
                cmbBoolParam.Items.Add("oila limiti");
                cmbBoolParam.Items.Add("ega");
                cmbBoolParam.Items.Add("AyolJsh");
                cmbBoolParam.Items.Add("vaqt"); // <-- Vaqt qo'shildi

                cmbBoolParam.SelectedItem = g.booltekshir;
                cmbBoolParam.Visible = (g.tipi == "Bool");

                cmbBoolParam.SelectedIndexChanged += (s, e) =>
                {
                    g.booltekshir = cmbBoolParam.SelectedItem?.ToString();
                    numParam.Visible = (g.tipi == "Bool" && g.booltekshir == "vaqt");

                    if (g.booltekshir == "vaqt" && g.vaqtsoat < 1)
                    {
                        g.vaqtsoat = 600;
                        numParam.Value = 600;
                    }
                    AvtomatikSaqlash();
                };
                
                // 3. Input text tipi uchun ComboBox
                ComboBox cmbInputTextParam = new ComboBox();
                cmbInputTextParam.Location = new Point(315, 1);
                cmbInputTextParam.Width = 50;
                cmbInputTextParam.DropDownStyle = ComboBoxStyle.DropDownList;
                cmbInputTextParam.Items.Add("jshshr");
                cmbInputTextParam.Items.Add("sana");
                cmbInputTextParam.Items.Add("egaJsh");
                cmbInputTextParam.Items.Add("AyolJsh");
                cmbInputTextParam.SelectedItem = g.inputTextParam;
                cmbInputTextParam.Visible = (g.tipi == "Input text");

                cmbInputTextParam.SelectedIndexChanged += (s, e) =>
                {
                    g.inputTextParam = cmbInputTextParam.SelectedItem?.ToString();
                    AvtomatikSaqlash();
                };

                cmbTipi.SelectedIndexChanged += (s, e) =>
                {
                    g.tipi = cmbTipi.SelectedItem.ToString();

                    numParam.Visible = (g.tipi == "Kombinatsiya" || g.tipi == "Copy" || (g.tipi == "Bool" && g.booltekshir == "vaqt"));
                    cmbBoolParam.Visible = (g.tipi == "Bool");
                    cmbInputTextParam.Visible = (g.tipi == "Input text");
                    AvtomatikSaqlash();
                };

                rowPanel.Controls.Add(lblNo);
                rowPanel.Controls.Add(cmbTipi);
                rowPanel.Controls.Add(txtsec);
                rowPanel.Controls.Add(txtKordinatlar);
                rowPanel.Controls.Add(txtFalse);
                rowPanel.Controls.Add(txtTrue);
                rowPanel.Controls.Add(numParam);
                rowPanel.Controls.Add(cmbBoolParam);
                rowPanel.Controls.Add(cmbInputTextParam);

                flowLayoutPanel1.Controls.Add(rowPanel);
            }
        }

        public Color EkrandanRangniOlish(int x, int y)
        {
            using (Bitmap bmp = new Bitmap(1, 1))
            {
                using (Graphics g = Graphics.FromImage(bmp))
                {
                    g.CopyFromScreen(x, y, 0, 0, new System.Drawing.Size(1, 1));
                }
                return bmp.GetPixel(0, 0);
            }
        }

        [DllImport("user32.dll")]
        public static extern void mouse_event(uint dwFlags, uint dx, uint dy, uint dwData, int dwExtraInfo);
        private const uint MOUSEEVENTF_LEFTDOWN = 0x0002;
        private const uint MOUSEEVENTF_LEFTUP = 0x0004;

        [DllImport("user32.dll")]
        public static extern IntPtr GetDC(IntPtr hwnd);
        [DllImport("user32.dll")]
        public static extern int ReleaseDC(IntPtr hwnd, IntPtr hdc);

        public async Task<bool> KutishVaF8Tekshir(int millisekund)
        {
            int interval = 50;
            int otganVaqt = 0;
            while (otganVaqt < millisekund)
            {
                if ((GetAsyncKeyState(VK_F8) & 0x8000) != 0 || !isrunning)
                {
                    return false;
                }
                await Task.Delay(interval);
                otganVaqt += interval;
            }
            return true;
        }

        public class OilaAzosiModel
        {
            public int Index { get; set; }
            public string Jshshr { get; set; }
            public string TugilganSana { get; set; }
        }

        // --- BOSHLASH / DAVOM ETISH TUGMASI ---
        public static bool isrunning = false;
        public string currentdastur;
        public static int siklstart = 1;
        
        private async void boshlash_Click(object sender, EventArgs e)
        {
            if (isrunning) return;

            var guruhlar = Guruh.Olish();
            if (guruhlar == null || guruhlar.Count == 0)
            {
                MessageBox.Show("Avval guruhlar yarating va ularga koordinatalar qo'shing!",
                                "Ogohlantirish", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                isrunning = true;

                this.Invoke((MethodInvoker)(() =>
                {
                    boshlash.Text = "Ishlayapti...";
                    this.Text = "Avtomatizatsiya ishlayapti... (To'xtatish uchun F8 ni bosing)";
                }));

                await Task.Run(async () =>
                {
                    if (currentGuruh == null)
                    {
                        currentGuruh = guruhlar[0];
                    }

                    while (isrunning)
                    {
                        int c;
                        if (currentdastur == "Fam")
                        {
                            Asb a = new Asb();
                            
                            
                            string jsh = a.GetJshshrF1(fayl1, siklstart);
                            Tips.egJsh = jsh;
                            if (string.IsNullOrEmpty(jsh))
                            {
                                break;
                            }

                            string kdstr = a.GetKadastrF2(fayl2, jsh);

                            if (string.IsNullOrEmpty(kdstr))
                            {
                                Form1.siklstart++;
                                continue;
                            }

                            var rawOila = a.Oilasi(fayl2, kdstr);
                            List<Oilasi> oila = a.Oila(rawOila);

                            if (oila == null || oila.Count == 0)
                            {
                                Form1.siklstart++;
                                continue;
                            }

                            Tips.shanaraq.Clear();
                            Tips.shanaraq = oila;
                            c = oila.Count;
                            
                            
                            Tips.index = 0;
                            Tips.jshindex = 0;
                            Tips.sanaindeks = 0;
                            Tips.sanaw = 0;

                            currentGuruh = guruhlar[0];
                        }
                        else if (currentdastur == "Ayol Gov")
                        {
                            Asb a = new Asb();
                            string ayoljshr = a.GetJshshrF1(fayl1, siklstart);
                            
                            Tips.ayoljshr = ayoljshr;
                            if (string.IsNullOrEmpty(ayoljshr))
                            {
                                break;
                            }

                            currentGuruh = guruhlar[0];
                        }
                        else
                        {
                            if (currentGuruh == null)
                            {
                                currentGuruh = guruhlar[0];
                            }
                        }
                        string text;
                        while (currentGuruh != null && isrunning)
                        {
                            if ((GetAsyncKeyState(VK_F8) & 0x8000) != 0)
                            {
                                isrunning = false;
                                currentGuruh = null;
                                break;
                            }

                            this.Invoke((MethodInvoker)(() =>
                            {
                                label1.Text = $"Guruh ID : {currentGuruh.id} ({currentGuruh.tipi}) wait({currentGuruh.grsec} s)";
                                
                            }));
                            
                            
                            this.Invoke((MethodInvoker)(() =>
                            {
                                
                                text = $"id : {siklstart} ==> {Tips.index}/{Tips.shanaraq.Count}";
                                label2.Text= text ;

                            }));
                            bool natija = await Tips.GuruhniBajarAsync(currentGuruh);

                            if (!isrunning || (GetAsyncKeyState(VK_F8) & 0x8000) != 0)
                            {
                                isrunning = false;
                                currentGuruh = null;
                                break;
                            }

                            string keyingiIdStr = natija ? currentGuruh.tr : currentGuruh.fl;

                            if (string.IsNullOrWhiteSpace(keyingiIdStr))
                            {
                                currentGuruh = null;
                                break;
                            }

                            if (int.TryParse(keyingiIdStr, out int targetId))
                            {
                                currentGuruh = guruhlar.FirstOrDefault(g => g.id == targetId);
                            }
                            else
                            {
                                currentGuruh = null;
                            }

                            if (currentGuruh == null)
                            {
                                break;
                            }
                        }

                        if (currentdastur == "Fam")
                        {
                            Form1.siklstart++;
                            currentGuruh = null;
                        }
                        else if (currentdastur == "Ayol Gov")
                        {
                            Form1.siklstart++;
                            currentGuruh = null;
                        }
                        else
                        {
                            break;
                        }
                    }
                });

                if (!isrunning && currentGuruh == null)
                {
                    MessageBox.Show("Barcha guruhlar jarayoni muvaffaqiyatli yakunlandi!",
                                    "Muvaffaqiyat", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Xatolik yuz berdi: {ex.Message}",
                                "Xatolik", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                isrunning = false;

                this.Invoke((MethodInvoker)(() =>
                {
                    if (currentGuruh != null)
                    {
                        boshlash.Text = "Davom etish";
                        this.Text = "AutoClicker - To'xtatildi (Davom etish uchun bosing)";
                    }
                    else
                    {
                        boshlash.Text = "Boshlash";
                        this.Text = "AutoClicker - Tayyor";
                    }
                }));
            }
        }

        private void sayt_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Filter = "Barcha fayllar (*.*)|*.*|Matnli fayllar (*.txt)|*.txt|Rasmlar (*.jpg;*.png)|*.jpg;*.png";
                openFileDialog.Title = "Kerakli faylni tanlang";

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    fayl1 = openFileDialog.FileName;
                    Asb a = new Asb();
                    //label1.Text = a.GetJshshrF1(fayl1, 1);
                    sayt.Text = fayl1;
                }
            }
        }

        private void dizim_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Filter = "Barcha fayllar (*.*)|*.*|Matnli fayllar (*.txt)|*.txt|Rasmlar (*.jpg;*.png)|*.jpg;*.png";
                openFileDialog.Title = "Kerakli faylni tanlang";

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    fayl2 = openFileDialog.FileName;
                    dizim.Text = fayl2;
                }
            }
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            currentdastur = comboBox1.Text;
        }

        private void numericUpDown1_ValueChanged(object sender, EventArgs e)
        {
            siklstart = (int)numericUpDown1.Value;
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            GuruhlarniSaqlash();
        }

        private void btnopen_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Filter = "JSON fayllar (*.json)|*.json|Barcha fayllar (*.*)|*.*";
                openFileDialog.Title = "Guruhlar faylini tanlang yoki yarating";

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    tanlanganFaylYoli = openFileDialog.FileName;

                    try
                    {
                        if (File.Exists(tanlanganFaylYoli))
                        {
                            string jsonString = File.ReadAllText(tanlanganFaylYoli);
                            List<Guruh> yuklanganGuruhlar = JsonConvert.DeserializeObject<List<Guruh>>(jsonString);

                            if (yuklanganGuruhlar != null)
                            {
                                Guruh.Tozalash();
                                Kordinata.Tozalash();

                                foreach (var g in yuklanganGuruhlar)
                                {
                                    Guruh.Olish().Add(g);

                                    foreach (var gk in g.grkordinat)
                                    {
                                        if (!Kordinata.Olish().Any(k => k.Nomi == gk.Nomi))
                                        {
                                            new Kordinata(gk.Nomi, gk.X, gk.Y, gk.ksec);
                                        }
                                    }
                                }

                                GuruhlarniChiqarish();
                                KordinatlarniChiqarish();

                                int maxNum = 0;
                                foreach (var k in Kordinata.Olish())
                                {
                                    if (k.Nomi.StartsWith("k") && int.TryParse(k.Nomi.Substring(1), out int num))
                                    {
                                        if (num > maxNum) maxNum = num;
                                    }
                                }
                                kSoni = maxNum;

                                int maxId = 0;
                                foreach (var g in Guruh.Olish())
                                {
                                    if (g.id > maxId) maxId = g.id;
                                }
                                id = maxId;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Faylni o'qishda xatolik: {ex.Message}");
                    }
                }
            }
        }

        private void yaratish_Click(object sender, EventArgs e)
        {
            using (SaveFileDialog saveFileDialog = new SaveFileDialog())
            {
                saveFileDialog.Filter = "JSON fayllar (*.json)|*.json|Barcha fayllar (*.*)|*.*";
                saveFileDialog.Title = "Yangi guruhlar faylini yarating yoki mavjudini tanlang";
                saveFileDialog.FileName = "guruhlar_sozlamasi.json";

                if (saveFileDialog.ShowDialog() == DialogResult.OK)
                {
                    tanlanganFaylYoli = saveFileDialog.FileName;

                    try
                    {
                        if (!File.Exists(tanlanganFaylYoli))
                        {
                            File.WriteAllText(tanlanganFaylYoli, "[]");
                        }

                        string jsonString = File.ReadAllText(tanlanganFaylYoli);
                        List<Guruh> yuklanganGuruhlar = JsonConvert.DeserializeObject<List<Guruh>>(jsonString);

                        if (yuklanganGuruhlar != null)
                        {
                            Guruh.Tozalash();
                            Kordinata.Tozalash();

                            foreach (var g in yuklanganGuruhlar)
                            {
                                Guruh.Olish().Add(g);

                                foreach (var gk in g.grkordinat)
                                {
                                    if (!Kordinata.Olish().Any(k => k.Nomi == gk.Nomi))
                                    {
                                        new Kordinata(gk.Nomi, gk.X, gk.Y, gk.ksec);
                                    }
                                }
                            }
                            GuruhlarniChiqarish();
                            KordinatlarniChiqarish();

                            int maxNum = 0;
                            foreach (var k in Kordinata.Olish())
                            {
                                if (k.Nomi.StartsWith("k") && int.TryParse(k.Nomi.Substring(1), out int num))
                                {
                                    if (num > maxNum) maxNum = num;
                                }
                            }
                            kSoni = maxNum;

                            int maxId = 0;
                            foreach (var g in Guruh.Olish())
                            {
                                if (g.id > maxId) maxId = g.id;
                            }
                            id = maxId;
                        }

                        MessageBox.Show("Fayl muvaffaqiyatli tanlandi va tayyorlandi!", "Muvaffaqiyat", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Fayl bilan ishlashda xatolik: {ex.Message}", "Xatolik", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }
        public static string copySaqlashPapka = "";
        public static int soniyaHisoblagich = 0;

        private void timer1_Tick(object sender, EventArgs e)
        {
            // Har safar 1 sekund o'tganda hisoblagich 1 taga oshadi
            soniyaHisoblagich++;

            // Labelga chiqarish (masalan: "O'tgan vaqt: 5 s")
            label5.Text = $"Vaqt: {soniyaHisoblagich}";
        }

        private void vaqtnum_ValueChanged(object sender, EventArgs e)
        {

        }
        
        private void label6_Click(object sender, EventArgs e)
        {

        }
        private int totalSeconds = 0;
        private void timer2_Tick(object sender, EventArgs e)
        {
            totalSeconds++;

            int hours = totalSeconds / 3600;
            int minutes = (totalSeconds % 3600) / 60;
            int seconds = totalSeconds % 60;
            label6.Text = $"{hours:D2}:{minutes:D2}:{seconds:D2}";
        }

        private void button4_Click(object sender, EventArgs e)
        {
            using (SaveFileDialog dialog = new SaveFileDialog())
            {
                dialog.Filter = "Excel fayllari (*.xlsx)|*.xlsx";
                dialog.Title = "Excel faylini tanlang";
                dialog.FileName = "Copy_Natijalar.xlsx";
                dialog.DefaultExt = "xlsx";
                dialog.AddExtension = true;
                dialog.OverwritePrompt = false;

                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    copySaqlashPapka = dialog.FileName;
                    button4.Text = Path.GetFileName(copySaqlashPapka);
                }
            }
        }
    }
}