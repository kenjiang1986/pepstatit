using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Job.DTO;
using Job.Helper;
using MySql.Data.MySqlClient;
using Nest;
using Newtonsoft.Json;
using OfficeOpenXml;
using OfficeOpenXml.FormulaParsing.Excel.Functions.Text;
using OfficeOpenXml.Style;
using OfficeOpenXml.Table;
using Quartz;

namespace Job
{
    public class StatService 
    {
        private readonly static ElasticClient Client = EsClient.GetClient();

        private readonly string testDbCon = ConfigurationManager.AppSettings["TestDbConnection"];
        private readonly string DbCon = ConfigurationManager.AppSettings["DbConnection"];
        private readonly string testUserCon = ConfigurationManager.AppSettings["TestUserConnection"];
        private readonly string userCon = ConfigurationManager.AppSettings["UserConnection"];

        private string startDate = DateTime.Now.AddDays(-6).ToString();
        private string endDate = DateTime.Now.ToString();

        private Dictionary<string, string> StatContent = new Dictionary<string, string>
        {
            {StatType.InquiryResult.ToString(),  "取得询价结果-Peacock.InWork2.MvcWebSite.Controllers.ProjectController"},
            {StatType.InquiryHistory.ToString(),  "获取历史询价记录-Peacock.InWork2.MvcWebSite.Controllers.InquiryController"},
            {StatType.OfferCase.ToString(),  "报盘案例-Peacock.InWork4.Services.API.BaseAPIService"},
            {StatType.DealCase.ToString(),  "成交案例-Peacock.InWork4.Services.API.BaseAPIService"},
            {StatType.ReportCase.ToString(),  "报告案例-Peacock.InWork4.Services.API.BaseAPIService"},
            {StatType.UserLogin.ToString(),  "用户登录-Peacock.InWork2.BLL.UserBLL"},
            {StatType.IntegratedQuery.ToString(),  "综合查询》查询"},
            //微信端
            {StatType.ConfrimList.ToString(),  "微信端获取确认责任列表-Peacock.PEP.WeChat.Service.ChargeService"},
            {StatType.Statlog.ToString(),  "微信端获取流程跟踪列表-Peacock.PEP.WeChat.Service.ChargeService"},
            {StatType.ConfirmFee.ToString(),  "微信端进行收费确认-Peacock.PEP.WeChat.Service.ChargeService"},
            {StatType.Feedback.ToString(),  "微信端获取项目反馈列表-Peacock.PEP.WeChat.Service.ChargeService"},
            {StatType.SMS.ToString(),  "微信端发送短信-Peacock.PEP.WeChat.Service.ChargeService"},
            {StatType.AlterFee.ToString(),  "微信端最低收费修改-Peacock.PEP.WeChat.Service.ChargeService"},
            //简版
            {StatType.PepOfferCase.ToString(),  "报盘案例-Peacock.PEP.Service.BaseAPIService"},
            {StatType.PepDealCase.ToString(),  "成交案例-Peacock.PEP.Service.BaseAPIService"},
            {StatType.PepReportCase.ToString(),  "报告案例-"},
            {StatType.WaicaiUser.ToString(),  "根据公司获取外业用户-Peacock.PEP.Service.OutTaskService"},
            {StatType.ProjectList.ToString(),  "获取项目列表-Peacock.PEP.Service.ProjectService"},
            {StatType.ProjectData.ToString(),  "获取项目-Peacock.PEP.Service.ProjectService"},
            {StatType.ProjectSubmit.ToString(),  "项目提交审核-Peacock.PEP.Service.ProjectService"},
            {StatType.ProjectApprove.ToString(),  "项目审核通过-Peacock.PEP.Service.ProjectService"},
            {StatType.PepUserLogin.ToString(),  "*登录*"},
            {StatType.SearchOnlineProject.ToString(),  "查询线上报告-Peacock.PEP.Service.OnlineBusinessService"},
            {StatType.Scan.ToString(),  "微信端真伪查询-Peacock.PEP.WeChat.Service.AuthCheckService"},
        };

        public StatService()
        { }

        public StatService(string startDate, string endDate)
        {
            if (!string.IsNullOrEmpty(startDate))
            {
                this.startDate = startDate;
            }

            if (!string.IsNullOrEmpty(endDate))
            {
                this.endDate = endDate;
            }
        }

