using AspNetWebApiCore.Common;

namespace AspNetWebApiCore.Models.Response
{
    public class BaseResponse<T>
    {
        public int ClientID { get; set; }
        public bool Status { get; set; }
        public string? Message { get; set; }
        public ResponseReason.EnumReason Reason { get; set; }
        public T? Data { get; set; }
    }
}
