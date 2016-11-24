using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Job.DTO
{
    public class UserStat
    {
        /// <summary>
        /// 用户名称
        /// </summary>
        public string UserName { get; set; } 
        
        /// <summary>
        /// 公司名称
        /// </summary>
        public string Company { get; set; }

        /// <summary>
        /// 用户数量 
        /// </summary>
        public int UserCount { get; set; }

        /// <summary>
        /// 估值数量
        /// </summary>
        public int InquiryCount { get; set; }

        /// <summary>
        /// 接单数量
        /// </summary>
        public int AddProjectCount { get; set; }

        /// <summary>
        /// 外勘数量
        /// </summary>
        public int OutTaskCount { get; set; }

        /// <summary>
        /// 报告完成数量
        /// </summary>
        public int ProjectFinishCount { get; set; }

        /// <summary>
        /// 取得询价结果
        /// </summary>
        public int InquiryResult { get; set; }

        /// <summary>
        /// 获取历史询价记录
        /// </summary>
        public int InquiryHistory { get; set; }

        /// <summary>
        /// 获取报盘案例
        /// </summary>
        public int OfferCase { get; set; }

        /// <summary>
        /// 获取成交案例
        /// </summary>
        public int DealCase { get; set; }

        /// <summary>
        /// 获取报告案例
        /// </summary>
        public int ReportCase { get; set; }

        /// <summary>
        /// 企业版用户登录 
        /// </summary>
        public int UserLogin { get; set; }

        /// <summary>
        /// 综合查询
        /// </summary>
        public int IntegratedQuery { get; set; }
        

        /// <summary>
        /// 根据公司获取外采用户
        /// </summary>
        public int WaicaiUser { get; set; }

        /// <summary>
        /// 获取立项列表
        /// </summary>
        public int ProjectList { get; set; }

        /// <summary>
        /// 免费版用户登录
        /// </summary>
        public int PepUserLogin { get; set; }

        /// <summary>
        /// 查询线上报告
        /// </summary>
        public int SearchOnlineProject { get; set; }

        /// <summary>
        /// 获取项目详细信息
        /// </summary>
        public int ProjectData { get; set; }

        /// <summary>
        /// 项目提交审核
        /// </summary>
        public int ProjectSubmit { get; set; }

        /// <summary>
        /// 项目审核通过
        /// </summary>
        public int ProjectApprove { get; set; }

        #region 微信端

        /// <summary>
        /// 微信端获取确认责任列表
        /// </summary>
        public int ConfrimListCount { get; set; }

        /// <summary>
        /// 微信端获取流程跟踪列表
        /// </summary>
        public int StatlogCount { get; set; }

        /// <summary>
        /// 微信端获取项目反馈列表
        /// </summary>
        public int FeedbackCount { get; set; }

        /// <summary>
        /// 微信端进行最低收费修改
        /// </summary>
        public int AlterFeeCount { get; set; }

        /// <summary>
        /// 微信端进行收费确认
        /// </summary>
        public int ConfirmFeeCount { get; set; }

        /// <summary>
        /// 微信端发送短信
        /// </summary>
        public int SMSCount { get; set; }

        /// <summary>
        /// 微信端真伪验证
        /// </summary>
        public int ScanCount { get; set; }

        #endregion 
    }
}
