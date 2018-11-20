// Copyright (c) Microsoft. All rights reserved.

namespace Microsoft.Azure.Devices.Edge.Hub.Amqp
{
    using System.Linq;
    using System.Collections.Generic;
    using System.Net.Security;
    using System.Security.Cryptography.X509Certificates;
    using Microsoft.Azure.Amqp.Transport;
    using Microsoft.Azure.Amqp.X509;

    public class EdgeHubTlsTransport : TlsTransport
    {
        IList<X509Certificate2> remoteCertificateChain;

        public EdgeHubTlsTransport(TransportBase innerTransport, TlsTransportSettings tlsSettings)
            : base(innerTransport, tlsSettings)
        {

        }

        protected override X509Principal CreateX509Principal(X509Certificate2 certificate)
        {
            var principal = new EdgeHubX509Principal(new X509CertificateIdentity(certificate, true), this.remoteCertificateChain);
            // release chain elements from here since principal has this
            this.remoteCertificateChain = null;
            return principal;
        }

        protected override bool ValidateRemoteCertificate(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            // copy of the chain elements since they are destroyed after this method completes 
            this.remoteCertificateChain = chain == null ? new List<X509Certificate2>() :
                chain.ChainElements.Cast<X509ChainElement>().Select(element => element.Certificate).ToList();
            return base.ValidateRemoteCertificate(sender, certificate, chain, sslPolicyErrors);
        }
    }
}
