using Genocs.Common.Dependency;
using Genocs.Common.Interfaces;
using Xunit;

namespace Genocs.Common.UnitTests.Dependency;

public class HostServiceMarkerTests
{
    [Theory]
    [InlineData(typeof(INotificationSender))]
    [InlineData(typeof(IJobService))]
    [InlineData(typeof(ISerializerService))]
    public void HostServiceInterfaces_ExtendTransientDependencyOnly(Type serviceType)
    {
        Assert.True(typeof(ITransientDependency).IsAssignableFrom(serviceType));
#pragma warning disable CS0618
        Assert.False(typeof(ITransientService).IsAssignableFrom(serviceType));
#pragma warning restore CS0618
    }
}
