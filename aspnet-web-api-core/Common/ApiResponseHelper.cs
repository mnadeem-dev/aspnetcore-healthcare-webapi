
using AspNetWebApiCore.Models.Response;
using System.ComponentModel;

namespace AspNetWebApiCore.Common
{
    public class ApiResponseHelper<T>
    {
        

        //******* Regular method syntax:*******
       
        public static BaseResponse<T> SuccessResponse(T data, int ClientID=0, string message = "Success")
        {            
            return new BaseResponse<T> 
            {   
                Status = true, 
                Reason = ResponseReason.EnumReason.Success,
                ClientID = ClientID,
                Message = message, 
                Data = data 
            };
        }

        public static BaseResponse<T> FailResponse( int ClientID = 0, string message="Error")
        {
            return new BaseResponse<T>
            {
                Status = false,
                Reason = ResponseReason.EnumReason.Error,
                ClientID = ClientID,
                Message = message, //ResponseReason.ToDescriptionString(ResponseReason.EnumReason.Error), //message,
                Data = default
            };
        }
        public static BaseResponse<T> NoContent(int ClientID = 0, string message = "No Content")
        {
            return new BaseResponse<T>
            {
                Status = false,
                Reason = ResponseReason.EnumReason.NotFound,
                ClientID = ClientID,
                Message = ResponseReason.ToDescriptionString(ResponseReason.EnumReason.NotFound), //message,
                Data = default
            };
        }
        public static BaseResponse<T> BadRequest(int ClientID = 0, string message = "Bad Request")
        {
            return new BaseResponse<T>
            {
                Status = false,
                Reason = ResponseReason.EnumReason.EmptyParameter,
                ClientID = ClientID,
                Message = message,
                Data = default
            };
        }
    }
}
