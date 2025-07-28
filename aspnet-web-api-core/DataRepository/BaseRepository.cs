

namespace AspNetWebApiCore.DataRepository
{
    public class BaseRepository
    {
        private readonly IConfiguration _configuration;
        public readonly string DataFolderPath = string.Empty;
        public readonly string PatientFilePath = string.Empty;
        public readonly string HealthCareUnitPath = string.Empty;

        public BaseRepository(IConfiguration configuration) 
        {
            _configuration = configuration;
            //== Get configuration from appsettings.json 
            DataFolderPath = _configuration["Data:folder"];  
            PatientFilePath = _configuration["Data:patient_data_file"];
            PatientFilePath = Path.Combine(DataFolderPath, PatientFilePath);
            HealthCareUnitPath = _configuration["Data:healthcareunit_data_file"];
            HealthCareUnitPath = Path.Combine(DataFolderPath, HealthCareUnitPath);
        }
    }  
}