        public void Run()
        {
        Run:
            try
            {
                LogHelper.WriteLog("开始执行统计任务");
                var testList = GetStatData(testDbCon, testUserCon, SystemCode.PEPDemo);
                var list = GetStatData(DbCon, userCon, SystemCode.PEP);
                var pepList = GetPepStatData();
                var listDc = new Dictionary<string, object>();
                listDc.Add(ListType.List.ToString(), list);
                listDc.Add(ListType.DemoList.ToString(), testList);
                listDc.Add(ListType.SimpleList.ToString(), pepList);
                SaveFile(listDc);
                LogHelper.WriteLog("获取统计数据完成");
            }
            catch (Exception ex)
            {
                LogHelper.Error("执行任务错误", ex);
                goto Run;
            }
        }


        /// <summary>
        /// 获取统计数据
        /// </summary>
        /// <param name="startDate"></param>
        /// <param name="endDate"></param>
        /// <returns></returns>
        public IList<UserStat> GetStatData(string dbcon, string usercon, string sysCode)
        {
            Dictionary<string, object> paras = new Dictionary<string, object>();
            paras.Add("@startDate", new ParamesDTO() { Value = startDate});
            paras.Add("@endDate", new ParamesDTO() { Value = string.Format("{0} 23:59:00", endDate)});
            //paras.Add("@startDate", new ParamesDTO() { Value = "2016-7-11 00:00:00" });
            //paras.Add("@endDate", new ParamesDTO() { Value = "2016-7-17 23:59:00" });
            IDbConnection con = new SqlConnection(dbcon);

            var inquiryList = SqlHelper.CallProcedure<UserStat>("GetInquiryCount", paras, con);
            var addProjectList = SqlHelper.CallProcedure<UserStat>("GetAddProjectCount", paras, con);
            var outTaskList = SqlHelper.CallProcedure<UserStat>("GetOutTaskCount", paras, con);
            var finishList = SqlHelper.CallProcedure<UserStat>("GetFinishProjectCount", paras, con);

            var conn = new MySqlConnection(usercon);
            var userParas = new Dictionary<string, object>();
            userParas.Add("?CDate", new ParamesDTO() { Value = endDate });
            var userList = SqlHelper.CallMySqlProcedure<UserStat>("GetCompanyCount", userParas, conn);


            //获取日志统计数据
            var inquiryResultList = GetLogData(sysCode, StatType.InquiryResult);
            var historyList = GetLogData(sysCode, StatType.InquiryHistory);
            var offerList = GetLogData(sysCode, StatType.OfferCase);
            var reportList = GetLogData(sysCode, StatType.ReportCase);
            var dealList = GetLogData(sysCode, StatType.DealCase);
            var loginList = GetLogData(sysCode, StatType.UserLogin);
            var queryList = GetLogData(sysCode, StatType.IntegratedQuery);

            var confirmList = GetLogData(SystemCode.PEPWechat, StatType.ConfrimList);
            var logList = GetLogData(SystemCode.PEPWechat, StatType.Statlog);
            var feebackList = GetLogData(SystemCode.PEPWechat, StatType.Feedback);
            var confirmFeeList = GetLogData(SystemCode.PEPWechat, StatType.ConfirmFee);
            var feeList = GetLogData(SystemCode.PEPWechat, StatType.AlterFee);
            var smsList = GetLogData(SystemCode.PEPWechat, StatType.SMS);
            var scanList = GetLogData(SystemCode.PEPWechat, StatType.Scan);

            var list = inquiryList.Union(addProjectList).Union(outTaskList)
                .Union(finishList).Union(userList).Union(inquiryResultList)
                .Union(historyList).Union(offerList).Union(reportList).Union(dealList)
                .Union(confirmList).Union(logList).Union(feebackList).Union(confirmFeeList)
                .Union(feeList).Union(smsList).Union(scanList).Union(loginList)
                .Union(queryList).ToList();
                
            var result = ConvertList(list);
                                     
            return result;
        }

