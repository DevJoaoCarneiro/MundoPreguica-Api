using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Request
{
    public class LoginRequestDto
    {
        public string Mail { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }
}
