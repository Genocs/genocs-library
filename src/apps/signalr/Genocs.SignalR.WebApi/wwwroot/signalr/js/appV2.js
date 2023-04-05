'use strict';

const connection = new signalR.HubConnectionBuilder()
    .withUrl("http://localhost:5007/notificationHub")
    .configureLogging(signalR.LogLevel.Information)
    .build();

async function start() {
    try {
        await connection.start();
        console.log("SignalR Connected.");
    } catch (err) {
        console.log(err);
        setTimeout(start, 5000);
    }
};

connection.onclose(async () => {
    console.log("Close");
    //await start();
});

// Start the connection.
start();
