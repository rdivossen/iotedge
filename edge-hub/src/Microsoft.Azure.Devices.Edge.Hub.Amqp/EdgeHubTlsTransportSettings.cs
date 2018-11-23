// Copyright (c) Microsoft. All rights reserved.

namespace Microsoft.Azure.Devices.Edge.Hub.Amqp
{
    using System;
    using Microsoft.Azure.Amqp.Transport;
    using Microsoft.Azure.Devices.Edge.Hub.Core;
    using Microsoft.Azure.Devices.Edge.Hub.Core.Identity;

    public class EdgeHubTlsTransportSettings : TlsTransportSettings
    {
        readonly string iotHubHostName;
        readonly IClientCredentialsFactory clientCredentialsFactory;
        readonly IAuthenticator authenticator;

        public EdgeHubTlsTransportSettings(
            TransportSettings innerSettings,
            bool isInitiator,
            string iotHubHostName,
            IClientCredentialsFactory clientCredentialsProvider,
            IAuthenticator authenticator)
            : base(innerSettings, isInitiator)
        {
            this.iotHubHostName = iotHubHostName;
            this.clientCredentialsFactory = clientCredentialsProvider;
            this.authenticator = authenticator;
        }

        public override TransportListener CreateListener()
        {
            if (this.Certificate == null)
            {
                throw new InvalidOperationException("Server certificate must be set");
            }

            return new EdgeHubTlsTransportListener(this, this.iotHubHostName, this.clientCredentialsFactory, this.authenticator);
        }
    }
}
