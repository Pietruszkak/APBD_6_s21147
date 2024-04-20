using System.Data.SqlClient;
using APBD_6_s21147.DTOs;
using APBD_6_s21147.Validators;
using FluentValidation;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddValidatorsFromAssemblyContaining<CreateAnimalRequestValidator>();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapGet("animals", (IConfiguration configuration, string orderBy="Name") =>
{
    
    var animals = new List<GetAllAnimalsResponse>();
    using (var sqlConnection = new SqlConnection(configuration.GetConnectionString("Default")))
    {
        var test = "SELECT * FROM Animals ORDER BY ";
        orderBy = orderBy.ToLower();
        switch(orderBy) 
        {
            case "name":
                test += "name ASC;";
                break;
            case "description":
                test += "description ASC;";
                break;
            case "category":
                test += "category ASC;";
                break;
            case "area":
                test += "area ASC;";
                break;
            default:
                test += "name ASC;";
                break;
        }
        var sqlCommand = new SqlCommand(test, sqlConnection);
        
        sqlCommand.Connection.Open();
        var reader = sqlCommand.ExecuteReader();
        while (reader.Read())
        {
            animals.Add(new GetAllAnimalsResponse(
                reader.GetInt32(0), 
                reader.GetString(1), 
                reader.GetString(2), 
                reader.GetString(3), 
                reader.GetString(4)
            ));
        }
    }
    return Results.Ok(animals);
});

app.MapGet("animals/{id:int}", (IConfiguration configuration, int id) =>
{
    using (var sqlConnection = new SqlConnection(configuration.GetConnectionString("Default")))
    {
        var sqlCommand = new SqlCommand("SELECT * FROM Animals WHERE IdAnimal = @id;", sqlConnection);
        sqlCommand.Parameters.AddWithValue("@id", id);
        sqlCommand.Connection.Open();
        var reader = sqlCommand.ExecuteReader();
        if (!reader.Read())
            return Results.NotFound();
        
        return Results.Ok(new GetAllAnimalsResponse(
            reader.GetInt32(0), 
            reader.GetString(1), 
            reader.GetString(2), 
            reader.GetString(3), 
            reader.GetString(4)
            ));
    }
});
app.MapPost("animals", (IConfiguration configuration, CreateAnimalRequest request, IValidator<CreateAnimalRequest> validator) =>
{
    var validation = validator.Validate(request);
    if (!validation.IsValid) 
        return Results.ValidationProblem(validation.ToDictionary());
    using (var sqlConnection = new SqlConnection(configuration.GetConnectionString("Default")))
    {
        var sqlCommand = new SqlCommand(
            "INSERT INTO Animals (Name, Description, Category, Area) values (@1,@2,@3,@4);", 
            sqlConnection);
        sqlCommand.Parameters.AddWithValue("@1", request.Name);
        sqlCommand.Parameters.AddWithValue("@2", request.Description);
        sqlCommand.Parameters.AddWithValue("@3", request.Category);
        sqlCommand.Parameters.AddWithValue("@4", request.Area);
        sqlCommand.Connection.Open();

        sqlCommand.ExecuteNonQuery();
    }
    return Results.Created("", null);
});
app.MapPut("animals/{id:int}", (IConfiguration configuration,int id, CreateAnimalRequest request, IValidator<CreateAnimalRequest> validator) =>
{
    
    using (var sqlConnection = new SqlConnection(configuration.GetConnectionString("Default")))
    {
        var validation = validator.Validate(request);
        if (!validation.IsValid) 
            return Results.ValidationProblem(validation.ToDictionary());
        var sqlCommand = new SqlCommand(
            "UPDATE Animals SET Name = @1, Description = @2, Category = @3, Area = @4 WHERE IdAnimal = @id;",
            sqlConnection);
        sqlCommand.Parameters.AddWithValue("@1", request.Name);
        sqlCommand.Parameters.AddWithValue("@2", request.Description);
        sqlCommand.Parameters.AddWithValue("@3", request.Category);
        sqlCommand.Parameters.AddWithValue("@4", request.Area);
        sqlCommand.Parameters.AddWithValue("@id", id);
        sqlCommand.Connection.Open();

        var affectedRows=sqlCommand.ExecuteNonQuery();
        return affectedRows == 0 ? Results.NotFound() : Results.NoContent();
    }
});
app.MapDelete("animals/{id:int}", (IConfiguration configuration, int id) =>
{
    using (var sqlConnection = new SqlConnection(configuration.GetConnectionString("Default")))
    {
        var sqlCommand = new SqlCommand("DELETE FROM Animals WHERE IdAnimal = @id;", sqlConnection);
        sqlCommand.Parameters.AddWithValue("@id", id);
        sqlCommand.Connection.Open();
        
        var affectedRows=sqlCommand.ExecuteNonQuery();
        return affectedRows == 0 ? Results.NotFound() : Results.NoContent();
    }
});

app.Run();
