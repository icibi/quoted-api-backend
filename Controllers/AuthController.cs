using backend.Model;
using backend.Services;
using Microsoft.AspNetCore.Mvc;


namespace backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly AuthServices _authServices;

        public AuthController(AuthServices authServices)
        {
            _authServices = authServices;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginReqDTO loginRequest)
        {
            if (loginRequest == null || string.IsNullOrEmpty(loginRequest.Username) || string.IsNullOrEmpty(loginRequest.Password))
            {
                return BadRequest(new { Message = "Invalid login request." });
            }

            UserDTO userDetails = await _authServices.ValidateUserAsync(loginRequest.Username, loginRequest.Password);

            if (userDetails != null)
            {
                return Ok(userDetails);
            }
            else
            {
                return Unauthorized(new { Message = "Invalid username or password." });
            }
        }

            //signup
            [HttpPost("signup")]
            public async Task<IActionResult> Signup([FromBody] SignupDTO signupRequest)
            {
                if (signupRequest == null || string.IsNullOrWhiteSpace(signupRequest.Email) ||
                    string.IsNullOrWhiteSpace(signupRequest.Username) ||
                    string.IsNullOrWhiteSpace(signupRequest.Password))
                {
                    return BadRequest("All fields are required.");
                }

                Boolean response = await _authServices.SignupAsync(signupRequest);

                if (response)
                {
                    return Ok(new { Message = "Signup successful" });

                }

                return StatusCode(500, new { Message = "User already exists. Please signup with a different username and email." });
            }


            //user can change their email and password
            [HttpPatch("user-update/{Uid}")]
            public async Task<IActionResult> Update([FromRoute] string Uid, [FromBody] UserUpdateDTO userUpdate)
            {
                if (userUpdate == null || string.IsNullOrWhiteSpace(userUpdate.Email) ||
                    string.IsNullOrWhiteSpace(userUpdate.Password))
                {
                    return BadRequest(new { Message = "All fields are required." });
                }

                Boolean r = await _authServices.PatchUserAsync(Uid, userUpdate);

                if (r)
                {
                    return Ok(new { Message = "Account updated successfully." });
                }


                return StatusCode(500, new { Message = "User already exists. Please signup with a different username and email." });
            }

            //admin update accounts
            [HttpPut("admin-update/{Uid}")]
            public async Task<IActionResult> AdminUpdate([FromRoute] string Uid, [FromBody] User userUpdate)
            {
                if (userUpdate == null || string.IsNullOrWhiteSpace(userUpdate.Email) ||
                    string.IsNullOrWhiteSpace(userUpdate.Password))
                {
                    return BadRequest(new { Message = "All fields are required." });
                }

                Boolean r = await _authServices.AdminUpdateUserAsync(Uid, userUpdate);

                if (r)
                {
                    return Ok(new { Message = "Account updated successfully." });
                }


                return StatusCode(500, new { Message = "User already exists. Please signup with a different username and email." });
            }

            //consider
            //delete user by id
            [HttpDelete("remove/{Uid}")]
            public async Task<IActionResult> Delete([FromRoute] string Uid)
            {
                Boolean r = await _authServices.DeleteUserById(Uid);

                if (r)
                {
                    return Ok(new { Message = "User has been deleted." });
                }
                return StatusCode(500, new { Message = "User doesn't exist." });
            }       
    }
}