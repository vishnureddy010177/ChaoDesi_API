using ChaoDesi.Domain.Entities;

namespace ChaoDesi.Application.Interfaces;

public interface IJwtTokenService
{
    string GenerateToken(User user, string userTypeCode);
}
