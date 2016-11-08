/*
 * Copyright (c) Dominick Baier, Brock Allen.  All rights reserved.
 * see LICENSE
 */

using System.Security.Cryptography.X509Certificates;
using DevSts.Assets;

namespace DevSts
{
    public class DevStsConstants
    {        
        public const string DevStsIssuerHost = "EmbeddedSts";
        public const string TokenIssuerName = "urn:Thinktecture:EmbeddedSTS";

        internal const int SamlTokenLifetime = 60 * 10;

        internal const string SigningCertificateFile = "EmbeddedSigningCert.pfx";
        internal const string SigningCertificatePassword = "password";

        internal const string SignInFile = "SignIn.html";
        internal const string SignOutFile = "SignOut.html";
        internal const string IndexFile = "index.html";

        internal const string WsFedPath = "_sts";

        public static X509Certificate2 SigningCertificate
        {
            get
            {
                var signingCertificate = new X509Certificate2(AssetManager.LoadBytes(SigningCertificateFile), SigningCertificatePassword);
                return signingCertificate;
            }
        }
    }
}
