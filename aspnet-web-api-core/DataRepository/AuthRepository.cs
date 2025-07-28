using AspNetWebApiCore.Common;
using AspNetWebApiCore.Models.Entities;
using AspNetWebApiCore.Models.Response;
using System.Text.Json;

namespace AspNetWebApiCore.DataRepository
{
    public class AuthRepository : BaseRepository
    {

        //uses constructor injection to pass configuration settings (IConfiguration) from the ASP.NET Core dependency injection system to the base class (BaseRespository). 
        public AuthRepository(IConfiguration configuration) : base(configuration)
        {
        }

        #region Single Token for all User Category (Patient, HealthCareUnit)
        public async Task<AuthUserTokenResponse> GetDbSettings(string Client_GUID, string user_category)
        {
          
            var _response = new AuthUserTokenResponse();
            switch (user_category)
            {
                case "Patient":                   

                    var Info = await GetPatientByGuIdAsync(Client_GUID);
                    if (Info != null)
                    {
                        _response.ClientID = Info.HealthCareUnit_ID;
                        _response.ClientKey = Info.GUID;
                        _response.PatientID = Info.Patient_ID;
                    }
                    break;
                case "HealthCareUnit":

                    var unit = await GetAll_HealthCareUnits_ByGuIdAsync(Client_GUID);
                    if (unit != null)
                    {
                        _response.ClientID = unit.HealthCareUnit_ID;
                        _response.ClientKey = unit.GUID;                       
                    }
                    break;
                default:
                    break;
            }         

            return _response;    

        }
        #endregion

        public async Task<AuthUserTokenResponse> GetPatientSettings(string Patient_GUID)
        {          
            var _response = new AuthUserTokenResponse(); 
            var Info = await GetPatientByGuIdAsync(Patient_GUID);
            if (Info != null) {
                _response.ClientID = Info.HealthCareUnit_ID;
                _response.ClientKey = Info.GUID;
                _response.PatientID = Info.Patient_ID;
            }
            return _response;
        }
        public async Task<AuthUserTokenResponse> GetClientSettings(string Client_GUID)
        {          
            var _response = new AuthUserTokenResponse();
            var unit = await GetPatientByGuIdAsync(Client_GUID);
            if (unit != null)
            {
                _response.ClientID = unit.HealthCareUnit_ID;
                _response.ClientKey = unit.GUID;               
            }
            return _response;
        }

        #region Patient and Healthcare Unit Data Retrieval
        // patient json data     
        public async Task<PatientInfo?> GetPatientByGuIdAsync(string id)
        {
            var Info = await GetAll_Patients_Async();
            return Info.FirstOrDefault(p => p.GUID == id);
        }
        public async Task<IEnumerable<PatientInfo>> GetAll_Patients_Async()
        {
            var data = await File.ReadAllTextAsync(PatientFilePath);
            return JsonSerializer.Deserialize<List<PatientInfo>>(data) ?? new();
        }
        // healthcare unit json data       
        public async Task<HealthCareUnit?> GetAll_HealthCareUnits_ByGuIdAsync(string guid)
        {
            var unit = await GetAll_Units_Async();
            return unit.FirstOrDefault(h => h.GUID == guid);
        }
        public async Task<IEnumerable<HealthCareUnit>> GetAll_Units_Async()
        {
            var data = await File.ReadAllTextAsync(HealthCareUnitPath);
            return JsonSerializer.Deserialize<List<HealthCareUnit>>(data) ?? new();
        }

        #endregion
    }
}
