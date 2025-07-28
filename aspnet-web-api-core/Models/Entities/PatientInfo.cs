using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AspNetWebApiCore.Models.Entities
{
    public class PatientInfo
    {
        public int HealthCareUnit_ID { get; set; }
        public long Patient_ID { get; set; }
        public string FirstName { get; set; }
        public string SurName { get; set; }
        public DateTime DOB { get; set; }
        public string Patient_PostCode { get; set; }
        public string Email { get; set; }       
        public string Gender { get; set; }  
        public string Address_Line { get; set; }
        public string Address_Line2 { get; set; }
        public string Address_Line3 { get; set; }
        public string Address_Line4 { get; set; }
        public string Address_Line5 { get; set; }
        public string City { get; set; }
        public string Phone1 { get; set; }
        public string Phone2 { get; set; }    
        public string GUID { get; set; } // This is the GUID of the Patient of a specific HealthCareUnit, which is used for authorization purposes in Patient related operations.

    }
}