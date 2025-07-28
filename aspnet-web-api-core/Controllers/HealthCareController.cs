
using Asp.Versioning;
using AspNetWebApiCore.Common;
using AspNetWebApiCore.DataRepository;
using AspNetWebApiCore.Models;
using AspNetWebApiCore.Models.Entities;
using AspNetWebApiCore.Models.Request;
using AspNetWebApiCore.Models.Response;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Security.Claims;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace AspNetWebApiCore.Controllers
{

    /// <summary>
    /// This controller is fully authorized for Healthcare Roles and Audiances ( Token being used here,was created based on HealthCareUnit-GUID)
    /// Provides endpoints for managing operations for HealthCare Units.
    /// </summary>
    /// <remarks>This controller includes methods for searching patients by phone number,  It
    /// enforces role-based and policy-based authorization to ensure secure access to its endpoints.
    /// </remarks>

    [ApiController]

    [ApiVersion("1.0")]
    [ApiVersion("2.0")]

    [Route("v{version:apiVersion}/healthcare-telecomm-service")]

    [Authorize(Roles = "TeleComm")]
    [Authorize(Policy = "TeleCommAudience")]   // To enforce a named authorization policy called "TeleCommAudience", which was added in program.cs file as:  options.AddPolicy("TeleCommAudience" => options)

    public class HealthCareController : ApiBaseController
    {


        private readonly HealthCareRepository _repository;
        public HealthCareController(IHttpContextAccessor httpContextAccessor, HealthCareRepository Repost, ErrorLogRepository errRepos) : base(httpContextAccessor, errRepos)
        {
            _repository = Repost;
        }
        
        private BaseResponse<string> HandleException(Exception ex)
        {
            exceptionManager.LogException(ex, HealthCareUnit_ID).ConfigureAwait(false).GetAwaiter().GetResult();
            var error_response = ApiResponseHelper<string>.FailResponse(Client_ID, ex.Message);            
            return error_response;
        }
        /// <summary>
        /// This endpoint is handling request for searching patients by phone number.
        /// </summary>
        /// <param name="Phone"></param>
        /// <returns></returns>

        [Authorize(Roles = "TeleComm")]
        [Authorize(Policy = "TeleCommAudience")]   // To enforce a named authorization policy called "PatientAudience", which was added in program.cs file as:  options.AddPolicy("PatientAudience", policy => )
        [HttpGet("search-patients-by-phone/{Phone}")]
        [MapToApiVersion("1.0")]
        public async Task<ActionResult<BaseResponse<List<PatientInfo>>>> GetPatientInfo_ByPhone(string Phone)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(Phone)) 
                {
                    var resp = ApiResponseHelper<string>.BadRequest(Client_ID, "Empty Parameter");
                    return StatusCode((int)HttpStatusCode.BadRequest, resp);
                }
                
                var response = await _repository.GetPatients_ByPhone(Phone, HealthCareUnit_ID, Client_ID);
                return StatusCode(response.Status == true ? (int)HttpStatusCode.OK : (int)HttpStatusCode.NotFound, response);
            }
            catch (Exception ex)
            {
               var errorResponse = HandleException(ex);
                return StatusCode((int)HttpStatusCode.InternalServerError, errorResponse);
            }
        }

        [HttpGet("crash-api")]
        [MapToApiVersion("1.0")]
        public IActionResult Crash()
        {
            throw new Exception("Something went wrong!");
        }

    }
}
