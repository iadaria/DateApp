using System;
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
    [Route("api/users/{userId}/[controller]")]
    [ApiController]
    public class MessagesController : ControllerBase
    {
        private readonly IDatingRepository _repo;
        private readonly IMapper _mapper;

        public MessagesController(IDatingRepository repo, IMapper mapper)
        {
            _repo = repo;
            _mapper = mapper;
        }

        [HttpGet("{id}", Name = "GetMessage")]
        public async Task<IActionResult> GetMessage(int userId, int id)
        {
            if ( id != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value) )
                return BadRequest("Нет прав для доступа!");;

            var messageFromRepo = await _repo.GetMessage(id);

            if (messageFromRepo == null)
                return NotFound();

            return Ok(messageFromRepo);
        }

        [HttpGet]
        public async Task<IActionResult> GetMessageForUser(int userId, [FromQuery]MessageParams messageParams)
        {
            if ( userId != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value) )
                return BadRequest("Нет прав для доступа!");;

            messageParams.UserId = userId;
            
            var messagesFromRepo = await _repo.GetMessagesForUser(messageParams);

            var messages = _mapper.Map<IEnumerable<MessageToReturnDto>>(messagesFromRepo);

            Response.AddPagination(messagesFromRepo.CurrentPage, 
                messagesFromRepo.PageSize,
                messagesFromRepo.TotalCount,
                messagesFromRepo.PageSize);
            
            return Ok(messages);
        }

        [HttpGet("thread/{recipientId}")]
        public async Task<IActionResult> GetMessageThread(int userId, int recipientId)
        {
            if ( userId != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value) )
                return BadRequest("Нет прав для доступа!");;

            var messageFromRepo = await _repo.GetMessageThread(userId, recipientId);

            var messageThread = _mapper.Map<IEnumerable<MessageToReturnDto>>(messageFromRepo);

            return Ok(messageThread);
        }

        [HttpPost]
        public async Task<IActionResult> CreateMessage(int userId, MessageForCreationDto MessageForCreationDto)
        {
            var sender = await _repo.GetUser(userId);

            if ( sender.Id != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value) )
                return BadRequest("Нет прав для доступа!");;

            MessageForCreationDto.SenderId = userId;

            var recipient = await _repo.GetUser(MessageForCreationDto.RecipientId);

            if (recipient == null)
                return BadRequest("Пользователь не найден.");// ("Could not find user");

            var message = _mapper.Map<Message>(MessageForCreationDto);

            _repo.Add(message);

            if(await _repo.SaveAll()) {
                var messageToReturn = _mapper.Map<MessageToReturnDto>(message);
                return CreatedAtRoute("GetMessage", new {id = message.Id}, messageToReturn);
            }

            throw new Exception("Создание сообщения: возникла ошибка при сохранении"); //("Creating the message failed on save");
        }

        [HttpPost("{id}")]
        public async Task<IActionResult> DeleteMessage(int id, int userId) 
        {
            if ( userId != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value) )
                return BadRequest("Нет прав для доступа!");;

            var messageFromRepo = await _repo.GetMessage(id);

            if (messageFromRepo.SenderId == userId)
                messageFromRepo.SenderDeleted = true;

            if (messageFromRepo.RecipientId == userId)
                messageFromRepo.RecipientDeleted = true;

            if (messageFromRepo.SenderDeleted && messageFromRepo.RecipientDeleted)
                _repo.Delete(messageFromRepo);

            if (await _repo.SaveAll())
                return NoContent();

            throw new Exception("Ошибка при удалении сообщения");//("Error deleting the message");
        }

        [HttpPost("{id}/read")]
        public async Task<IActionResult> MarkMessageAsRead(int userId, int id)
        {
            if ( userId != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value) )
                return BadRequest("Нет прав для доступа!");;

            var message = await _repo.GetMessage(id);

            if (message.RecipientId != userId) //Если получатель не я - т.е отправили не мне
                return BadRequest("Нет прав для доступа!");;

            message.IsRead = true;
            message.DateRead = DateTime.Now;

            await _repo.SaveAll();

            return NoContent();
        }

    }

}