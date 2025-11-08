using AutoMapper;
using BonusesApp.Core.Models.Bonuses;
using BonusesApp.Core.Services.Bonuses.Interfaces;
using BonusesApp.Server.ViewModels.Bonuses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BonusesApp.Server.Controllers;

[Microsoft.AspNetCore.Components.Route("api/bonuses")]
public class BonusesController : BaseApiController
{
    private readonly IBonusService _bonusService;
    
    public BonusesController(ILogger<BaseApiController> logger, IMapper mapper, IBonusService bonusService) 
        : base(logger, mapper)
    {
        _bonusService = bonusService;
    }

    /// <summary>
    /// Публичная проверка бонусов по номеру телефона (без авторизации)
    /// </summary>
    /// <param name="phoneNumber">Номер телефона</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpGet("check")]
    [AllowAnonymous]
    [ProducesResponseType(200, Type = typeof(BonusesVM))]
    [ProducesResponseType(404)]
    public async Task<IActionResult> CheckBonuses([FromQuery] string phoneNumber, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(phoneNumber))
        {
            return BadRequest("Phone number is required");
        }

        // Нормализация номера телефона
        var normalizedPhone = phoneNumber.Replace(" ", "").Replace("-", "").Replace("(", "").Replace(")", "");
        if (!normalizedPhone.StartsWith("+7") && !normalizedPhone.StartsWith("7"))
        {
            normalizedPhone = "+7" + normalizedPhone;
        }
        else if (normalizedPhone.StartsWith("7") && !normalizedPhone.StartsWith("+7"))
        {
            normalizedPhone = "+" + normalizedPhone;
        }

        var (bonuses, _) = await _bonusService.GetBonusesAsync(0, 1, normalizedPhone, cancellationToken);
        
        if (bonuses == null || bonuses.Count == 0)
        {
            return NotFound("Client with the specified phone number was not found");
        }

        var responseBonus = Mapper.Map<BonusesVM>(bonuses[0]);
        return Ok(responseBonus);
    }

    /// <summary>
    /// Получение бонусов клиентов
    /// </summary>
    /// <param name="offset"></param>
    /// <param name="pageSize"></param>
    /// <param name="search"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpGet]
    [Authorize]
    [ProducesResponseType(200, Type = typeof(BonusesResponse))]
    [ProducesResponseType(403)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetBonuses([FromQuery] int offset, [FromQuery] int pageSize, [FromQuery]string? search, CancellationToken cancellationToken)
    {
        var (bonuses, totalCount) = await _bonusService.GetBonusesAsync(offset, pageSize, search, cancellationToken);
        var responseBonuses = Mapper.Map<List<BonusesVM>>(bonuses);
        var response = new BonusesResponse
        {
            Bonuses = responseBonuses,
            TotalCount = totalCount,
        };
        return Ok(response);
    }

    /// <summary>
    /// Удаление бонусов клиента
    /// </summary>
    /// <param name="id">Идентификатор записи</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpDelete("{id:int}")]
    [Authorize]
    [ProducesResponseType(204)]
    [ProducesResponseType(403)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> DeleteBonuses(int id, CancellationToken cancellationToken)
    {
        await _bonusService.DeleteBonusesAsync(id, cancellationToken);
        return NoContent();
    }

    
    /// <summary>
    /// Удаление бонусов клиента
    /// </summary>
    /// <param name="id">Идентификатор записи</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpPost("{id:int}/giveBonus")]
    [Authorize]
    [ProducesResponseType(204)]
    [ProducesResponseType(403)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GiveBonus(int id, CancellationToken cancellationToken)
    {
        await _bonusService.GiveBonusesAsync(id, cancellationToken);
        return NoContent();
    }

    /// <summary>
    /// Удаление бонусов клиента
    /// </summary>
    /// <param name="id">Идентификатор записи</param>
    /// <param name="request">Данные по записи</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpPatch]
    [Authorize]
    [ProducesResponseType(204)]
    [ProducesResponseType(403)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> UpdateBonuses(BonusesUpdateRequest request, CancellationToken cancellationToken)
    {
        var bonuses = Mapper.Map<BonusesEntity>(request);
        await _bonusService.UpdateBonusesAsync(bonuses, cancellationToken);
        return NoContent();
    }

    /// <summary>
    /// Добавление новой записи о бонусах клиента
    /// </summary>
    /// <param name="request">Информация по добавляемым бонусам</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpPost]
    [Authorize]
    [ProducesResponseType(204)]
    [ProducesResponseType(403)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> AddBonuses(AddBonusesRequest request, CancellationToken cancellationToken)
    {
        PrepareRequest(request);        
        var newBonuses = Mapper.Map<BonusesEntity>(request);
        await _bonusService.AddBonusesAsync(newBonuses, cancellationToken);
        return NoContent();
    }

    /// <summary>
    /// Добавление новой записи о бонусах клиента
    /// </summary>
    /// <param name="request">Информация по добавляемым бонусам</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpPost("SetSettingToAll")]
    [Authorize]
    [ProducesResponseType(204)]
    [ProducesResponseType(403)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> SetDefaultSetting([FromBody]SetSettingToAllRequest request, CancellationToken cancellationToken)
    {
        await _bonusService.SetSettingToAll(request.Setting, cancellationToken);
        return NoContent();
    }

    private void PrepareRequest(AddBonusesRequest request)
    {
        if (!request.PhoneNumber.StartsWith("+7"))
        {
            request.PhoneNumber = "+7" + request.PhoneNumber;
        }
    }
}
 