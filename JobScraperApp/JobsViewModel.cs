using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JobScraperApp
{
    public class JobsViewModel
    {
        public ObservableCollection<Job> Jobs { get; set; }
       
        public JobsViewModel(string position, string ssalary, string esalary)
        {
            var context = new JobContext();

           if(position.Length>0)
                Jobs = new ObservableCollection<Job>(context.Jobs.Where(j => j.JobTitle.Contains(position)));
           else
                Jobs = new ObservableCollection<Job>(context.Jobs);

           
                int salaryfrom = 0;
            int salaryto = 0;
            int.TryParse(ssalary, out salaryfrom);
            int.TryParse(esalary, out salaryto);

            if(salaryfrom > 0)
                Jobs = new ObservableCollection<Job>( Jobs.Where(j => j.SalaryMin>=salaryfrom ));
            if (salaryto > 0)
                Jobs = new ObservableCollection<Job>(Jobs.Where(j => j.SalaryMax >= salaryto));


        }

        public JobsViewModel()
        {
            var context = new JobContext();
            Jobs = new ObservableCollection<Job>(context.Jobs);

        }





    }
    }

