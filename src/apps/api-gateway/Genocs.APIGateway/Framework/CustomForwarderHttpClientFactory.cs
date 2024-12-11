using System.Net;
using Yarp.ReverseProxy.Forwarder;

namespace Genocs.APIGateway.Framework;

internal class CustomForwarderHttpClientFactory(CorrelationIdFactory correlationIdFactory) : IForwarderHttpClientFactory
{
    private readonly CorrelationIdFactory _correlationIdFactory = correlationIdFactory;

    public HttpMessageInvoker CreateClient(ForwarderHttpClientContext context)
    {
        if (context.OldClient != null && context.NewConfig == context.OldConfig)
        {
            return context.OldClient;
        }

        var newClientOptions = context.NewConfig;

        var handler = new SocketsHttpHandler
        {
            UseProxy = false,
            AllowAutoRedirect = false,
            AutomaticDecompression = DecompressionMethods.None,
            UseCookies = false
        };

        if (newClientOptions.SslProtocols.HasValue)
        {
            handler.SslOptions.EnabledSslProtocols = newClientOptions.SslProtocols.Value;
        }

        // TODO: Enable this
        //if (newClientOptions.ClientCertificate != null)
        //{
        //    handler.SslOptions.ClientCertificates = new X509CertificateCollection
        //    {
        //        newClientOptions.ClientCertificate
        //    };
        //}

        if (newClientOptions.MaxConnectionsPerServer != null)
        {
            handler.MaxConnectionsPerServer = newClientOptions.MaxConnectionsPerServer.Value;
        }

        if (newClientOptions.DangerousAcceptAnyServerCertificate is true)
        {
            handler.SslOptions.RemoteCertificateValidationCallback =
                (sender, cert, chain, errors) => cert.Subject == "demo.io";
        }

        return new CustomHttpMessageInvoker(_correlationIdFactory, handler, true);
    }

    private class CustomHttpMessageInvoker(CorrelationIdFactory correlationIdFactory, HttpMessageHandler handler, bool disposeHandler)
        : HttpMessageInvoker(handler, disposeHandler)
    {
        private readonly CorrelationIdFactory _correlationIdFactory = correlationIdFactory;

        public override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            string correlationId = _correlationIdFactory.Create();
            request.Headers.TryAddWithoutValidation("x-correlation-id", correlationId);
            return await base.SendAsync(request, cancellationToken);
        }
    }
}