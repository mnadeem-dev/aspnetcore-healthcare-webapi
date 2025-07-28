using System;
using System.Threading;
using System.Threading.Tasks;

using AspNetWebApiCore.Models;
using AspNetWebApiCore.DataRepository;
using System.IO;
using Microsoft.Extensions.Configuration;
namespace AspNetWebApiCore.Common
{
    public class ExceptionManager 

    {
        //private readonly IConfiguration _configuration;
        private readonly ErrorLogRepository _Error_LogRepos;
        private readonly HttpContext _httpContext;

        private static string RootPath = "";
        private static string LogFilePath = "";
        private static ReaderWriterLockSlim _readWriteLock = new ReaderWriterLockSlim();
        private static StreamWriter SW;

        public ExceptionManager(ErrorLogRepository erroLog_Repos,HttpContext http_context)
        {
            _Error_LogRepos = erroLog_Repos;
            _httpContext = http_context;
        }
        public async Task LogException(Exception exception, long? HealthCareUnit_ID = null)
        {
            var errorLog = new ErrorLog
            {
                HealthCareUnit_ID = HealthCareUnit_ID,
                InnerException = exception.InnerException?.Message,
                StackTrace = exception.StackTrace,
                Message = exception.Message,
            };

            if (_httpContext != null)
            {
                
                errorLog.UrlReferer = _httpContext.Request.Path;
                errorLog.Controller = _httpContext.GetRouteValue("controller").ToString();
                errorLog.Action = _httpContext.GetRouteValue("action").ToString();
            }
           
            await _Error_LogRepos.LogError(exception,errorLog);
            
        }
        public async Task LogInfo(string _exceptionMessage, string Info, long? HealthCareUnit_ID = null)
        {
            _readWriteLock.EnterWriteLock();
            try
            {
                RootPath = System.AppDomain.CurrentDomain.BaseDirectory + "\\Log\\Others";
                if (!Directory.Exists(RootPath))
                    Directory.CreateDirectory(RootPath);
                LogFilePath = RootPath + "\\ClientLog_" + HealthCareUnit_ID + ".txt";
                if (File.Exists(LogFilePath))
                    System.IO.File.WriteAllText(LogFilePath, string.Empty);
                // Append text to the file
                using (StreamWriter sw = File.AppendText(LogFilePath))
                {
                    sw.WriteLine(DateTime.Now.ToString("yyyy/MM/dd hh:mm:ss tt") + " : SQL Exception Message: " + _exceptionMessage + ". Other Info: " + Info);
                    sw.Close();
                }
            }
            catch (Exception ex)
            {
            }
            finally
            {
                // Release lock
                _readWriteLock.ExitWriteLock();
            }
        }
    }
}
