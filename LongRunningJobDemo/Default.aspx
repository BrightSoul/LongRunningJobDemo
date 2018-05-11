<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="LongRunningJobDemo.Default" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
<meta http-equiv="Content-Type" content="text/html; charset=utf-8"/>
    <title>Demo Hangfire + SignalR</title>
    <style>
        * {font-family: Calibri, Arial, sans-serif;}
        div {padding:10px;}
        button {padding:5px; background:white; border:1px solid #333; min-width:80px;}
        button:hover { background-color:#ddd;}
        #stato { color:white; padding:3px; }
        .green { background-color:green; }
        .red { background-color:red; }
        .yellow {background-color:goldenrod;}
    </style>
</head>
<body>
    <h1>Demo Hangfire + SignalR</h1>
    <h4>Per eseguire operazioni di lunga durata ed avere informazioni sul loro progresso.</h4>
    <fieldset>
    <legend>Gestione del job</legend>
    <div>Stato: <span id="stato">Attendi...</span></div>
    <div id="progresso" style="visibility:hidden">Progresso: <progress id="barra" max="100"></progress> <span id="stima"></span></div>
    <div>
        <button id="avvia" style="display:none;" type="button" onclick="hubProgresso.server.avvia()">Avvia</button>
        <button id="ferma" style="display:none;" type="button" onclick="hubProgresso.server.ferma()">Ferma</button>
    </div>
    </fieldset>
    <p id="avviso" style="font-size: smaller; display:none;">Mentre il job è in esecuzione puoi visitare altre pagine, come la <a href="<%: ResolveClientUrl("/Jobs") %>">dashboard di Hangfire</a>, per poi tornare qui e verificare il progresso.</p>

    <!-- Scripts da includere -->
    <script src="<%: ResolveClientUrl("~/Scripts/jquery-1.6.4.min.js") %>"></script>
    <script src="<%: ResolveClientUrl("~/Scripts/jquery.signalR-2.2.0.min.js") %>"></script>
    <script src="<%: ResolveClientUrl("~/signalr/hubs") %>"></script>

    <!-- Funzioni per collegarsi all'hub -->
    <script>
        var hubProgresso = $.connection.hubProgresso;
        var lettura = null;

        //Questa è la funzione javascript che verrà "invocata dal server"
        hubProgresso.client.aggiornaProgresso = function (inEsecuzione, percentuale) {
            //Aggiorno i valori nella pagina quando il server mi manda 
            //la nuova percentuale e il nuovo stato
            var stato = document.getElementById("stato");
            stato.innerText = inEsecuzione ? "In corso" : "Fermo";
            stato.className = inEsecuzione ? "green" : "red"

            //Mostro gli elementi dell'interfaccia in base allo stato del job
            var progresso = document.getElementById("progresso");
            var barra = document.getElementById("barra");
            var avvia = document.getElementById("avvia");
            var ferma = document.getElementById("ferma");
            var avviso = document.getElementById("avviso");
            var stima = document.getElementById("stima");

            if (inEsecuzione) {
                avvia.style.display = "none";
                avviso.style.display = "";
                if (percentuale) {
                    barra.value = percentuale;
                    barra.innerText = percentuale + "%";
                    stato.innerText = "Avviato";
                    ferma.style.display = "";
                    stato.className = "green";

                    //Stimo un tempo di completamento
                    if (lettura == null) {
                        lettura = { timestamp: new Date(), percentuale: percentuale };
                    } else if (lettura.percentuale < percentuale) {
                        var differenzaDiTempo = (new Date()).getTime() - lettura.timestamp.getTime();
                        var differenzaPuntiPercentuali = percentuale - lettura.percentuale;
                        var puntiPercentualiRestanti = 100 - percentuale;
                        var secondi = Math.round((differenzaDiTempo / differenzaPuntiPercentuali) * puntiPercentualiRestanti / 1000);

                        stima.innerText = "terminerà tra " + secondi + " secondi";
                    }


                } else {
                    stima.innerText = "";
                    barra.removeAttribute("value");
                    stato.innerText = "Attendere...";
                    ferma.style.display = "none";
                    stato.className = "yellow";
                }
                progresso.style.visibility = "visible";
            } else {
                lettura = null;
                avvia.style.display = "";
                ferma.style.display = "none";
                progresso.style.visibility = "hidden";
                avviso.style.display = "none";
                stato.innerText = "Fermo";
                stato.className = "red";
            }




        };
        //Avvio la connessione all'hub
        $.connection.hub.start();
    </script>
</body>
</html>
