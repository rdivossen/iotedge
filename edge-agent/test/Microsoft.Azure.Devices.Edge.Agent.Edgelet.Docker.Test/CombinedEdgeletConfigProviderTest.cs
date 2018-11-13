// Copyright (c) Microsoft. All rights reserved.

namespace Microsoft.Azure.Devices.Edge.Agent.Edgelet.Docker.Test
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;
    using global::Docker.DotNet.Models;
    using Microsoft.Azure.Devices.Edge.Agent.Core;
    using Microsoft.Azure.Devices.Edge.Agent.Docker;
    using Microsoft.Azure.Devices.Edge.Util.Test.Common;
    using Microsoft.Extensions.Configuration;
    using Moq;
    using Xunit;

    [Unit]
    public class CombinedEdgeletConfigProviderTest
    {
        [Fact]
        public void TestCreateValidation()
        {
            Assert.Throws<ArgumentNullException>(() => new CombinedEdgeletConfigProvider(new[] { new AuthConfig(), }, null));
            Assert.Throws<ArgumentNullException>(() => new CombinedEdgeletConfigProvider(null, Mock.Of<IConfigSource>()));
        }

        [Fact]
        public void TestVolMount()
        {
            // Arrange
            var runtimeInfo = new Mock<IRuntimeInfo<DockerRuntimeConfig>>();
            runtimeInfo.SetupGet(ri => ri.Config).Returns(new DockerRuntimeConfig("1.24", string.Empty));

            var module = new Mock<IModule<DockerConfig>>();
            module.SetupGet(m => m.Config).Returns(new DockerConfig("nginx:latest"));
            module.SetupGet(m => m.Name).Returns(Constants.EdgeAgentModuleName);
            
            var mockAppSetting = new Mock<IAgentAppSettings>();
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                mockAppSetting.SetupGet(s => s.WorkloadUri).Returns("unix:///C:/path/to/workload/sock");
                mockAppSetting.SetupGet(s => s.ManagementUri).Returns("unix:///C:/path/to/mgmt/sock");
            }
            else
            {
                mockAppSetting.SetupGet(s => s.WorkloadUri).Returns("unix:///path/to/workload.sock");
                mockAppSetting.SetupGet(s => s.ManagementUri).Returns("unix:///path/to/mgmt.sock");
            }

            var configSource = Mock.Of<IConfigSource>(s => s.AppSettings == mockAppSetting.Object);
            ICombinedConfigProvider<CombinedDockerConfig> provider = new CombinedEdgeletConfigProvider(new[] { new AuthConfig() }, configSource);

            // Act
            CombinedDockerConfig config = provider.GetCombinedConfig(module.Object, runtimeInfo.Object);

            // Assert
            Assert.NotNull(config.CreateOptions);
            Assert.NotNull(config.CreateOptions.HostConfig);
            Assert.NotNull(config.CreateOptions.HostConfig.Binds);
            Assert.Equal(2, config.CreateOptions.HostConfig.Binds.Count);

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                Assert.Equal("C:\\path\\to\\workload:C:\\path\\to\\workload", config.CreateOptions.HostConfig.Binds[0]);
                Assert.Equal("C:\\path\\to\\mgmt:C:\\path\\to\\mgmt", config.CreateOptions.HostConfig.Binds[1]);
            } else {
                Assert.Equal("/path/to/workload.sock:/path/to/workload.sock", config.CreateOptions.HostConfig.Binds[0]);
                Assert.Equal("/path/to/mgmt.sock:/path/to/mgmt.sock", config.CreateOptions.HostConfig.Binds[1]);
            }
        }

        [Fact]
        public void TestNoVolMountForNonUds()
        {
            // Arrange
            var runtimeInfo = new Mock<IRuntimeInfo<DockerRuntimeConfig>>();
            runtimeInfo.SetupGet(ri => ri.Config).Returns(new DockerRuntimeConfig("1.24", string.Empty));

            var module = new Mock<IModule<DockerConfig>>();
            module.SetupGet(m => m.Config).Returns(new DockerConfig("nginx:latest"));
            module.SetupGet(m => m.Name).Returns(Constants.EdgeAgentModuleName);

            var mockAppSetting = new Mock<IAgentAppSettings>();
            mockAppSetting.SetupGet(s => s.WorkloadUri).Returns("http://localhost:2375/");
            mockAppSetting.SetupGet(s => s.ManagementUri).Returns("http://localhost:2376/");
            var configSource = Mock.Of<IConfigSource>(s => s.AppSettings == mockAppSetting.Object);
            ICombinedConfigProvider<CombinedDockerConfig> provider = new CombinedEdgeletConfigProvider(new[] { new AuthConfig() }, configSource);

            // Act
            CombinedDockerConfig config = provider.GetCombinedConfig(module.Object, runtimeInfo.Object);

            // Assert
            Assert.NotNull(config.CreateOptions);
            Assert.Null(config.CreateOptions.HostConfig);
        }

        [Fact]
        public void InjectNetworkAliasTest()
        {
            // Arrange
            var runtimeInfo = new Mock<IRuntimeInfo<DockerRuntimeConfig>>();
            runtimeInfo.SetupGet(ri => ri.Config).Returns(new DockerRuntimeConfig("1.24", string.Empty));

            var module = new Mock<IModule<DockerConfig>>();
            module.SetupGet(m => m.Config).Returns(new DockerConfig("nginx:latest"));
            module.SetupGet(m => m.Name).Returns("mod1");

            var mockAppSetting = new Mock<IAgentAppSettings>();
            mockAppSetting.SetupGet(s => s.WorkloadUri).Returns("unix:///var/run/iotedgedworkload.sock");
            mockAppSetting.SetupGet(s => s.ManagementUri).Returns("unix:///var/run/iotedgedmgmt.sock");
            mockAppSetting.SetupGet(s => s.NetworkId).Returns("testnetwork1");
            mockAppSetting.SetupGet(s => s.EdgeDeviceHostName).Returns("edhk1");
            var configSource = Mock.Of<IConfigSource>(s => s.AppSettings == mockAppSetting.Object);

            ICombinedConfigProvider<CombinedDockerConfig> provider = new CombinedEdgeletConfigProvider(new[] { new AuthConfig() }, configSource);

            // Act
            CombinedDockerConfig config = provider.GetCombinedConfig(module.Object, runtimeInfo.Object);

            // Assert
            Assert.NotNull(config.CreateOptions);
            Assert.NotNull(config.CreateOptions.NetworkingConfig);
            Assert.NotNull(config.CreateOptions.NetworkingConfig.EndpointsConfig);
            Assert.NotNull(config.CreateOptions.NetworkingConfig.EndpointsConfig["testnetwork1"]);
            Assert.Null(config.CreateOptions.NetworkingConfig.EndpointsConfig["testnetwork1"].Aliases);
        }

        [Fact]
        public void InjectNetworkAlias_EdgeHubTest()
        {
            // Arrange
            var runtimeInfo = new Mock<IRuntimeInfo<DockerRuntimeConfig>>();
            runtimeInfo.SetupGet(ri => ri.Config).Returns(new DockerRuntimeConfig("1.24", string.Empty));

            var module = new Mock<IModule<DockerConfig>>();
            module.SetupGet(m => m.Config).Returns(new DockerConfig("nginx:latest"));
            module.SetupGet(m => m.Name).Returns(Constants.EdgeHubModuleName);

            var mockAppSetting = new Mock<IAgentAppSettings>();
            mockAppSetting.SetupGet(s => s.WorkloadUri).Returns("unix:///var/run/iotedgedworkload.sock");
            mockAppSetting.SetupGet(s => s.ManagementUri).Returns("unix:///var/run/iotedgedmgmt.sock");
            mockAppSetting.SetupGet(s => s.NetworkId).Returns("testnetwork1");
            mockAppSetting.SetupGet(s => s.EdgeDeviceHostName).Returns("edhk1");
            var configSource = Mock.Of<IConfigSource>(s => s.AppSettings == mockAppSetting.Object);

            ICombinedConfigProvider<CombinedDockerConfig> provider = new CombinedEdgeletConfigProvider(new[] { new AuthConfig() }, configSource);

            // Act
            CombinedDockerConfig config = provider.GetCombinedConfig(module.Object, runtimeInfo.Object);

            // Assert
            Assert.NotNull(config.CreateOptions);
            Assert.NotNull(config.CreateOptions.NetworkingConfig);
            Assert.NotNull(config.CreateOptions.NetworkingConfig.EndpointsConfig);
            Assert.NotNull(config.CreateOptions.NetworkingConfig.EndpointsConfig["testnetwork1"]);
            Assert.Equal("edhk1", config.CreateOptions.NetworkingConfig.EndpointsConfig["testnetwork1"].Aliases[0]);
        }

        [Fact]
        public void InjectNetworkAlias_HostNetworkTest()
        {
            // Arrange
            var runtimeInfo = new Mock<IRuntimeInfo<DockerRuntimeConfig>>();
            runtimeInfo.SetupGet(ri => ri.Config).Returns(new DockerRuntimeConfig("1.24", string.Empty));

            string hostNetworkCreateOptions = "{\"NetworkingConfig\":{\"EndpointsConfig\":{\"host\":{}}},\"HostConfig\":{\"NetworkMode\":\"host\"}}";
            var module = new Mock<IModule<DockerConfig>>();
            module.SetupGet(m => m.Config).Returns(new DockerConfig("nginx:latest", hostNetworkCreateOptions));
            module.SetupGet(m => m.Name).Returns("mod1");

            var mockAppSetting = new Mock<IAgentAppSettings>();
            mockAppSetting.SetupGet(s => s.WorkloadUri).Returns("unix:///var/run/iotedgedworkload.sock");
            mockAppSetting.SetupGet(s => s.ManagementUri).Returns("unix:///var/run/iotedgedmgmt.sock");
            mockAppSetting.SetupGet(s => s.NetworkId).Returns("testnetwork1");
            mockAppSetting.SetupGet(s => s.EdgeDeviceHostName).Returns("edhk1");

            var configSource = Mock.Of<IConfigSource>(s => s.AppSettings == mockAppSetting.Object);

            ICombinedConfigProvider<CombinedDockerConfig> provider = new CombinedEdgeletConfigProvider(new[] { new AuthConfig() }, configSource);

            // Act
            CombinedDockerConfig config = provider.GetCombinedConfig(module.Object, runtimeInfo.Object);

            // Assert
            Assert.NotNull(config.CreateOptions);
            Assert.NotNull(config.CreateOptions.NetworkingConfig);
            Assert.NotNull(config.CreateOptions.NetworkingConfig.EndpointsConfig);
            Assert.False(config.CreateOptions.NetworkingConfig.EndpointsConfig.ContainsKey("testnetwork1"));
            Assert.NotNull(config.CreateOptions.NetworkingConfig.EndpointsConfig["host"]);
            Assert.Null(config.CreateOptions.NetworkingConfig.EndpointsConfig["host"].Aliases);
            Assert.Equal("host", config.CreateOptions.HostConfig.NetworkMode);
        }
    }
}
