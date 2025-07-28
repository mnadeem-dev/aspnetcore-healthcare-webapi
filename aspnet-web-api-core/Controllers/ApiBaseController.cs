using AspNetWebApiCore.Common;
using AspNetWebApiCore.DataRepository;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

//using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.JsonWebTokens;
using System.Security.Claims;

namespace AspNetWebApiCore.Controllers
{
 
    [ApiController]
    public abstract class ApiBaseController : ControllerBase
    {
        protected readonly ExceptionManager exceptionManager;

        protected readonly Int32 Client_ID;
        protected readonly Int32 HealthCareUnit_ID;
        protected readonly long Patient_ID;

        protected readonly string ClientKey;
        //protected readonly string PatientKey;
        protected readonly string UserName;
        protected readonly string Password;
       
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ClaimsPrincipal _user;
        protected ApiBaseController(IHttpContextAccessor httpContextAccessor, ErrorLogRepository errorRepos)
        {

            exceptionManager = new ExceptionManager(errorRepos, httpContextAccessor.HttpContext);

            _httpContextAccessor = httpContextAccessor;
            _user = _httpContextAccessor.HttpContext.User;

            string _role = _user.FindFirst(ClaimTypes.Role).Value;
            string ClientId = _user.FindFirst(ClaimTypes.NameIdentifier).Value;
            string _Guid = _user.FindFirst(ClaimTypes.Name).Value;
            string UserName = _user.FindFirst(JwtRegisteredClaimNames.Name).Value;

            HealthCareUnit_ID = Convert.ToInt32(ClientId);
            Client_ID = Convert.ToInt32(ClientId);
            ClientKey = _Guid;
            string patientID = "0";
            //if (_role == "Patient")
            //{
            patientID = _user.FindFirst(ClaimTypes.PostalCode).Value;
            Patient_ID = Convert.ToInt64(patientID);
            // }

        }
       
    }
}
