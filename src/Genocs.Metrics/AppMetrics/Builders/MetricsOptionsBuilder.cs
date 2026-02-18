using Genocs.Metrics.AppMetrics.Configurations;

namespace Genocs.Metrics.AppMetrics.Builders;

internal sealed class MetricsOptionsBuilder : IMetricsOptionsBuilder
{
    private readonly MetricsOptions _settings = new();

    public IMetricsOptionsBuilder Enable(bool enabled)
    {
        _settings.Enabled = enabled;
        return this;
    }

    // public IMetricsOptionsBuilder WithInfluxEnabled(bool influxEnabled)
    // {
    //     _settings.InfluxEnabled = influxEnabled;
    //     return this;
    // }

    public IMetricsOptionsBuilder WithPrometheusEnabled(bool prometheusEnabled)
    {
        _settings.PrometheusEnabled = prometheusEnabled;
        return this;
    }

    public IMetricsOptionsBuilder WithPrometheusFormatter(string prometheusFormatter)
    {
        _settings.PrometheusFormatter = prometheusFormatter;
        return this;
    }

    /*
    public IMetricsOptionsBuilder WithInfluxUrl(string influxUrl)
    {
        _settings.InfluxUrl = influxUrl;
        return this;
    }

    public IMetricsOptionsBuilder WithDatabase(string database)
    {
        _settings.Database = database;
        return this;
    }
    */

    public IMetricsOptionsBuilder WithInterval(int interval)
    {
        _settings.Interval = interval;
        return this;
    }

    public IMetricsOptionsBuilder WithTags(IDictionary<string, string> tags)
    {
        _settings.Tags = tags;
        return this;
    }

    public MetricsOptions Build()
        => _settings;
}