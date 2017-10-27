using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JobScraperApp
{
    public class Job
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }
        public long JobId { get; set; }
        public DateTime PostDate { get; set; }
        public string JobTitle { get; set; }
        public int SalaryMax { get; set; }
        public int SalaryMin { get; set; }
        public int Salary { get; set; }
        public string Company { get; set; }
    }
}
