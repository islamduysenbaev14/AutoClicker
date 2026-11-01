using ClosedXML.Excel;
using DocumentFormat.OpenXml.Office.SpreadSheetML.Y2023.DataSourceVersioning;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoCliker
{
    public class Oilasi
    {
        public int Index { get; set; }           // Tartib raqami (1, 2, 3...)
        public string Jshshr { get; set; }       // A ustuni
        public string BirthDate { get; set; }    // B ustuni
    }
    internal class Asb
    {
        public string GetJshshrF1(string filePath, int rowIndex)
        {
            try
            {
                using (var workbook = new XLWorkbook(filePath))
                {
                    // Birinchi sahifani (Worksheet) olish
                    var worksheet = workbook.Worksheet(1);

                    // rowIndex - qator raqami (1 dan boshlanadi), 1 - A ustuni
                    var cell = worksheet.Cell(rowIndex, 1);

                    return cell.Value.ToString();
                }
            }
            catch (Exception ex)
            {
                return $"Xatolik: {ex.Message}";
            }
        }
        public string GetKadastrF2(string filePath, string targetJshshr)
        {
            try
            {
                using (var workbook = new XLWorkbook(filePath))
                {
                    var worksheet = workbook.Worksheet(1); // Birinchi sahifa

                    // Ishlatilgan barcha qatorlar bo'yicha sikl
                    foreach (var row in worksheet.RowsUsed())
                    {
                        // A ustuni - 1-ustun
                        string cellAValue = row.Cell(1).Value.ToString().Trim();

                        // Agar A ustunidagi qiymat qidirilayotgan JSHSHRga teng bo'lsa
                        if (cellAValue == targetJshshr)
                        {
                            // C ustuni - 3-ustun qiymatini qaytarish
                            return row.Cell(3).Value.ToString().Trim();
                        }
                    }
                }
                return "Bunday JSHSHR topilmadi";
            }
            catch (Exception ex)
            {
                return $"Xatolik: {ex.Message}";
            }
        }
        public static string GetBirthDate(string jshshr)
        {
            // JSHSHR 14 ta belgidan iborat ekanligini tekshirish
            if (string.IsNullOrWhiteSpace(jshshr) || jshshr.Length != 14)
            {
                throw new ArgumentException("JSHSHR 14 ta belgidan iborat bo'lishi kerak.");
            }

            // 1-raqam asosida asrni aniqlash
            char genderCenturyChar = jshshr[0];
            string century = "19";

            if (genderCenturyChar == '3' || genderCenturyChar == '4')
            {
                century = "19";
            }
            else if (genderCenturyChar == '5' || genderCenturyChar == '6')
            {
                century = "20";
            }

            // Kun, oy va yilning oxirgi 2 raqamini ajratib olish
            string day = jshshr.Substring(1, 2);
            string month = jshshr.Substring(3, 2);
            string yearShort = jshshr.Substring(5, 2);

            string fullYear = century + yearShort;

            // "kk.oo.yyyy" formatida qaytarish
            return $"{day}.{month}.{fullYear}";
        }

        
        public List<Oilasi> Oilasi(string filePath, string targetCadastre)
        {
            var resultList = new List<Oilasi>();

            try
            {
                using (var workbook = new XLWorkbook(filePath))
                {
                    var worksheet = workbook.Worksheet(1); // Birinchi sahifa
                    int matchIndex = 1;

                    foreach (var row in worksheet.RowsUsed())
                    {
                        // C ustuni - 3-ustun (Kadastr raqami)
                        string cellCValue = row.Cell(3).Value.ToString().Trim();

                        if (cellCValue == targetCadastre.Trim())
                        {
                            var item = new Oilasi
                            {
                                Index = matchIndex++,
                                Jshshr = row.Cell(1).Value.ToString().Trim(),    // A ustuni

                                BirthDate = GetBirthDate(row.Cell(1).Value.ToString().Trim())
                            };

                            resultList.Add(item);
                        }
                    }

                }
            }
            catch (Exception ex)
            {
                // Xatolik yuz bersa konsolga yoki logga chiqarish mumkin
                System.Diagnostics.Debug.WriteLine($"Xatolik: {ex.Message}");
            }

            return resultList;
        }
        public List<Oilasi> Oila(List<Oilasi> originalList)
        {
            // 1. JSHSHR bo'yicha guruhlab, har bir guruhdan faqat birinchisini qoldiramiz
            var uniqueList = originalList
                .GroupBy(p => p.Jshshr)
                .Select(g => g.First())
                .ToList();

            // 2. Qolgan elementlarning Index raqamlarini 1 dan boshlab tartib bilan qayta raqamlaymiz
            int newIndex = 1;
            foreach (var item in uniqueList)
            {
                item.Index = newIndex++;
            }

            return uniqueList;
        }
        public int Oilasoni<T>(List<T> list)
        {
            return list?.Count ?? 0;
        }

    }

}
