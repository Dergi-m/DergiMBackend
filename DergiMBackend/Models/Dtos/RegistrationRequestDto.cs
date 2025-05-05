namespace DergiMBackend.Models.Dtos
{
    public class RegistrationRequestDto
    {
        public required string UserName { get; set; }
        public required string Password { get; set; }
        public required string Name { get; set; }
        public required string Email { get; set; }
    }
}
