// Copyright (c) Microsoft. All rights reserved.

namespace Microsoft.Azure.Devices.Edge.Hub.Amqp.Test
{
    using System;
    using System.Security.Cryptography.X509Certificates;
    using Microsoft.Azure.Devices.Edge.Hub.Amqp.Settings;
    using Microsoft.Azure.Devices.Edge.Hub.Core;
    using Microsoft.Azure.Devices.Edge.Hub.Core.Identity;
    using Microsoft.Azure.Devices.Edge.Util.Test.Common;
    using Moq;
    using Xunit;

    public class DefaultTransportSettingsTest
    {
        [Fact]
        [Unit]
        public void TestInvalidConstructorInputs()
        {
            const string Scheme = "amqps";
            const string HostName = "restaurantatendofuniverse.azure-devices.net";
            const int Port = 5671;
            X509Certificate2 tlsCertificate = CertificateHelper.GenerateSelfSignedCert("TestCert");
            const bool clientCertsAllowed = true;
            const string iotHubName = "iothub";
            IClientCredentialsFactory clientCredentialsProvider = Mock.Of<IClientCredentialsFactory>();
            IAuthenticator authenticator = Mock.Of<IAuthenticator>();

            Assert.Throws<ArgumentException>(() => new DefaultTransportSettings(null, HostName, Port, tlsCertificate, clientCertsAllowed, iotHubName, clientCredentialsProvider, authenticator));
            Assert.Throws<ArgumentException>(() => new DefaultTransportSettings("", HostName, Port, tlsCertificate, clientCertsAllowed, iotHubName, clientCredentialsProvider, authenticator));
            Assert.Throws<ArgumentException>(() => new DefaultTransportSettings("    ", HostName, Port, tlsCertificate, clientCertsAllowed, iotHubName, clientCredentialsProvider, authenticator));
            Assert.Throws<ArgumentException>(() => new DefaultTransportSettings(Scheme, null, Port, tlsCertificate, clientCertsAllowed, iotHubName, clientCredentialsProvider, authenticator));
            Assert.Throws<ArgumentException>(() => new DefaultTransportSettings(Scheme, "", Port, tlsCertificate, clientCertsAllowed, iotHubName, clientCredentialsProvider, authenticator));
            Assert.Throws<ArgumentException>(() => new DefaultTransportSettings(Scheme, "   ", Port, tlsCertificate, clientCertsAllowed, iotHubName, clientCredentialsProvider, authenticator));
            Assert.Throws<ArgumentOutOfRangeException>(() => new DefaultTransportSettings(Scheme, HostName, -1, tlsCertificate, clientCertsAllowed, iotHubName, clientCredentialsProvider, authenticator));
            Assert.Throws<ArgumentOutOfRangeException>(() => new DefaultTransportSettings(Scheme, HostName, 70000, tlsCertificate, clientCertsAllowed, iotHubName, clientCredentialsProvider, authenticator));
            Assert.Throws<ArgumentNullException>(() => new DefaultTransportSettings(Scheme, HostName, Port, null, clientCertsAllowed, iotHubName, clientCredentialsProvider, authenticator));
            Assert.Throws<ArgumentNullException>(() => new DefaultTransportSettings(Scheme, HostName, Port, tlsCertificate, clientCertsAllowed, null, clientCredentialsProvider, authenticator));
            Assert.Throws<ArgumentNullException>(() => new DefaultTransportSettings(Scheme, HostName, Port, tlsCertificate, clientCertsAllowed, iotHubName, null, authenticator));
            Assert.Throws<ArgumentNullException>(() => new DefaultTransportSettings(Scheme, HostName, Port, tlsCertificate, clientCertsAllowed, iotHubName, clientCredentialsProvider, null));
            Assert.NotNull(new DefaultTransportSettings(Scheme, HostName, Port, tlsCertificate, clientCertsAllowed, iotHubName, clientCredentialsProvider, authenticator));
            Assert.NotNull(new DefaultTransportSettings(Scheme, HostName, Port, tlsCertificate, !clientCertsAllowed, iotHubName, clientCredentialsProvider, authenticator));
        }
    }
}
