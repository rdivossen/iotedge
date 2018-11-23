// Copyright (c) Microsoft. All rights reserved.

namespace Microsoft.Azure.Devices.Edge.Hub.Amqp
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net.Security;
    using System.Security.Cryptography.X509Certificates;
    using System.Threading.Tasks;
    using Microsoft.Azure.Amqp.Transport;
    using Microsoft.Azure.Amqp.X509;
    using Microsoft.Azure.Devices.Edge.Hub.Core;
    using Microsoft.Azure.Devices.Edge.Hub.Core.Identity;
    using Microsoft.Azure.Devices.Edge.Util;

    public class EdgeHubTlsTransport : TlsTransport
    {
        readonly string iotHubHostName;
        readonly IClientCredentialsFactory clientCredentialsFactory;
        readonly IAuthenticator authenticator;
        IClientCredentials identity;

        public EdgeHubTlsTransport(
            TransportBase innerTransport,
            TlsTransportSettings tlsSettings,
            string iotHubHostName,
            IClientCredentialsFactory clientCredentialsProvider,
            IAuthenticator authenticator)
            : base(innerTransport, tlsSettings)
        {
            this.iotHubHostName = iotHubHostName;
            this.clientCredentialsFactory = clientCredentialsProvider;
            this.authenticator = authenticator;
        }

        protected override X509Principal CreateX509Principal(X509Certificate2 certificate)
        {
            return new EdgeHubX509Principal(new X509CertificateIdentity(certificate, true), new AmqpAuthentication(true, Option.Some(this.identity)));
        }

        protected override bool ValidateRemoteCertificate(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            bool result;
            if (certificate != null)
            {
                X509Certificate2 clientCert = new X509Certificate2(certificate);
                // copy of the chain elements since they are destroyed after this method completes 
                IList<X509Certificate2> remoteCertificateChain = chain == null ? new List<X509Certificate2>() :
                    chain.ChainElements.Cast<X509ChainElement>().Select(element => element.Certificate).ToList();
                result = this.AuthenticateAsync(clientCert, remoteCertificateChain).Result;
                if (result)
                {
                    result = base.ValidateRemoteCertificate(sender, certificate, chain, sslPolicyErrors);
                }

                return result;
            }

            return false;
        }

        private async Task<bool> AuthenticateAsync(X509Certificate2 certificate, IList<X509Certificate2> chain)
        {
            (string iotHubName, string deviceId, string moduleId) = CertificateHelper.ParseIdentity(certificate);

            // we MUST have a device ID
            if (string.IsNullOrWhiteSpace(deviceId))
            {
                //throw new EdgeHubConnectionException("Identity does not contain device ID.");
                return false;
            }

            // if moduleId is set so must the iothub
            if (!string.IsNullOrWhiteSpace(moduleId))
            { 
                if (!string.IsNullOrWhiteSpace(iotHubName) && !this.iotHubHostName.Equals(iotHubName, StringComparison.OrdinalIgnoreCase))
                {
                    return false;
                    //throw new EdgeHubConnectionException($"Identity contains an invalid IotHubHostName {iotHubName}, expected value {this.iotHubHostName}.");
                }
            }

            // TODO: Figure out where the device client type parameter value should come from.
            IClientCredentials deviceIdentity = this.clientCredentialsFactory.GetWithX509Cert(deviceId, moduleId, string.Empty, certificate, chain);
            bool result = await this.authenticator.AuthenticateAsync(deviceIdentity);
            if (result == true)
            {
                this.identity = deviceIdentity;
            }
            return result;
        }
    }
}
