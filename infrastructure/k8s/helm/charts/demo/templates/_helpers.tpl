{{/* vim: set filetype=mustache: */}}

{{/* Chart name. */}}
{{- define "demo.name" -}}
{{- default .Chart.Name .Values.nameOverride | trunc 63 | trimSuffix "-" -}}
{{- end -}}

{{/* Chart name and version, used as a label. */}}
{{- define "demo.chart" -}}
{{- printf "%s-%s" .Chart.Name .Chart.Version | replace "+" "_" | trunc 63 | trimSuffix "-" -}}
{{- end -}}

{{/*
Common environment variables injected into every demo container.
Mirrors the shared `environment:` block in docker-compose.yml.

Usage:
  {{ include "demo.commonEnv" . | nindent 12 }}
*/}}
{{- define "demo.commonEnv" -}}
- name: ASPNETCORE_ENVIRONMENT
  value: {{ .Values.env.aspnetcoreEnvironment | quote }}
- name: mongodb__connectionString
  value: {{ .Values.env.mongodb.connectionString | quote }}
- name: seq__url
  value: {{ .Values.env.seq.url | quote }}
- name: exporter__otlpEndpoint
  value: {{ .Values.env.exporter.otlpEndpoint | quote }}
{{- end -}}

{{/*
Extra environment variables injected only into web API containers
(ASP.NET URL bindings).
*/}}
{{- define "demo.webapiEnv" -}}
- name: ASPNETCORE_URLS
  value: "http://+:{{ .containerPort }}"
{{- end -}}

{{/*
Probe block. Accepts the service-specific values map.
Usage:
  {{ include "demo.probes" .Values.demoWebapi | nindent 10 }}
*/}}
{{- define "demo.probes" -}}
{{- if .probes.enabled }}
livenessProbe:
  httpGet:
    path: {{ .probes.liveness.path }}
    port: {{ .probes.liveness.port }}
readinessProbe:
  httpGet:
    path: {{ .probes.readiness.path }}
    port: {{ .probes.readiness.port }}
{{- end }}
{{- end -}}
