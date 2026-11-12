using System.Net;
using EmployeeManager.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Web;
using Microsoft.Identity.Web.Resource;
using Microsoft.IdentityModel.Protocols.Configuration;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddMicrosoftIdentityWebApi(builder.Configuration.GetSection("AzureAdB2C"));
builder.Services.AddAuthorization();
builder.Services.AddDbContextPool<EmployeeManagerContext>(opt =>
{
    opt.UseNpgsql(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        o =>
        {
            // additional configuration goes here
            o.SetPostgresVersion(13, 0);
        }
    );
});

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

/*
 * Programatically apply the migrations, this creates the tables for the schema
 * that we've defined code-first using EF.
 * We do this in code because we're using a containerised DB.
 * According to StackOverflow users, this method could cause issues in a real production app.
*/
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<EmployeeManagerContext>();
    db.Database.Migrate();
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}
else
{
    app.UseExceptionHandler(exceptionHandlerApp =>
    {
        exceptionHandlerApp.Run(async context =>
        {
            context.Response.StatusCode = StatusCodes.Status500InternalServerError;
            await context.Response.WriteAsync("Something went wrong.");
        });
    });
}

app.UseHttpsRedirection();

var scopeRequiredByApi = app.Configuration["AzureAd:Scopes"] ?? "Employee.Manage";

/*
 *
 *
 *
 * Testing, seed the DB.
 */
app.MapPost("/test-seed", async Task<IResult>(HttpContext httpContext, EmployeeManagerContext dbContext, LinkGenerator linkGenerator) =>
{
    httpContext.VerifyUserHasAnyAcceptedScope(scopeRequiredByApi);
    var userId = httpContext.User.GetObjectId();
    if (userId != null)
    {
        ICollection<Employee> myEmployees = [
            new() { UserId=userId, Name="Alice", Notes="On vacation in Wonderland.", WorkItems=(List<WorkItem>)[] },
            new() { UserId=userId, Name="Bob", Notes="Remember to book his bricklaying course.", WorkItems=(List<WorkItem>)[] },
            new() { UserId=userId, Name="Charlie", Notes="Away on business trip to Malbolge.", WorkItems=(List<WorkItem>)[] },
            new() { UserId=userId, Name="Daniel", Notes="TODO process disciplinary for wearing white vans.", WorkItems=(List<WorkItem>)[]},
            
            new() { UserId=$"not {userId}", Name="Xander", Notes="You're not authorised to access this employee.", WorkItems=(List<WorkItem>)[]},
            new() { UserId=$"not {userId}", Name="Zoe", Notes="Or this one.", WorkItems=(List<WorkItem>)[]}
        ];
        
        await dbContext.Employees.AddRangeAsync(myEmployees);
        dbContext.SaveChanges();
        return TypedResults.Created(linkGenerator.GetUriByName(
            httpContext, "getEmployees"
        ), myEmployees);
    }
    else
    {
        return TypedResults.StatusCode((int)HttpStatusCode.Unauthorized);
    }
})
    .RequireAuthorization();

/*
 *
 *
 * 
 * Endpoints for Employee CRUD.
 */
app.MapGet("/employee/list", IResult (HttpContext httpContext, EmployeeManagerContext dbContext) =>
{
    httpContext.VerifyUserHasAnyAcceptedScope(scopeRequiredByApi);
    // get all the employees for this user
    return TypedResults.Ok(
        dbContext.Employees
            .Where(x => x.UserId == httpContext.User.GetObjectId())
            .OrderBy(x => x.Name)
    );
})
    .WithName("getEmployees")
    .RequireAuthorization();

app.MapGet("/employee", IResult (HttpContext httpContext, EmployeeManagerContext dbContext, [FromQuery] int id) =>
    {
        httpContext.VerifyUserHasAnyAcceptedScope(scopeRequiredByApi);
        // get the employee with the matching ID, if they're associated with user
        var found = dbContext.Employees.Find(id);

        return found == null || found.UserId != httpContext.User.GetObjectId()
            ? TypedResults.NotFound() // if the record doesn't exist, or the user's not authorized to view it, it doesn't exist
            : TypedResults.Ok(found); // otherwise respond with it
    })
    .WithName("getEmployee")
    .RequireAuthorization();


app.MapPost("/employee", IResult (HttpContext httpContext, EmployeeManagerContext dbContext, LinkGenerator linkGenerator, [FromBody] Employee employee) =>
    {
        httpContext.VerifyUserHasAnyAcceptedScope(scopeRequiredByApi);

        employee.UserId = httpContext.User.GetObjectId();
        var created = dbContext.Add(employee).Entity;
        dbContext.SaveChanges();

        return TypedResults.Created(
            linkGenerator.GetUriByName(
                httpContext, "getEmployee",
                new { id = created.Id }
            ),
            created
        );
    })
    .WithName("createEmployee")
    .RequireAuthorization();

app.MapPut("/employee", IResult (HttpContext httpContext, EmployeeManagerContext dbContext) =>
    {
        httpContext.VerifyUserHasAnyAcceptedScope(scopeRequiredByApi);
        return TypedResults.StatusCode((int)HttpStatusCode.NotImplemented);
    })
    .WithName("updateEmployee")
    .RequireAuthorization();

