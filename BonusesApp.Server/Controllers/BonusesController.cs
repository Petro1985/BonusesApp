using AutoMapper;
using BonusesApp.Core.Models.Bonuses;
using BonusesApp.Core.Services.Bonuses.Interfaces;
using BonusesApp.Server.ViewModels.Bonuses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BonusesApp.Server.Controllers;

[Microsoft.AspNetCore.Components.Route("api/bonuses")]
[Authorize]
public class BonusesController : BaseApiController
{
    private readonly IBonusService _bonusService;
    
    public BonusesController(ILogger<BaseApiController> logger, IMapper mapper, IBonusService bonusService) 
        : base(logger, mapper)
    {
        _bonusService = bonusService;
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
    /// Добавление новой записи о бонусах клиента
    /// </summary>
    /// <param name="request">Информация по добавляемым бонусам</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpPost]
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

    private void PrepareRequest(AddBonusesRequest request)
    {
        if (!request.PhoneNumber.StartsWith("+7"))
        {
            request.PhoneNumber = "+7" + request.PhoneNumber;
        }
    }
}
 