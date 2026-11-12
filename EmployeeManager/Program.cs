using System.Net;
using System.Reflection;
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

app.MapGet("/employee", IResult (HttpContext httpContext, EmployeeManagerContext dbContext, [FromQuery] int id) 
    => DetailsPerUserEntity(httpContext, dbContext.Employees, id)
)
    .WithName("getEmployee")
    .RequireAuthorization();


app.MapPost("/employee", IResult (HttpContext httpContext, EmployeeManagerContext dbContext, LinkGenerator linkGenerator, [FromBody] Employee employee) =>
    {
        var ret = PostPerUserEntity(httpContext, linkGenerator, dbContext.Employees, employee);
        dbContext.SaveChanges();
        return ret;
    })
    .WithName("createEmployee")
    .RequireAuthorization();

app.MapPut("/employee", IResult (HttpContext httpContext, EmployeeManagerContext dbContext, LinkGenerator linkGenerator, [FromBody] Employee employee) =>
    {
        var ret = PutPerUserEntity(httpContext, linkGenerator, dbContext.Employees, employee);
        dbContext.SaveChanges();
        return ret;
    })
    .WithName("updateEmployee")
    .RequireAuthorization();

app.MapDelete("/employee", IResult (HttpContext httpContext, EmployeeManagerContext dbContext, [FromQuery] int id) =>
    { 
        var ret = DeletePerUserEntity(httpContext, dbContext.Employees, id);
        return ret;
    })
    .WithName("deleteEmployee")
    .RequireAuthorization();

/*
 *
 *
 * 
 * Endpoints for simultaneously updating a Task & Employee.
 */
app.MapPost("/assigned-task", IResult (HttpContext httpContext, EmployeeManagerContext dbContext, [FromQuery] int taskId, [FromBody] int employeeId) =>
    {
        httpContext.VerifyUserHasAnyAcceptedScope(scopeRequiredByApi);
        return TypedResults.StatusCode((int)HttpStatusCode.NotImplemented); // TODO
    })
    .WithName("assignUserTask")
    .RequireAuthorization();
app.MapDelete("/assigned-task", IResult (HttpContext httpContext, EmployeeManagerContext dbContext, [FromQuery] int taskId) =>
    {
        httpContext.VerifyUserHasAnyAcceptedScope(scopeRequiredByApi);
        return TypedResults.StatusCode((int)HttpStatusCode.NotImplemented); // TODO
    })
    .WithName("unassignUserTask")
    .RequireAuthorization();

/*
 *
 *
 * 
 * Endpoints for Task CRUD.
 */

app.MapPost("/task", IResult (HttpContext httpContext, EmployeeManagerContext dbContext) =>
    {
        httpContext.VerifyUserHasAnyAcceptedScope(scopeRequiredByApi);
        return TypedResults.StatusCode((int)HttpStatusCode.NotImplemented); // TODO
    })
    .WithName("createTask")
    .RequireAuthorization();

app.MapPut("/task", IResult (HttpContext httpContext, EmployeeManagerContext dbContext) =>
    {
        httpContext.VerifyUserHasAnyAcceptedScope(scopeRequiredByApi);
        return TypedResults.StatusCode((int)HttpStatusCode.NotImplemented); // TODO
    })
    .WithName("updateTasks")
    .RequireAuthorization();

app.MapDelete("/task", IResult (HttpContext httpContext, EmployeeManagerContext dbContext) =>
    {
        httpContext.VerifyUserHasAnyAcceptedScope(scopeRequiredByApi);
        return TypedResults.StatusCode((int)HttpStatusCode.NotImplemented); // TODO
    })
    .WithName("deleteTasks")
    .RequireAuthorization();

/*
 *
 *
 * 
 * Endpoints for simultaneously updating a Task & TaskCategory.
 */

app.MapPost("/categorise-task", IResult (HttpContext httpContext, EmployeeManagerContext dbContext, [FromQuery] int taskId, [FromBody] int categoryId) =>
    {
        httpContext.VerifyUserHasAnyAcceptedScope(scopeRequiredByApi);
        return TypedResults.StatusCode((int)HttpStatusCode.NotImplemented); // TODO
    })
    .WithName("assignTaskCategory")
    .RequireAuthorization();
app.MapDelete("/categorise-task", IResult (HttpContext httpContext, EmployeeManagerContext dbContext, [FromQuery] int taskId) =>
    {
        httpContext.VerifyUserHasAnyAcceptedScope(scopeRequiredByApi);
        return TypedResults.StatusCode((int)HttpStatusCode.NotImplemented); // TODO
    })
    .WithName("unassignTaskCategory")
    .RequireAuthorization();

/*
 *
 *
 * 
 * Endpoints for TaskCategory CRUD.
 */

