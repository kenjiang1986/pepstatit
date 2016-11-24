using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Job.DTO
{
    /// <summary>
    /// 评E评统计类型
    /// </summary>
    public enum StatType
    {
        InquiryResult,

        InquiryHistory,

        OfferCase,

        DealCase,

        ReportCase,

        ConfrimList,

        Statlog,

        Feedback,

        AlterFee,

        ConfirmFee,

        SMS,

        UserLogin,

        IntegratedQuery,

        PepInquiryResult,

        PepInquiryHistory,

        PepOfferCase,

        PepDealCase,

        PepReportCase,

        WaicaiUser,

        ProjectList,

        PepUserLogin,

        SearchOnlineProject,

        ProjectData,

        ProjectSubmit,

        ProjectApprove,

        Scan
    }

    /// <summary>
    /// sheet列表类型
    /// </summary>
    public enum ListType
    {
        List,

        DemoList,

        WechatList,

        SimpleList
    }

   /// <summary>
   /// 微信统计类型
   /// </summary>
    public enum PepStatType
    {
        PepOfferCase,

        PepDealCase,

        PepReportCase,
       
        WaicaiUser,

        ProjectList,

        PepUserLogin,

        SearchOnlineProject,

        ProjectData,

        ProjectSubmit,

        ProjectApprove
    }
}
