using Hangfire.Storage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LongRunningJobDemo
{
    public static class ExtensionMethods
    {
        public static bool JobInEsecuzione(this IMonitoringApi api)
        {
            return api.ProcessingCount() + api.EnqueuedCount("default") > 0;
        }
    }
}