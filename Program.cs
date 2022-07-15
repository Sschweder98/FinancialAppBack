using Microsoft.EntityFrameworkCore;
using System;
using System.Diagnostics;
using System.Reflection;
using System.Xml;
using System.IO;

namespace financial
{
    class Program
    {
        private static FinancialContext database;
        private static csvManager _csvManager = new csvManager();
        private static dataMapper _dataMapper = new dataMapper();

        static void Main(string[] args)
        {
            Initialize();
            _csvManager.loadCSV("C:\\Users\\sschw\\Downloads\\kskwn_export.CSV");
            _dataMapper.map_csvData_to_finance_data(_csvManager.ksk_csv, ref database);
        }

        private static void getEntries()
        {
            foreach (var entry in database._finance_data)
            {
                Console.WriteLine(entry.recipient1);
                Console.WriteLine(entry.recipient2);
            }
        }

        private static void Initialize()
        {
            database = new FinancialContext();
        }

    }

    public class dataMapper
    {
        public void map_csvData_to_finance_data(List<csvManager.ksk_csv_row> csv_data, ref FinancialContext database)
        {
            foreach (csvManager.ksk_csv_row row in csv_data)
            {
                String unqiue = GenerateKey(row);
                Int32 Count = (from finance_data tmp in database._finance_data where tmp.uniqe_key == unqiue select tmp).Count();
                if (Count == 0)
                {
                    //Create Entry

                }
            }
        }

        public string GenerateKey(csvManager.ksk_csv_row row)
        {
            //date
            String unique = row.Buchungstag.Replace(".", "");
            //name of recipient
            unique += "_" + row.Beguenstigter_Zahlungspflichtiger.ToLower().Replace(" ", "_").Substring(0, 5);
            //iban of recipient
            if (row.Kontonummer_IBAN.Length >= 5)
            {
                unique += "_" + row.Kontonummer_IBAN.Substring(0, 5);
            }
            else
            {
                unique += "XXXXX";
            }
            //value
            unique += "_" + row.Betrag.Replace("_", "").Replace(",", "X");
            return unique;
        }
    }

    public class csvManager
    {
        private StreamReader reader;
        public List<ksk_csv_row> ksk_csv = new List<ksk_csv_row>();
        public void loadCSV(String file)
        {
            ksk_csv.Clear();
            reader = new StreamReader(file);
            while (reader.Peek() >= 0)
            {
                analyse_line(reader.ReadLine());
            }

        }

        private void analyse_line(String line)
        {
            if (line.Contains("Auftragskonto"))
            {
                return;
            }
            string[] line_arr = line.Split(";");
            ksk_csv.Add(new ksk_csv_row()
            {
                Auftragskonto = line_arr[0],
                Buchungstag = line_arr[1],
                Valutadatum = line_arr[2],
                Buchungstext = line_arr[3],
                Verwendungszweck = line_arr[4],
                Glaeubiger_ID = line_arr[5],
                Mandatsreferenz = line_arr[6],
                Kundenreferenz__End_to_End_ = line_arr[7],
                Sammlerreferenz = line_arr[8],
                Lastschrift_Ursprungsbetrag = line_arr[9],
                Auslagenersatz_Ruecklastschrift = line_arr[10],
                Beguenstigter_Zahlungspflichtiger = line_arr[11],
                Kontonummer_IBAN = line_arr[12],
                BIC__SWIFT_Code_ = line_arr[13],
                Betrag = line_arr[14],
                Waehrung = line_arr[15],
                Info = line_arr[16]
            });
        }

        public class ksk_csv_row
        {

            public string Auftragskonto { get; set; }
            public string Buchungstag { get; set; }
            public string Valutadatum { get; set; }
            public string Buchungstext { get; set; }
            public string Verwendungszweck { get; set; }
            public string Glaeubiger_ID { get; set; }
            public string Mandatsreferenz { get; set; }
            public string Kundenreferenz__End_to_End_ { get; set; }
            public string Sammlerreferenz { get; set; }
            public string Lastschrift_Ursprungsbetrag { get; set; }
            public string Auslagenersatz_Ruecklastschrift { get; set; }
            public string Beguenstigter_Zahlungspflichtiger { get; set; }
            public string Kontonummer_IBAN { get; set; }
            public string BIC__SWIFT_Code_ { get; set; }
            public string Betrag { get; set; }
            public string Waehrung { get; set; }
            public string Info { get; set; }

        }

    }

    public class categoryManager
    {
        public List<finance_categories> categories;

        public categoryManager(List<finance_categories> _categories)
        {
            categories = _categories;
        }

        public finance_categories getCategoryByName(String name)
        {
            var tmp = (from category in categories where category.category_name == name select category).FirstOrDefault();
            if (tmp != null)
            {
                return tmp;
            }
            else
            {
                return null;
            }
        }

        public finance_categories findCategory(finance_data _Data)
        {
            if (_Data.description1.ToLower().Contains("agip"))
            {
                return getCategoryByName("Tankstelle");
            }
            return null;
        }


    }

    public class FinancialContext : DbContext
    {
        public DbSet<finance_data> _finance_data { get; set; }
        public DbSet<finance_categories> _finance_categories { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            String connectionString = get_conn_from_xml();
            var serverVersion = new MySqlServerVersion(new Version(8, 0, 27));
            optionsBuilder.UseMySql(connectionString, serverVersion, o => o
            .MinBatchSize(1)
            .MaxBatchSize(200));
        }

        private string get_conn_from_xml()
        {
            XmlDocument xmlDoc = new XmlDocument();
            try
            {
                xmlDoc.Load("sql.xml");
                XmlNodeList nodes = xmlDoc.SelectNodes("/connection");
                return nodes[0].SelectSingleNode("connstring").InnerText;
            }
            catch
            {
                Console.WriteLine("Fehler beim laden der XML");
                return "";
            }
        }

    }

    public class finance_data
    {
        public int id { get; set; }
        public string uniqe_key { get; set; }
        public DateTime date { get; set; }
        public string description1 { get; set; }
        public string description2 { get; set; }
        public string category { get; set; }
        public Boolean cost_fixed { get; set; }
        public string recipient1 { get; set; }
        public int recipient2 { get; set; }
        public double value { get; set; }
    }

    public class finance_categories
    {
        public int id { get; set; }
        public string category_name { get; set; }
    }

}