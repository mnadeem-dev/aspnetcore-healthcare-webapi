using Asp.Versioning;
using AspNetWebApiCore.Common;
using AspNetWebApiCore.DataRepository;
using AspNetWebApiCore.Models.Request;
using AspNetWebApiCore.Models.Response;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.Data;
using System.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Runtime;
using System.Security.Claims;
using System.Text;



namespace AspNetWebApiCore.Controllers
{
    /// <summary>
    /// This Controller Handles operations related to JWT tokens.
    /// </summary>

    [ApiController]
    [ApiVersion("1.0")]
    [ApiVersion("2.0")]  
    //[Route("v{version:apiVersion}/[controller]")]
    [Route("v{version:apiVersion}/auth")]
    public class AuthController : ControllerBase
    {
        private readonly IRoles _roles;
        private readonly IConfiguration _config;
        private readonly AuthRepository _Repos;

        public AuthController(IRoles role, IConfiguration config)
        {
            _roles = role;
            _config = config;          
            _Repos = new AuthRepository(_config);
          
        }

        #region Single-Token API end-point for All Roles
        // Single token End point for multiple roles

        /// <summary>
        /// To get token based on role for different users. This API is for verification client-key and verify Api user credentials to gett Api token. 
        /// </summary>

        [HttpPost("login")]
        [MapToApiVersion("1.0")]
        public async Task<IActionResult> LoginV1([FromBody] AuthUserTokenRequest api_user_req)
        {
            var user_role = _roles.FindByUsername(api_user_req.UserName);
            if (user_role is null || !user_role.Enabled) 
                return Unauthorized("Invalid API User!");

            if (api_user_req == null
                || api_user_req.ClientKey == ""
                || api_user_req.UserName == ""
                || api_user_req.Password == ""                
                || api_user_req.Password != user_role.Password
               )
            {
                return Unauthorized("Invalid API User Credentials provided!");

            }
            // TODO: Verify ClientKey from db: first check Role based on username and then to decide either to get patient GUID or HealthCareUnit GUID
            // NOTE: UserName cannot be duplicated across JwtUsers[] Array 
            AuthUserTokenResponse ApiTokenResp = null;

            ApiTokenResp = await _Repos.GetDbSettings(api_user_req.ClientKey, user_role.UserCategory); // Based on Role decide either to get patient GUID or HealthCareUnit GUID from db

            if (user_role.UserCategory == "Patient")
            {
                if (ApiTokenResp == null
                    || ApiTokenResp.ClientID == 0
                    || ApiTokenResp.PatientID == 0)
                {
                    return Unauthorized("Client not found!");
                }
            }
            if (user_role.UserCategory == "HealthCareUnit")
            {
                if (ApiTokenResp == null 
                    || ApiTokenResp.ClientID == 0  )           
                {
                    return Unauthorized("Client not found!");
                }
            }
            await Task.Run(() =>
            {
                ApiTokenResp = GenerateJwtToken(api_user_req, ApiTokenResp.ClientID, ApiTokenResp.PatientID, user_role);              
            });

            return Ok(ApiTokenResp);

        }

        #endregion

        #region Patient-Token API end-point - Based on Patient GUID (being used for NextJS App)

        /// <summary>
        /// To get token for patient module. This API is for verification of patient client-key and verify Api user credentials and to getting Api token. 
       /// </summary>

        [HttpPost("patient-token")]
        [MapToApiVersion("1.0")]
        public async Task<IActionResult> PatientLoginV1([FromBody] AuthUserTokenRequest api_user_req)
        {

            var user_role = _roles.FindByUsername(api_user_req.UserName);
            if (user_role is null || !user_role.Enabled)
                return Unauthorized("Invalid API User!");

            if (api_user_req == null
                || api_user_req.ClientKey == ""
                || api_user_req.UserName == ""
                || api_user_req.Password == ""             
                || api_user_req.Password != user_role.Password
               )
            {
                return Unauthorized("Invalid API User Credentials provided!");

            }
            //TODO: Verify ClientKey from db
            AuthUserTokenResponse Patient_ApiTokenResp = null;

            //============

            // TODO: Verify ClientKey from db: first check Role based on username and then to decide either to get patient GUID or HealthCareUnit GUID
            // NOTE: UserName can not be duplicated across JwtUsers[] Aray

            AuthUserTokenResponse ApiTokenResp = null;          
            ApiTokenResp = await _Repos.GetDbSettings(api_user_req.ClientKey, user_role.UserCategory); // Based on Role decide either to get patient GUID or HealthCareUnit GUID from db
            
            if (ApiTokenResp == null
                || ApiTokenResp.ClientID == 0
                || ApiTokenResp.PatientID == 0)
            {
                return Unauthorized("Client not found!");
            }

            AuthUserTokenRequest token_req = new AuthUserTokenRequest
            {
                ClientKey = api_user_req.ClientKey,
                UserName = api_user_req.UserName,
                Password = api_user_req.Password
            };
            await Task.Run(() =>
            {
                ApiTokenResp = GenerateJwtToken(token_req, ApiTokenResp.ClientID, ApiTokenResp.PatientID, user_role);            });
            //============

            Patient_ApiTokenResp = new AuthUserTokenResponse
            {
                ClientID = ApiTokenResp.ClientID,
                PatientID = ApiTokenResp.PatientID,
                ClientKey = api_user_req.ClientKey,
                API_Token = ApiTokenResp.API_Token,
                Token_IssuedTime = ApiTokenResp.Token_IssuedTime,
                Token_ExpiredTime = ApiTokenResp.Token_ExpiredTime
            };

            return Ok(Patient_ApiTokenResp);

        }

