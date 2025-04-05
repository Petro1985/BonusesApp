using AutoMapper;
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
    /// <param name="limit"></param>
    /// <param name="search"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpGet]
    [ProducesResponseType(200, Type = typeof(BonusesVM))]
    [ProducesResponseType(403)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetBonuses([FromQuery] int offset, [FromQuery] int limit, [FromQuery]string? search, CancellationToken cancellationToken)
    {
        var bonuses = await _bonusService.GetBonusesAsync(offset, limit, search, cancellationToken);
        var response = Mapper.Map<List<BonusesVM>>(bonuses);
        return Ok(response);
    }
    
}
 