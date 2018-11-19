// Copyright (c) Microsoft. All rights reserved.

namespace Microsoft.Azure.Devices.Edge.Hub.Amqp.Settings
{
    using System;
    using System.Security.Cryptography.X509Certificates;
    using Microsoft.Azure.Amqp.Transport;
    using Microsoft.Azure.Devices.Edge.Util;

    public class DefaultTransportSettings : ITransportSettings
    {
        readonly bool clientCertAuthAllowed;

        public DefaultTransportSettings(
            string scheme,
            string hostName,
            int port,
            X509Certificate2 tlsCertificate,
            bool clientCertAuthAllowed)
        {
            this.HostName = Preconditions.CheckNonWhiteSpace(hostName, nameof(hostName));
            this.clientCertAuthAllowed = Preconditions.CheckNotNull(clientCertAuthAllowed, nameof(clientCertAuthAllowed));

            var address = new UriBuilder
            {
                Host = hostName,
                Port = Preconditions.CheckRange(port, 0, ushort.MaxValue, nameof(port)),
                Scheme = Preconditions.CheckNonWhiteSpace(scheme, nameof(scheme))
            };

            var tcpSettings = new TcpTransportSettings()
            {
                Host = address.Host,
                Port = address.Port
            };

            var tlsSettings = new TlsTransportSettings(tcpSettings, false)
            {
                TargetHost = address.Host,
                Certificate = Preconditions.CheckNotNull(tlsCertificate, nameof(tlsCertificate)),
                // NOTE: The following property doesn't appear to be used by the AMQP library.
                //       Not sure that setting this to true/false makes any difference!
                CheckCertificateRevocation = false,
            };

            if (this.clientCertAuthAllowed == true)
            {
                tlsSettings.CertificateValidationCallback = (sender, cert, chain, sslPolicyErrors) => true;
            }
            this.Settings = tlsSettings;
        }

        public string HostName { get; }

        public TransportSettings Settings { get; }
    }
}
