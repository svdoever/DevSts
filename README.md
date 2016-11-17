# DevSts

DevSts is a zero-configuration STS for ASP.NET MVC development with OWIN.

## Table of Contents

- [Background](#background)
- [Installation](#installation)
- [Usage](#usage)
  - [web.config](#web-config)
  - [Startup.Auth.cs](#startup-auth-cs)
  - [DevStsUsers.json](#devstsusers-json)
- [Contribute](#contribute)
  - [Creating websites in IIS Manager](#creating-websites-in-iis-manager)
  - [Creating an installation package](#creating-an-installation-package)
  - [Test the installation package](#test-the-installation-package)
- [License](#license)

## Background

DevSts is derived from the depricated EmbeddedSts project that is part of Thinktecture.IdentityModel.
We renamed the project to DevSts because this version is a minimal subset of the original EmbeddedSts project.

Large portions of the code base come from the project https://github.com/IdentityModel/Thinktecture.IdentityModel.
That codebase was forked by Bas Lijten at https://github.com/BasLijten/Thinktecture.IdentityModel. 
Bas Lijten extended the ThinkTecture.IdentityModel project to include support for FederationMetadata.xml, so the development
STS could easily be consumed from OWIN as described in the blog post https://blogs.msdn.microsoft.com/webdev/2014/02/20/using-claims-in-your-web-app-is-easier-with-the-new-owin-security-components/.

See the following blog posts from Bas Lijten:

- https://blog.baslijten.com/how-to-setup-a-simple-sts-for-web-application-development/
- http://blog.baslijten.com/configure-claims-based-web-applications-using-owin-wsfederation-middleware/
- https://blog.baslijten.com/setup-your-development-environment-for-high-trust-saml-claims-based-sharepoint-provider-hosted-applications-using-owin-and-an-easy-to-use-sts-part-3/

## Installation

Download the files in the `Installer` folder to a folder on your local machine.
Open a PowerShell console shell with administrator permissions and run the Powerhell script `InstallDevSts.ps1 -SiteName DevSts`. 
If you ommit the `-SiteName` parameter, the default site name will be **DevSts**. You can use any site name here.

This script will create a website `DevSts` where the DevSts server will be running. Open it with http://DevSts.

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

## Usage

This section describes how to use DevSts within your own project when using OWIN.

### web.config

When using OWIN for athenticating against an STS server we need the following configuration in the appSettings
section of your web.config file:

```
<appSettings>
    <add key="ida:ADFSMetadata" value="http://DevSts/FederationMetadata" />
    <add key="ida:Wtrealm" value="urn:Whatever" />
    <add key="ida:ReplyAddress" value="http://DevSts_test"/>
</appSettings>
```

When working with a real STS server the app settings will look something like:

```
<appSettings>
    <add key="ida:ADFSMetadata" value="https://sts.mycompany.com/FederationMetadata/2007-06/FederationMetadata.xml" />
    <add key="ida:Wtrealm" value="urn:mysite:realm" /> <!-- ask your ADFS administrator for this realm -->
    <add key="ida:ReplyAddress" value="https://mysite.mycompany.com/"/>
</appSettings>
```

Note that the url and realm will differ for your company.

### Startup.Auth.cs

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

Note that the current implementation of DevSts only works over http:// and not over https://. This means that during development your consuming site must run on http:// as well.

### DevStsUsers.json

For the set of available users and their claims DevSts uses a file `DevStsUsers.json`. 
This file lives in the `App_Data` folder of the DevSts website, so in a default configuration
of DevSts at the location `c:\inetpub\wwwroot\DevSts\App_Data\DevStsUsers.json`.

If this file does not exist, DevSts will generate sample JSon file with the following content:

```
[
  {
    "Name": "Alice",
    "Claims": [
      {
        "Type": "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name",
        "Value": "Alice"
      },
      {
        "Type": "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress",
        "Value": "alice@alice.com"
      },
      {
        "Type": "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/givenname",
        "Value": "Alice"
      },
      {
        "Type": "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/surname",
        "Value": "Doe"
      }
    ]
  },
  {
    "Name": "Bob",
    "Claims": [
      {
        "Type": "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name",
        "Value": "Bob"
      },
      {
        "Type": "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress",
        "Value": "bob@bob.com"
      },
      {
        "Type": "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/givenname",
        "Value": "Bob"
      },
      {
        "Type": "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/surname",
        "Value": "de Bouwer"
      }
    ]
  }
]
```

Change this file with the set of test users and corresponding cleams as required for your application.

When your application authenticates against DevSts a list box of available users is displayed that you
can authenticate with. If there is only one user available in `DevStsUsers.json` the application automatically
authenticated against this user.

## Contribute

DevSts is a project developed in Visual Studio 2015 with ASP.NET MVC and the .Net 4.6 framework.

The `DevSts` folder contains the implementation of the Development STS server.
The `DevStsOwinVanillaVisualStudio2015` folder contains a vanilla Visual Studio 
2015 sample project that connects to *DevSts* server using OWIN. When you authenticate against DevSts from the dample application you 
can see the claims of currently authenticated user under the **About** tab.

### Creating websites in IIS Manager

When developing on DevSts we need to configure our development environment as follows:

- Create a website in IIS Manager with name **DevSts** and hostheader **DevSts** and point it to `....\DevSts\DevSts`
- Create a website in IIS Manager with name **DevSts_test** and hostheader **DevSts_test** and point it to `....\DevSts\DevStsOwinVanillaVisualStudio2015`
- `Add 127.0.0.1 DevSts` to file `C:\Windows\System32\drivers\etc\hosts`
- `Add 127.0.0.1 DevSts_test` to file `C:\Windows\System32\drivers\etc\hosts`
- Set both projects as startup project and start from within Visual Studio

### Creating an installation package

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

### Test the installation package

To test the installation package:

- Open a PowerShell console shell with administrator permissions and run the Powerhell script `InstallDevSts.ps1 -SiteName DevStsTest`. 
- Configure your web.config as follows:

    ```
    <appSettings>
        <add key="ida:ADFSMetadata" value="http://DevStsTest/FederationMetadata" />
        <add key="ida:Wtrealm" value="urn:Whatever" />
        <add key="ida:ReplyAddress" value="http://DevSts_test"/>
    </appSettings>
    ```

The test site http://DevSts_test will now use the DevSts instance installed from the installation package.

## License

See the [LICENSE](https://github.com/svdoever/DevSts/blob/master/LICENSE) file for the copyright information.

