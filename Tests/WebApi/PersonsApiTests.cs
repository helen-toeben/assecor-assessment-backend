using System.Net;
using FluentAssertions;

namespace Tests.WebApi;

public class PersonsApiTests : IClassFixture<WebApiFactory>
{
    private readonly HttpClient _client;

    public PersonsApiTests(WebApiFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetAll_ReturnOkAndJsonResult()
    {
        //Act
        HttpResponseMessage response = await _client.GetAsync("/persons");
        
        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Content.Headers.ContentType!.MediaType.Should().Be("application/json");
    }

    [Fact]
    public async Task GetById_ReturnsOkAndJsonResult()
    {
        //Act
        HttpResponseMessage response = await _client.GetAsync("persons/1");
        
        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Content.Headers.ContentType!.MediaType.Should().Be("application/json");
    }

    [Fact]
    public async Task GetById_ReturnsNotFoundWhenMissingRessource()
    {
        //Act
        HttpResponseMessage response = await _client.GetAsync("persons/999");
        
        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetByColor_ReturnOkAndJsonResult()
    {
        //Act
        HttpResponseMessage response = await _client.GetAsync("persons/color/blau");
        
        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Content.Headers.ContentType!.MediaType.Should().Be("application/json");
    }
}