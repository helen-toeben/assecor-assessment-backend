using Application.Contract;
using Application.Models;

namespace Application.Repositories;

public interface IPersonRepository
{
    Task<IReadOnlyList<Person>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<Person?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Person>> GetByColorAsync(string color, CancellationToken cancellationToken = default);
    Task<Person> AddAsync(CreatePersonCommand command, CancellationToken cancellationToken = default);
}