        private IList<UserStat> GetPepStatData()
        {
            //获取日志统计数据
            var reportList = GetLogData(SystemCode.PEPSimple, StatType.PepReportCase);
            var dealList = GetLogData(SystemCode.PEPSimple, StatType.PepDealCase);
            var offerList = GetLogData(SystemCode.PEPSimple, StatType.PepOfferCase);
            var waicaiList = GetLogData(SystemCode.PEPSimple, StatType.WaicaiUser);
            var projectList = GetLogData(SystemCode.PEPSimple, StatType.ProjectList);
            var projectDataList = GetLogData(SystemCode.PEPSimple, StatType.ProjectData);
            var projectSubmitList = GetLogData(SystemCode.PEPSimple, StatType.ProjectSubmit);
            var projectApproveList = GetLogData(SystemCode.PEPSimple, StatType.ProjectApprove);
            var loginList = GetLogData(SystemCode.PEPSimple, StatType.PepUserLogin);
            var onlineList = GetLogData(SystemCode.PEPSimple, StatType.SearchOnlineProject);

            LogHelper.WriteLog("外采：" + JsonConvert.SerializeObject(waicaiList));


            var list = waicaiList.Union(projectList).Union(projectDataList)
                .Union(projectSubmitList).Union(projectSubmitList).Union(projectApproveList)
                .Union(loginList).Union(onlineList)
                 .Union(reportList).Union(dealList).Union(offerList)
                .ToList();

            var result = ConvertPepList(list);

            return result;
        }

