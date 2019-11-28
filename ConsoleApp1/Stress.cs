using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;

namespace zurick
{
    internal class Stress
    {
        public static long executingItens;
        public static Random random;
        private static ReaderWriterLock locker = new ReaderWriterLock();
        public static List<Execution> executions;
        public static long executed;

        private static string apiUrl = "http://localhost:8000/api/contacts";

        private static HttpClient client = new HttpClient();
        private static StreamWriter logFile = null;
        private static StreamWriter ent = null;
        private static StreamWriter sai = null;

        public Stress(string fileName)
        {
            if (logFile == null)
            {
                random = new Random();
                executions = new List<Execution>();

                logFile = File.CreateText(fileName);
                ent = File.CreateText(fileName.Replace("_log.csv", "ent.txt"));
                sai = File.CreateText(fileName.Replace("_log.csv", "sai.txt"));
                logFile.AutoFlush = true;
                logFile.WriteLine("{0};{1};{2};{3};{4};{5};{6};{7};",
                   "#",
                  "Inicio Execucao",
                  "Dados Enviados",
                  "Retorno",
                  "Erro",
                  "Fim Execucao",
                  "Tempo de Execucao",
                  "Status");
            }
        }

        public bool IsExecuting()
        {
            return executingItens > 0;
        }

        public void Execute(string data)
        {
            for (int i = 0; i < 10; i++)
            {
                Execution execution = new Execution();
                execution.SDate = DateTime.Now;
                string curdata = data.Replace(@"@quote", (1 + i).ToString().PadLeft(15, '0').Substring(0, 15));
                execution.ErrorData = string.Empty;
                execution.SentData = curdata;
                try
                {
                    execution.StartDate = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss.fff");
                    var result = CallAPI(curdata);
                    execution.ExecutionFinished = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss.fff");
                    execution.ResultData = result.Value;
                    if (result.Key)
                        execution.State = "SUCCESS";
                    else
                        execution.State = "ERROR";
                    execution.EFinished = DateTime.Now;
                    execution.ExecutionId = executed + 1;
                }
                catch (Exception ex)
                {
                    execution.State = "ERROR";
                    execution.ExecutionFinished = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss.fff");
                    execution.EFinished = DateTime.Now;
                    execution.ErrorData = ex.Message;
                    execution.ExecutionId = executed + 1;
                }
                finally
                {
                    executed++;
                    WriteToFile(execution);
                }
            }
        }

        private bool WriteToFile(Execution execution)
        {
            try
            {
                locker.AcquireWriterLock(int.MaxValue);
                logFile.WriteLine("{0};{1};{2};{3};{4};{5};{6};{7};",
                        execution.ExecutionId,
                        execution.StartDate,
                        execution.SentData.Replace(Environment.NewLine, " "),
                        string.IsNullOrEmpty(execution.ResultData) ? "" : execution.ResultData.Replace(Environment.NewLine, " "),
                        string.IsNullOrEmpty(execution.ErrorData) ? "" : execution.ErrorData.Replace(Environment.NewLine, " "),
                        execution.ExecutionFinished,
                        (execution.EFinished - execution.SDate).TotalSeconds,
                        execution.State);
                executions.Add(execution);
                Console.Write(string.Format("\r{0} chamadas executadas", executions.Count));
                ent.WriteLine(execution.SentData);
                sai.WriteLine(execution.ResultData);
                return true;
            }
            catch
            {
                return false;
            }
            finally
            {
                locker.ReleaseWriterLock();
            }
        }

        public void CloseFile(double elapsed)
        {
            logFile.WriteLine("Execucoes por segundo :;{0};", elapsed);
            Console.Write(string.Format("\r{0} execuções por segundo", elapsed));
            logFile.Close();
            ent.Close();
            sai.Close();
        }

        private KeyValuePair<bool, string> CallAPI(string data)
        {
            KeyValuePair<bool, string> result = new KeyValuePair<bool, string>(true, string.Empty);
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(apiUrl);
            request.Timeout = 96000;

            request.Method = "POST";

            System.Text.UTF8Encoding encoding = new System.Text.UTF8Encoding();
            Byte[] byteArray = encoding.GetBytes(data);

            request.ContentLength = byteArray.Length;
            request.ContentType = @"application/json";

            using (Stream dataStream = request.GetRequestStream())
            {
                dataStream.Write(byteArray, 0, byteArray.Length);
            }

            try
            {
                using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                {
                    using (Stream stream = response.GetResponseStream())
                    {
                        StreamReader reader = new StreamReader(stream, Encoding.UTF8);
                        result = new KeyValuePair<bool, string>(true, reader.ReadToEnd());
                    }
                }
            }
            catch (WebException ex)
            {
                if (ex.Response != null && ((HttpWebResponse)ex.Response).StatusCode == HttpStatusCode.BadRequest)
                {
                    using (Stream stream = ex.Response.GetResponseStream())
                    {
                        StreamReader reader = new StreamReader(stream, Encoding.UTF8);
                        result = new KeyValuePair<bool, string>(false, reader.ReadToEnd());
                    }
                }
                else
                    throw ex;
            }
            return result;
        }
    }
}