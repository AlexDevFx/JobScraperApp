using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JobScraperApp
{
    public class JobContext : DbContext
    {
        public JobContext() : base("name=JobDBParserConnectionString")
        {

        }

        public DbSet<Job> Jobs { get; set; }
    }
}