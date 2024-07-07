namespace Genocs.Metrics.AppMetrics.Configurations;

public interface IMetricsSettingsBuilder
{
    IMetricsSettingsBuilder Enable(bool enabled);
    IMetricsSettingsBuilder WithInfluxEnabled(bool influxEnabled);
    IMetricsSettingsBuilder WithPrometheusEnabled(bool prometheusEnabled);
    IMetricsSettingsBuilder WithPrometheusFormatter(string prometheusFormatter);
    IMetricsSettingsBuilder WithInfluxUrl(string influxUrl);
    IMetricsSettingsBuilder WithDatabase(string database);
    IMetricsSettingsBuilder WithInterval(int interval);
    IMetricsSettingsBuilder WithTags(IDictionary<string, string> tags);
    MetricsSettings Build();
}