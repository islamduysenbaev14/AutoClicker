using System.Collections.Generic;

namespace AutoCliker
{
    public class Kordinata
    {
        // Oddiy o'zgaruvchilar
        public string Nomi;
        public int X;
        public int Y;
        public int S;

        // Statik ro'yxat (barcha kordinatlar shu yerda yig'iladi)
        public static List<Kordinata> Royxat = new List<Kordinata>();

        // Konstruktor (yangi kordinat yaratilganda ishlaydi)
        public Kordinata(string nomi, int x, int y, int s)
        {
            Nomi = nomi;
            X = x;
            Y = y;
            S = s;
            Royxat.Add(this);
        }
        public static void Tozalash()
        {
            Royxat.Clear();
        }
        public static List<Kordinata> Olish()
        {
            return Royxat;
        }
    }
}