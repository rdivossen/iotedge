// Copyright (c) Microsoft. All rights reserved.

namespace Microsoft.Azure.Devices.Edge.Hub.Amqp
{
    using Microsoft.Azure.Amqp.Transport;

    public class EdgeHubTlsTransportListener : TlsTransportListener
    {
        public EdgeHubTlsTransportListener(TlsTransportSettings transportSettings)
            : base(transportSettings)
        {

        }

        protected override TlsTransport OnCreateTransport(TransportBase innerTransport, TlsTransportSettings tlsTransportSettings)
        {
            return new EdgeHubTlsTransport(innerTransport, tlsTransportSettings);
        }
    }
}
