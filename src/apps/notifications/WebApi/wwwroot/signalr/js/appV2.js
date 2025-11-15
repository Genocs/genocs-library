'use strict';
(function () {

    const $jwt = document.getElementById("jwt");
    const $connect = document.getElementById("connect");
    const $messages = document.getElementById("messages");

    const connection = new signalR.HubConnectionBuilder()
        .withUrl("https://localhost:5014/notificationHub")
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

    // Start the connection.
    $connect.onclick = function () {
        const jwt = $jwt.value;
        if (!jwt || /\s/g.test(jwt)) {
            alert('Invalid JWT.')
            return;
        }

        appendMessage('Connecting to Genocs Hub...');
        connection.start()
            .then(() => {
                console.log("SignalR Connected.");
                connection.invoke('initializeAsync', $jwt.value);
            })
            .catch(err => appendMessage(err));
    }

    connection.onclose(async () => {
        console.log('Close');
        await start();
    });


    connection.on('connected', _ => {
        appendMessage('Connected.', 'primary');
    });

    connection.on('disconnected', _ => {
        appendMessage('Disconnected, invalid token.', 'danger');
    });

    connection.on('operation_pending', (operation) => {
        appendMessage('Operation pending.', "light", operation);
    });

    connection.on('operation_completed', (operation) => {
        appendMessage('Operation completed.', "success", operation);
    });

    connection.on('operation_rejected', (operation) => {
        appendMessage('Operation rejected.', "danger", operation);
    });


    connection.on("PublishNotification", (operation) => {
        appendMessage('Notification pushed.', 'success', operation)
    });

    function appendMessage(message, type, data) {
        var dataInfo = "";
        if (data) {
            dataInfo += "<div>" + JSON.stringify(data) + "</div>";
        }
        $messages.innerHTML += `<li class="list-group-item list-group-item-${type}">${message} ${dataInfo}</li>`;
    }


    // Start the connection.
    //start();
})();