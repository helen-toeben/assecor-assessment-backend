using Application.Contract;
using Application.Models;
using Application.Repositories;
using Microsoft.AspNetCore.Mvc;
using WebApi.Contract;

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
            return Problem(
                statusCode: StatusCodes.Status404NotFound,
                title:"Person not found",
                detail: $"No person with id {id} exists.");
        }
        
        return Ok(person);
    }

    [HttpGet("color/{color}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetByColor(string color, CancellationToken cancellationToken)
    {
        IReadOnlyList<Person> persons = await _personRepository.GetByColorAsync(color, cancellationToken);
      
        return Ok(persons);
    }

    [HttpPost]
    [Consumes("application/json")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreatePerson(CreatePersonRequest createPersonRequest, CancellationToken cancellationToken)
    {
        try
        {
            CreatePersonCommand command = new CreatePersonCommand(
                createPersonRequest.Name, 
                createPersonRequest.Lastname,
                createPersonRequest.Zipcode, 
                createPersonRequest.City, 
                createPersonRequest.Color);
            
            Person created = await _personRepository.AddAsync(command, cancellationToken);
            
            return CreatedAtAction(
                nameof(GetById), 
                new { id = created.Id }, 
                created);
        }
        catch (ArgumentException ex)
        {
            return Problem(
                statusCode: StatusCodes.Status400BadRequest,
                title:"Invalid person date",
                detail: ex.Message);
        }
    }
}