app.MapDelete("/employee", IResult (HttpContext httpContext, EmployeeManagerContext dbContext) =>
    { 
        httpContext.VerifyUserHasAnyAcceptedScope(scopeRequiredByApi);
        return TypedResults.StatusCode((int)HttpStatusCode.NotImplemented);
    })
    .WithName("deleteEmployee")
    .RequireAuthorization();

/*
 *
 *
 * 
 * Endpoints for simultaneously updating a Task & Employee.
 */
app.MapPost("/assigned-task", IResult (HttpContext httpContext, EmployeeManagerContext dbContext) =>
    {
        httpContext.VerifyUserHasAnyAcceptedScope(scopeRequiredByApi);
        return TypedResults.StatusCode((int)HttpStatusCode.NotImplemented);
    })
    .WithName("assignUserTask")
    .RequireAuthorization();
app.MapDelete("/assigned-task", IResult (HttpContext httpContext, EmployeeManagerContext dbContext) =>
    {
        httpContext.VerifyUserHasAnyAcceptedScope(scopeRequiredByApi);
        return TypedResults.StatusCode((int)HttpStatusCode.NotImplemented);
    })
    .WithName("unassignUserTask")
    .RequireAuthorization();

/*
 *
 *
 * 
 * Endpoints for Task CRUD.
 */
app.MapGet("/task", IResult (HttpContext httpContext, EmployeeManagerContext dbContext) =>
    {
        httpContext.VerifyUserHasAnyAcceptedScope(scopeRequiredByApi);
        return TypedResults.StatusCode((int)HttpStatusCode.NotImplemented);
    })
    .WithName("getTasks")
    .RequireAuthorization();


app.MapPost("/task", IResult (HttpContext httpContext, EmployeeManagerContext dbContext) =>
    {
        httpContext.VerifyUserHasAnyAcceptedScope(scopeRequiredByApi);
        return TypedResults.StatusCode((int)HttpStatusCode.NotImplemented);
    })
    .WithName("createTask")
    .RequireAuthorization();

app.MapPut("/task", IResult (HttpContext httpContext, EmployeeManagerContext dbContext) =>
    {
        httpContext.VerifyUserHasAnyAcceptedScope(scopeRequiredByApi);
        return TypedResults.StatusCode((int)HttpStatusCode.NotImplemented);
    })
    .WithName("updateTasks")
    .RequireAuthorization();

app.MapDelete("/task", IResult (HttpContext httpContext, EmployeeManagerContext dbContext) =>
    {
        httpContext.VerifyUserHasAnyAcceptedScope(scopeRequiredByApi);
        return TypedResults.StatusCode((int)HttpStatusCode.NotImplemented);
    })
    .WithName("deleteTasks")
    .RequireAuthorization();

/*
 *
 *
 * 
 * Endpoints for simultaneously updating a Task & TaskCategory.
 */

app.MapPost("/categorise-task", IResult (HttpContext httpContext, EmployeeManagerContext dbContext) =>
    {
        httpContext.VerifyUserHasAnyAcceptedScope(scopeRequiredByApi);
        return TypedResults.StatusCode((int)HttpStatusCode.NotImplemented);
    })
    .WithName("assignTaskCategory")
    .RequireAuthorization();
app.MapDelete("/categorise-task", IResult (HttpContext httpContext, EmployeeManagerContext dbContext) =>
    {
        httpContext.VerifyUserHasAnyAcceptedScope(scopeRequiredByApi);
        return TypedResults.StatusCode((int)HttpStatusCode.NotImplemented);
    })
    .WithName("unassignTaskCategory")
    .RequireAuthorization();

/*
 *
 *
 * 
 * Endpoints for TaskCategory CRUD.
 */
app.MapGet("/task-category", IResult (HttpContext httpContext, EmployeeManagerContext dbContext) =>
    {
        httpContext.VerifyUserHasAnyAcceptedScope(scopeRequiredByApi);
        return TypedResults.StatusCode((int)HttpStatusCode.NotImplemented);
    })
    .WithName("getTaskCategory")
    .RequireAuthorization();


app.MapPost("/task-category", IResult (HttpContext httpContext, EmployeeManagerContext dbContext) =>
    {
        httpContext.VerifyUserHasAnyAcceptedScope(scopeRequiredByApi);
        return TypedResults.StatusCode((int)HttpStatusCode.NotImplemented);
    })
    .WithName("TODO")
    .RequireAuthorization();

app.MapPut("/task-category", IResult (HttpContext httpContext, EmployeeManagerContext dbContext) =>
    {
        httpContext.VerifyUserHasAnyAcceptedScope(scopeRequiredByApi);
        return TypedResults.StatusCode((int)HttpStatusCode.NotImplemented);
    })
    .WithName("createTaskCategory")
    .RequireAuthorization();

app.MapDelete("/task-category", IResult (HttpContext httpContext, EmployeeManagerContext dbContext) =>
    {
        httpContext.VerifyUserHasAnyAcceptedScope(scopeRequiredByApi);
        return TypedResults.StatusCode((int)HttpStatusCode.NotImplemented);
    })
    .WithName("deleteTaskCategory")
    .RequireAuthorization();
    
app.Run();
return;

IResult ApiCatch(Exception e)
{
    return TypedResults.InternalServerError();     
}