// Copyright (c) Microsoft. All rights reserved.

namespace Microsoft.Azure.Devices.Edge.Hub.Amqp
{
    using System;
    using System.Collections.Generic;
    using System.Security.Cryptography.X509Certificates;
    using System.Security.Principal;
    using Microsoft.Azure.Amqp.X509;
    using Microsoft.Azure.Devices.Edge.Util;

    class EdgeHubX509Principal : X509Principal
    {
        public EdgeHubX509Principal(X509CertificateIdentity identity, AmqpAuthentication amqpAuthentication)
            : base(identity)
        {
            this.AmqpAuthentication = Preconditions.CheckNotNull(amqpAuthentication, nameof(amqpAuthentication));
        }

        public AmqpAuthentication AmqpAuthentication { get; }
    }
}
