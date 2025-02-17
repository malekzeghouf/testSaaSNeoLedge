using System.ComponentModel.DataAnnotations;


namespace AuthenticationApi.Application.DTO
{
    public record UserDTO(
        [Required] string Name,
        [Required] string Email,
        [Required] string Password ,
        [Required] string Role 
    );
}
