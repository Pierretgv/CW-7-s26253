using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using TravelAgency.Exceptions;
using TravelAgency.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddScoped<IDbService, DbService>();

var app = builder.Build();

app.UseExceptionHandler(appBuilder =>
{
    appBuilder.Run(async context =>
    {
        var exceptionHandlerPathFeature = context.Features.Get<IExceptionHandlerPathFeature>();
        var exception = exceptionHandlerPathFeature?.Error;

        context.Response.ContentType = "application/json";

        var problemDetails = new ProblemDetails
        {
            Status = StatusCodes.Status500InternalServerError,
            Title = "An unexpected error occurred.",
            Detail = exception?.Message
        };

        switch (exception)
        {
            case ClientNotFoundException:
            case TripNotFoundException:
            case NotRegisteredForTripException:
                context.Response.StatusCode = StatusCodes.Status404NotFound;
                problemDetails.Status = StatusCodes.Status404NotFound;
                break;

            case InvalidClientDataException:
            case ClientTripRegistrationException:
            case TripMaxParticipantsExceededException:
                context.Response.StatusCode = StatusCodes.Status400BadRequest;
                problemDetails.Status = StatusCodes.Status400BadRequest;
                break;

            case DatabaseConnectionException:
                context.Response.StatusCode = StatusCodes.Status503ServiceUnavailable;
                problemDetails.Status = StatusCodes.Status503ServiceUnavailable;
                break;
        }

        await context.Response.WriteAsJsonAsync(problemDetails);
    });
});

app.UseSwagger();
app.UseSwaggerUI();
app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.Run();