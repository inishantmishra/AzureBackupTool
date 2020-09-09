using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace AzureBackupTool.Models
{
    public class AzureBackupLogs
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        [Required]
        public int DMSServiceInfoId { get; set; }
        [Required]
        public string ContainerName { get; set; }
        [Required]
        public int NoOfBackupFiles { get; set; }
        public string Category { get; set; }
        public DateTime CreatedOn { get; set; } = DateTime.Now;
    }
}
