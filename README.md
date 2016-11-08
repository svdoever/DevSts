*DevSts* is a zero-configuration STS for ASP.NET MVC development with OWIN.

DevSts is derived from the depricated EmbeddedSts project that is part of Thinktecture.IdentityModel.
We renamed the project to DevSts because this version is a minimal subset of the original EmbeddedSts project.

Large portions of the code base come from the project https://github.com/IdentityModel/Thinktecture.IdentityModel.
That codebase was forked by Bas Lijten at https://github.com/BasLijten/Thinktecture.IdentityModel. Bas Lijten
extended the ThinkTecture.IdentityModel project to include support for FederationMetadata.xml, so the development
STS could easily be consumed from OWIN.

See the following blog posts from Bas Lijten:

- https://blog.baslijten.com/how-to-setup-a-simple-sts-for-web-application-development/
- http://blog.baslijten.com/configure-claims-based-web-applications-using-owin-wsfederation-middleware/
- https://blog.baslijten.com/setup-your-development-environment-for-high-trust-saml-claims-based-sharepoint-provider-hosted-applications-using-owin-and-an-easy-to-use-sts-part-3/

The ```DevSts``` folder contains the implementation of the Development STS server.
The ```DevStsOwinVanillaVisualStudio2015``` folder contains a vanilla Visual Studio 
2015 sample project that connects to *DevSts*.

When using OWIN for athenticating against an STS server only two settings are required, 
because the return url is normally configured in the STS server itself for a give realm.
For ```DevSts``` we currently don't have the notion of different applications, so the
return url must be configured:

    <appSettings>
        <add key="ida:ADFSMetadata" value="http://DevSts_dev/FederationMetadata" />
        <add key="ida:Wtrealm" value="urn:Whatever" />
        <add key="ida:ReplyAddress" value="http://DevSts_test"/>
    </appSettings>

In a default generated project by Visual Studio we now also need to modify the file
```App_Start\Startup.Auth.cs``` to be as follows:

    public partial class Startup
    {
        private static string realm = ConfigurationManager.AppSettings["ida:Wtrealm"];
        private static string adfsMetadata = ConfigurationManager.AppSettings["ida:ADFSMetadata"];
        private static string replyAddress = ConfigurationManager.AppSettings["ida:ReplyAddress"];

        public void ConfigureAuth(IAppBuilder app)
        {
            app.SetDefaultSignInAsAuthenticationType(CookieAuthenticationDefaults.AuthenticationType);

            app.UseCookieAuthentication(new CookieAuthenticationOptions());

            app.UseWsFederationAuthentication(
                new WsFederationAuthenticationOptions
                {
                    Wtrealm = realm,
                    MetadataAddress = adfsMetadata,
                    Wreply = replyAddress
                });
        }
    }