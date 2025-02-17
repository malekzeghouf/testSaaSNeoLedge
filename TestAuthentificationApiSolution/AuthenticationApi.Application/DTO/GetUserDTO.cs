using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;


namespace AuthenticationApi.Application.DTO
{
    public record GetUserDTO(
        int id,
  [Required] string Name,
  [Required] string Email,
  [Required] string Role
    );
}