        private IList<UserStat> GetLogData(string sysCode, StatType statType)
        {
            var content = string.Format("{0}.requestconcent.raw", sysCode);
            var timeStamp = string.Format("{0}.@timestamp", sysCode);
            //var endDate = string.Format( "{0}T23:59:00",DateTime.Now.ToString());
            //var startDate = string.Format("{0}T00:00:00", DateTime.Now.AddDays(-6));
            var startDate = string.Format("{0}T00:00:00", this.startDate);
            var endDate = string.Format("{0}T23:59:00", this.endDate);

            ISearchResponse<Log> searchResults = null;

            if (statType != StatType.PepUserLogin)
            {
                searchResults = Client.Search<Log>
                    (x => x.Query(p => p.Term(s =>
                        s.Field(content)
                            .Value(StatContent[statType.ToString()]))
                                       && p.TermRange(s1 => s1.Field(timeStamp)
                                           .GreaterThanOrEquals(startDate)
                                           .LessThanOrEquals(endDate))).Take(10000));
            }
            else
            {
                searchResults = Client.Search<Log>
              (x => x.Query(p => p.QueryString(s =>
                  s.DefaultField(content)
                  .Query(StatContent[statType.ToString()]))
                  && p.TermRange(s1 => s1.Field(timeStamp)
                          .GreaterThanOrEquals(startDate)
                          .LessThanOrEquals(endDate))).Take(10000));
            }

            LogHelper.WriteLog(statType.ToString() + ":" + searchResults.Total);

            IList<Log> list = sysCode == SystemCode.PEPSimple ? searchResults.Documents.GroupBy(x => x.User)
                    .Select(y => new Log
                    {
                        User = y.Key,
                        StatCount = y.Count()
                    }).ToList() :
                     searchResults.Documents.GroupBy(x => x.Company)
                .Select(y => new Log
                {
                    Company = y.Key,
                    StatCount = y.Count()
                }).ToList();

            LogHelper.WriteLog(JsonConvert.SerializeObject(list));
            
            var result = new List<UserStat>();

           switch (statType)
            {
                case StatType.InquiryResult:
                    result = list.Select(x => new UserStat { Company = x.Company.Contains("仁达") && x.Company.Contains("分公司") ? x.Company.Substring(0, x.Company.IndexOf('分')) : x.Company, InquiryResult = x.StatCount }).ToList();
                    break;
                case StatType.InquiryHistory:
                    result = list.Select(x => new UserStat { Company = x.Company.Contains("仁达") && x.Company.Contains("分公司") ? x.Company.Substring(0, x.Company.IndexOf('分')) : x.Company, InquiryHistory = x.StatCount }).ToList();
                    break;
                case StatType.OfferCase:
                    result = list.Select(x => new UserStat { Company = x.Company.Contains("仁达") && x.Company.Contains("分公司") ? x.Company.Substring(0, x.Company.IndexOf('分')) : x.Company, OfferCase = x.StatCount }).ToList();
                    break;
                case StatType.DealCase:
                    result = list.Select(x => new UserStat { Company = x.Company.Contains("仁达") && x.Company.Contains("分公司") ? x.Company.Substring(0, x.Company.IndexOf('分')) : x.Company, DealCase = x.StatCount }).ToList();
                    break;
                case StatType.ReportCase:
                    result = list.Select(x => new UserStat { Company = x.Company.Contains("仁达") && x.Company.Contains("分公司") ? x.Company.Substring(0, x.Company.IndexOf('分')) : x.Company, ReportCase = x.StatCount }).ToList();
                    break;
                case StatType.UserLogin:
                    result = list.Select(x => new UserStat { Company = x.Company.Contains("仁达") && x.Company.Contains("分公司") ? x.Company.Substring(0, x.Company.IndexOf('分')) : x.Company, UserLogin = x.StatCount }).ToList();
                    break;
                case StatType.IntegratedQuery:
                    result = list.Select(x => new UserStat { Company = x.Company.Contains("仁达") && x.Company.Contains("分公司") ? x.Company.Substring(0, x.Company.IndexOf('分')) : x.Company, IntegratedQuery = x.StatCount }).ToList();
                    break;
                case StatType.ConfrimList:
                    result = list.Select(x => new UserStat { Company = x.Company.Contains("仁达") && x.Company.Contains("分公司") ? x.Company.Substring(0, x.Company.IndexOf('分')) : x.Company, ConfrimListCount = x.StatCount }).ToList();
                    break;
                case StatType.Statlog:
                    result = list.Select(x => new UserStat { Company = x.Company.Contains("仁达") && x.Company.Contains("分公司") ? x.Company.Substring(0, x.Company.IndexOf('分')) : x.Company, StatlogCount = x.StatCount }).ToList();
                    break;
                case StatType.Feedback:
                    result = list.Select(x => new UserStat { Company = x.Company.Contains("仁达") && x.Company.Contains("分公司") ? x.Company.Substring(0, x.Company.IndexOf('分')) : x.Company, FeedbackCount = x.StatCount }).ToList();
                    break;
                case StatType.AlterFee:
                    result = list.Select(x => new UserStat { Company = x.Company.Contains("仁达") && x.Company.Contains("分公司") ? x.Company.Substring(0, x.Company.IndexOf('分')) : x.Company, AlterFeeCount = x.StatCount }).ToList();
                    break;
                case StatType.ConfirmFee:
                    result = list.Select(x => new UserStat { Company = x.Company.Contains("仁达") && x.Company.Contains("分公司") ? x.Company.Substring(0, x.Company.IndexOf('分')) : x.Company, ConfirmFeeCount = x.StatCount }).ToList();
                    break;
                case StatType.SMS:
                    result = list.Select(x => new UserStat { Company = x.Company.Contains("仁达") && x.Company.Contains("分公司") ? x.Company.Substring(0, x.Company.IndexOf('分')) : x.Company, SMSCount = x.StatCount }).ToList();
                    break;
                case StatType.Scan:
                    result = list.Select(x => new UserStat { Company = x.Company, ScanCount = x.StatCount }).ToList();
                    break;
                case StatType.WaicaiUser:
                    result = list.Select(x => new UserStat { UserName = x.User, WaicaiUser = x.StatCount }).ToList();
                    break;
                case StatType.ProjectList:
                    result = list.Select(x => new UserStat { UserName = x.User, ProjectList = x.StatCount }).ToList();
                    break;
                case StatType.ProjectData:
                    result = list.Select(x => new UserStat { UserName = x.User, ProjectData = x.StatCount }).ToList();
                    break;
                case StatType.ProjectSubmit:
                    result = list.Select(x => new UserStat { UserName = x.User, ProjectSubmit = x.StatCount }).ToList();
                    break;
                case StatType.ProjectApprove:
                    result = list.Select(x => new UserStat { UserName = x.User, ProjectApprove = x.StatCount }).ToList();
                    break;
                case StatType.PepUserLogin:
                    result = list.Select(x => new UserStat { UserName = x.User, PepUserLogin = x.StatCount }).ToList();
                    break;
                case StatType.SearchOnlineProject:
                    result = list.Select(x => new UserStat { UserName = x.User, SearchOnlineProject = x.StatCount }).ToList();
                    break;
                case StatType.PepInquiryResult:
                    result = list.Select(x => new UserStat { UserName = x.User, InquiryResult = x.StatCount }).ToList();
                    break;
                case StatType.PepInquiryHistory:
                    result = list.Select(x => new UserStat { UserName = x.User, InquiryHistory = x.StatCount }).ToList();
                    break;
                case StatType.PepOfferCase:
                    result = list.Select(x => new UserStat { UserName = x.User, OfferCase = x.StatCount }).ToList();
                    break;
                case StatType.PepReportCase:
                    result = list.Select(x => new UserStat { UserName = x.User, ReportCase = x.StatCount }).ToList();
                    break;
                case StatType.PepDealCase:
                    result = list.Select(x => new UserStat { UserName = x.User, DealCase = x.StatCount }).ToList();
                    break;
            }

            return result;
        }

