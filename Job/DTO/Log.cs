using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Job.DTO
{
    public class Log
    {
        /// <summary>
        /// 请求内容
        /// </summary>
        [JsonProperty("requestconcent")]
        public string Content { get; set; }

        /// <summary>
        /// 公司名称
        /// </summary>
        [JsonProperty("remote_company")]
        public string Company { get; set; }

        /// <summary>
        /// 用户名称
        /// </summary>
        [JsonProperty("remote_user")]
        public string User { get; set; }

        public int StatCount { get; set; }
    }
}
