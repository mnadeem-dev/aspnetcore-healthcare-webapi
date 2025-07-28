
using AspNetWebApiCore.Common;
using AspNetWebApiCore.Models.Entities;
using AspNetWebApiCore.Models.Response;
using System.Text.Json;

namespace AspNetWebApiCore.DataRepository
{
    public class PatientRepository : BaseRepository
    {     
        public PatientRepository(IConfiguration configuration) : base(configuration)
        {
        }  
        public async Task<BaseResponse<PatientInfo>> GetPatientInfo(long PatientId, int HealthCareUnit_ID, int Client_ID)
        {
            try
            {
                var response = new BaseResponse<PatientInfo>();
                var _Info = new PatientInfo();
                _Info = await GetPatientByPatientIdAsync(PatientId, HealthCareUnit_ID);               
                if (_Info != null)
                {
                    response = ApiResponseHelper<PatientInfo>.SuccessResponse(_Info, Client_ID);
                }
                else
                {
                    response = ApiResponseHelper<PatientInfo>.NoContent(Client_ID);
                }
                return response;
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally 
            { 
               // Ensure that any necessary cleanup is done here, such as closing database connections or releasing resources 
            }
        }      
        public async Task<IEnumerable<PatientInfo>> GetAllAsync()
        {
            var data = await File.ReadAllTextAsync(PatientFilePath);
            return JsonSerializer.Deserialize<List<PatientInfo>>(data) ?? new();
        }
        public async Task<PatientInfo?> GetPatientByPatientIdAsync(long PatientId, int HealthCareUnit_ID)
        {
            var Info = await GetAllAsync();
            return Info.FirstOrDefault(p => p.Patient_ID == PatientId && p.HealthCareUnit_ID == HealthCareUnit_ID);
        }

    }
}