       private IList<UserStat> ConvertList(List<UserStat> list)
        {
            list.ForEach(x =>
            {
                var group = list.Where(y => x.Company == y.Company);
                x.UserCount = group.Sum(y => y.UserCount);
                x.InquiryCount = group.Sum(y => y.InquiryCount);
                x.AddProjectCount = group.Sum(y => y.AddProjectCount);
                x.OutTaskCount = group.Sum(y => y.OutTaskCount);
                x.ProjectFinishCount = group.Sum(y => y.ProjectFinishCount);
                x.InquiryResult = group.Sum(y => y.InquiryResult);
                x.InquiryHistory = group.Sum(y => y.InquiryHistory);
                x.OfferCase = group.Sum(y => y.OfferCase);
                x.DealCase = group.Sum(y => y.DealCase);
                x.ReportCase = group.Sum(y => y.ReportCase);
                x.UserLogin = group.Sum(y => y.UserLogin);
                x.IntegratedQuery = group.Sum(y => y.IntegratedQuery);
                x.ConfrimListCount = group.Sum(y => y.ConfrimListCount);
                x.FeedbackCount = group.Sum(y => y.FeedbackCount);
                x.StatlogCount = group.Sum(y => y.StatlogCount);
                x.AlterFeeCount = group.Sum(y => y.AlterFeeCount);
                x.ConfirmFeeCount = group.Sum(y => y.ConfirmFeeCount);
                x.SMSCount = group.Sum(y => y.SMSCount);
                x.ScanCount = group.Sum(y => y.ScanCount);

            });

            return list.Distinct(new PropertyComparer<UserStat>("Company"))
              .OrderByDescending(x => x.AddProjectCount).ThenBy(x => x.Company).ToList();
        }


       private IList<UserStat> ConvertPepList(List<UserStat> list)
       {
           list.ForEach(x =>
           {
               var group = list.Where(y => x.UserName == y.UserName);
               x.ReportCase = group.Sum(y => y.ReportCase);
               x.OfferCase = group.Sum(y => y.OfferCase);
               x.DealCase = group.Sum(y => y.DealCase);
               x.WaicaiUser = group.Sum(y => y.WaicaiUser);
               x.ProjectData = group.Sum(y => y.ProjectData);
               x.ProjectList = group.Sum(y => y.ProjectList);
               x.ProjectSubmit = group.Sum(y => y.ProjectSubmit);
               x.PepUserLogin = group.Sum(y => y.PepUserLogin);
               x.SearchOnlineProject = group.Sum(y => y.SearchOnlineProject);
               x.ProjectApprove = group.Sum(y => y.ProjectApprove);
           });

           return list.Distinct(new PropertyComparer<UserStat>("UserName")).ToList();
       }

        private void SaveFile(Dictionary<string, object> listDc)
        {
            try
            {
                using (ExcelPackage ep = new ExcelPackage())
                {
                    ExcelWorksheet sheet = ep.Workbook.Worksheets.Add("企业版");
                    AddSheet(sheet, (List<UserStat>)listDc[ListType.List.ToString()]);
                    
                    ExcelWorksheet wSheet = ep.Workbook.Worksheets.Add("演示环境");
                    AddSheet(wSheet, (List<UserStat>)listDc[ListType.DemoList.ToString()]);

                    ExcelWorksheet pepSheet = ep.Workbook.Worksheets.Add("免费版");
                    AddPepSheet(pepSheet, (List<UserStat>)listDc[ListType.SimpleList.ToString()]);

                    ExcelWorksheet wechatSheet = ep.Workbook.Worksheets.Add("微信端");
                    AddWechatSheet(wechatSheet, (List<UserStat>)listDc[ListType.List.ToString()]);
                    
                    string fileName = ConfigurationManager.AppSettings["FilePath"] + DateTime.Now.ToString("yyyy-MM-dd") +
                                 ".xls";
                    ep.SaveAs(new FileInfo(fileName));
                }
            }
            catch(Exception ex)
            {
                LogHelper.Error("保存文件错误", ex);
                throw;
            }
        }

