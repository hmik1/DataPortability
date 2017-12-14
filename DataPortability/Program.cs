using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using iTextSharp;
using iTextSharp.text;
using iTextSharp.text.pdf;
using iTextSharp.text.html;
using iTextSharp.text.html.simpleparser;
using System.IO;
using System.Text.RegularExpressions;

namespace DataPortability
{
    class Program
    {

        static void Main(string[] args)
        {
            

            Console.Write("Please enter your query: ");
            string query = System.Console.ReadLine();
            Regex regex = new Regex(@"\b"+query+@"\b", RegexOptions.IgnoreCase);
            string line;
            List<string> columns = new List<string>();
            List<string> values = new List<string>();

            // Read the file and display it line by line.  
            System.IO.StreamReader file = new System.IO.StreamReader("C:\\Users\\mikul\\Documents\\PROJEKT\\wwwhr_2017-10-30.sql");
            while ((line = file.ReadLine()) != null)
            {
                string[] rijeci = line.Split();
                if (rijeci[0] == "INSERT")
                {
                    //pokupi nazive stupaca
                    rijeci = (line.Split(new Char[] { '(', ')' }))[1].Split(',');
                    for (int i = 0; i < rijeci.Length; i++)
                    {
                        rijeci[i] = rijeci[i].Trim(new Char[] { ' ', '`', ',' });
                        if (!columns.Contains(rijeci[i]))
                        {
                            columns.Add(rijeci[i]);
                        }
                    }
                }

                if (rijeci[0] == "VALUES")
                {
                    // pokupi sve retke dodane u tablicu
                    bool end = false;
                    while (true)
                    {
                        line = file.ReadLine();
                        if (line[line.Length - 1] == ';') {
                            end = true;
                            line = line.Trim(';');
                        }
                        values.Add(line.Trim(new Char[] { '(', ')', ' ', '\t', ',' }));
                        if (end) {
                            break;
                        }
                        
                    }

                }
            }
            List<string> results = new List<string>();

            foreach (string item in values)
            {
                // postoji li traženi pojam u retku
                if (regex.IsMatch(item))
                {
                    results.Add(item);
                }
            }
            if (!results.Any()) {
                Console.WriteLine("NO MATCHES");
                Console.ReadKey();
                System.Environment.Exit(0);
            }
            //------------------Ispis na konzolu---------------------
            /*foreach (string item in results)
            {
                List<string> val = splitval(item);

                for (int i = 0; i < columns.Count(); i++)
                {
                    Console.WriteLine(columns[i] + ": " + val[i]);
                }
                Console.WriteLine();
                Console.WriteLine();
            }*/
            // ---------------------------PDF ------------------------------------
            Console.WriteLine("Stvaram PDF-a");

            BaseFont arial = BaseFont.CreateFont("c:\\windows\\fonts\\arial.ttf", BaseFont.IDENTITY_H, BaseFont.EMBEDDED);
            Font NormalFont = new iTextSharp.text.Font(arial, 12, Font.NORMAL);
            Font boldFont = new iTextSharp.text.Font(arial, 12, iTextSharp.text.Font.BOLD);

            FileStream fs = new FileStream("C:\\Users\\mikul\\Documents\\PROJEKT\\" + query + ".pdf", FileMode.Create, FileAccess.Write, FileShare.None);
            Document doc = new Document();
            PdfWriter writer = PdfWriter.GetInstance(doc, fs);
            doc.Open();
            foreach (string item in results)
            {
                List<string> val = splitval(item);

                for (int i = 0; i < columns.Count(); i++)
                {

                    doc.Add(new Paragraph
                    {
                        new Chunk(columns[i] + ": ", boldFont),
                        new Chunk(val[i], NormalFont
                        )
                    });

                }
                doc.Add(new Paragraph("\n"));
                doc.Add(new Paragraph("\n"));
            }

            doc.Close();
            Console.WriteLine("PDF stvoren\n");
            //------------------------CSV----------------------------------------
            Console.WriteLine("Stvaram CSV");
            string text = "";
            text = String.Join(",", columns) + ",";
            foreach (string item in results)
            {
                List<string> val = splitval(item);
                text += String.Join(",", val);
            }

            File.WriteAllText("C:\\Users\\mikul\\Documents\\PROJEKT\\" + query + ".csv", text);
            Console.WriteLine("CSV stvoren\n");

            file.Close();
            // Suspend the screen.  
            System.Console.ReadLine();
        }

        //Podijeli vrijednosti u retku po stupcima
        static List<string> splitval(String line)
        {
            List<string> sval = new List<string>();
            string value = "";
            bool ignore = false;
            bool flag = false;
            foreach (char character in line)
            {
                if (character == '\\')
                {
                    flag = true;
                }
                else if (character == '\'' && flag == false)
                {
                    ignore = !ignore;
                }
                else if (character != ',' || ignore == true)
                {
                    value += character;
                    flag = false;
                }
                else
                {

                    sval.Add(value);
                    value = "";
                }
            }
            sval.Add(value);
            return sval;
        }
    }
}
