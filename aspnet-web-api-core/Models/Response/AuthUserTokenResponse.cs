namespace AspNetWebApiCore.Models.Response
{
    public class AuthUserTokenResponse
    {
        public int ClientID { get; set; }
        public long PatientID { get; set; }     //will be 0 Incase of HealthCareUnit 
        public string ClientKey { get; set; }   // GUID to be used for each Patient or HealthCareUnit
        public string API_Token { get; set; }
        public DateTimeOffset? Token_IssuedTime { get; set; }
        public DateTimeOffset? Token_ExpiredTime { get; set; }
    }
}
