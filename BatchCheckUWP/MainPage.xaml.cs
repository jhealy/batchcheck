using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

using DevFish.Azure.Batch.Common.Core;
using System.Text;

using Microsoft.Azure.Batch;
using Microsoft.Azure.Batch.Auth;

namespace BatchCheckUWP
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();
        }

        private void Button_Connect_Click(object sender, RoutedEventArgs e)
        {
            StringBuilder sb = new StringBuilder(1024);
            try
            {
                // read account settings, dump
                AccountSettings accountSettings = SampleHelpers.LoadAccountSettings();
                sb.AppendLine("--- accountSettings ---");
                sb.AppendLine(accountSettings.ToString());

                // read job settings, dump
                JobSettings jobSettings = SampleHelpers.LoadJobSettings();
                sb.AppendLine("--- jobSettings ---");
                sb.AppendLine(jobSettings.ToString());

                // connect to batch, dump status
                BatchSharedKeyCredentials cred = new BatchSharedKeyCredentials(
                    accountSettings.BatchServiceUrl,
                    accountSettings.BatchAccountName,
                    accountSettings.BatchAccountKey
                );

                sb.AppendLine($"credentials created: {cred.AccountName},{cred.BaseUrl}");

                using (BatchClient client = BatchClient.Open(cred))
                {
                    sb.AppendLine($"batchclient opened successfully");
                    // enumerate pools
                    sb.AppendLine("--- pools ---");
                    foreach (var pool in client.PoolOperations.ListPools())
                    {
                        sb.AppendLine($"pool found: id:{pool.Id} vmsize:{pool.VirtualMachineSize} state:{pool.State.ToString()}");
                    }
                    sb.AppendLine("--- applications ---");
                    foreach (var app in client.ApplicationOperations.ListApplicationSummaries())
                    {
                        sb.AppendLine($"application found: {app.Id} {app.Versions[0]}");
                    }
                } // batchclient

            }
            catch (AggregateException ae)
            {
                sb.AppendLine("aggregate exception caught");
                sb.AppendLine(SampleHelpers.AggregateExceptionDump(ae.Flatten()));
                System.Diagnostics.Trace.Write(sb.ToString(), "ERROR");

            }
            catch (Exception ex)
            {
                sb.AppendLine("exception caught");
                sb.AppendLine(ex.ToString());
                System.Diagnostics.Trace.Write(sb.ToString(), "ERROR");
            }

            TextBlock_Out.Text = sb.ToString();
        }

        private async void Button_JobA_Click(object sender, RoutedEventArgs e)
        {
            const string DQUOTE = "\"";

            string joba_task_cmdline = $"cmd /c {DQUOTE}set AZ_BATCH & timeout /t 30 > NUL{DQUOTE}";

            StringBuilder sb = new StringBuilder(1024);
            sb.AppendLine("Submitting job");
            string jobid = NamingHelpers.GenJobName("A");
            sb.AppendLine($"jobid={jobid}");
            string taskid = NamingHelpers.GenTaskName("JOBA");
            sb.AppendLine($"taskid={taskid}");

            sb.AppendLine($"task command line={joba_task_cmdline}");

            // read account settings, dump
            AccountSettings accountSettings = SampleHelpers.LoadAccountSettings();

            // read job settings, dump
            JobSettings jobSettings = SampleHelpers.LoadJobSettings();

            // connect to batch, dump status
            BatchSharedKeyCredentials cred = new BatchSharedKeyCredentials(
                accountSettings.BatchServiceUrl,
                accountSettings.BatchAccountName,
                accountSettings.BatchAccountKey
            );

            sb.AppendLine($"batchcred created to {accountSettings.BatchAccountName} at {accountSettings.BatchServiceUrl}");
            using (BatchClient client = BatchClient.Open(cred))
            {
                PoolInformation pool = new PoolInformation();
                pool.PoolId = jobSettings.PoolID;

                sb.AppendLine("creating job " + jobid);
                CloudJob ourJob = client.JobOperations.CreateJob(jobid, pool);
                ourJob.OnAllTasksComplete = Microsoft.Azure.Batch.Common.OnAllTasksComplete.TerminateJob;

                await ourJob.CommitAsync();
                sb.AppendLine("job created " + jobid);

                // Get the bound version of the job with all of its properties populated
                CloudJob committedJob = await client.JobOperations.GetJobAsync(jobid);
                sb.AppendLine("bound version of job retrieved " + jobid);

                sb.AppendLine("submitting task " + taskid);
                // Create the tasks that the job will execute
                CloudTask task = new CloudTask(taskid, joba_task_cmdline);
                await client.JobOperations.AddTaskAsync(jobid, task);
                sb.AppendLine("task submitted " + taskid);

                TextBox_JobID.Text = jobid;
                TextBox_Task.Text = taskid;

                sb.AppendLine("task submitted.  use job status button to see job and task checks");

                TextBlock_Out.Text = sb.ToString();
            }
        }

        private async void Button_JobStatus_Click(object sender, RoutedEventArgs e)
        {
            StringBuilder sb = new StringBuilder(1024);
            // read account settings, dump
            AccountSettings accountSettings = SampleHelpers.LoadAccountSettings();

            // read job settings, dump
            JobSettings jobSettings = SampleHelpers.LoadJobSettings();

            // connect to batch, dump status
            BatchSharedKeyCredentials cred = new BatchSharedKeyCredentials(
                accountSettings.BatchServiceUrl,
                accountSettings.BatchAccountName,
                accountSettings.BatchAccountKey
            );

            sb.AppendLine($"batchcred created to {accountSettings.BatchAccountName} at {accountSettings.BatchServiceUrl}");
            using (BatchClient client = BatchClient.Open(cred))
            {
                string jobid = TextBox_JobID.Text.Trim();

                CloudJob job = null;
                sb.AppendLine($"GetJob({jobid})");
                try
                {
                    job = await client.JobOperations.GetJobAsync(jobid);
                }
                catch ( Exception ex )
                {
                    job = null;
                    sb.AppendLine($"job not found.  jobid=[{jobid}]");
                    sb.AppendLine("job not found exception: " + ex.ToString());
                }

                if (job != null)
                {
                    TimeSpan? jobdur = job.ExecutionInformation.EndTime - job.ExecutionInformation.StartTime;
                    if ( jobdur==null )
                    { 
                        sb.AppendLine($"job state:{job.State} ");
                    }
                    else
                    {
                        sb.AppendLine($"job state:{job.State} duration: {jobdur}");
                    }

                    foreach (CloudTask t in job.ListTasks())
                    {
                        TimeSpan? dur = t.ExecutionInformation.EndTime - t.ExecutionInformation.StartTime;
                        if (dur == null)
                        {
                            sb.AppendLine($"task: {t.Id} {t.State} start: {t.ExecutionInformation.StartTime} end:{t.ExecutionInformation.EndTime}");
                        }
                        else
                        {
                            sb.AppendLine($"task: {t.Id} {t.State} duration:{dur} start: {t.ExecutionInformation.StartTime} end:{t.ExecutionInformation.EndTime}");
                        }
                    }
                }
            }

            TextBlock_Out.Text = sb.ToString();
        }

        private void Button_ListJobs_Click(object sender, RoutedEventArgs e)
        {
            StringBuilder sb = new StringBuilder(1024);
            sb.AppendLine("listing all jobs");
            // read account settings, dump
            AccountSettings accountSettings = SampleHelpers.LoadAccountSettings();

            // connect to batch, dump status
            BatchSharedKeyCredentials cred = new BatchSharedKeyCredentials(
                accountSettings.BatchServiceUrl,
                accountSettings.BatchAccountName,
                accountSettings.BatchAccountKey
            );

            sb.AppendLine($"batchcred created to {accountSettings.BatchAccountName} at {accountSettings.BatchServiceUrl}");
            using (BatchClient client = BatchClient.Open(cred))
            {
                var jobs = client.JobOperations.ListJobs();
                foreach (CloudJob job in jobs)
                {
                    sb.AppendLine($"{job.Id} {job.State} pool: {job.ExecutionInformation.PoolId} start:{job.ExecutionInformation.StartTime} end: {job.ExecutionInformation.EndTime}");
                }
            }
            TextBlock_Out.Text = sb.ToString();
        }

        private async void Button_JobB_Click(object sender, RoutedEventArgs e)
        {
            const string DQUOTE = "\"";

            string jobb_task_cmdline = $"cmd /c {DQUOTE}set AZ_BATCH & timeout /t 30 > NUL{DQUOTE}";

            StringBuilder sb = new StringBuilder(1024);
            sb.AppendLine("Submitting jobB - has 20 tasks in it");
            string jobid = NamingHelpers.GenJobName("B");
            sb.AppendLine($"jobid={jobid}");

            sb.AppendLine($"task command line={jobb_task_cmdline}");

            // read account settings, dump
            AccountSettings accountSettings = SampleHelpers.LoadAccountSettings();

            // read job settings, dump
            JobSettings jobSettings = SampleHelpers.LoadJobSettings();

            // connect to batch, dump status
            BatchSharedKeyCredentials cred = new BatchSharedKeyCredentials(
                accountSettings.BatchServiceUrl,
                accountSettings.BatchAccountName,
                accountSettings.BatchAccountKey
            );

            sb.AppendLine($"batchcred created to {accountSettings.BatchAccountName} at {accountSettings.BatchServiceUrl}");
            using (BatchClient client = BatchClient.Open(cred))
            {
                PoolInformation pool = new PoolInformation();
                pool.PoolId = jobSettings.PoolID;

                sb.AppendLine("creating job " + jobid);
                CloudJob ourJob = client.JobOperations.CreateJob(jobid, pool);
                ourJob.OnAllTasksComplete = Microsoft.Azure.Batch.Common.OnAllTasksComplete.TerminateJob;

                await ourJob.CommitAsync();
                sb.AppendLine("job created " + jobid);

                // Get the bound version of the job with all of its properties populated
                CloudJob committedJob = await client.JobOperations.GetJobAsync(jobid);
                sb.AppendLine("bound version of job retrieved " + jobid);

                string taskid = "notset";
                System.Collections.Generic.List<CloudTask> tasks = new System.Collections.Generic.List<CloudTask>();

                for (int ii = 1; ii <= 20; ii++)
                {
                    taskid = NamingHelpers.GenTaskName("JOBB");
                    sb.AppendLine("adding task " + taskid);

                    // Create the tasks that the job will execute
                    CloudTask task = new CloudTask(taskid, jobb_task_cmdline);
                    tasks.Add(task);
                }

                sb.AppendLine("submitting tasks");
                await client.JobOperations.AddTaskAsync(jobid, tasks);                
                sb.AppendLine("task submitted.  use job status button to see job and task checks");

                TextBox_JobID.Text = jobid;
                TextBox_Task.Text = taskid;

                TextBlock_Out.Text = sb.ToString();
            }
        }
    }
}
