/*
 * Copyright (c) Dominick Baier, Brock Allen.  All rights reserved.
 * see LICENSE
 */

using System;
using System.Collections.Generic;
using System.IdentityModel.Configuration;
using System.IdentityModel.Metadata;
using System.IdentityModel.Protocols.WSTrust;
using System.IdentityModel.Tokens;
using System.Xml;
using System.Xml.Linq;

namespace DevSts.WsFed
{
    class DevStsTokenServiceConfiguration : SecurityTokenServiceConfiguration
    {

        public DevStsTokenServiceConfiguration()
            : base(false)
        {
            this.SecurityTokenService = typeof(DevStsTokenService);
            this.TokenIssuerName = DevStsConstants.TokenIssuerName;
            this.SigningCredentials = new X509SigningCredentials(DevStsConstants.SigningCertificate);
            this.DefaultTokenLifetime = TimeSpan.FromMinutes(DevStsConstants.SamlTokenLifetime);
        }

        public XElement GetFederationMetadata(string endPoint, IList<string> claims)
        {                        
            System.ServiceModel.EndpointAddress item = new System.ServiceModel.EndpointAddress(endPoint + "/_sts/");
            EntityDescriptor entityDescriptor = new EntityDescriptor(new EntityId(this.TokenIssuerName));
            SecurityTokenServiceDescriptor securityTokenServiceDescriptor = new SecurityTokenServiceDescriptor();
            entityDescriptor.RoleDescriptors.Add(securityTokenServiceDescriptor);            
            KeyDescriptor keyDescriptor = new KeyDescriptor(this.SigningCredentials.SigningKeyIdentifier);
            keyDescriptor.Use = KeyType.Signing;
            securityTokenServiceDescriptor.Keys.Add(keyDescriptor);
            for (int i = 0; i < claims.Count; i++)
            {
                securityTokenServiceDescriptor.ClaimTypesOffered.Add(new DisplayClaim(claims[i], claims[i], string.Empty));
            }

            var endPointReference = new EndpointReference(item.Uri.ToString());
            securityTokenServiceDescriptor.PassiveRequestorEndpoints.Add(endPointReference);
            securityTokenServiceDescriptor.ProtocolsSupported.Add(new System.Uri("http://docs.oasis-open.org/wsfed/federation/200706"));
            securityTokenServiceDescriptor.SecurityTokenServiceEndpoints.Add(endPointReference);
            entityDescriptor.SigningCredentials = base.SigningCredentials;
            MetadataSerializer metadataSerializer = new MetadataSerializer();
            XElement result = null;
            using (System.IO.MemoryStream memoryStream = new System.IO.MemoryStream())
            {
                metadataSerializer.WriteMetadata(memoryStream, entityDescriptor);
                memoryStream.Flush();
                memoryStream.Seek(0L, System.IO.SeekOrigin.Begin);
                XmlReaderSettings settings = new XmlReaderSettings();
                settings.IgnoreComments = true;
                settings.IgnoreWhitespace = true;                
                XmlReader reader = XmlReader.Create(memoryStream);
                result = XElement.Load(reader);
            }            

            return result;
        }       
    }
}
