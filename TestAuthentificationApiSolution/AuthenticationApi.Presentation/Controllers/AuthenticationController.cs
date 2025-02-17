using AuthenticationApi.Application.DTO;
using AuthenticationApi.Application.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SharedLibrary.Responses;

namespace AuthenticationApi.Presentation.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthenticationController(IUser userInterface, MinioService _minioService) : ControllerBase
    {
        

        [HttpGet("test-connection")]
        public async Task<IActionResult> TestConnection()
        {

            await _minioService.TestMinioConnection();
            return Ok("Connection test completed");
        }


        [HttpPost("register")]
        public async Task<ActionResult<Response>> Register(UserDTO userDTO)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var result = await userInterface.Register(userDTO);
            return result.etat ? Ok(result) : BadRequest(result);


        }

        [HttpPost("login")]
        public async Task<ActionResult<Response>> login([FromBody] LoginDTO userDTO)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var result = await userInterface.Login(userDTO);
            return result.etat ? Ok(result) : BadRequest(result);
        }


        [HttpGet("{id:int}")]
        public async Task<ActionResult<GetUserDTO>> GetUser(int id)
        {
            if (id <= 0) return BadRequest("Invalid user id");
            var user = await userInterface.GetUser(id);
            return user.id > 0 ? Ok(user) : NotFound(Request);

        }
    }
}
