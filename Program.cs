using Microsoft.EntityFrameworkCore;
using System;
using System.Diagnostics;
using System.Reflection;
using System.Xml;

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

        private static void LoadEntriesToDB(){

        }

        private static void getEntries(){
            foreach (var entry in database._finance_data){
                Console.WriteLine(entry.recipient1);
            }
        }

        private static void Initialize(){
            database = new FinancialContext();
        }


    }

    public class categoryManager{
        public List<finance_categories> categories;

        public categoryManager(List<finance_categories> _categories){
            categories = _categories;
        }

        public finance_categories findCategory(finance_data _Data){
            if (_Data.description1.Contains("AGIP")){
                return (from tmp in categories where tmp.category_name == "Tankstelle" select tmp).FirstOrDefault();
            }
            return null;
        }


    }

    public class FinancialContext : DbContext{
        public DbSet<finance_data> _finance_data {get; set;}
        public DbSet<finance_categories> _finance_categories {get; set;}

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder){
            String connectionString = get_conn_from_xml();
            var serverVersion= new MySqlServerVersion(new Version(8,0,27));
            optionsBuilder.UseMySql(connectionString, serverVersion, o => o
            .MinBatchSize(1)
            .MaxBatchSize(200));
        }

        private string get_conn_from_xml(){
            XmlDocument xmlDoc = new XmlDocument();
            try{
                xmlDoc.Load("sql.xml");
                XmlNodeList nodes = xmlDoc.SelectNodes("/connection");
                return nodes[0].SelectSingleNode("connstring").InnerText;
            }
            catch{
                Console.WriteLine("Fehler beim laden der XML");
                return "";
            }
        }

    }

    public class finance_data{
        public int id {get; set;}
        public DateTime date {get; set;}
        public string description1 {get; set;}
        public string description2 {get; set;}
        public string category {get; set;}
        public Boolean cost_fixed {get; set;}
        public string recipient1 {get; set;}
        public int recipient2 {get; set;}
        public double value {get; set;}
    }

    public class finance_categories{
        public int id {get; set;}
        public string category_name {get; set;}
    }

}