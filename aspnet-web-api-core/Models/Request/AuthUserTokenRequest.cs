using System.ComponentModel.DataAnnotations;

namespace AspNetWebApiCore.Models.Request
{
    public class AuthUserTokenRequest
    {
        [Required] 
        public string ClientKey { get; set; } // GUID to be used for each patient or HealthCareUnit related token creation

        [Required]
        public string UserName { get; set; }

        [Required]
        public string Password { get; set; }
    }
}
