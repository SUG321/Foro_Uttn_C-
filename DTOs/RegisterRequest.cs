namespace FORO_UTTN_API.DTOs
{
    public class RegisterRequest
    {
        public string NombreCompleto { get; set; }
        public string Email { get; set; }
        public string Contraseña { get; set; }
        public string ConfirmacionContraseña { get; set; }
    }
}
