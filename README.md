# About DevSts

*DevSts* is a zero-configuration STS for ASP.NET MVC development with OWIN.

## Background

DevSts is derived from the depricated EmbeddedSts project that is part of Thinktecture.IdentityModel.
We renamed the project to DevSts because this version is a minimal subset of the original EmbeddedSts project.

Large portions of the code base come from the project https://github.com/IdentityModel/Thinktecture.IdentityModel.
That codebase was forked by Bas Lijten at https://github.com/BasLijten/Thinktecture.IdentityModel. 
Bas Lijten extended the ThinkTecture.IdentityModel project to include support for FederationMetadata.xml, so the development
STS could easily be consumed from OWIN.

See the following blog posts from Bas Lijten:

- https://blog.baslijten.com/how-to-setup-a-simple-sts-for-web-application-development/
- http://blog.baslijten.com/configure-claims-based-web-applications-using-owin-wsfederation-middleware/
- https://blog.baslijten.com/setup-your-development-environment-for-high-trust-saml-claims-based-sharepoint-provider-hosted-applications-using-owin-and-an-easy-to-use-sts-part-3/

The `DevSts` folder contains the implementation of the Development STS server.
The `DevStsOwinVanillaVisualStudio2015` folder contains a vanilla Visual Studio 
2015 sample project that connects to *DevSts*.

When using OWIN for athenticating against an STS server only two settings are required, 
because the return url is normally configured in the STS server itself for a give realm.
For `DevSts` we currently don't have the notion of different applications, so the
return url must be configured:

```
<appSettings>
    <add key="ida:ADFSMetadata" value="http://DevSts_dev/FederationMetadata" />
    <add key="ida:Wtrealm" value="urn:Whatever" />
    <add key="ida:ReplyAddress" value="http://DevSts_test"/>
</appSettings>
```

In a default generated project by Visual Studio we now also need to modify the file `App_Start\Startup.Auth.cs` to be as follows:
```
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
```

# Installation

Download the files in the ```Installer``` folder to a folder on your local machine.
Open a PowerShell console shell with administrator permissions and run the Powerhell script `InstallDevSts.ps1`.

This script will create a website ```DevSts``` where the DevSts server will be running. Open it with http://DevSts.

When you run the script you will see the following output:

```
Installing DevSts as site 'DevSts'                                                                
Deleting old StsDev folder 'C:\inetpub\wwwroot\DevSts'                                            
Unzipping StsDev to 'C:\inetpub\wwwroot\DevSts'                                                   
Creating website 'DevSts'                                                                         
Set right to folder 'C:\inetpub\wwwroot\DevSts' for application pool 'IIS AppPool\DefaultAppPool' 
processed file: C:\inetpub\wwwroot\DevSts                                                         
Successfully processed 1 files; Failed processing 0 files                                         
Adding hostname 'DevSts' to the hosts file                                                        
Done.   
```  

# Usage

DevSts is a project developed in Visual Studio 2015 with ASP.NET MVC and the .Net 4.6 framework.

## Creating websites in IIS Manager

- Create a website in IIS Manager with name DevSts_dev and hostheader DevSts_dev and point it to `....\DevSts\DevSts`
- Create a website in IIS Manager with name DevSts_test and hostheader DevSts_test and point it to `....\DevSts\DevStsOwinVanillaVisualStudio2015`
- `Add 127.0.0.1 DevSts_dev` to file `C:\Windows\System32\drivers\etc\hosts`
- `Add 127.0.0.1 DevSts_test` to file `C:\Windows\System32\drivers\etc\hosts`
- Set both projects as startup project and start from within Visual Studio



## Creating an installation package:

- Right-click DevSts project
- Select Publish...
- On Profile: 

    - Select the profile "Publish site for IIS"

- On Connection:

    - Publish method: File System
	- Target location: ....\DevSts\Installer\Files
	
- Settings:

    - Configuration: Release
	- File Publish Options:
	    - Precompile during publishing (configure: Merge all outputs to a single assembly - DevStsSite)

Now zip the contents of the folder 	`Installer\Files` to the file `Installer\DevSts.zip`.	
