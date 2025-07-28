using AspNetWebApiCore.Models;

using System.Data;
using System.Reflection.Metadata;

namespace AspNetWebApiCore.DataRepository
{
    public class ErrorLogRepository : BaseRepository
    {
        public ErrorLogRepository(IConfiguration configuration) : base(configuration)
        {
        }
        public async Task LogError(Exception ex, ErrorLog errorLog)
        {
            try
            {               
                await Task.Run(() =>
                 {
                     Serilog.Log.Error(ex, "Exception for clientID: " + errorLog.HealthCareUnit_ID + ", Controller: " + errorLog.Controller + ", ActionMethod: " + errorLog.Action);
                 });
            }
            catch (Exception)
            {
                // Handle any exceptions that occur while logging the error               
            }
            finally
            {
                // Ensure that the error log is saved to the database or any other storage if needed
                // This could be a call to a database repository method to save the errorLog object                
            }
        }
    }
}
