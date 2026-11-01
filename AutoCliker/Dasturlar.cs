using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoCliker
{
    public class Dastur
    {
        public int Id { get; set; }
        public string Nomi { get; set; }

        // Kombo-boksda to'g'ri nom ko'rinishi uchun ToString ni override qilamiz
        public override string ToString()
        {
            return Nomi;
        }

    }
}
