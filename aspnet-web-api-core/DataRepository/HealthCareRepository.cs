
using AspNetWebApiCore.Common;
using AspNetWebApiCore.Models.Entities;
using AspNetWebApiCore.Models.Response;
using System.Data;
using System.Text.Json;

namespace AspNetWebApiCore.DataRepository
{
    public class HealthCareRepository : BaseRepository
    {
       
        public HealthCareRepository(IConfiguration configuration) : base(configuration)
        {
        } 
        public async Task<BaseResponse<List<PatientInfo>>> GetPatients_ByPhone(string phone, int HealthCareUnit_ID, int Client_ID)
        {
            try
            {

                var response = new BaseResponse<List<PatientInfo>>();
                var  _list_PatientInfo = new List<PatientInfo>();
                _list_PatientInfo = (List<PatientInfo>)await GetPatientByPhoneAsync(phone, HealthCareUnit_ID);

                if (_list_PatientInfo != null)
                {
                    response = ApiResponseHelper<List<PatientInfo>>.SuccessResponse(_list_PatientInfo, Client_ID);
                }
                else
                {
                    response = ApiResponseHelper<List<PatientInfo>>.NoContent(Client_ID);
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
        public async Task<IEnumerable<PatientInfo>> GetAll_Patients_Async()
        {
            var data = await File.ReadAllTextAsync(PatientFilePath);

            return JsonSerializer.Deserialize<List<PatientInfo>>(data) ?? new();
        }
        public async Task<IEnumerable<PatientInfo>> GetPatientByPhoneAsync(string phone, int HealthCareUnit_ID)
        {
            var list_Info = await GetAll_Patients_Async();
            return list_Info.Where(p => (p.Phone1 == phone || p.Phone2 == phone) && p.HealthCareUnit_ID == HealthCareUnit_ID).ToList<PatientInfo>();
        }  
    }
}