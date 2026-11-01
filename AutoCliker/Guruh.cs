using System;
using System.Collections.Generic;
using System.Drawing;

namespace AutoCliker
{
   
    public class GuruhKordinata
    {       public string Nomi;
            public int ksec;
            public int X; // Ekrandagi X pikseli
            public int Y; // Ekrandagi Y pikseli
        
    }

    public class Guruh
    {
        public int id;
        public string tr = "";
        public string fl = "";
        public string tipi = "";
        public int grsec = 1;
        public string KordinatMatni = "";
        public int comsoni = 1;
        public int tab = 1;
        public string booltekshir = "";
        public int vaqtsoat ;
        public bool JshshrTekshirilsinmi { get; set; } = false;
        //public string boolParam;
        public string inputTextParam;
        public List<GuruhKordinata> grkordinat = new List<GuruhKordinata>();

        public static List<Guruh> Guruhlar = new List<Guruh>();
        public Guruh() { }

        public Guruh(int id, string tipi, int grsec, string fl, string tr)
        {
            this.id = id;
            this.tipi = tipi;
            this.grsec = grsec;
            this.fl = fl;
            this.tr = tr;
            Guruhlar.Add(this);
        }

        public static void Tozalash()
        {
            Guruhlar.Clear();
        }
        public static List<Guruh> Olish()
        {
            return Guruhlar;
        }
    }
}