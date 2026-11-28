using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Orion.Application.DTOs.Modules;
using Orion.Application.Interfaces;

namespace Orion.Api.Controllers;

/// <summary>
/// RESTful endpoints exposing Orion's 4 core learning path modules.
/// </summary>
[ApiController]
[Route("api/modules")]
[Tags("Modules")]
public sealed class ModulesController(ILearningModuleService learningModuleService) : ControllerBase
{
    /// <summary>
    /// Lists the 4 active learning modules ordered by curriculum sequence (1: Python, 2: HTML &amp; CSS, 3: JavaScript, 4: PostgreSQL).
    /// </summary>
    [HttpGet]
    [AllowAnonymous]
    [EndpointSummary("List active learning modules")]
    [ProducesResponseType<IReadOnlyList<LearningModuleDto>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<LearningModuleDto>>> GetActiveModules(
        CancellationToken cancellationToken)
    {
        var modules = await learningModuleService.GetActiveModulesAsync(cancellationToken);
        return Ok(modules);
    }
}