        private ExcelWorksheet AddSheet(ExcelWorksheet wSheet, IList<UserStat> list)
        {
            wSheet.Cells[1, 1].Value = "公司名称";
            wSheet.Cells[1, 2].Value = "用户数量";
            wSheet.Cells[1, 3].Value = "估值次数";
            wSheet.Cells[1, 4].Value = "接单数（立项）";
            wSheet.Cells[1, 5].Value = "外勘次数";
            wSheet.Cells[1, 6].Value = "出报告次数";
            wSheet.Cells[1, 7].Value = "取得询价结果";
            wSheet.Cells[1, 8].Value = "获取历史询价记录";
            wSheet.Cells[1, 9].Value = "获取报盘案例";
            wSheet.Cells[1, 10].Value = "获取成交案例";
            wSheet.Cells[1, 11].Value = "获取报告案例";
            wSheet.Cells[1, 12].Value = "用户登录次数";
            wSheet.Cells[1, 13].Value = "综合查询次数";

            for (int s = 1; s <= 13; s++)
            {
                //设置列头样式
                wSheet.Cells[1, s].Style.Fill.PatternType = ExcelFillStyle.Solid;
                wSheet.Cells[1, s].Style.Fill.BackgroundColor.SetColor(Color.FromArgb(196, 215, 155));

                for (int i = 1; i < list.Count; i++)
                {
                    wSheet.Cells[i, s].Style.Border.BorderAround(ExcelBorderStyle.Thin, Color.FromArgb(0, 0, 0));
                }
            }

            for (int i = 0; i < list.Count; i++)
            {
                wSheet.Cells[i + 2, 1].Value = list[i].Company;
                wSheet.Cells[i + 2, 2].Value = list[i].UserCount == 0 ? string.Empty : list[i].UserCount.ToString();
                wSheet.Cells[i + 2, 3].Value = list[i].InquiryCount == 0 ? string.Empty : list[i].InquiryCount.ToString();
                wSheet.Cells[i + 2, 4].Value = list[i].AddProjectCount == 0 ? string.Empty : list[i].AddProjectCount.ToString();
                wSheet.Cells[i + 2, 5].Value = list[i].OutTaskCount == 0 ? string.Empty : list[i].OutTaskCount.ToString();
                wSheet.Cells[i + 2, 6].Value = list[i].ProjectFinishCount == 0 ? string.Empty : list[i].ProjectFinishCount.ToString();
                wSheet.Cells[i + 2, 7].Value = list[i].InquiryResult == 0 ? string.Empty : list[i].InquiryResult.ToString();
                wSheet.Cells[i + 2, 8].Value = list[i].InquiryHistory == 0 ? string.Empty : list[i].InquiryHistory.ToString();
                wSheet.Cells[i + 2, 9].Value = list[i].OfferCase == 0 ? string.Empty : list[i].OfferCase.ToString();
                wSheet.Cells[i + 2, 10].Value = list[i].DealCase == 0 ? string.Empty : list[i].DealCase.ToString();
                wSheet.Cells[i + 2, 11].Value = list[i].ReportCase == 0 ? string.Empty : list[i].ReportCase.ToString();
                wSheet.Cells[i + 2, 12].Value = list[i].UserLogin == 0 ? string.Empty : list[i].UserLogin.ToString();
                wSheet.Cells[i + 2, 13].Value = list[i].IntegratedQuery == 0 ? string.Empty : list[i].IntegratedQuery.ToString();
            }

            return wSheet;
        }

