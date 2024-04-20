namespace APBD_6_s21147.DTOs;

public record GetAllAnimalsResponse(int IdAnimal, string Name, string Description, string Category, string Area);
public record CreateAnimalRequest(string Name, string Description, string Category, string Area);