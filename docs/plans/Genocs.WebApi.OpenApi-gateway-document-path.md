# Plan: gateway-safe OpenAPI document paths in `Genocs.WebApi.OpenApi`

Implementation brief for an agent working in the **genocs-library** repository (package source). This file lives in the enterprise-app repo because that is where the bug was observed. Do not implement this inside enterprise-app.

## Goal

Swagger UI served through the YARP API gateway at

`http://localhost:5500/notifications-service/swagger`

must load

`http://localhost:5500/notifications-service/swagger/Notifications/swagger.json`

and **Try it out** must call the gateway prefix, not the in-cluster origin.

## Current behavior (verified on `Genocs.WebApi.OpenApi` 9.1.0-beta01)

`UseOpenApiDocs()` registers Swashbuckle and emits a host-root spec URL. The live `swagger/index.js` contains:

```javascript
var configObject = {"urls":[{"url":"/swagger/Notifications/swagger.json","name":"Notifications Service"}], ...};

configObject.urls.forEach(function (item) {
    if (item.url.startsWith("http") || item.url.startsWith("/")) return;
    item.url = window.location.href.split(/[?#]/)[0].replace("index.html", item.url).split('#')[0];
});
```

The gateway route is `notifications-service/{**catchall}` with `PathPattern: {**catchall}`. A browser request to `/swagger/Notifications/swagger.json` matches no route. The prefixed URL ` /notifications-service/swagger/Notifications/swagger.json` already returns the document (HTTP 200).

The document `servers[0].url` is `http://localhost:5541` (from `openapi.servers`). **Try it out** therefore leaves the gateway even after the spec loads.

There is no package option today that changes the spec URL. `routePrefix`, `name`, and `servers` do not. An empty `routePrefix` still yields a host-root path (`/Notifications/swagger.json`).

Swashbuckle redirects `/swagger` with a relative `Location: swagger/index.html`, so the UI page stays under the gateway prefix. Only the spec URL and the server URL are wrong.

## Required behavior

Two independent opt-in settings. Defaults preserve today's URLs.

### 1. `documentPath`

Config key: `openapi.documentPath`. Values: `absolute` (default) and `relative`.

| `routePrefix` | `name` | `absolute` | `relative` |
|---|---|---|---|
| `swagger` | `Notifications` | `/swagger/Notifications/swagger.json` | `Notifications/swagger.json` |
| `docs` | `v1` | `/docs/v1/swagger.json` | `v1/swagger.json` |
| empty | `v1` | `/v1/swagger.json` | `v1/swagger.json` |

Use that string for Swagger UI (`SwaggerEndpoint`) and ReDoc (`SpecUrl`).

`relative` works because Swashbuckle's `index.js` resolves a URL that does not start with `/` or `http` by replacing `index.html` in the current page URL. At `.../notifications-service/swagger/index.html` the result is `.../notifications-service/swagger/Notifications/swagger.json`. The same relative value still works when the UI is opened directly at `/swagger/index.html`.

Unknown values fail at startup with a message that lists `absolute` and `relative`. Do not silently fall back.

### 2. `useForwardedPrefix`

Config key: `openapi.useForwardedPrefix`. Default: `false`.

When `true`, a Swashbuckle document filter (or `PreSerializeFilter`) runs per request:

- Read the `X-Forwarded-Prefix` header.
- Accept it only when it is a single path prefix: starts with `/`, no scheme, no `//`, no `?`, no `#`, no trailing slash except the value `/`.
- When valid, set the OpenAPI `servers` list to one entry whose `url` is that prefix (for example `/notifications-service`).
- When the header is missing or invalid, keep the configured `openapi.servers` unchanged.

When `false`, ignore the header completely.

This is opt-in because a client-supplied prefix would otherwise change the server URL. Callers must overwrite the header at the trusted proxy. In enterprise-app that is YARP:

```json
"transforms": [
  { "PathPattern": "{**catchall}" },
  { "RequestHeader": "X-Forwarded-Prefix", "Set": "/notifications-service" }
]
```

Do not implement the YARP change in the library repo.

## Non-goals