app.MapGet("/work-item-category/list", IResult (HttpContext httpContext, EmployeeManagerContext dbContext) =>
    {
        httpContext.VerifyUserHasAnyAcceptedScope(scopeRequiredByApi);
        return TypedResults.StatusCode((int)HttpStatusCode.NotImplemented); // TODO
    })
    .WithName("listTaskCategory")
    .RequireAuthorization();

app.MapGet("/work-item-category", IResult (HttpContext httpContext, EmployeeManagerContext dbContext, [FromQuery] int id)
    => DetailsPerUserEntity(httpContext, dbContext.WorkItemCategories, id)
)
    .WithName("getTaskCategory")
    .RequireAuthorization();


app.MapPost("/work-item-category", IResult (HttpContext httpContext, EmployeeManagerContext dbContext, LinkGenerator linkGenerator, [FromBody] WorkItemCategory category) =>
    {
        var ret = PostPerUserEntity(httpContext, linkGenerator, dbContext.WorkItemCategories, category);
        dbContext.SaveChanges();
        return ret;
    })
    .WithName("TODO")
    .RequireAuthorization();

app.MapPut("/work-item-category", IResult (HttpContext httpContext, EmployeeManagerContext dbContext, LinkGenerator linkGenerator, [FromBody] WorkItemCategory category) =>
    {
        var ret = PutPerUserEntity(httpContext, linkGenerator, dbContext.WorkItemCategories, category);
        dbContext.SaveChanges();
        return ret;
    })
    .WithName("createTaskCategory")
    .RequireAuthorization();

app.MapDelete("/work-item-category", IResult (HttpContext httpContext, EmployeeManagerContext dbContext, [FromQuery] int id) =>
    { 
        var ret = DeletePerUserEntity(httpContext, dbContext.WorkItemCategories, id);
        return ret;
    })
    .WithName("deleteTaskCategory")
    .RequireAuthorization();
    
app.Run();
return;

IResult DetailsPerUserEntity<T>(HttpContext httpContext, DbSet<T> dbSet, int id) where T : PerUserEntity
{
    httpContext.VerifyUserHasAnyAcceptedScope(scopeRequiredByApi);
    // get the employee with the matching ID, if they're associated with user
    var found = dbSet.Find(id);

    return found == null || found.VisibleToUser(httpContext.User)
        ? TypedResults.Ok(found) // respond with it if it exists and the user's authorized to access it
        : TypedResults.NotFound(); // otherwise respond not found. Even if they're unauthorized, we don't want them to know it exists.
}

/*
 * Insert a PerUserEntity into the DB, checking authorization etc.
 * Caller is responsible for calling SaveChanges.
 */
IResult PostPerUserEntity<T> ( HttpContext httpContext, LinkGenerator linkGenerator, DbSet<T> dbSet, T x) where T : PerUserEntity
{
    httpContext.VerifyUserHasAnyAcceptedScope(scopeRequiredByApi);

    if (x.Id != 0)
    {
        return TypedResults.BadRequest("Id cannot be provided.");
    }
        
    x.UserId = httpContext.User.GetObjectId();
    var created = dbSet.Add(x).Entity;

    return TypedResults.Created(
        linkGenerator.GetUriByName(
            httpContext, "getEmployee",
            new { id = created.Id }
        ),
        created
    );
}

/*
 * Update a PerUserEntity in the DB, checking authorization etc.
 * Caller is responsible for calling SaveChanges.
 */
IResult PutPerUserEntity<T> ( HttpContext httpContext, LinkGenerator linkGenerator, DbSet<T> dbSet, T x) where T : PerUserEntity
{
    httpContext.VerifyUserHasAnyAcceptedScope(scopeRequiredByApi);

    // only perform the update if the user is authorized to access this entity
    var found = dbSet.Find(x.Id);
    if (found != null && found.VisibleToUser(httpContext.User))
    {
        x.UserId = found.UserId;
        var updated = dbSet.Update(x).Entity;
        
        return TypedResults.Ok(updated);
    }
    else
    {
        // otherwise the entity doesn't exist (from their perspective).
        return TypedResults.NotFound();
    }
}

/*
 * Update a PerUserEntity in the DB, checking authorization etc.
 * Caller does not need to call SaveChanges.
 */
IResult DeletePerUserEntity<T>(HttpContext httpContext, DbSet<T> dbSet, int id) where T : PerUserEntity
{
    // delete all the entities with this ID associated with this user (should only ever be zero-or-one)
    dbSet
        .Where(x => x.Id == id && x.UserId == httpContext.User.GetObjectId())
        .ExecuteDelete();
    
    // the method should be idempotent, so we always return Ok
    return TypedResults.Ok();
}