        private ExcelWorksheet AddPepSheet(ExcelWorksheet wSheet, IList<UserStat> list)
        {
            wSheet.Cells[1, 1].Value = "用户名称";
            wSheet.Cells[1, 2].Value = "估值次数";
            wSheet.Cells[1, 3].Value = "接单数（立项）";
            wSheet.Cells[1, 4].Value = "外勘次数";
            wSheet.Cells[1, 5].Value = "出报告次数";
            wSheet.Cells[1, 6].Value = "获取报盘案例";
            wSheet.Cells[1, 7].Value = "获取成交案例";
            wSheet.Cells[1, 8].Value = "获取报告案例";
            wSheet.Cells[1, 9].Value = "根据公司获取外采用户";
            wSheet.Cells[1, 10].Value = "获取立项列表";
            wSheet.Cells[1, 11].Value = "用户登录";
            wSheet.Cells[1, 12].Value = "查询线上报告";
            wSheet.Cells[1, 13].Value = "获取项目详细信息";
            wSheet.Cells[1, 14].Value = "项目提交审核";
            wSheet.Cells[1, 15].Value = "项目审核通过";


            for (int s = 1; s <= 15; s++)
            {
                //设置列头样式
                wSheet.Cells[1, s].Style.Fill.PatternType = ExcelFillStyle.Solid;
                wSheet.Cells[1, s].Style.Fill.BackgroundColor.SetColor(Color.FromArgb(196, 215, 155));
                for (int i = 1; i < list.Count; i++)
                {
                    wSheet.Cells[i, s].Style.Border.BorderAround(ExcelBorderStyle.Thin, Color.FromArgb(0, 0, 0));
                }
            }

            for (int i = 0; i < list.Count; i++)
            {
                wSheet.Cells[i + 2, 1].Value = list[i].UserName;
                wSheet.Cells[i + 2, 2].Value = list[i].InquiryCount == 0
                    ? string.Empty
                    : list[i].InquiryCount.ToString();
                wSheet.Cells[i + 2, 3].Value = list[i].AddProjectCount == 0
                    ? string.Empty
                    : list[i].AddProjectCount.ToString();
                wSheet.Cells[i + 2, 4].Value = list[i].OutTaskCount == 0
                    ? string.Empty
                    : list[i].OutTaskCount.ToString();
                wSheet.Cells[i + 2, 5].Value = list[i].ProjectFinishCount == 0
                    ? string.Empty
                    : list[i].ProjectFinishCount.ToString();
                wSheet.Cells[i + 2, 6].Value = list[i].OfferCase == 0 ? string.Empty : list[i].OfferCase.ToString();
                wSheet.Cells[i + 2, 7].Value = list[i].DealCase == 0 ? string.Empty : list[i].DealCase.ToString();
                wSheet.Cells[i + 2, 8].Value = list[i].ReportCase == 0 ? string.Empty : list[i].ReportCase.ToString();
                wSheet.Cells[i + 2, 9].Value = list[i].WaicaiUser == 0 ? string.Empty : list[i].WaicaiUser.ToString();
                wSheet.Cells[i + 2, 10].Value = list[i].ProjectList == 0 ? string.Empty : list[i].ProjectList.ToString();
                wSheet.Cells[i + 2, 11].Value = list[i].PepUserLogin == 0
                    ? string.Empty
                    : list[i].PepUserLogin.ToString();
                wSheet.Cells[i + 2, 12].Value = list[i].SearchOnlineProject == 0
                    ? string.Empty
                    : list[i].SearchOnlineProject.ToString();
                wSheet.Cells[i + 2, 13].Value = list[i].ProjectData == 0 ? string.Empty : list[i].ProjectData.ToString();
                wSheet.Cells[i + 2, 14].Value = list[i].ProjectSubmit == 0
                    ? string.Empty
                    : list[i].ProjectSubmit.ToString();
                wSheet.Cells[i + 2, 15].Value = list[i].ProjectApprove == 0
                    ? string.Empty
                    : list[i].ProjectApprove.ToString();
            }
            return wSheet;
        }

        private ExcelWorksheet AddWechatSheet(ExcelWorksheet wSheet, IList<UserStat> list)
        {
            wSheet.Cells[1, 1].Value = "公司名称";
            wSheet.Cells[1, 2].Value = "微信端获取确认责任列表";
            wSheet.Cells[1, 3].Value = "微信端获取流程跟踪列表";
            wSheet.Cells[1, 4].Value = "微信端获取项目反馈列表";
            wSheet.Cells[1, 5].Value = "微信端进行最低收费修改";
            wSheet.Cells[1, 6].Value = "微信端进行收费确认";
            wSheet.Cells[1, 7].Value = "微信端发送短信";
            wSheet.Cells[1, 8].Value = "微信端真伪验证";

            for (int s = 1; s <= 8; s++)
            {
                //设置列头样式
                wSheet.Cells[1, s].Style.Fill.PatternType = ExcelFillStyle.Solid;
                wSheet.Cells[1, s].Style.Fill.BackgroundColor.SetColor(Color.FromArgb(196, 215, 155));
                for (int i = 1; i < list.Count; i++)
                {
                    wSheet.Cells[i, s].Style.Border.BorderAround(ExcelBorderStyle.Thin, Color.FromArgb(0, 0, 0));
                }
            }

            for (int i = 0; i < list.Count; i++)
            {
                wSheet.Cells[i + 2, 1].Value = list[i].Company;
                wSheet.Cells[i + 2, 2].Value = list[i].ConfrimListCount == 0 ? string.Empty : list[i].ConfrimListCount.ToString();
                wSheet.Cells[i + 2, 3].Value = list[i].StatlogCount == 0 ? string.Empty : list[i].StatlogCount.ToString();
                wSheet.Cells[i + 2, 4].Value = list[i].FeedbackCount == 0 ? string.Empty : list[i].FeedbackCount.ToString();
                wSheet.Cells[i + 2, 5].Value = list[i].AlterFeeCount == 0 ? string.Empty : list[i].AlterFeeCount.ToString();
                wSheet.Cells[i + 2, 6].Value = list[i].ConfirmFeeCount == 0 ? string.Empty : list[i].ConfirmFeeCount.ToString();
                wSheet.Cells[i + 2, 7].Value = list[i].SMSCount == 0 ? string.Empty : list[i].SMSCount.ToString();
                wSheet.Cells[i + 2, 8].Value = list[i].ScanCount == 0 ? string.Empty : list[i].ScanCount.ToString();
            }

            return wSheet;
        }