- Do not change the default to `relative`.
- Do not read `X-Forwarded-Prefix` unless `useForwardedPrefix` is true.
- Do not add a per-service public base URL setting. The relative spec URL does not need one, and the prefix header covers **Try it out**.
- Do not rewrite response bodies inside the library.
- Do not change route templates for the JSON document itself. It stays `/{routePrefix}/{name}/swagger.json` on the service.

## Where to change the library

Find the existing `UseOpenApiDocs` implementation and the `SwaggerEndpoint` / `set_SpecUrl` call sites. Package strings in 9.1.0-beta01 confirm those APIs exist (`SwaggerEndpoint`, `set_SpecUrl`, `WithRoutePrefix`, `WithServers`, `FormatEmptyRoutePrefix`). Also update:

- `OpenApiOptions` (the `openapi` configuration object)
- configuration binding
- `IOpenApiOptionsBuilder` and its builder methods (`WithDocumentPath`, `UseForwardedPrefix` or equivalent names already used by the builder style)
- the agent documentation shipped with the library for this package

Match existing option naming, XML docs, and test style in that repo. Do not invent a second options type.

## Implementation steps

1. Add `DocumentPath` (string, default `"absolute"`) and `UseForwardedPrefix` (bool, default `false`) to `OpenApiOptions`.
2. Add a single helper, used by both UIs, that builds the spec URL from `routePrefix`, `name`, and `documentPath`. Normalize an empty `routePrefix` the same way `FormatEmptyRoutePrefix` already does.
3. Pass the helper result to `SwaggerEndpoint` and ReDoc `SpecUrl` instead of the hard-coded host-root string.
4. When `UseForwardedPrefix` is true, register one filter that replaces `servers` only for a valid `X-Forwarded-Prefix` on the current `HttpRequest`.
5. Validate `documentPath` when options are built, not on each request.

## Tests

- URL helper: the three rows in the table above, plus a rejected value such as `root`.
- `WebApplicationFactory` (or the repo's existing host test style): with `documentPath=relative`, `GET /swagger/index.js` contains `"url":"Notifications/swagger.json"` (or the equivalent document name) and does not contain `"/swagger/Notifications/swagger.json"`.
- With `documentPath` omitted, the index script still contains the absolute `/swagger/...` URL.
- With `useForwardedPrefix=true` and header `X-Forwarded-Prefix: /notifications-service`, the generated document `servers[0].url` is `/notifications-service`.
- With the option false, the same header does not change `servers`.
- With the option true and header `https://evil.example`, `servers` stay at the configured value.

## Docs to update in the library

Update the `Genocs.WebApi.OpenApi` agent documentation so a later agent will not assume the spec URL is always host-root:

- Document `documentPath` and `useForwardedPrefix`, including defaults.
- State that `servers` still control **Try it out** when the header is absent.
- State that a relative spec URL is what makes Swagger UI work behind a path-stripping reverse proxy.
- Add a failure note: if the UI still requests `/swagger/{name}/swagger.json` from the site root, `documentPath` is not `relative`.

## After the package is published (enterprise-app, separate change)

Do this only in enterprise-app, after a new `9.1.x` prerelease is consumed:

1. Bump `Genocs.WebApi.OpenApi` on the WebApi hosts.
2. Set `openapi.documentPath` to `relative` and `openapi.useForwardedPrefix` to `true` on services exposed by the gateway.
3. Add the `X-Forwarded-Prefix` request-header transform on each `*-service` route in `src/apigateway/WebApi/appsettings.json`.
4. Remove `src/apigateway/WebApi/Framework/SwaggerUiPathTransformProvider.cs` and its `AddTransforms<SwaggerUiPathTransformProvider>()` registration in `src/apigateway/WebApi/Program.cs`. That transform rewrites `index.js` and `swagger.json` and will fight the library.
5. Rebuild the gateway and the notifications service. Confirm the browser requests `/notifications-service/swagger/Notifications/swagger.json` and **Try it out** stays on port 5500.

## Done when

- Default hosts still emit `/swagger/{name}/swagger.json`.
- `documentPath=relative` emits `{name}/swagger.json` in Swagger UI and ReDoc.
- `useForwardedPrefix=true` honors only a safe `X-Forwarded-Prefix`.
- Tests and the package agent doc cover both settings.
