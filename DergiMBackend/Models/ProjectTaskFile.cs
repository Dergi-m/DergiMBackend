namespace DergiMBackend.Models
{
    public class ProjectTaskFile
    {
        public Guid ProjectTaskId { get; set; }
        public ProjectTask ProjectTask { get; set; } = default!;

        public int ProjectFileId { get; set; }
        public ProjectFile ProjectFile { get; set; } = default!;
    }
}
