using System;

namespace TableX.Models
{
    // Respuesta de POST api/auth/login:
    // { "token": "...", "expiresAt": "...", "user": { "id": 1, "username": "caja", "role": "caja" } }
    public class LoginResponse
    {
        public string? Token { get; set; }
        public DateTime ExpiresAt { get; set; }
        public UserInfo? User { get; set; }
    }
}
