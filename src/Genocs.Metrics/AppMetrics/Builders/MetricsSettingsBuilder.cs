using Genocs.Metrics.AppMetrics.Configurations;

namespace Genocs.Metrics.AppMetrics.Builders;

internal sealed class MetricsSettingsBuilder : IMetricsSettingsBuilder
{
    private readonly MetricsSettings _settings = new();

    public IMetricsSettingsBuilder Enable(bool enabled)
    {
        _settings.Enabled = enabled;
        return this;
    }

    public IMetricsSettingsBuilder WithInfluxEnabled(bool influxEnabled)
    {
        _settings.InfluxEnabled = influxEnabled;
        return this;
    }

    public IMetricsSettingsBuilder WithPrometheusEnabled(bool prometheusEnabled)
    {
        _settings.PrometheusEnabled = prometheusEnabled;
        return this;
    }

    public IMetricsSettingsBuilder WithPrometheusFormatter(string prometheusFormatter)
    {
        _settings.PrometheusFormatter = prometheusFormatter;
        return this;
    }

    public IMetricsSettingsBuilder WithInfluxUrl(string influxUrl)
    {
        _settings.InfluxUrl = influxUrl;
        return this;
    }

    public IMetricsSettingsBuilder WithDatabase(string database)
    {
        _settings.Database = database;
        return this;
    }

    public IMetricsSettingsBuilder WithInterval(int interval)
    {
        _settings.Interval = interval;
        return this;
    }

    public IMetricsSettingsBuilder WithTags(IDictionary<string, string> tags)
    {
        _settings.Tags = tags;
        return this;
    }

    public MetricsSettings Build()
        => _settings;
}