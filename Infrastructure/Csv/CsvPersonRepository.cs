using Application.Contract;
using Application.Models;
using Application.Repositories;
using Microsoft.Extensions.Options;

namespace Infrastructure.Csv;

public class CsvPersonRepository :  IPersonRepository
{
    private readonly List<Person> _persons;
    private readonly string _filePath;
    private readonly SemaphoreSlim _fileLock = new (1, 1);

    private static readonly Dictionary<string, string> ColorMapping = new()
    {
        { "1", "blau" },
        { "2", "grün" },
        { "3", "violett" },
        { "4", "rot" },
        { "5", "gelb" },
        { "6", "türkis" },
        { "7", "weiß" }
    };

    public CsvPersonRepository(IOptions<CsvPersonRepositoryOptions> options)
    {
        _filePath =  ResolveFilePath(options.Value.FilePath);
        _persons = LoadPersons(_filePath);
        EnsureNewLineAtEndOfFile(_filePath);
    }
    public Task<IReadOnlyList<Person>> GetAllAsync(CancellationToken cancellationToken = default)
        => Task.FromResult<IReadOnlyList<Person>>(_persons);

    public Task<Person?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        => Task.FromResult(_persons.FirstOrDefault(person => person.Id == id));

    public Task<IReadOnlyList<Person>> GetByColorAsync(string color, CancellationToken cancellationToken = default)
        => Task.FromResult<IReadOnlyList<Person>>(_persons
            .Where(person => person.Color.Equals(color.Trim(), StringComparison.CurrentCultureIgnoreCase)).ToList());

    public async Task<Person> AddAsync(CreatePersonCommand command, CancellationToken cancellationToken = default)
    {
        await _fileLock.WaitAsync(cancellationToken);
        try
        {
            string colourCode = MapColorCode(command.Color);
            
            int nextId = File.ReadLines(_filePath).Count() + 1;
            
            Person newPerson = MapToNewPerson(command, nextId);
            
            await WriteToFile(cancellationToken, newPerson, colourCode);

            _persons.Add(newPerson);

            return newPerson;
        }
        finally
        {
            _fileLock.Release();
        }
    }

    private static List<Person> LoadPersons(string filePath)
    {
        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException($"File not found at {filePath}.");
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

    private void EnsureNewLineAtEndOfFile(string filePath)
    {
        using FileStream stream = new FileStream(
            filePath,
            FileMode.Open,
            FileAccess.ReadWrite,
            FileShare.None);

        stream.Seek(-1, SeekOrigin.End);
        int lastByte = stream.ReadByte();

        if (lastByte == '\n')
            return;

        stream.Seek(0, SeekOrigin.End);
        using StreamWriter writer = new StreamWriter(stream);
        writer.WriteLine();
    }

    private string MapColorCode(string color)
    {
        string colourCode = ColorMapping.FirstOrDefault
                (keyValuePair => keyValuePair.Value.Equals(color, StringComparison.CurrentCultureIgnoreCase))
            .Key;

        if (string.IsNullOrWhiteSpace(colourCode))
        {
            throw new ArgumentException($"Unknown color '{color}'");
        }

        return colourCode;
    }

    private Person MapToNewPerson(CreatePersonCommand command, int nextId)
    {
        string name = command.Name.Trim();
        string lastname = command.Lastname.Trim();
        string zipcode = command.Zipcode.Trim();
        string city = command.City.Trim();
        string color = command.Color.Trim();
        
        Person newPerson = new Person
        {
            Id = nextId,
            Name = name,
            Lastname = lastname,
            Zipcode = zipcode,
            City = city,
            Color = color
        };
        return newPerson;
    }

    //caller must hold fileLock
    private async Task WriteToFile(CancellationToken cancellationToken, Person newPerson, string colourCode)
    {
        string csvLine =
            $"{newPerson.Lastname}, {newPerson.Name}, {newPerson.Zipcode} {newPerson.City}, {colourCode}";

        await File.AppendAllTextAsync(_filePath, csvLine + Environment.NewLine, cancellationToken);
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
            Color = ColorMapping.SingleOrDefault(keyValuePair => keyValuePair.Key == colorCode).Value ?? "unbekannt"
        };

        return true;
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