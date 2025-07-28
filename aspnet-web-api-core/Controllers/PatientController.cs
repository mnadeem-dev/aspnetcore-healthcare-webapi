
using Asp.Versioning;
using AspNetWebApiCore.Common;
using AspNetWebApiCore.DataRepository;
using AspNetWebApiCore.Models.Entities;
using AspNetWebApiCore.Models.Response;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Text.RegularExpressions;
using static System.Net.Mime.MediaTypeNames;

namespace AspNetWebApiCore.Controllers
{
    /// <summary>
    /// This controller is fully authorized for Patient Roles and Patient Audiances ( Token being used here, was created based on Patient-GUID)
    /// This controller will be used to manage patient related operations such as getting patient profile data.
    /// </summary>
    /// 
    [ApiController]

    [ApiVersion("1.0")]
    [ApiVersion("2.0")]

    [Route("v{version:apiVersion}/patient")]   
   
    [Authorize(Roles = "Patient")]
    [Authorize(Policy = "PatientAudience")]  //Instructs ASP.NET Core to enforce a named authorization policy called "PatientAudience". which was added in program.cs file as:  options.AddPolicy("PatientAudience", policy => )


    public class PatientController : ApiBaseController
    {
        
        private readonly PatientRepository _repository;
        public PatientController(IHttpContextAccessor httpContextAccessor, PatientRepository patRepost, ErrorLogRepository errRepos) : base(httpContextAccessor, errRepos)
        {
            _repository = patRepost;
        }
      
        /// <summary>
        /// To get patient detail based on passed patient-id and token. 
        /// This API is for getting patient basic profile data. 
        /// This api will be used where ever we need patient details
        /// </summary>

 
        [HttpGet("get-patient-info/{PatientId}")]
        [MapToApiVersion("1.0")]
        public async Task<ActionResult<BaseResponse<PatientInfo>>> GetPatientInfoById(int PatientId)
        {
            try
            {
                var response = await _repository.GetPatientInfo(PatientId, HealthCareUnit_ID,Client_ID);
                return StatusCode(response.Status == true ? (int)HttpStatusCode.OK : (int)HttpStatusCode.NotFound, response);
            }
            catch (Exception ex)
            {               
                var errorResponse = HandleException(ex);
                return StatusCode((int)HttpStatusCode.InternalServerError, errorResponse);
            }
        }

        private BaseResponse<string> HandleException(Exception ex)
        {
            exceptionManager.LogException(ex, HealthCareUnit_ID).ConfigureAwait(false).GetAwaiter().GetResult();
            var error_response = ApiResponseHelper<string>.FailResponse(Client_ID, ex.Message);          
            return error_response;
        }

        [HttpGet("crash-api")]
        [MapToApiVersion("1.0")]
        public IActionResult Crash()
        {
            throw new Exception("Something went wrong!");
        }



    }
}
