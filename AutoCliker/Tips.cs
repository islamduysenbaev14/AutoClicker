using ClosedXML.Excel;
using DocumentFormat.OpenXml.Math;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AutoCliker
{
    public static class Tips
    {
        [DllImport("user32.dll")]
        public static extern void mouse_event(uint dwFlags, uint dx, uint dy, uint dwData, int dwExtraInfo);

        // F8 tugmasini tekshirish uchun qo'shildi
        [DllImport("user32.dll")]
        public static extern short GetAsyncKeyState(int vKey);
        public const int VK_F8 = 0x77;

        private const uint MOUSEEVENTF_LEFTDOWN = 0x0002;
        private const uint MOUSEEVENTF_LEFTUP = 0x0004;

        // Amallar turlari uchun o'zgarmas (const) matnlar
        public const string TipClick = "Click";
        public const string TipCopy = "Copy";
        public const string TipInputText = "Input text";
        public const string TipRangTekshirish = "bool(color)";
        public const string TipKombinatsiya = "Kombinatsiya";
        public const string TipBool = "Bool";

        // Guruh turiga qarab tegishli amalni asinxron bajaruvchi asosiy metod
        public static async Task<bool> GuruhniBajarAsync(Guruh g)
        {
            bool grnatija = true;

            if (g.tipi == TipRangTekshirish)
            {
                grnatija = await RangTekshirishBajarAsync(g);
            }
            else if (g.tipi == TipKombinatsiya)
            {
                await KombinatsiyaBajarAsync(g);
                grnatija = true;
            }
            else if (g.tipi == TipClick)
            {
                await ClickBajarAsync(g);
                grnatija = true;
            }
            else if (g.tipi == TipCopy)
            {
                await CopyBajarasync(g);
                grnatija = true;
            }
            else if (g.tipi == TipInputText)
            {
                await InputTextBajarAsync(g);
                grnatija = true;
            }
            else if (g.tipi == TipBool)
            {
                // BU YERGA await QO'SHILDI:
                grnatija = await BoolBajar(g);
            }

            return grnatija;
        }

        public static int sanaw = 0;

        public static async Task<bool> BoolBajar(Guruh g)
        {
            if (g.tipi == "Bool")
            {
                if (g.booltekshir == "oila limiti" || g.booltekshir == "ega")
                {
                    // Tips.index - joriy kiritilgan oila a'zosi indeksi
                    // Tips.shanaraq.Count - oila a'zolarining umumiy soni
                    if (Tips.index < Tips.shanaraq.Count)
                    {
                        Tips.index++; // Keyingi a'zoga o'tamiz
                        return false; // Hali tugagani yo'q -> 'fl' bo'yicha 4-guruhga qaytadi
                    }
                    else
                    {
                        Tips.index = 0;
                        return true;  // Hammasi kiritildi -> 'tr' bo'yicha keyingi qadamga o'tadi
                    }
                }
                else if (g.booltekshir == "AyolJsh")
                {
                    return true;
                }
                else if (g.booltekshir == "vaqt")
                {
                    
                    int otganVaqt = Form1.soniyaHisoblagich;
                    int myVaqt = g.vaqtsoat;
                    
                    
                    // Agar F8 bosilsa yoki dastur to'xtatilsa chiqib ketadi (Form1.VK_F8 o'rniga shunchaki VK_F8)
                    if (myVaqt > otganVaqt)
                    {
                        return false;
                    }
                    else {
                        Form1.soniyaHisoblagich = 0;
                        
                        return true;  }
                }
            }

            return true; // Xatolik chiqmasligi uchun standart qiymat
        }

        // 1. Click amalini bajaruvchi metod
        public static async Task ClickBajarAsync(Guruh g)
        {
            foreach (var kordinatItem in g.grkordinat)
            {
                var k = Kordinata.Royxat.FirstOrDefault(x => x.Nomi == kordinatItem.Nomi);

                if (k != null)
                {
                    Cursor.Position = new Point(k.X, k.Y);
                    mouse_event(MOUSEEVENTF_LEFTDOWN | MOUSEEVENTF_LEFTUP, (uint)k.X, (uint)k.Y, 0, 0);
                    await Task.Delay(kordinatItem.ksec * 1000);
                }
            }
            await Task.Delay(g.grsec * 1000);
        }

        // 2. Koordinataga ko'p marta bosib, oxirida bir marta copy qiluvchi metod
        //public static async Task CopyBajarasync(Guruh g, int bosishSoni)
        //{
        //    try
        //    {
        //        foreach (var kordinatItem in g.grkordinat)
        //        {
        //            var k = Kordinata.Royxat.FirstOrDefault(x => x.Nomi == kordinatItem.Nomi);
        //            if (k != null)
        //            {
        //                Cursor.Position = new Point(k.X, k.Y);

        //                for (int i = 0; i < bosishSoni; i++)
        //                {
        //                    mouse_event(MOUSEEVENTF_LEFTDOWN | MOUSEEVENTF_LEFTUP, (uint)k.X, (uint)k.Y, 0, 0);
        //                    await Task.Delay(kordinatItem.ksec * 1000);
        //                }
        //            }
        //        }

        //        await Task.Delay(100);
        //        SendKeys.SendWait("^c");

        //        if (g.grsec > 0)
        //        {
        //            await Task.Delay(g.grsec * 1000);
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        System.Diagnostics.Debug.WriteLine($"CopyBajar xatoligi: {ex.Message}");
        //    }
        //}
        public static async Task<bool> CopyBajarasync(Guruh g)
        {
            string faylYoli = Form1.copySaqlashPapka;

            if (string.IsNullOrWhiteSpace(faylYoli))
            {
                MessageBox.Show("Avval saqlanadigan Excel faylini tanlang!");
                return false;
            }

            bool yangiFayl = !File.Exists(faylYoli);

            try
            {
                using (var workbook = yangiFayl ? new XLWorkbook() : new XLWorkbook(faylYoli))
                {
                    var worksheet = workbook.Worksheets.FirstOrDefault();

                    if (worksheet == null)
                        worksheet = workbook.Worksheets.Add("Natijalar");

                    if (worksheet.Cell(1, 1).IsEmpty())
                        worksheet.Cell(1, 1).Value = "Nusxalangan qiymat";

                    // --- 1. Guruh ichidagi koordinatalar bo'ylab yurish ---
                    foreach (var kordinatItem in g.grkordinat)
                    {
                        if (!Form1.isrunning || (GetAsyncKeyState(VK_F8) & 0x8000) != 0)
                            return false;

                        var k = Kordinata.Royxat.FirstOrDefault(x => x.Nomi == kordinatItem.Nomi);
                        if (k == null) continue;

                        Cursor.Position = new Point(k.X, k.Y);
                        await Task.Delay(200);

                        // Sichqonchani g.tab miqdoricha bosish
                        int bosishSoni = g.tab;
                        if (bosishSoni < 0) bosishSoni = 0;

                        for (int i = 0; i < bosishSoni; i++)
                        {
                            if (!Form1.isrunning || (GetAsyncKeyState(VK_F8) & 0x8000) != 0)
                            {
                                return false;
                            }

                            mouse_event(MOUSEEVENTF_LEFTDOWN | MOUSEEVENTF_LEFTUP, (uint)k.X, (uint)k.Y, 0, 0);
                            await Task.Delay(100);
                        }

                        // --- TAB TUGMASINI BOSISH QISMI ---
                        int tabSoni = g.tab; // Kordinatadagi Tab soni
                        if (tabSoni > 0)
                        {
                            var mainForm = Application.OpenForms.OfType<Form1>().FirstOrDefault();
                            if (mainForm != null)
                            {
                                for (int t = 0; t < tabSoni; t++)
                                {
                                    mainForm.Invoke((MethodInvoker)(() =>
                                    {
                                        SendKeys.SendWait("{TAB}");
                                    }));
                                    await Task.Delay(100);
                                }
                            }
                        }

                        if (!Form1.isrunning || (GetAsyncKeyState(VK_F8) & 0x8000) != 0)
                            return false;

                        string qiymat = "";
                        var formInstance = Application.OpenForms.OfType<Form1>().FirstOrDefault();
                        if (formInstance == null) continue;

                        // Ctrl+C yuborish (Nusxalash)
                        formInstance.Invoke((MethodInvoker)(() => { SendKeys.SendWait("^c"); }));
                        await Task.Delay(300);

                        // Clipboarddan o'qish
                        formInstance.Invoke((MethodInvoker)(() =>
                        {
                            try
                            {
                                if (Clipboard.ContainsText())
                                    qiymat = Clipboard.GetText();
                            }
                            catch { }
                        }));

                        // Agar qiymat bo'sh bo'lmasa Excel fayliga yozish
                        if (!string.IsNullOrWhiteSpace(qiymat))
                        {
                            int qator = (worksheet.LastRowUsed()?.RowNumber() ?? 1) + 1;
                            worksheet.Cell(qator, 1).Value = qiymat;

                            try { workbook.SaveAs(faylYoli); } catch { }
                        }

                        await Task.Delay(kordinatItem.ksec * 1000);
                    }

                    

                    try
                    {
                        if (yangiFayl)
                            workbook.SaveAs(faylYoli);
                        else
                            workbook.Save();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Faylni saqlashda xatolik: {ex.Message}");
                    }
                }

                if (g.grsec > 0)
                {
                    await Task.Delay(g.grsec * 1000);
                }

                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Xatolik: {ex.Message}");
                return false;
            }
        }

        [DllImport("user32.dll")]
        public static extern void keybd_event(byte bVk, byte bScan, uint dwFlags, int dwExtraInfo);

        private const byte VK_CONTROL = 0x11;
        private const byte VK_V = 0x56;
        private const uint KEYEVENTF_KEYUP = 0x0002;

        public static List<Oilasi> shanaraq = new List<Oilasi>();
        public static int index = 0;
        public static int jshindex = 0;
        public static int sanaindeks = 0;
        public static string egJsh;
        public static string ayoljshr;

        // 3. Input text (Ctrl + V yordamida matn kiritish) amali
        public static async Task InputTextBajarAsync(Guruh g)
        {
            foreach (var kordinatItem in g.grkordinat)
            {
                var k = Kordinata.Royxat.FirstOrDefault(x => x.Nomi == kordinatItem.Nomi);
                if (k != null)
                {
                    Cursor.Position = new Point(k.X, k.Y);
                    mouse_event(MOUSEEVENTF_LEFTDOWN | MOUSEEVENTF_LEFTUP, (uint)k.X, (uint)k.Y, 0, 0);
                    await Task.Delay(500);

                    string qiymat = "";
                    bool isJshshr = false;
                    bool isSana = false;

                    if (g.inputTextParam == "jshshr")
                    {
                        if (jshindex >= 0 && jshindex < shanaraq.Count)
                        {
                            qiymat = shanaraq[jshindex].Jshshr;
                            isJshshr = true;
                        }
                    }
                    else if (g.inputTextParam == "sana")
                    {
                        if (sanaindeks >= 0 && sanaindeks < shanaraq.Count)
                        {
                            qiymat = shanaraq[sanaindeks].BirthDate;
                            isSana = true;
                        }
                    }
                    else if (g.inputTextParam == "egaJsh")
                    {
                        qiymat = egJsh;
                    }
                    else if (g.inputTextParam == "AyolJsh")
                    {
                        qiymat = ayoljshr;
                    }


                    if (!string.IsNullOrEmpty(qiymat))
                    {
                        var mainForm = Application.OpenForms.OfType<Form1>().FirstOrDefault();
                        if (mainForm != null)
                        {
                            mainForm.Invoke((MethodInvoker)(() =>
                            {
                                if (g.inputTextParam == "AyolJsh")
                                {
                                    mouse_event(MOUSEEVENTF_LEFTDOWN | MOUSEEVENTF_LEFTUP,
                                        (uint)k.X, (uint)k.Y, 0, 0);

                                    System.Threading.Thread.Sleep(100);

                                    mouse_event(MOUSEEVENTF_LEFTDOWN | MOUSEEVENTF_LEFTUP,
                                        (uint)k.X, (uint)k.Y, 0, 0);

                                    System.Threading.Thread.Sleep(100);


                                    Clipboard.SetText(qiymat);
                                    System.Threading.Thread.Sleep(100);

                                    keybd_event(VK_CONTROL, 0, 0, 0);
                                    keybd_event(VK_V, 0, 0, 0);
                                    keybd_event(VK_V, 0, KEYEVENTF_KEYUP, 0);
                                    keybd_event(VK_CONTROL, 0, KEYEVENTF_KEYUP, 0);
                                }

                                else
                                {

                                    Clipboard.SetText(qiymat);
                                    System.Threading.Thread.Sleep(100);

                                    keybd_event(VK_CONTROL, 0, 0, 0);
                                    keybd_event(VK_V, 0, 0, 0);
                                    keybd_event(VK_V, 0, KEYEVENTF_KEYUP, 0);
                                    keybd_event(VK_CONTROL, 0, KEYEVENTF_KEYUP, 0);
                                }

                            }));
                        }

                        // Indekslarni bittaga oshiramiz
                        if (isJshshr) jshindex++;
                        if (isSana) sanaindeks++;
                    }

                    await Task.Delay(kordinatItem.ksec * 1000);
                }
            }

            if (g.grsec > 0)
            {
                await Task.Delay(g.grsec * 1000);
            }
        }

        private static async Task<bool> RangTekshirishBajarAsync(Guruh g)
        {
            await Task.Delay(100);
            return true;
        }

        private static async Task KombinatsiyaBajarAsync(Guruh g)
        {
            await Task.Delay(100);
        }

       
    }
}