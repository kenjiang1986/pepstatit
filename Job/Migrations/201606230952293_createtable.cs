namespace Job.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class createtable : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.UserStats",
                c => new
                    {
                        Id = c.Long(nullable: false, identity: true),
                        UserName = c.String(),
                        Company = c.String(),
                        UserCount = c.Int(nullable: false),
                        InquiryCount = c.Int(nullable: false),
                        AddProjectCount = c.Int(nullable: false),
                        OutTaskCount = c.Int(nullable: false),
                        ProjectFinishCount = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.UserStats");
        }
    }
}
