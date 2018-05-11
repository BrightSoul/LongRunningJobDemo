using Hangfire;
using LongRunningJobDemo.Hubs;
using Microsoft.AspNet.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Web;

namespace LongRunningJobDemo.Jobs
{
    public sealed class MioJobDemo
    {

        public void ElaborazioneDiLungaDurata(IJobCancellationToken cancellationToken)
        {
            for (var i = 0; i<=100; i++)
            {
                HubProgresso.AggiornaProgresso(inEsecuzione: true, percentuale: i);

                //Simulo lentezza nell'elaborazione
                Thread.Sleep(500);

                //Interrompo l'esecuzione se è stato richiesto l'arresto del job
                try
                {
                    cancellationToken.ThrowIfCancellationRequested();
                }
                catch {
                    break; //throw;
                }
            }
            HubProgresso.AggiornaProgresso(inEsecuzione: false);
        }

    }
}