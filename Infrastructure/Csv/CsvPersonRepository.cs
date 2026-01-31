using Application.Models;
using Application.Repositories;
using Microsoft.Extensions.Options;

namespace Infrastructure.Csv;

public class CsvPersonRepository :  IPersonRepository
{
    private readonly List<Person> _persons;

    public CsvPersonRepository(IOptions<CsvPersonRepositoryOptions> options)
    {
        string filePath = options.Value.FilePath;
        _persons = LoadPersons(filePath);
    }

    public Task<IReadOnlyList<Person>> GetAllAsync(CancellationToken cancellationToken = default)
        => Task.FromResult<IReadOnlyList<Person>>(_persons);

    public Task<Person?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        => Task.FromResult(_persons.FirstOrDefault(person => person.Id == id));

    public Task<IReadOnlyList<Person>> GetByColorAsync(string color, CancellationToken cancellationToken = default)
        => Task.FromResult<IReadOnlyList<Person>>(_persons
            .Where(person => person.Color.Equals(color.Trim(), StringComparison.CurrentCultureIgnoreCase)).ToList());

    public Task AddAsync(Person person, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    private static List<Person> LoadPersons(string filePath)
    {
        filePath = ResolveFilePath(filePath);
        
        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException($"File not found at {filePath}.", filePath);
        }
        
        List<Person> persons = [];
        int lineNumber = 0;
        
        foreach (string line in File.ReadAllLines(filePath))
        {
            lineNumber++;

            if (string.IsNullOrWhiteSpace(line))
            {
                continue;
            }

            if (TryParseLine(lineNumber, line, out Person person))
            {
                persons.Add(person);
            }
        }
        
        return persons;
    }

    private static string ResolveFilePath(string filePath)
    {
        return Path.IsPathRooted(filePath) ? filePath : Path.Combine(AppContext.BaseDirectory, filePath);
    }

    private static bool TryParseLine(int lineNumber, string line, out Person person)
    {
        person = null!;
        
        string[] parts = line.Split(',', StringSplitOptions.TrimEntries);

        //required parts: LastName, Name, ZipCode + City, Color Code
        if (parts.Length is not 4)
        {
            return false;
        }
         
        string lastName = parts[0];
        string name =  parts[1];
        string address = parts[2];
        string colorCode = parts[3];

        bool emptyEntriesExist = string.IsNullOrWhiteSpace(lastName) || string.IsNullOrWhiteSpace(name) ||
                            string.IsNullOrWhiteSpace(address) || string.IsNullOrWhiteSpace(colorCode);
        if (emptyEntriesExist)
        {
            return false;
        }

        if (!TrySplitAddress(address, out string zipCode, out string city))
        {
            return false;
        }

        person = new Person
        {
            Id = lineNumber,
            Name = name,
            Lastname = lastName,
            Zipcode = zipCode,
            City = city,
            Color = MapColor(colorCode)
        };

        return true;
    }

    private static string MapColor(string colorCode)
    {
        return colorCode switch
        {
            "1" => "blau",
            "2" => "grün",
            "3" => "violett",
            "4" => "rot",
            "5" => "gelb",
            "6" => "türkis",
            "7" => "weiß",
            _ => "unbekannt"
        };
    }

    private static bool TrySplitAddress(string address, out string zipCode, out string city)
    {
        zipCode = string.Empty;
        city = string.Empty;
        
        string[] parts = address.Split(' ',  StringSplitOptions.TrimEntries);

        //required parts: zipCode, city (possibly consisting of several parts)
        if (parts.Length < 2)
        {
            return false;
        }
        
        zipCode = parts[0];
        city = string.Join(" ", parts.Skip(1));
        
        bool emptyEntriesExist = string.IsNullOrWhiteSpace(zipCode) || string.IsNullOrWhiteSpace(city);
        
        return emptyEntriesExist ? false : true;
    }
}