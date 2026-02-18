namespace Genocs.Metrics.AppMetrics.Configurations;

public interface IMetricsOptionsBuilder
{
    IMetricsOptionsBuilder Enable(bool enabled);

    /*
    IMetricsOptionsBuilder WithInfluxEnabled(bool influxEnabled);
    IMetricsOptionsBuilder WithInfluxUrl(string influxUrl);
    IMetricsOptionsBuilder WithDatabase(string database);
    */
    IMetricsOptionsBuilder WithPrometheusEnabled(bool prometheusEnabled);
    IMetricsOptionsBuilder WithPrometheusFormatter(string prometheusFormatter);
    IMetricsOptionsBuilder WithInterval(int interval);
    IMetricsOptionsBuilder WithTags(IDictionary<string, string> tags);
    MetricsOptions Build();
}