using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Quartz;
using Quartz.Impl;

namespace Job
{
    public class RunTask
    {
        public void Run()
        {
            //1、实例化作业工厂
            ISchedulerFactory schedf = new StdSchedulerFactory();
            IScheduler sched = schedf.GetScheduler();
            //2.创建出来一个具体的作业
            IJobDetail job = JobBuilder.Create<TaskService>().Build();
            //3.创建并配置一个触发器
            //ISimpleTrigger trigger = (ISimpleTrigger)TriggerBuilder.Create()
            //    .WithSimpleSchedule(x => x.WithIntervalInSeconds(100).WithRepeatCount(int.MaxValue)).StartNow()
            //    .Build();

            ICronTrigger trigger = (ICronTrigger)TriggerBuilder.Create()
                .WithCronSchedule(ConfigurationManager.AppSettings["Cron"]).StartNow()
                .Build();

            //4.加入作业调度池中
            sched.ScheduleJob(job, trigger);
            //5.开始运行
            sched.Start();
        }
    }
}
