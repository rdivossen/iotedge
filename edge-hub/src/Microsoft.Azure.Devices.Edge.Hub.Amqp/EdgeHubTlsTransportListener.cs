// Copyright (c) Microsoft. All rights reserved.

namespace Microsoft.Azure.Devices.Edge.Hub.Amqp
{
    using Microsoft.Azure.Amqp.Transport;
    using Microsoft.Azure.Devices.Edge.Hub.Core;
    using Microsoft.Azure.Devices.Edge.Hub.Core.Identity;

    public class EdgeHubTlsTransportListener : TlsTransportListener
    {
        readonly string iotHubHostName;
        readonly IClientCredentialsFactory clientCredentialsFactory;
        readonly IAuthenticator authenticator;

        public EdgeHubTlsTransportListener(
            TlsTransportSettings transportSettings,
            string iotHubHostName,
            IClientCredentialsFactory clientCredentialsProvider,
            IAuthenticator authenticator)
            : base(transportSettings)
        {
            this.iotHubHostName = iotHubHostName;
            this.clientCredentialsFactory = clientCredentialsProvider;
            this.authenticator = authenticator;
        }

        protected override TlsTransport OnCreateTransport(TransportBase innerTransport, TlsTransportSettings tlsTransportSettings)
        {
            return new EdgeHubTlsTransport(innerTransport, tlsTransportSettings, this.iotHubHostName, this.clientCredentialsFactory, this.authenticator);
        }
    }
}
