// Copyright (c) Microsoft. All rights reserved.

namespace Microsoft.Azure.Devices.Edge.Hub.Amqp
{
    using System;
    using Microsoft.Azure.Amqp.Transport;

    public class EdgeHubTlsTransportSettings : TlsTransportSettings
    {
        public EdgeHubTlsTransportSettings(TransportSettings innerSettings, bool isInitiator)
            : base(innerSettings, isInitiator)
        {

        }

        public override TransportListener CreateListener()
        {
            if (this.Certificate == null)
            {
                throw new InvalidOperationException("Server certificate must be set");
            }

            return new EdgeHubTlsTransportListener(this);
        }
    }
}
