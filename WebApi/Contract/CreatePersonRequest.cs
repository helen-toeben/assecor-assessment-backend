using System.ComponentModel.DataAnnotations;

namespace WebApi.Contract;

public record CreatePersonRequest
(
    [Required] string Name, 
    [Required] string Lastname, 
    [Required] string Zipcode, 
    [Required] string City, 
    [Required] string Color);