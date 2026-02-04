using Application.Interfaces;
using Application.Request;
using Application.Response;
using Application.Service;
using Domain.entities;
using Microsoft.AspNetCore.Mvc;

namespace Api.controller
{
    [ApiController]
    [Route("api/user")]
    public class UserController : ControllerBase
    {

        private readonly IUserServices _userService;

        public UserController(IUserServices userService)
        {
            _userService = userService;
        }

        [HttpPost]
        public async Task<IActionResult> registerUser([FromBody] UserRequestDTO userRequestDTO)
        {

            try
            {
                var result = await _userService.createUser(userRequestDTO);

                return result.Status switch
                {
                    "invalid_argument" => BadRequest(result),
                    "not_found" => NotFound(result),
                    "internal_error" => StatusCode(500, result),
                    "error" => StatusCode(500, result),
                    _ => Ok(result)
                };
            }
            catch (Exception)
            {

                return StatusCode(500, new { Message = "Erro interno.", Status = "error" });
            }
            
             
        }

    }
}
