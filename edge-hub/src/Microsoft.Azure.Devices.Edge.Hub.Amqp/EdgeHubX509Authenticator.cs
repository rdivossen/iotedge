// Copyright (c) Microsoft. All rights reserved.

namespace Microsoft.Azure.Devices.Edge.Hub.Amqp
{
    using System.Collections.Generic;
    using System.Security.Cryptography.X509Certificates;
    using Microsoft.Azure.Amqp.X509;

    class EdgeHubX509Principal : X509Principal
    {
        IList<X509Certificate2> chainCertificates;

        public EdgeHubX509Principal(X509CertificateIdentity identity, IList<X509Certificate2> chainCertificates)
            : base(identity)
        {
            this.chainCertificates = chainCertificates;
        }

        public IList<X509Certificate2> GetCertificateChainElements()
        {
            return this.chainCertificates;
        }
    }
}
