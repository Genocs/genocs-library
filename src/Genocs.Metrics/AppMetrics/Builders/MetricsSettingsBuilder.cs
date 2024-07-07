using Genocs.Metrics.AppMetrics.Configurations;

namespace Genocs.Metrics.AppMetrics.Builders;

internal sealed class MetricsSettingsBuilder : IMetricsSettingsBuilder
{
    private readonly MetricsSettings _options = new();

    public IMetricsSettingsBuilder Enable(bool enabled)
    {
        _options.Enabled = enabled;
        return this;
    }

    public IMetricsSettingsBuilder WithInfluxEnabled(bool influxEnabled)
    {
        _options.InfluxEnabled = influxEnabled;
        return this;
    }

    public IMetricsSettingsBuilder WithPrometheusEnabled(bool prometheusEnabled)
    {
        _options.PrometheusEnabled = prometheusEnabled;
        return this;
    }

    public IMetricsSettingsBuilder WithPrometheusFormatter(string prometheusFormatter)
    {
        _options.PrometheusFormatter = prometheusFormatter;
        return this;
    }

    public IMetricsSettingsBuilder WithInfluxUrl(string influxUrl)
    {
        _options.InfluxUrl = influxUrl;
        return this;
    }

    public IMetricsSettingsBuilder WithDatabase(string database)
    {
        _options.Database = database;
        return this;
    }

    public IMetricsSettingsBuilder WithInterval(int interval)
    {
        _options.Interval = interval;
        return this;
    }

    public IMetricsSettingsBuilder WithTags(IDictionary<string, string> tags)
    {
        _options.Tags = tags;
        return this;
    }

    public MetricsSettings Build()
        => _options;
}