        #endregion

        #region HealthCareUnit-Token API end-point - Based on HealthCareUnit GUID

        /// <summary>
        /// To get token for HealthCareUnit module. This API is for verification of HealthCareUnit client-key and verify Api user credentials and for getting Api token. 
        /// </summary>

        [HttpPost("healthcare-unit-token")]
        [MapToApiVersion("1.0")]
        public async Task<IActionResult> HealthCareUnitLoginV1([FromBody] AuthUserTokenRequest api_user_req)
        {

            var user_role = _roles.FindByUsername(api_user_req.UserName);
            if (user_role is null || !user_role.Enabled)
                return Unauthorized("Invalid API User!");

            if (api_user_req == null
                || api_user_req.ClientKey == ""
                || api_user_req.UserName == ""
                || api_user_req.Password == ""             
                || api_user_req.Password != user_role.Password
               )
            {
                return Unauthorized("Invalid API User Credentials provided!");

            }

            //TODO: Verify ClientKey from db
            AuthUserTokenResponse HealthCareUnit_ApiTokenResp = null;

            //============

            // TODO: Verify ClientKey from db: first check Role based on username and then to decide either to get patient GUID or HealthCareUnit GUID
            // NOTE: UserName can not be duplicated across JwtUsers[] Aray

            AuthUserTokenResponse ApiTokenResp ;
            ApiTokenResp = await _Repos.GetDbSettings(api_user_req.ClientKey, user_role.UserCategory); // Based on Role decide either to get patient GUID or HealthCareUnit GUID from db

            if ( ApiTokenResp == null
              || ApiTokenResp.ClientID == 0
               )
            {
                return Unauthorized("Client not found!");
            }

            AuthUserTokenRequest token_req = new AuthUserTokenRequest
            {
                ClientKey = api_user_req.ClientKey,
                UserName = api_user_req.UserName,
                Password = api_user_req.Password
            };
            await Task.Run(() =>
            {
                ApiTokenResp = GenerateJwtToken(token_req, ApiTokenResp.ClientID, ApiTokenResp.PatientID, user_role);
            });

            //============
            HealthCareUnit_ApiTokenResp = new AuthUserTokenResponse
            {
                ClientID = ApiTokenResp.ClientID,              
                ClientKey = api_user_req.ClientKey,
                API_Token = ApiTokenResp.API_Token,
                Token_IssuedTime = ApiTokenResp.Token_IssuedTime,
                Token_ExpiredTime = ApiTokenResp.Token_ExpiredTime
            };

            return Ok(HealthCareUnit_ApiTokenResp);

        }

        #endregion

        #region Generate JWT Token
        private AuthUserTokenResponse GenerateJwtToken(AuthUserTokenRequest api_user_req, int _ClientID, long PatientID, UserAccount user_role)
        {
            AuthUserTokenResponse ApiToken = new AuthUserTokenResponse();

            var tokenHandler = new JwtSecurityTokenHandler();

            //The JWT_SECRET_KEY must be at least 32 characters long.
            var JwtKey = Encoding.UTF8.GetBytes(GlobalAppSettings.ApiSecretKey);          
            double exp_minutes = Convert.ToDouble(user_role.TokenLifetimeMinutes);


            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Name, api_user_req.UserName),
                new Claim(ClaimTypes.NameIdentifier, _ClientID.ToString()),//TODO: Needs to read client_Id from DB //Convert.ToString(api_user.Client_ID)
                new Claim(ClaimTypes.Name,api_user_req.ClientKey ), //Guid.NewGuid().ToString()
                new Claim(ClaimTypes.PostalCode,PatientID.ToString()  ),
                new Claim(ClaimTypes.Role, user_role.RoleName)               
            };

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims), 
                Expires = DateTime.UtcNow.AddMinutes(exp_minutes), 
                Issuer = GlobalAppSettings.Issuer, // Must match as mention in programm.cs
                Audience = user_role.Audience, // Must match as mention in programm.cs
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(JwtKey), SecurityAlgorithms.HmacSha256),
                
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            string _token_string = tokenHandler.WriteToken(token);

            DateTime vallid_from = token.ValidFrom;
            DateTime valid_to = token.ValidTo;

            ApiToken.ClientID = _ClientID;
            ApiToken.PatientID = PatientID;
            ApiToken.ClientKey = api_user_req.ClientKey;
            ApiToken.API_Token = _token_string;
            ApiToken.Token_IssuedTime = vallid_from;
            ApiToken.Token_ExpiredTime = valid_to;

            return ApiToken;
        }
        #endregion
    }
}
