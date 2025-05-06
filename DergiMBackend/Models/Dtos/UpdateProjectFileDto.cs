namespace DergiMBackend.Models.Dtos
{
    public class UpdateProjectFileDto
    {
        public Guid Id { get; set; }
        public string FileUrl { get; set; } = default!;
        public string LocalFileUrl { get; set; } = default!;
    }

}
