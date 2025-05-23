﻿namespace DergiMBackend.Models.Dtos
{
    public class ProjectFileDto
    {
        public Guid Id { get; set; }
        public string FileUrl { get; set; } = default!;
        public string LocalFileUrl { get; set; } = default!;
        public Guid ProjectId { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
