using AzureBackupTool.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using Microsoft.Azure.Management;
using Microsoft.Azure.Management.Storage.Models;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using System.Threading.Tasks;

namespace AzureBackupTool
{
    class Program
    {
        public static IConfigurationRoot Configuration;
        static void Main(string[] args)
        {
           
            // Set up configuration sources.
            var builder = new ConfigurationBuilder()
                .SetBasePath(Path.Combine(AppContext.BaseDirectory))
                .AddJsonFile("appSettings.json", optional: true);

            Configuration = builder.Build();
            
            try
            {
                GetDataForBackup();
            }
            catch(Exception ex)
            {
                throw ex;
            }
        }

        public static void GetDataForBackup()
        {
            using (var context = new AppDBContext(Configuration))
            {
                IList<DMSServiceInfo> dmsServiceList = new List<DMSServiceInfo>();
                dmsServiceList = context.DMSServiceInfo.AsNoTracking()
                                 .Where(x => x.AzStorageConnectionString != null && !x.IsDeleted && x.WantBackup)
                                 .ToList();

                string destinationConnectionString = Configuration.GetSection("GenericStorageSettings:ConnectionString").Value;
                BlobServiceClient destBlobClient = new BlobServiceClient(destinationConnectionString);
                var destContainers = destBlobClient.GetBlobContainers();

                //Create Containers if destination does not have
                foreach (var service in dmsServiceList)
                {
                    service.AzStorageContainer = service.AzStorageContainer.ToLower();
                    if (destContainers.Where(b => b.Name == service.AzStorageContainer).Count() == 0)
                    {
                        destBlobClient.CreateBlobContainer(service.AzStorageContainer);
                    }
                    
                    BlobContainerClient sourceContainerClient = new BlobContainerClient(service.AzStorageConnectionString.Replace(" ", ""), service.AzStorageContainer.Replace(" ", ""));
                    BlobContainerClient destContainerClient = destBlobClient.GetBlobContainerClient(service.AzStorageContainer);

                    try
                    {
                        DeleteOldFiles(destContainerClient);
                    }
                    catch(Exception ex)
                    {
                        ExceptionLog log = new ExceptionLog();
                        log.DMSServiceInfoId = service.Id;
                        log.InnerException = ex.InnerException.ToString();
                        log.StackTrace = ex.StackTrace.ToString();
                        log.ExceptionMessage = "Error Executing while Backup- Deleting old files:: "+ ex.Message.ToString();
                        context.ExceptionLogs.Add(log);
                        context.SaveChanges();
                        throw ex;
                    }

                    try
                    {
                        ExecuteBackup(context,service, sourceContainerClient, destContainerClient).ConfigureAwait(false);
                    }
                    catch (Exception ex)
                    {
                        ExceptionLog log = new ExceptionLog();
                        log.DMSServiceInfoId = service.Id;
                        log.InnerException = ex.InnerException.ToString();
                        log.StackTrace = ex.StackTrace.ToString();
                        log.ExceptionMessage = "Error Executing while Backup- Executing Backup:: " + ex.Message.ToString();
                        context.ExceptionLogs.Add(log);
                        context.SaveChanges();
                        throw ex;
                    }

                }
            }
        }

        public static void DeleteOldFiles(BlobContainerClient destContainerClient)
        {
                var blobs = destContainerClient.GetBlobs().ToList();
                bool dailyBackup = Configuration.GetSection("ScheduleSettings:Daily").Value.ToLower() == "true" ? true : false;
                bool weeklyBackup = Configuration.GetSection("ScheduleSettings:Weekly").Value.ToLower() == "true" ? true : false;
                foreach (var blob in blobs)
                {
                    if (dailyBackup && blob.Name.Contains("Daily"))
                    {
                        destContainerClient.DeleteBlob(blob.Name);
                    }
                    if (weeklyBackup && blob.Name.Contains("Weekly"))
                    {
                        destContainerClient.DeleteBlob(blob.Name);
                    }
                }
        }

        public static async Task ExecuteBackup(AppDBContext context,DMSServiceInfo service, BlobContainerClient sourceContainerClient, BlobContainerClient destContainerClient)
        {
            
            var blobList = sourceContainerClient.GetBlobs().ToList();
            bool dailyBackup = Configuration.GetSection("ScheduleSettings:Daily").Value.ToLower() == "true" ? true : false;
            bool weeklyBackup = Configuration.GetSection("ScheduleSettings:Weekly").Value.ToLower() == "true" ? true : false;

            int dailyCounter = 0;
            int weeklyCounter = 0;
            foreach (var blob in blobList)
            {
                BlobClient sourceBlob = sourceContainerClient.GetBlobClient(blob.Name);
                if (dailyBackup)
                {
                    BlobClient destBlob = destContainerClient.GetBlobClient("Daily/" + blob.Name);
                    // Start the copy operation.
                    destBlob.StartCopyFromUri(sourceBlob.Uri);
                    dailyCounter = dailyCounter + 1;
                }
                if(weeklyBackup)
                {
                    BlobClient destBlob = destContainerClient.GetBlobClient("Weekly/" + blob.Name);
                    // Start the copy operation.
                    destBlob.StartCopyFromUri(sourceBlob.Uri);
                    weeklyCounter = weeklyCounter + 1;
                }
            }

            if (dailyBackup)
            {
                AzureBackupLogs log = new AzureBackupLogs();
                log.Category = "Daily";
                log.ContainerName = service.AzStorageContainer;
                log.NoOfBackupFiles = dailyCounter;
                log.DMSServiceInfoId = service.Id;
                context.AzureBackupLogs.Add(log);
            }
            if(weeklyBackup)
            {
                AzureBackupLogs log = new AzureBackupLogs();
                log.Category = "Weekly";
                log.ContainerName = service.AzStorageContainer;
                log.NoOfBackupFiles = weeklyCounter;
                log.DMSServiceInfoId = service.Id;
                context.AzureBackupLogs.Add(log);
            }
            context.SaveChanges();
                    
        }
    }
}
