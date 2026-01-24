using Domain.entities;
using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Interfaces
{
    public interface ITokenService
    {
        string GenerateToken(User user);

        RefreshToken GenerateRefreshToken(Guid userId);
    }
}
