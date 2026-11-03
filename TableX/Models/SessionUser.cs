namespace TableX.Models
{
    public static class SessionUser
    {
        public static string? Username { get; set; }
        public static string? Token { get; set; }
        public static string? Role { get; set; }

        public static void Clear()
        {
            Username = null;
            Token = null;
            Role = null;
        }
    }
}
