using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace AzureBackupTool.Models
{
    public class ExceptionLog
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public DMSServiceInfo DMSServiceInfo { get; set; }
        public int DMSServiceInfoId { get; set; }
        public string ExceptionMessage { get; set; }
        public string StackTrace { get; set; }
        public string InnerException { get; set; }
        public DateTime CreatedDate { get; set; } = DateTime.Now;
    }
}
