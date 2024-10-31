"use strict";

var connection = new signalR.HubConnectionBuilder()
    .withUrl("/online")
    .withAutomaticReconnect()  // Ajout pour gérer les reconnexions automatiques
    .configureLogging(signalR.LogLevel.Information)  // Ajout de logs pour plus de détails
    .build();

// Ajuster le timeout du serveur à 60 secondes
connection.serverTimeoutInMilliseconds = 60000;  // 60 secondes de timeout

connection.on("ReceiveMessage", function (message) {
    console.log("message", message);
});

connection.start().then(function () {
    console.log("connected");
    myFunction();

    setInterval(function () {
        myFunction()
    }, 10000)
}).catch(function (err) {
    return console.error(err.toString());
});

function myFunction() {
    let res = connection.invoke("SendMessage").catch(function (err) {
        return console.error(err.toString());
    });
}
