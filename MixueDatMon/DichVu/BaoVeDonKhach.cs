using System.Security.Cryptography;
using System.Text.Json;
using System.Text;
using MixueDatMon.MoHinh.VM;
using Microsoft.AspNetCore.DataProtection;

namespace MixueDatMon.DichVu;

/// <summary>Server-issued, expiring ownership receipt for guest orders. Legacy unsigned cookies are rejected.</summary>
public sealed class BaoVeDonKhach(IDataProtectionProvider provider)
{
    public const string CookieName = "mx_don_cua_toi";
    private readonly IDataProtector protector = provider.CreateProtector("MixueDatMon.CustomerOrders.v2");
    private readonly IDataProtector tickets = provider.CreateProtector("MixueDatMon.CustomerCheckout.v1");
    public sealed record Receipt(string[] Codes, DateTimeOffset Expires);
    public sealed record Ticket(string Code, string Fingerprint, DateTimeOffset Expires);
    private static string Fingerprint(YeuCauDatHang request) => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(JsonSerializer.Serialize(request))));
    public string IssueTicket(YeuCauDatHang request) => tickets.Protect(JsonSerializer.Serialize(
        new Ticket("KH" + Guid.NewGuid().ToString("N")[..18].ToUpperInvariant(), Fingerprint(request), DateTimeOffset.UtcNow.AddHours(24))));
    public string? CheckoutCode(string? token, YeuCauDatHang request)
    {
        if (string.IsNullOrEmpty(token) || token.Length > 12000) return null;
        try
        {
            var ticket = JsonSerializer.Deserialize<Ticket>(tickets.Unprotect(token));
            return ticket is not null && ticket.Expires > DateTimeOffset.UtcNow && ticket.Fingerprint == Fingerprint(request) ? ticket.Code : null;
        }
        catch (Exception e) when (e is CryptographicException or JsonException or FormatException) { return null; }
    }

    public string Seal(IEnumerable<string> codes, DateTimeOffset expires) =>
        protector.Protect(JsonSerializer.Serialize(new Receipt(codes.Distinct().Take(15).ToArray(), expires)));

    public string[] Read(HttpContext context)
    {
        var value = context.Request.Cookies[CookieName];
        if (string.IsNullOrEmpty(value) || value.Length > 12000) return [];
        try
        {
            var receipt = JsonSerializer.Deserialize<Receipt>(protector.Unprotect(value));
            return receipt is not null && receipt.Expires > DateTimeOffset.UtcNow
                ? receipt.Codes.Distinct().Take(15).ToArray() : [];
        }
        catch (Exception e) when (e is CryptographicException or JsonException or FormatException) { return []; }
    }

    public bool Owns(HttpContext context, string? code) =>
        !string.IsNullOrWhiteSpace(code) && Read(context).Contains(code, StringComparer.Ordinal);

    public void Remember(HttpContext context, string code)
    {
        var expires = DateTimeOffset.UtcNow.AddDays(30);
        context.Response.Cookies.Append(CookieName, Seal(new[] { code }.Concat(Read(context)), expires), new CookieOptions
        {
            Expires = expires, HttpOnly = true, Secure = context.Request.IsHttps,
            SameSite = SameSiteMode.Lax, IsEssential = true, Path = "/"
        });
    }
}
