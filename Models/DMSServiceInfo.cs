using Npgsql.TypeHandlers;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace AzureBackupTool.Models
{
    public class DMSServiceInfo
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        [Required]
        public System.Guid TenantId { get; set; }
        [Required]
        public string DatabaseServer { get; set; }
        [Required]
        public string DatabaseUser { get; set; }
        [Required]
        public string DatabaseName { get; set; }
        [Required]
        public string DatabasePassword { get; set; }
        [Required]
        public string DatabasePort { get; set; }
        public string DMSTenantURL { get; set; }
        public string AzStorageConnectionString { get; set; }
        public string AzStorageContainer { get; set; }
        [Required]
        public Boolean IsDbProvisioned { get; set; } = false;
        [Required]
        public string Location { get; set; }
        [Required]
        public Boolean IsReplication { get; set; } = false;
        public Boolean IsDeleted { get; set; } = false;
        public Boolean WantBackup { get; set; } = true;
    }
}
