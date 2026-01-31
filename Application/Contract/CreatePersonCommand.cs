namespace Application.Contract;

public record CreatePersonCommand(
    string Name,
    string Lastname,
    string Zipcode,
    string City,
    string Color
);
