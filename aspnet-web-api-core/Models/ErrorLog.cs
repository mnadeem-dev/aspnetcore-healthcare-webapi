namespace AspNetWebApiCore.Models
{
    public class ErrorLog
    {
        public string Message { get; set; }
        public string InnerException { get; set; }
        public string StackTrace { get; set; }
        public string UrlReferer { get; set; }
        public string Controller { get; set; }
        public string Action { get; set; }
        public long? HealthCareUnit_ID { get; set; }
        public long? Patient_ID { get; set; }
    }
}
