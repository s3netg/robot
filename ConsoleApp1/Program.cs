using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace zurick
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            executa();
        }

        private static void executa()
        {
            string executions = string.Empty;
            long initialOrder = 1;
            long initialQuote = 1;
            long execs = 9;

            string fileName = string.Format("f:\\{0}_log.csv", DateTime.Now.ToString("ddMMyyyy_HHmmss"));
            Stress stress = new Stress(fileName);
            List<Task> tasks = new List<Task>();
            long currentQuote = initialQuote;
            DateTime sDate = DateTime.Now;

            var x = execs / 9;
            if (execs % 9 > 0)
            {
                x++;
            }

            stress.Execute(GetData());

            DateTime fDate = DateTime.Now;
            stress.CloseFile((execs * 9) / (fDate - sDate).TotalSeconds);
        }

        private static string GetData()
        {
            StringBuilder result = new StringBuilder();

            result.AppendLine("{");
            result.AppendLine("\"name\": \"mary@quote\",");
            result.AppendLine("\"email\": \"mary@novo.com\",");
            result.AppendLine("\"phone\": \"0004873707\",");
            result.AppendLine("\"gender\": \"female\"");
            result.AppendLine("}");
            return result.ToString();
        }
    }
}