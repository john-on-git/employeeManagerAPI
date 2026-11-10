using EmployeeManager.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Web;
using Microsoft.Identity.Web.Resource;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddMicrosoftIdentityWebApi(builder.Configuration.GetSection("AzureAdB2C"));
builder.Services.AddAuthorization();
builder.Services.AddDbContext<EmployeeManagerContext>(opt =>
{
    opt.UseSqlServer("postgres://postgres:password1@localhost:5432/employeeManagerDB");
});

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

var scopeRequiredByApi = app.Configuration["AzureAd:Scopes"] ?? "Employee.Manage";

/*
 * Endpoints for Employee CRUD.
 */
app.MapGet("/employee", (context) =>
{
    // TODO
    throw new NotImplementedException();
})
    .WithName("TODO")
    .RequireAuthorization();


app.MapPost("/employee", (context) =>
    {
        // TODO
        throw new NotImplementedException();
    })
    .WithName("TODO")
    .RequireAuthorization();

app.MapPut("/employee", (context) =>
    {
        // TODO
        throw new NotImplementedException();
    })
    .WithName("TODO")
    .RequireAuthorization();

app.MapDelete("/employee", (context) =>
    {
        // TODO
        throw new NotImplementedException();
    })
    .WithName("TODO")
    .RequireAuthorization();

/*
 * Endpoints for simultaneously updating a Task & Employee.
 */
app.MapPost("/assigned-task", (context) =>
    {
        // TODO
        throw new NotImplementedException();
    })
    .WithName("TODO")
    .RequireAuthorization();
app.MapDelete("/assigned-task", (context) =>
    {
        // TODO
        throw new NotImplementedException();
    })
    .WithName("TODO")
    .RequireAuthorization();

/*
 * Endpoints for Task CRUD.
 */
app.MapGet("/task", (context) =>
    {
        // TODO
        throw new NotImplementedException();
    })
    .WithName("TODO")
    .RequireAuthorization();


app.MapPost("/task", (context) =>
    {
        // TODO
        throw new NotImplementedException();
    })
    .WithName("TODO")
    .RequireAuthorization();

app.MapPut("/task", (context) =>
    {
        // TODO
        throw new NotImplementedException();
    })
    .WithName("TODO")
    .RequireAuthorization();

app.MapDelete("/task", (context) =>
    {
        // TODO
        throw new NotImplementedException();
    })
    .WithName("TODO")
    .RequireAuthorization();

/*
 * Endpoints for simultaneously updating a Task & TaskCategory.
 */

app.MapPost("/categorise-task", (context) =>
    {
        // TODO
        throw new NotImplementedException();
    })
    .WithName("TODO")
    .RequireAuthorization();
app.MapDelete("/categorise-task", (context) =>
    {
        // TODO
        throw new NotImplementedException();
    })
    .WithName("TODO")
    .RequireAuthorization();

/*
 * Endpoints for TaskCategory CRUD.
 */
app.MapGet("/task-category", (context) =>
    {
        // TODO
        throw new NotImplementedException();
    })
    .WithName("TODO")
    .RequireAuthorization();


app.MapPost("/task-category", (context) =>
    {
        // TODO
        throw new NotImplementedException();
    })
    .WithName("TODO")
    .RequireAuthorization();

app.MapPut("/task-category", (context) =>
    {
        // TODO
        throw new NotImplementedException();
    })
    .WithName("TODO")
    .RequireAuthorization();

app.MapDelete("/task-category", (context) =>
    {
        // TODO
        throw new NotImplementedException();
    })
    .WithName("TODO")
    .RequireAuthorization();
    
app.Run();