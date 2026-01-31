using Application.Models;
using Infrastructure.Csv;
using Microsoft.Extensions.Options;
using FluentAssertions;

namespace Tests.Infrastructure;

public class CsvPersonRepositoryTests
{
    private static CsvPersonRepository CreateCsvPersonRepository(string filePath)
    {
        var options = Options.Create(new CsvPersonRepositoryOptions { FilePath = filePath });
        return new CsvPersonRepository(options);
    }

    [Fact]
    public void ThrowsFileNotFoundExceptionForUnknownFiles()
    {
        //Arrange
        string nonExistentPath = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        IOptions<CsvPersonRepositoryOptions> options = Options.Create(new CsvPersonRepositoryOptions { FilePath = nonExistentPath });
        
        //Act
        Action action = () =>
        {
            _ = new CsvPersonRepository(options);
        };
        
        //Assert
        action.Should().Throw<FileNotFoundException>();
    }

    [Fact]
    public async Task GetAllAsync_ParsesValidLinesAndUsesLineNumbersAsId()
    {
        //Arrange
        using TempCsvFile file = new TempCsvFile(
            "Müller, Hans, 67742 Lauterecken, 1",
            "Petersen, Peter, 18439 Stralsund, 2");

        CsvPersonRepository sut = CreateCsvPersonRepository(file.Path);
        
        //Act
        IReadOnlyList<Person> allEntries = await sut.GetAllAsync();
        
        //Assert
        allEntries.Count.Should().Be(2);
        allEntries[0].Id.Should().Be(1);
        allEntries[1].Id.Should().Be(2);
        
        allEntries[0].Name.Should().Be("Hans");
        allEntries[0].Lastname.Should().Be("Müller");
        allEntries[0].Zipcode.Should().Be("67742");
        allEntries[0].City.Should().Be("Lauterecken");
        allEntries[0].Color.Should().Be("blau");
    }

    [Fact]
    public async Task GetAllAsync_SkipsIncompleteEntries()
    {
        //Arrange
        using TempCsvFile file = new TempCsvFile(
            "Müller, Hans, 67742 Lauterecken, 1",
            "Bart, Bertram",
            "Petersen, Peter, 18439 Stralsund, 2",
            "Meyer, Achim, 12345, 2");

        CsvPersonRepository sut = CreateCsvPersonRepository(file.Path);
        
        //Act
        IReadOnlyList<Person> allEntries = await sut.GetAllAsync();
        
        //Assert
        allEntries.Count.Should().Be(2);
        allEntries.Should().NotContain(person => person.Lastname == "Bart" || person.Lastname == "Meyer");
        allEntries[1].Id.Should().Be(3);
    }
    
    [Fact]
    public async Task GetAllAsync_SkipsEntriesWithExtraFields()
    {
        //Arrange
        using TempCsvFile file = new TempCsvFile(
            "Müller, Hans, 67742 Lauterecken, 1, extra, field",
            "Petersen, Peter, 18439 Stralsund, 2");

        CsvPersonRepository sut = CreateCsvPersonRepository(file.Path);
        
        //Act
        IReadOnlyList<Person> allEntries = await sut.GetAllAsync();
        
        //Assert
        allEntries.Count.Should().Be(1);
        allEntries.Should().NotContain(person => person.Lastname == "Müller");
        allEntries[0].Id.Should().Be(2);
    }
    
    [Fact]
    public async Task GetAllAsync_UnknownColorCodesAreMappedToUnbekannt()
    {
        //Arrange
        using TempCsvFile file = new TempCsvFile(
            "Müller, Hans, 67742 Lauterecken, 999");

        CsvPersonRepository sut = CreateCsvPersonRepository(file.Path);
        
        //Act
        IReadOnlyList<Person> allEntries = await sut.GetAllAsync();
        
        //Assert
        allEntries[0].Color.Should().Be("unbekannt");
    }
    
    [Fact]
    public async Task GetAllAsync_ParsesSpecialCharacters()
    {
        //Arrange
        using TempCsvFile file = new TempCsvFile(
            "Anderßon, ÄndérŞ, 321-32 Schweden - ☀, 1");

        CsvPersonRepository sut = CreateCsvPersonRepository(file.Path);
        
        //Act
        IReadOnlyList<Person> allEntries = await sut.GetAllAsync();
        
        //Assert
        allEntries[0].Name.Should().Be("ÄndérŞ");
        allEntries[0].Lastname.Should().Be("Anderßon");
        allEntries[0].Zipcode.Should().Be("321-32");
        allEntries[0].City.Should().Be("Schweden - ☀");
        allEntries[0].Color.Should().Be("blau");
    }
    
    [Fact]
    public async Task GetByIdAsync_ReturnsPersonOrNull()
    {
        //Arrange
        using TempCsvFile file = new TempCsvFile(
            "Müller, Hans, 67742 Lauterecken, 1");

        CsvPersonRepository sut = CreateCsvPersonRepository(file.Path);
        
        //Act
        Person? person1 = await sut.GetByIdAsync(1);
        Person? person2 = await sut.GetByIdAsync(2);
        
        //Assert
        person1?.Lastname.Should().Be("Müller");
        person2.Should().BeNull();
    }
    
    [Fact]
    public async Task GetByColorAsync_TrimsAndMatchesCaseInsensitive()
    {
        //Arrange
        using TempCsvFile file = new TempCsvFile(
            "Müller, Hans, 67742 Lauterecken, 1",
            "Petersen, Peter, 18439 Stralsund, 1",
            "Mustermann, Max, 12345 Hamburg, 2");

        CsvPersonRepository sut = CreateCsvPersonRepository(file.Path);
        
        //Act
        IReadOnlyList<Person> personsWithColorBlue = await sut.GetByColorAsync("  BlAu   ");
        
        //Assert
        personsWithColorBlue.Count.Should().Be(2);
        personsWithColorBlue.Should().OnlyContain(person => person.Color == "blau");
    }
    
    [Theory]
    [InlineData("brau")]
    [InlineData("rosa")]
    [InlineData("")]
    public async Task GetByColorAsync_ReturnsEmptyListForInvalidInput(string invalidColor)
    {
        //Arrange
        using TempCsvFile file = new TempCsvFile(
            "Müller, Hans, 67742 Lauterecken, 1",
            "Petersen, Peter, 18439 Stralsund, 1",
            "Mustermann, Max, 12345 Hamburg, 2");

        CsvPersonRepository sut = CreateCsvPersonRepository(file.Path);
        
        //Act
        IReadOnlyList<Person> personsWithInvalidColor = await sut.GetByColorAsync(invalidColor);
        
        //Assert
        personsWithInvalidColor.Count.Should().Be(0);
    }
}