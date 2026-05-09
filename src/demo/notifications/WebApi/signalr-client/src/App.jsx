import { useMemo, useRef, useState } from "react";
import * as signalR from "@microsoft/signalr";

function formatPayload(payload) {
  if (payload === undefined || payload === null) {
    return "";
  }

  if (typeof payload === "string") {
    return payload;
  }

  try {
    return JSON.stringify(payload, null, 2);
  } catch {
    return String(payload);
  }
}

function getDefaultHubUrl() {
  if (typeof window === "undefined") {
    return "";
  }

  return `${window.location.origin}/notificationHub`;
}

export default function App() {
  const [jwt, setJwt] = useState("");
  const [hubUrl, setHubUrl] = useState(getDefaultHubUrl);
  const [status, setStatus] = useState("Disconnected");
  const [messages, setMessages] = useState([]);
  const [isConnecting, setIsConnecting] = useState(false);
  const connectionRef = useRef(null);

  const canConnect = useMemo(() => {
    return !isConnecting && jwt.trim().length > 0 && hubUrl.trim().length > 0;
  }, [hubUrl, isConnecting, jwt]);

  const appendMessage = (title, tone = "neutral", payload = null) => {
    setMessages((previous) => [
      {
        id: crypto.randomUUID(),
        title,
        tone,
        payload: formatPayload(payload),
        timestamp: new Date().toLocaleTimeString()
      },
      ...previous
    ]);
  };

  const registerHandlers = (connection) => {
    connection.on("connected", () => {
      setStatus("Connected");
      appendMessage("Connected.", "success");
    });

    connection.on("disconnected", () => {
      setStatus("Disconnected");
      appendMessage("Disconnected, invalid token.", "danger");
    });

    connection.on("operation_pending", (operation) => {
      appendMessage("Operation pending.", "neutral", operation);
    });

    connection.on("operation_completed", (operation) => {
      appendMessage("Operation completed.", "success", operation);
    });

    connection.on("operation_rejected", (operation) => {
      appendMessage("Operation rejected.", "danger", operation);
    });

    connection.on("order_created", (order) => {
      appendMessage("Order created.", "success", order);
    });

    connection.on("PublishNotification", (operation) => {
      appendMessage("Notification pushed.", "success", operation);
    });

    connection.onreconnecting(() => {
      setStatus("Reconnecting");
      appendMessage("Connection lost. Reconnecting...", "warning");
    });

    connection.onreconnected(() => {
      setStatus("Connected");
      appendMessage("Reconnected.", "success");
    });

    connection.onclose(() => {
      setStatus("Disconnected");
      appendMessage("Connection closed.", "warning");
    });
  };

  const connect = async () => {
    const trimmedJwt = jwt.trim();
    const trimmedHubUrl = hubUrl.trim();

    if (!trimmedJwt) {
      appendMessage("JWT is required.", "danger");
      return;
    }

    if (!trimmedHubUrl) {
      appendMessage("Hub URL is required.", "danger");
      return;
    }

    if (connectionRef.current) {
      await disconnect();
    }

    setIsConnecting(true);
    setStatus("Connecting");

    const connection = new signalR.HubConnectionBuilder()
      .withUrl(trimmedHubUrl)
      .withAutomaticReconnect()
      .configureLogging(signalR.LogLevel.Information)
      .build();

    registerHandlers(connection);

    try {
      await connection.start();
      connectionRef.current = connection;
      await connection.invoke("initializeAsync", trimmedJwt);
      setStatus("Connected");
      appendMessage("Handshake completed.", "success");
    } catch (error) {
      setStatus("Disconnected");
      appendMessage("Unable to connect.", "danger", error?.message ?? error);
      await connection.stop();
    } finally {
      setIsConnecting(false);
    }
  };

  const disconnect = async () => {
    if (!connectionRef.current) {
      return;
    }

    const connection = connectionRef.current;
    connectionRef.current = null;

    try {
      await connection.stop();
      setStatus("Disconnected");
      appendMessage("Disconnected by user.", "neutral");
    } catch (error) {
      appendMessage("Error while disconnecting.", "danger", error?.message ?? error);
    }
  };

  return (
    <div className="app-shell">
      <main className="panel">
        <header>
          <p className="eyebrow">Genocs Notifications</p>
          <h1>SignalR Client</h1>
          <p className="subtitle">
            Connect to the notifications hub and inspect realtime events.
          </p>
        </header>

        <section className="controls">
          <label htmlFor="hubUrl">Hub URL</label>
          <input
            id="hubUrl"
            value={hubUrl}
            onChange={(event) => setHubUrl(event.target.value)}
            placeholder="https://localhost:5540/notificationHub"
          />

          <label htmlFor="jwt">JWT</label>
          <textarea
            id="jwt"
            value={jwt}
            onChange={(event) => setJwt(event.target.value)}
            rows={4}
            placeholder="Paste JWT token"
          />

          <div className="buttons">
            <button onClick={connect} disabled={!canConnect} className="primary">
              {isConnecting ? "Connecting..." : "Connect"}
            </button>
            <button onClick={disconnect} className="ghost">
              Disconnect
            </button>
          </div>
        </section>

        <section className="status-row">
          <span>Status</span>
          <strong data-status={status.toLowerCase()}>{status}</strong>
        </section>

        <section className="messages">
          <h2>Messages</h2>
          {messages.length === 0 ? (
            <p className="empty">No events yet.</p>
          ) : (
            <ul>
              {messages.map((message) => (
                <li key={message.id} data-tone={message.tone}>
                  <div className="line">
                    <span>{message.title}</span>
                    <time>{message.timestamp}</time>
                  </div>
                  {message.payload ? <pre>{message.payload}</pre> : null}
                </li>
              ))}
            </ul>
          )}
        </section>
      </main>
    </div>
  );
}
