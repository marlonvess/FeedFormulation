namespace FeedFormulation.Api.Services;

public interface ICurrentUserService
{
    Guid GetTenantId();
    string GetUserRole();
    Guid GetUserId();
}