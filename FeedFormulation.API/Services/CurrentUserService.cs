using System.Security.Claims;

namespace FeedFormulation.Api.Services;

public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public Guid GetTenantId()
    {
        // Vai procurar no Token JWT um claim (campo) chamado "tenantId"
        var tenantClaim = _httpContextAccessor.HttpContext?.User?.FindFirst("tenantId")?.Value;

        // REDE DE SEGURANÇA PROVISÓRIA:
        // Como o React ainda não envia Tokens, vamos manter o ID de testes temporariamente
        // se o token não existir. Assim o seu site não quebra enquanto implementamos a segurança!
        if (string.IsNullOrEmpty(tenantClaim))
        {
            return Guid.Parse("11111111-1111-1111-1111-111111111111");
        }

        return Guid.Parse(tenantClaim);
    }

    public string GetUserRole()
    {
        // Procura a Role do utilizador (Admin, Manager, FarmWorker, etc.)
        return _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.Role)?.Value ?? "FarmWorker";
    }

    public Guid GetUserId()
    {
        var userIdClaim = _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return string.IsNullOrEmpty(userIdClaim) ? Guid.Empty : Guid.Parse(userIdClaim);
    }
}