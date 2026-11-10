using System.Security.Claims;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Connections.Features;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.SignalR;
using MixueDatMon.DichVu;
using MixueDatMon.HubThoiGianThuc;
using Xunit;

namespace MixueDatMon.Tests;

public class CustomerHubAuthorizationTests
{
    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task CustomerGroupJoin_RequiresProtectedOwnership(bool owns)
    {
        var provider = new EphemeralDataProtectionProvider();
        var http = new DefaultHttpContext();
        if (owns) http.Request.Headers.Cookie = BaoVeDonKhach.CookieName + "=" +
            new BaoVeDonKhach(provider).Seal(["ORDER-A"], DateTimeOffset.UtcNow.AddHours(1));
        else http.Request.Headers.Cookie = BaoVeDonKhach.CookieName + "=ORDER-A";
        var groups = new RecordingGroups();
        var hub = new DonHangHub(provider) { Context = new Caller(http), Groups = groups };
        if (owns)
        {
            await hub.ThamGiaDon("ORDER-A");
            Assert.Equal("don-ORDER-A", groups.Joined);
        }
        else
        {
            await Assert.ThrowsAsync<HubException>(() => hub.ThamGiaDon("ORDER-A"));
            Assert.Null(groups.Joined);
        }
    }

    private sealed class HttpFeature(HttpContext http) : IHttpContextFeature
    { public HttpContext HttpContext { get; set; } = http; }

    private sealed class Caller : HubCallerContext
    {
        public Caller(HttpContext http) => Features.Set<IHttpContextFeature>(new HttpFeature(http));
        public override string ConnectionId => "test-customer-connection";
        public override string? UserIdentifier => null;
        public override ClaimsPrincipal? User => null;
        public override IDictionary<object, object?> Items { get; } = new Dictionary<object, object?>();
        public override IFeatureCollection Features { get; } = new FeatureCollection();
        public override CancellationToken ConnectionAborted => CancellationToken.None;
        public override void Abort() { }
    }

    private sealed class RecordingGroups : IGroupManager
    {
        public string? Joined { get; private set; }
        public Task AddToGroupAsync(string connectionId, string groupName, CancellationToken cancellationToken = default)
        { Joined = groupName; return Task.CompletedTask; }
        public Task RemoveFromGroupAsync(string connectionId, string groupName, CancellationToken cancellationToken = default) => Task.CompletedTask;
    }
}
