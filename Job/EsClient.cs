using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using Elasticsearch.Net;
using Nest;

namespace Job
{
    public class EsClient
    {
        public static ElasticClient GetClient()
        {
            var node = new Uri(ConfigurationManager.AppSettings["EsUrl"]);
            var settings = new ConnectionSettings(node);
            var client = new ElasticClient(settings);
            return client;
        }
    }
}
