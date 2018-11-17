using System.Threading.Tasks;
using Quartz;
using Quartz.Impl;

namespace TLCSharp.Bot
{
    public class scheduling
    {

        public static async Task update_bot()
        {
            //job will run every 5 minute
            int ScheduleIntervalInMinute = 30;
            string task_name = "update_bot";
            string job_name = task_name + "_job";
            string trigger_name = task_name + "_trigger";

            IScheduler scheduler;

            var schedulerFactory = new StdSchedulerFactory();
            scheduler = schedulerFactory.GetScheduler().Result;
            scheduler.Start().Wait();

            IJobDetail job = JobBuilder.Create<jobs.update_crypto>()
                                       .WithIdentity(JobKey.Create(job_name))
                                       .Build();

            ITrigger trigger = TriggerBuilder.Create()
                                             .WithIdentity(trigger_name)
                                             .StartNow()
                                             .WithSimpleSchedule(x => x.WithIntervalInMinutes(ScheduleIntervalInMinute).RepeatForever())
                                             .Build();

            await scheduler.ScheduleJob(job, trigger);

        }

    }
}