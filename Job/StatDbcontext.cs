using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Job.DTO;

namespace Job
{
    public class StatDbcontext : DbContext
    {
        public StatDbcontext(string dbConetction)
            : base(dbConetction)
        {
            Database.SetInitializer(new CreateDatabaseIfNotExists<StatDbcontext>());
        }


        public class MyContextFactory : IDbContextFactory<StatDbcontext>
        {
            public StatDbcontext Create()
            {
                return new StatDbcontext(ConfigurationManager.AppSettings["DbConnection"]);
            }
        }

        public DbSet<UserStat> UserStats { get; set; }
    }
}
