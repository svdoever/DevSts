DevSts is a minimal version of the depricated EmbeddedSts project that is part of Thinktecture.IdentityModel.
We renamed the project to DevSts because this version is a minimal subset of the original EmbeddedSts project.

Large portions of the code base come from the project https://github.com/IdentityModel/Thinktecture.IdentityModel.
That codebase was forked by Bas Lijten at https://github.com/BasLijten/Thinktecture.IdentityModel. Bas Lijten
extended the ThinkTecture.IdentityModel project to include support for FederationMetadata.xml, so the development
STS could easily be consumed from OWIN.

See the following blog posts from Bas Lijten:

- https://blog.baslijten.com/how-to-setup-a-simple-sts-for-web-application-development/
- http://blog.baslijten.com/configure-claims-based-web-applications-using-owin-wsfederation-middleware/
- https://blog.baslijten.com/setup-your-development-environment-for-high-trust-saml-claims-based-sharepoint-provider-hosted-applications-using-owin-and-an-easy-to-use-sts-part-3/
