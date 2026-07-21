# Notifications SignalR React Client

This folder contains a modern React + Vite client for the Notifications SignalR demo.

## Run in dev mode

```bash
npm install
npm run dev
```

## Build static assets for WebApi

```bash
npm install
npm run build
```

The build output is written to:

- `../wwwroot/signalr/index.html`
- `../wwwroot/signalr/assets/*`

When the Notifications WebApi is running, open:

- `/wwwroot/signalR`

The server redirects to `/wwwroot/signalr/index.html` and serves the React app.
