namespace AspNetWebApiCore.Models.Entities
{
    public class HealthCareUnit
    {  
        public int HealthCareUnit_ID { get; set; }      
        public string Name { get; set; }
        public string Address1 { get; set; }
        public string Address2 { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;       
        public string PostCode { get; set; } = string.Empty;      
        public string Phone1 { get; set; } = string.Empty;  
        public string Phone2 { get; set; } = string.Empty;
        public string Fax { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string GUID { get; set; } = string.Empty; // This is the GUID of the HealthCareUnit, which is used for authorization purposes in TeleComm and other related operations.

    }
}
