using Microsoft.AspNet.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Threading.Tasks;
using Hangfire;
using LongRunningJobDemo.Jobs;
using Microsoft.AspNet.SignalR.Hubs;

namespace LongRunningJobDemo.Hubs
{
    public class HubProgresso : Hub
    {
        public void Avvia()
        {
            var monitor = JobStorage.Current.GetMonitoringApi();
            //Se non c'è alcun job attualmente in esecuzione...
            if (!monitor.JobInEsecuzione())
            {
                //Allora lo avvio
                BackgroundJob.Enqueue<MioJobDemo>(mioJobDemo => mioJobDemo.ElaborazioneDiLungaDurata(JobCancellationToken.Null));
                AggiornaProgresso(inEsecuzione: true, percentuale: null, clients: Clients.All);
            }
        }
        public void Ferma()
        {
            var monitor = JobStorage.Current.GetMonitoringApi();
            //Se c'è un job, attualmente in esecuzione...
            if (monitor.JobInEsecuzione())
            {
                var jobs = monitor.ProcessingJobs(0, Convert.ToInt32(monitor.ProcessingCount()));
                foreach (var job in jobs)
                {
                    //...lo fermiamo
                    BackgroundJob.Delete(job.Key);
                }
            }
        }

        //Questo metodo lo invoca il job (MioJobDemo)
        public static void AggiornaProgresso(bool inEsecuzione, int? percentuale = null, dynamic clients = null)
        {


            //Ottengo un riferimento a questo hub SignalR per segnalare al client
            //l'aggiornamento di stato e di percentuale
            if (clients == null)
            clients = GlobalHost.ConnectionManager.GetHubContext<HubProgresso>().Clients.All;

            //invoco la funzione javascript aggiornaProgresso sul client
            if (percentuale.HasValue)
                clients.aggiornaProgresso(inEsecuzione, percentuale);
            else
                clients.aggiornaProgresso(inEsecuzione);
        }
        public override Task OnConnected()
        {
            //Quando un client si collega, gli notifico lo stato attuale del job
            //E' possibile che quando il client apre la pagina, il job sia già in esecuzione
            //perché il job va in esecuzione al di fuori della richiesta web
            //e quindi il client, durante l'esecuzione del job, è libero di cambiare pagina
            //o aprire e chiudere il browser, se vuole.
            //Il job continuerà ad andare finché IIS tiene viva l'applicazione.
            //http://docs.hangfire.io/en/latest/deployment-to-production/making-aspnet-app-always-running.html
            var monitor = JobStorage.Current.GetMonitoringApi();
            AggiornaProgresso(inEsecuzione: monitor.JobInEsecuzione(), percentuale: null, clients: Clients.All);

            return base.OnConnected();
        }
    }
}