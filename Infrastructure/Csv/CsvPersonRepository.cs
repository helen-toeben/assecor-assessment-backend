using Application.Models;
using Application.Repositories;
using Microsoft.Extensions.Options;

namespace Infrastructure.Csv;

public class CsvPersonRepository :  IPersonRepository
{
    public Task<IReadOnlyList<Person>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<Person?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<IReadOnlyList<Person>> GetByColorAsync(string color, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task AddAsync(Person person, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}