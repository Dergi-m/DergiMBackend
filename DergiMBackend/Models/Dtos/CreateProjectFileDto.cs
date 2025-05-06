using System.ComponentModel.DataAnnotations;

namespace DergiMBackend.Models.Dtos
{
    public class CreateProjectFileDto
    {
        [Required]
        public Guid ProjectId { get; set; }

        [Required]
        public string FileUrl { get; set; } = default!;

        [Required]
        public string LocalFileUrl { get; set; } = default!;
    }
}
