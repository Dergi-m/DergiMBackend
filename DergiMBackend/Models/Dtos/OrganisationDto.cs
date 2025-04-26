namespace DergiMBackend.Models.Dtos
{
    public class OrganisationDto
    {
        public required string UniqueName { get; set; }
        public required string Name { get; set; }
        public string? Description { get; set; }
        public List<UserDto> Members { get; set; } = new();
    }
}
