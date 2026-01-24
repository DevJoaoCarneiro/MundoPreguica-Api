using Domain.entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Domain.Entities
{
    public class RefreshToken
    {
        public int Id { get; set; }

        public string Token { get; set; }

        public DateTime Expires { get; set; }
        public DateTime Created { get; set; }

        public DateTime? Revoked { get; set; }
        public string? ReasonRevoked { get; set; }

        public string? ReplacedByToken { get; set; }

        public Guid UserId { get; set; }
        public virtual User? User { get; set; }
    }
}
