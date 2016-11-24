using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Quartz;

namespace Job
{
    public class TaskService : IJob
    {
        public void Execute(IJobExecutionContext context)
        {
            new StatService().Run();
        }
    }
}
