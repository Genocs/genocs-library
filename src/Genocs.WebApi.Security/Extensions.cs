using Genocs.Core.Builders;
using Genocs.WebApi.Security.Configurations;
using Microsoft.AspNetCore.Authentication.Certificate;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using System.Security.Cryptography.X509Certificates;

namespace Genocs.WebApi.Security;

public static class Extensions
{
    private const string SectionName = "security";
    private const string RegistryName = "security";

    /// <summary>
    /// Initializes the certificate authentication middleware.
    /// Remeber to add the middleware in the pipeline, by calling UseCertificateAuthentication.
    /// </summary>
    /// <param name="builder">The builder.</param>
    /// <param name="sectionName">The section name. Default is name is 'security'. Double check it's in place.</param>
    /// <param name="permissionValidatorType">The Certificate permission validation.</param>
    /// <returns></returns>
    public static IGenocsBuilder AddCertificateAuthentication(
                                                                this IGenocsBuilder builder,
                                                                string sectionName = SectionName,
                                                                Type? permissionValidatorType = null)
    {

        if (string.IsNullOrWhiteSpace(sectionName))
        {
            sectionName = SectionName;
        }

        var options = builder.GetOptions<SecurityOptions>(sectionName);
        builder.Services.AddSingleton(options);
        if (!builder.TryRegister(RegistryName))
        {
            var previousColor = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"{RegistryName} registration failed.");
            Console.ForegroundColor = previousColor;
            return builder;
        }

        if (options.Certificate?.Enabled != true)
        {
            Console.WriteLine("Certificate section is null or Enabled flag is 'False'", ConsoleColor.DarkYellow);
            Console.WriteLine("Certificate setup is incomplete!", ConsoleColor.DarkYellow);
            return builder;
        }

        if (permissionValidatorType is not null)
        {
            builder.Services.AddSingleton(typeof(ICertificatePermissionValidator), permissionValidatorType);
        }
        else
        {
            builder.Services.AddSingleton<ICertificatePermissionValidator, DefaultCertificatePermissionValidator>();
        }

        builder.Services.AddSingleton<CertificateMiddleware>();
        builder.Services
            .AddAuthentication(CertificateAuthenticationDefaults.AuthenticationScheme)
            .AddCertificate();

        builder.Services.AddCertificateForwarding(c =>
        {
            c.CertificateHeader = options.Certificate.GetHeaderName();
            c.HeaderConverter = headerValue =>
                string.IsNullOrWhiteSpace(headerValue)
                    ? null
                    : new X509Certificate2(StringToByteArray(headerValue));
        });

        return builder;
    }

    /// <summary>
    /// Adds the certificate authentication middleware to the pipeline.
    /// This must be called after the AddCertificateAuthentication method.
    /// </summary>
    /// <param name="app">The application builder.</param>
    /// <returns>The Application builder.</returns>
    public static IApplicationBuilder UseCertificateAuthentication(this IApplicationBuilder app)
    {
        var options = app.ApplicationServices.GetRequiredService<SecurityOptions>();
        if (options.Certificate?.Enabled != true)
        {
            Console.WriteLine("Certificate authentication is not enabled. Be sure both 'AddCertificateAuthentication' Security Option section are in place!");
            return app;
        }

        app.UseCertificateForwarding();
        app.UseMiddleware<CertificateMiddleware>();

        return app;
    }

    private static byte[] StringToByteArray(string hex)
    {
        int numberChars = hex.Length;
        byte[] bytes = new byte[numberChars / 2];

        for (int i = 0; i < numberChars; i += 2)
        {
            bytes[i / 2] = Convert.ToByte(hex.Substring(i, 2), 16);
        }

        return bytes;
    }
}