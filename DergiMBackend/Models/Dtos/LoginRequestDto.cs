using System.ComponentModel.DataAnnotations;

public class LoginRequestDto
{
    [Required]
    [MinLength(3)]
    public string UserName { get; set; } = default!;

    [Required]
    [MinLength(6)]
    public string Password { get; set; } = default!;
}
