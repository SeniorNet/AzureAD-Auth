using System.Security.Claims;
using Microsoft.IdentityModel.Clients.ActiveDirectory;

namespace AzureADAuth.Services
{
    public interface ITokenCacheFactory
    {
        TokenCache CreateForUser(ClaimsPrincipal user);
    }
}