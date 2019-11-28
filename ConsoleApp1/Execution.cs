using System;

namespace zurick
{
    internal class Execution
    {
        public long ExecutionId { get; set; }
        public string StartDate { get; set; }
        public DateTime SDate { get; set; }
        public string SentData { get; set; }
        public string ResultData { get; set; }
        public string ErrorData { get; set; }
        public string ExecutionFinished { get; set; }
        public DateTime EFinished { get; set; }
        public string State { get; set; }
    }
}