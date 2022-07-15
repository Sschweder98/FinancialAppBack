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

        static void Main(string[] args)
        {
            Initialize();
            getEntries();

        }

        private static void LoadEntriesToDB()
        {

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

    public class csvManager{
        private StreamReader reader;
        private List<ksk_csv_row> ksk_csv = new List<ksk_csv_row>();
        public void loadCSV(String file){
            ksk_csv.Clear();
            reader = new StreamReader(file);
            while (reader.Peek() >= 0){
                analyse_line(reader.ReadLine());
            }

        }

        private void analyse_line(String line){
            string[] line_arr = line.Split(";");
            for (int i = 1; i <= line_arr.Length - 1; i++){
                ksk_csv.Add(new ksk_csv_row(){Auftragskonto = line_arr[i]});
            }
        }

        public class ksk_csv_row{

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