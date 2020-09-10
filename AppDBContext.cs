using AzureBackupTool.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Text;

namespace AzureBackupTool
{
    public class AppDBContext : DbContext
    {
        private readonly IConfiguration _Config;

        public AppDBContext(IConfiguration config)
        {
            _Config = config;
        }

        public AppDBContext(DbContextOptions<AppDBContext> options)
            : base(options)
        {
        }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            
            optionsBuilder.UseNpgsql(_Config.GetConnectionString("DefaultConnection"),
                options=>options.EnableRetryOnFailure());
        }
        public virtual DbSet<DMSServiceInfo> DMSServiceInfo { get; set; }
        public virtual DbSet<ExceptionLog> ExceptionLogs { get; set; }
        public virtual DbSet<AzureBackupLogs> AzureBackupLogs { get; set; }

    }
}
