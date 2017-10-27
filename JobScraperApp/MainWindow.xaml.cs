using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Xceed.Wpf.Toolkit;
using System.Security.Cryptography.X509Certificates;
using System.Net.Security;
using Xceed.Wpf.DataGrid;

namespace JobScraperApp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        public MainWindow()
        {

            InitializeDB();
            InitializeComponent();
        }

        private void InitializeDB()
        {
            
            using (JobContext jobContext = new JobContext())
            {
                try
                {
                    if (!jobContext.Database.Exists())
                        jobContext.Database.Create();
                }
                catch (Exception ex)
                {
                    Xceed.Wpf.Toolkit.MessageBox.Show("Ошибка инициализации базы данных:" + ex.Message, "Ошибка инициализации", MessageBoxButton.OK, MessageBoxImage.Error);
                    Thread.CurrentThread.Abort();
                    
                }
                
            }
        }

        private CancellationTokenSource cts = new CancellationTokenSource();

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            
        }

        private void DownloadData()
        {
            try
            {
                Dispatcher.Invoke(new Action(() =>
                {
                    txtCurrentStatus.Text = "Загрузка данных..";
                }));

                
                using (JobContext jobContext = new JobContext())
                {
                    jobContext.Database.ExecuteSqlCommand("TRUNCATE TABLE [Jobs]");
                }
                
                System.Net.ServicePointManager.ServerCertificateValidationCallback =
                   delegate (object sender, X509Certificate certificate,
                   X509Chain chain, SslPolicyErrors sslPolicyErrors)
                   { return true; };

                string InitialJsonString="";
                using (WebClient InitialWC = new WebClient())
                {
                    Thread.Sleep(100);
                    string InitialURL = "https://api.zp.ru/v1/vacancies?limit=25";

                    //Initial call to download data
                    Task.Factory.StartNew(() =>
                    {
                        InitialJsonString = InitialWC.DownloadString(InitialURL);
                    }).Wait();

                }
                    dynamic InitialJObject = JObject.Parse(InitialJsonString);

                    var Total = (int)InitialJObject.metadata.resultset.count.Value;

                    var Offset = (int)InitialJObject.metadata.resultset.offset.Value;

                    var Limit = (int)InitialJObject.metadata.resultset.limit.Value;

                    var Vacancies = InitialJObject.vacancies as JArray;

                    int rows = 0;

                    int Pages = Total / Limit;
                    rows += rows + adddata(Vacancies);
                    Dispatcher.Invoke(new Action(() =>
                    {
                        txtCurrentStatus.Text = "Добавлено записей: " + rows;
                    }));

                   
                    List<string> URLList = new List<string>();
                    for (int i = 25; i < 9000; i += Limit)
                    {
                        URLList.Add(string.Concat("https://api.zp.ru/v1/vacancies?limit=25&offset=", i));
                    }

                    
                    try
                    {
                        cts = new CancellationTokenSource();

                        ParallelOptions po = new ParallelOptions();
                        po.MaxDegreeOfParallelism = 5; 
                        po.CancellationToken = cts.Token;


                        Parallel.ForEach<string>(URLList, po, (url, loopState) =>
                        {

                            if (loopState.ShouldExitCurrentIteration || loopState.IsExceptional)
                                loopState.Stop();
                            try
                            {
                                using (WebClient webClient = new WebClient())
                                {
                                    var jsonString1 = webClient.DownloadString(url);
                                    dynamic jObject1 = JObject.Parse(jsonString1);
                                    var Vacancies1 = jObject1.vacancies as JArray;
                                    rows += adddata(Vacancies1);
                                    Dispatcher.Invoke(new Action(() =>
                                    {
                                        txtCurrentStatus.Text = "Добавлено записей: " + rows;
                                    }));
                                }
                            }
                            catch
                            {
                            }
                            if (loopState.ShouldExitCurrentIteration || loopState.IsExceptional)
                                loopState.Stop();
                        });
                    }




                    catch (Exception ex)
                    {
                        busyIndicator.IsBusy = false;
                       
                    }

                
            }
            catch (Exception ex)
            {
                cts.Cancel();
                Xceed.Wpf.Toolkit.MessageBox.Show("Ошибка:" + ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                busyIndicator.IsBusy = false;
                return;
            }

            

        }
        private int  adddata(JArray Vacancies)
        {
            try
            {
                //Add all the jobs in the list to DB
                using (JobContext jobContext = new JobContext())
                {
                    foreach (dynamic vacancy in Vacancies)
                    {
                        var id = (long)vacancy.id;

                         var job = new Job()
                        {
                            PostDate = (DateTime)vacancy.add_date,
                            JobTitle = (string)vacancy.header,
                            JobId = (long)vacancy.id,
                            SalaryMin = (int)vacancy.salary_min,
                            SalaryMax = (int)vacancy.salary_max,
                            Company = (string)vacancy.company.title
                        };
                        jobContext.Jobs.Add(job);
                       
                    }
                    jobContext.SaveChanges();
                }
            }
            catch (Exception ex)
            {

            }

           return  Vacancies.Count(); 
        }

        private void btnExit_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private async void btnDownloadData_Click(object sender, RoutedEventArgs e)
        {

            busyIndicator.IsBusy = true;
            try
            {
                await Task.Run(() => DownloadData());
            }
            catch
            {

            }
            busyIndicator.IsBusy = false;

        }

        private void btnAbort_Click(object sender, RoutedEventArgs e)
        {
            cts.Cancel();
            busyIndicator.IsBusy = false;
        }

        private void DataGridItemProperty_QueryDistinctValue_Date(object sender, QueryDistinctValueEventArgs e)
        {
            if (e.DataSourceValue is DateTime)
            {
                e.DistinctValue = ((DateTime)e.DataSourceValue).ToString("MMMM");
            }
        }

        private void _dataGrid_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {

        }

        private void _dataGrid_Loaded(object sender, RoutedEventArgs e)
        {

        }

        private void btnRefreshData_Click(object sender, RoutedEventArgs e)
        {
            JobsViewModel vm = new JobScraperApp.JobsViewModel(txtPosition.Text,txtSalaryFrom.Text,txtSalaryTo.Text);
            dataGrid.ItemsSource = null;
            dataGrid.ItemsSource = vm.Jobs ;
        }
    }
}

