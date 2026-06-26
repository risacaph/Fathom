using System;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using Fathom.API.Services;
using Fathom.Common;
using Fathom.Common.Helpers;

namespace Fathom.Services;

public class UrlValidationService(ILocalizationService localizationService) : IUrlValidationService
{
    public async Task ValidateUrlAsync(string url)
    {
        if (!Uri.TryCreate(url, UriKind.Absolute, out var uri))
        {
            throw new FathomException(await localizationService.TranslateAsync("url-malformed"));
        }

        if (!string.Equals(uri.Scheme, "https", StringComparison.OrdinalIgnoreCase))
        {
            throw new FathomException(await localizationService.TranslateAsync("url-https-only"));
        }

        IPAddress[] addresses;
        try
        {
            addresses = await Dns.GetHostAddressesAsync(uri.Host);
        }
        catch (SocketException)
        {
            throw new FathomException(await localizationService.TranslateAsync("url-unable-to-resolve"));
        }

        if (addresses.Length == 0)
        {
            throw new FathomException(await localizationService.TranslateAsync("url-unable-to-resolve"));
        }

        foreach (var address in addresses)
        {
            if (IpBlocklist.IsBlockedAddress(address))
            {
                throw new FathomException(await localizationService.TranslateAsync("url-blocked-address"));
            }
        }
    }
}
