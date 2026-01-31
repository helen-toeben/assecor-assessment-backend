using Application.Models;
using Application.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace WebApi.Controllers;

[ApiController]
[Route("[controller]")]
[Produces("application/json")]
public class PersonsController : ControllerBase
{
    private readonly IPersonRepository _personRepository;

    public PersonsController(IPersonRepository personRepository)
    {
        _personRepository = personRepository;
    }

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        IReadOnlyList<Person> persons = await _personRepository.GetAllAsync(cancellationToken);
        return Ok(persons);
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken)
    {
        Person? person = await _personRepository.GetByIdAsync(id, cancellationToken);

        if (person is null)
        {
            return NotFound(new ProblemDetails
            {
                Status = StatusCodes.Status404NotFound,
                Title = "Person not found",
                Detail = $"No person with id {id} exists."
            });
        }
        
        return Ok(person);
    }

    [HttpGet("color/{color}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetByColor(string color, CancellationToken cancellationToken)
    {
        IReadOnlyList<Person> persons = await _personRepository.GetByColorAsync(color, cancellationToken);

        if (string.IsNullOrWhiteSpace(color))
        {
            return BadRequest(new ProblemDetails
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "Missing color",
                Detail = "Color must be provided."
            });
        }
        
        return Ok(persons);
    }
}