        #region  暂时不用

        /// <summary>
        /// 获取询价数量
        /// </summary>
        /// <param name="startDate"></param>
        /// <param name="endDate"></param>
        /// <returns></returns>
        private IList<UserStat> GetInquiryCount(string startDate, string endDate, string dbCon)
        {
            Dictionary<string, object> paras = new Dictionary<string, object>();
            paras.Add("@startDate", new ParamesDTO() { Value = startDate });
            paras.Add("@endDate", new ParamesDTO() { Value = endDate });
            IDbConnection con = new SqlConnection(dbCon);
            var list = SqlHelper.CallProcedure<UserStat>("GetInquiryCount", paras, con);
            return list;
        }

        /// <summary>
        /// 获取接单数量
        /// </summary>
        /// <param name="startDate"></param>
        /// <param name="endDate"></param>
        /// <returns></returns>
        private IList<UserStat> GetAddProjectCount(string startDate, string endDate, string dbCon)
        {
            Dictionary<string, object> paras = new Dictionary<string, object>();
            paras.Add("@startDate", new ParamesDTO() { Value = startDate });
            paras.Add("@endDate", new ParamesDTO() { Value = endDate });
            IDbConnection con = new SqlConnection(dbCon);
            var list = SqlHelper.CallProcedure<UserStat>("GetAddProjectCount", paras, con);
            return list;
        }

        /// <summary>
        /// 获取外采数量
        /// </summary>
        /// <param name="startDate"></param>
        /// <param name="endDate"></param>
        /// <returns></returns>
        private IList<UserStat> GetOutTaskCount(string startDate, string endDate, string dbCon)
        {
            Dictionary<string, object> paras = new Dictionary<string, object>();
            paras.Add("@startDate", new ParamesDTO() { Value = startDate });
            paras.Add("@endDate", new ParamesDTO() { Value = endDate });
            IDbConnection con = new SqlConnection(dbCon);
            var list = SqlHelper.CallProcedure<UserStat>("GetOutTaskCount", paras, con);
            return list;
        }

        /// <summary>
        /// 获取出报告数量
        /// </summary>
        /// <param name="startDate"></param>
        /// <param name="endDate"></param>
        /// <returns></returns>
        private IList<UserStat> GetFinishProjectCount(string startDate, string endDate, string dbCon)
        {
            Dictionary<string, object> paras = new Dictionary<string, object>();
            paras.Add("@startDate", new ParamesDTO() { Value = startDate });
            paras.Add("@endDate", new ParamesDTO() { Value = endDate });
            IDbConnection con = new SqlConnection(dbCon);
            var list = SqlHelper.CallProcedure<UserStat>("GetFinishProjectCount", paras, con);
            return list;
        }

        /// <summary>
        /// 获取用户数量
        /// </summary>
        /// <param name="startDate"></param>
        /// <param name="endDate"></param>
        /// <returns></returns>
        private IList<UserStat> GetUserCount(string testUserCon)
        {
            IDbConnection con = new MySqlConnection(testUserCon);
            var list = SqlHelper.CallProcedure<UserStat>("GetCompanyCount", null, con);
            return list;
        }


        #endregion
    }
}
