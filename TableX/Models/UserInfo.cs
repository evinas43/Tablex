namespace TableX.Models
{
    // Usuario tal como lo devuelve la API (UserDto): { "id": 1, "username": "caja", "role": "caja" }
    public class UserInfo
    {
        public int Id { get; set; }
        public string? Username { get; set; }
        public string? Role { get; set; } // "caja" o "camarero"
    }
}
