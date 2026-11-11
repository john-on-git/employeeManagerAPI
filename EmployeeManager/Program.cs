using System.Net;
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
    opt.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
});

Console.WriteLine($"Will Connect to: {builder.Configuration.GetConnectionString("DefaultConnection")}");

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
app.MapGet("/employee", (HttpContext httpContext, EmployeeManagerContext dbContext) =>
{
    // get all the employees for this user
    httpContext.VerifyUserHasAnyAcceptedScope(scopeRequiredByApi);
    return dbContext.Employees.OrderBy(x => x.Name);
})
    .WithName("getEmployees")
    .RequireAuthorization();


app.MapPost("/employee", (HttpContext httpContext, EmployeeManagerContext dbContext) =>
    {
        httpContext.VerifyUserHasAnyAcceptedScope(scopeRequiredByApi);
        return TypedResults.StatusCode((int)HttpStatusCode.NotImplemented);
    })
    .WithName("createEmployee")
    .RequireAuthorization();

app.MapPut("/employee", (HttpContext httpContext, EmployeeManagerContext dbContext) =>
    {
        
        httpContext.VerifyUserHasAnyAcceptedScope(scopeRequiredByApi);
        return TypedResults.StatusCode((int)HttpStatusCode.NotImplemented);
    })
    .WithName("updateEmployee")
    .RequireAuthorization();

app.MapDelete("/employee", (HttpContext httpContext, EmployeeManagerContext dbContext) =>
    {
        
        httpContext.VerifyUserHasAnyAcceptedScope(scopeRequiredByApi);
        return TypedResults.StatusCode((int)HttpStatusCode.NotImplemented);
    })
    .WithName("deleteEmployee")
    .RequireAuthorization();

/*
 * Endpoints for simultaneously updating a Task & Employee.
 */
app.MapPost("/assigned-task", (HttpContext httpContext, EmployeeManagerContext dbContext) =>
    {
        
        httpContext.VerifyUserHasAnyAcceptedScope(scopeRequiredByApi);
        return TypedResults.StatusCode((int)HttpStatusCode.NotImplemented);
    })
    .WithName("assignUserTask")
    .RequireAuthorization();
app.MapDelete("/assigned-task", (HttpContext httpContext, EmployeeManagerContext dbContext) =>
    {
        
        httpContext.VerifyUserHasAnyAcceptedScope(scopeRequiredByApi);
        return TypedResults.StatusCode((int)HttpStatusCode.NotImplemented);
    })
    .WithName("unassignUserTask")
    .RequireAuthorization();

/*
 * Endpoints for Task CRUD.
 */
app.MapGet("/task", (HttpContext httpContext, EmployeeManagerContext dbContext) =>
    {
        
        httpContext.VerifyUserHasAnyAcceptedScope(scopeRequiredByApi);
        return TypedResults.StatusCode((int)HttpStatusCode.NotImplemented);
    })
    .WithName("getTasks")
    .RequireAuthorization();


app.MapPost("/task", (HttpContext httpContext, EmployeeManagerContext dbContext) =>
    {
        
        httpContext.VerifyUserHasAnyAcceptedScope(scopeRequiredByApi);
        return TypedResults.StatusCode((int)HttpStatusCode.NotImplemented);
    })
    .WithName("createTask")
    .RequireAuthorization();

app.MapPut("/task", (HttpContext httpContext, EmployeeManagerContext dbContext) =>
    {
        
        httpContext.VerifyUserHasAnyAcceptedScope(scopeRequiredByApi);
        return TypedResults.StatusCode((int)HttpStatusCode.NotImplemented);
    })
    .WithName("updateTasks")
    .RequireAuthorization();

app.MapDelete("/task", (HttpContext httpContext, EmployeeManagerContext dbContext) =>
    {
        
        httpContext.VerifyUserHasAnyAcceptedScope(scopeRequiredByApi);
        return TypedResults.StatusCode((int)HttpStatusCode.NotImplemented);
    })
    .WithName("deleteTasks")
    .RequireAuthorization();

/*
 * Endpoints for simultaneously updating a Task & TaskCategory.
 */

app.MapPost("/categorise-task", (HttpContext httpContext, EmployeeManagerContext dbContext) =>
    {
        
        httpContext.VerifyUserHasAnyAcceptedScope(scopeRequiredByApi);
        return TypedResults.StatusCode((int)HttpStatusCode.NotImplemented);
    })
    .WithName("assignTaskCategory")
    .RequireAuthorization();
app.MapDelete("/categorise-task", (HttpContext httpContext, EmployeeManagerContext dbContext) =>
    {
        
        httpContext.VerifyUserHasAnyAcceptedScope(scopeRequiredByApi);
        return TypedResults.StatusCode((int)HttpStatusCode.NotImplemented);
    })
    .WithName("unassignTaskCategory")
    .RequireAuthorization();

/*
 * Endpoints for TaskCategory CRUD.
 */
app.MapGet("/task-category", (HttpContext httpContext, EmployeeManagerContext dbContext) =>
    {
        
        httpContext.VerifyUserHasAnyAcceptedScope(scopeRequiredByApi);
        return TypedResults.StatusCode((int)HttpStatusCode.NotImplemented);
    })
    .WithName("getTaskCategory")
    .RequireAuthorization();


app.MapPost("/task-category", (HttpContext httpContext, EmployeeManagerContext dbContext) =>
    {
        
        httpContext.VerifyUserHasAnyAcceptedScope(scopeRequiredByApi);
        return TypedResults.StatusCode((int)HttpStatusCode.NotImplemented);
    })
    .WithName("TODO")
    .RequireAuthorization();

app.MapPut("/task-category", (HttpContext httpContext, EmployeeManagerContext dbContext) =>
    {
        
        httpContext.VerifyUserHasAnyAcceptedScope(scopeRequiredByApi);
        return TypedResults.StatusCode((int)HttpStatusCode.NotImplemented);
    })
    .WithName("createTaskCategory")
    .RequireAuthorization();

app.MapDelete("/task-category", (HttpContext httpContext, EmployeeManagerContext dbContext) =>
    {
        
        httpContext.VerifyUserHasAnyAcceptedScope(scopeRequiredByApi);
        return TypedResults.StatusCode((int)HttpStatusCode.NotImplemented);
    })
    .WithName("deleteTaskCategory")
    .RequireAuthorization();
    
app.Run();