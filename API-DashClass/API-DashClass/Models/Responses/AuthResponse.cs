namespace API_DashClass.Models.Responses
{
    public class AuthResponse
    {
        public string? Mensaje { get; set; }
        public string? AccessToken { get; set; }
        public string? RefreshToken { get; set; }
        public DateTime? Expiracion { get; set; }
        public UsuarioAuthResponse? Usuario { get; set; }
    }

    public class UsuarioAuthResponse
    {
        public int IdUsuario { get; set; }
        public string Email { get; set; } = string.Empty;
        public string Nombre { get; set; } = string.Empty;
        public string Apellidos { get; set; } = string.Empty;
        public string? FotoPerfilUrl { get; set; }
    }
}