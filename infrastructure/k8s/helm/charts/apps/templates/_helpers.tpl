{{/* Expand the name of the chart. */}}
{{- define "apps.name" -}}
{{- default .Chart.Name .Values.nameOverride | trunc 63 | trimSuffix "-" -}}
{{- end -}}

{{/* Create a default fully qualified app name. */}}
{{- define "apps.fullname" -}}
{{- if .Values.fullnameOverride -}}
{{- .Values.fullnameOverride | trunc 63 | trimSuffix "-" -}}
{{- else -}}
{{- $name := default .Chart.Name .Values.nameOverride -}}
{{- if contains $name .Release.Name -}}
{{- .Release.Name | trunc 63 | trimSuffix "-" -}}
{{- else -}}
{{- printf "%s-%s" .Release.Name $name | trunc 63 | trimSuffix "-" -}}
{{- end -}}
{{- end -}}
{{- end -}}

{{/* Chart name and version as used by the chart label. */}}
{{- define "apps.chart" -}}
{{- printf "%s-%s" .Chart.Name .Chart.Version | replace "+" "_" | trunc 63 | trimSuffix "-" -}}
{{- end -}}

{{/*
Common environment variables injected into every service container.
Mirrors the shared `environment:` block of docker-compose.yml.

Usage:
  {{ include "apps.commonEnv" (dict "root" . "consulAddress" "apigateway") | nindent 12 }}
*/}}
{{- define "apps.commonEnv" -}}
{{- $root := .root -}}
- name: ASPNETCORE_ENVIRONMENT
  value: {{ $root.Values.env.aspnetcoreEnvironment | quote }}
- name: consul__url
  value: {{ $root.Values.env.consul.url | quote }}
- name: consul__address
  value: {{ .consulAddress | quote }}
- name: consul__port
  value: {{ $root.Values.env.consul.port | quote }}
- name: fabio__url
  value: {{ $root.Values.env.fabio.url | quote }}
- name: logger__seq__enabled
  value: {{ $root.Values.env.logger.seq.enabled | quote }}
- name: logger__seq__url
  value: {{ $root.Values.env.logger.seq.url | quote }}
- name: logger__seq__apiKey
  value: {{ $root.Values.env.logger.seq.apiKey | quote }}
- name: logger__httpPayload__enabled
  value: {{ $root.Values.env.logger.httpPayload.enabled | quote }}
- name: logger__httpPayload__captureRequestBody
  value: {{ $root.Values.env.logger.httpPayload.captureRequestBody | quote }}
- name: telemetry__exporter__enabled
  value: {{ $root.Values.env.telemetry.exporter.enabled | quote }}
- name: telemetry__exporter__otlpEndpoint
  value: {{ $root.Values.env.telemetry.exporter.otlpEndpoint | quote }}
- name: mongodb__connectionString
  value: {{ $root.Values.env.mongodb.connectionString | quote }}
{{- end -}}

{{/*
Probe block for a service. Accepts the service-specific values map.
Usage:
  {{ include "apps.probes" .Values.apigateway | nindent 10 }}
*/}}
{{- define "apps.probes" -}}
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
