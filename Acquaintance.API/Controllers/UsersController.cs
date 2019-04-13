using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using AutoMapper;
using Acquaintance.API.Data;
using Acquaintance.API.Dtos;
using Acquaintance.API.Helpers;
using Acquaintance.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Acquaintance.API.Controllers
{
    [ServiceFilter(typeof(LogUserActivity))]
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly IDatingRepository _repo;
        private readonly IMapper _mapper;

        public UsersController(IDatingRepository repo, IMapper mapper)
        {
            _repo = repo;
            this._mapper = mapper;
        }

        [HttpGet]
        public async Task<IActionResult> GetUsers([FromQuery]UserParams userParams) {
            var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);

            var userFromRepo = await _repo.GetUser(currentUserId);

            userParams.UserId = currentUserId;

            if (string.IsNullOrEmpty(userParams.Gender))
            {
                userParams.Gender = userFromRepo.Gender == "male" ? "female" : "male";
            }

            var users = await _repo.GetUsers(userParams);

            var usersToReturn = _mapper.Map<IEnumerable<UserForListDto>>(users);

            // Because we are inside an API controller we have access to the response
            // and because we have also written extension method
            Response.AddPagination(
                users.CurrentPage, 
                users.PageSize, 
                users.TotalCount, 
                users.TotalPages);

            return Ok(usersToReturn);
        }

        [HttpGet("{id}", Name = "GetUser")]
        public async Task<IActionResult> GetUser(int id)
        {
            var user = await _repo.GetUser(id);

            var userToReturn = _mapper.Map<UserForDetailedDto>(user);
            
            return Ok(userToReturn);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUser(int id, UserForUpdateDto userForUpdateDto) 
        {
            if ( id != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value) )
                return BadRequest("Нет прав для доступа!");;
            
            var userFromRepo = await _repo.GetUser(id);

            _mapper.Map(userForUpdateDto, userFromRepo);

            if (await _repo.SaveAll())
                return NoContent();
            
            throw new System.Exception($"При обновлении пользователя {id} возникла ошибка при сохранении");
        }

        [HttpPost("{id}/like/{recipientId}")]
        public async Task<IActionResult> LikeUser(int id, int recipientId)
        {
            if ( id != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value) )
                return BadRequest("Нет прав для доступа!");;
            
            var like = await _repo.GetLike(id, recipientId);

            if (like != null)
            {
                return BadRequest("Вы уже поставили лайк этому пользователю");//("Your already like this user");
            }

            if (await _repo.GetUser(recipientId) == null)
                return NotFound();
            
            like = new Like {
                LikerId = id,
                LikeeId = recipientId
            };

            _repo.Add<Like>(like);

            if (await _repo.SaveAll())
                return Ok();

            return BadRequest("Ошибка при попытке поставить лайк пользователю");//("Failed to like user");
        }
    }
}