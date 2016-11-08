/*
 * Copyright (c) Dominick Baier, Brock Allen.  All rights reserved.
 * see LICENSE
 */

using Microsoft.Web.Infrastructure.DynamicModuleHelper;
using System;
using System.IdentityModel.Services;
using System.IdentityModel.Services.Configuration;
using System.IdentityModel.Tokens;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

[assembly: PreApplicationStartMethod(typeof(DevSts.DevStsConfiguration), "Start")]

namespace DevSts
{
    public class DevStsConfiguration
    {
        public static void Start()
        {
            FederatedAuthentication.FederationConfigurationCreated += FederatedAuthentication_FederationConfigurationCreated;
            if (UseDevSts())
            {
                ConfigureRoutes();
                ConfigureWIF();
                DynamicModuleUtility.RegisterModule(typeof(DevStsModule));
                UserManager.WriteClaimsFile();
            }
        }

        internal static bool IsDevSts(string issuer)
        {
            Uri uri;
            return
                Uri.TryCreate(issuer, UriKind.Absolute, out uri) &&
                DevStsConstants.DevStsIssuerHost.Equals(uri.Host, StringComparison.OrdinalIgnoreCase);
        }

        static void FederatedAuthentication_FederationConfigurationCreated(object sender, FederationConfigurationCreatedEventArgs e)
        {
            if (IsDevSts(e.FederationConfiguration.WsFederationConfiguration.Issuer))
            {
                var inr = new ConfigurationBasedIssuerNameRegistry();
                inr.AddTrustedIssuer(DevStsConstants.SigningCertificate.Thumbprint,
                                     DevStsConstants.TokenIssuerName);
                
                var config = e.FederationConfiguration;
                config.IdentityConfiguration.IssuerNameRegistry = inr;

                var rpRealm = new Uri(config.WsFederationConfiguration.Realm);
                if (!config.IdentityConfiguration.AudienceRestriction.AllowedAudienceUris.Contains(rpRealm))
                {
                    config.IdentityConfiguration.AudienceRestriction.AllowedAudienceUris.Add(rpRealm);
                }
                config.IdentityConfiguration.CertificateValidationMode = System.ServiceModel.Security.X509CertificateValidationMode.None;
                config.IdentityConfiguration.RevocationMode = System.Security.Cryptography.X509Certificates.X509RevocationMode.NoCheck;
            }
        }

        private static bool UseDevSts()
        {
            var config = FederatedAuthentication.FederationConfiguration;
            return IsDevSts(config.WsFederationConfiguration.Issuer);
        }

        private static void ConfigureWIF()
        {
            var inr = new ConfigurationBasedIssuerNameRegistry();
            inr.AddTrustedIssuer(DevStsConstants.SigningCertificate.Thumbprint,
                                 DevStsConstants.TokenIssuerName);
            var config = FederatedAuthentication.FederationConfiguration;
            config.IdentityConfiguration.IssuerNameRegistry = inr;

            var rpRealm = new Uri(config.WsFederationConfiguration.Realm);
            if (!config.IdentityConfiguration.AudienceRestriction.AllowedAudienceUris.Contains(rpRealm))
            {
                config.IdentityConfiguration.AudienceRestriction.AllowedAudienceUris.Add(rpRealm);
            }
            config.IdentityConfiguration.CertificateValidationMode = System.ServiceModel.Security.X509CertificateValidationMode.None;
            config.IdentityConfiguration.RevocationMode = System.Security.Cryptography.X509Certificates.X509RevocationMode.NoCheck;

        }

        private static void ConfigureRoutes()
        {
            var routes = RouteTable.Routes;

            routes.MapRoute(
                "DevSts-WSFederationMetadata",
                "FederationMetadata",
                new { controller = "DevSts", action = "WsFederationMetadata" },
                null,
                new string[] { "Thinktecture.IdentityModel.EmbeddedSts" }
                );

            routes.MapRoute(
                "DevSts-WsFed",
                DevStsConstants.WsFedPath,
                new { controller = "DevSts", action = "Index" },
                null,
                new string[] { "Thinktecture.IdentityModel.EmbeddedSts" });
        }
    }
}
