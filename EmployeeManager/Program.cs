using System.Net;
using EmployeeManager.Models.EF;
using EmployeeManager.Models.ViewModel;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Web;
using Microsoft.Identity.Web.Resource;
using Npgsql;

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

app.UseExceptionHandler(exceptionHandlerApp =>
{
    exceptionHandlerApp.Run(async context =>
    {
        var exception = context.Features.GetRequiredFeature<IExceptionHandlerFeature>().Error;
        
        // try to a more specific error code by inspecting the error 
        if (exception.InnerException is PostgresException pgException)
        {
            context.Response.StatusCode = pgException.SqlState switch
            {
                "23000" or "23001" or "23502" or "23503" or "23505" or "23514" or "23P01" => StatusCodes.Status422UnprocessableEntity,
                _ => StatusCodes.Status500InternalServerError
            };
        }
        else
        {
            // default, 500
            context.Response.StatusCode = StatusCodes.Status500InternalServerError;
        }
        
        await context.Response.WriteAsync(
            app.Environment.IsDevelopment() 
                ? exception.ToString()
                : "Something went wrong."
        );
    });
});

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
        return TypedResults.Created(
            linkGenerator.GetUriByName(httpContext, "getEmployees"),
            (ICollection<EmployeeViewModel>)[..myEmployees.Select(x => new EmployeeViewModel(x))]
        );
    }

    return TypedResults.StatusCode((int)HttpStatusCode.Unauthorized);
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
            .Select(x => new EmployeeViewModel(x))
    );
})
    .WithName("getEmployees")
    .RequireAuthorization();

app.MapGet("/employee", IResult (HttpContext httpContext, EmployeeManagerContext dbContext, [FromQuery] int id) 
    => DetailsPerUserEntity(httpContext, dbContext.Employees, id, x => new EmployeeViewModel(x))
)
    .WithName("getEmployee")
    .RequireAuthorization();


app.MapPost("/employee", IResult (HttpContext httpContext, EmployeeManagerContext dbContext, LinkGenerator linkGenerator, [FromBody] EmployeeViewModel employee) =>
    {
        return CreatePerUserEntity(
            httpContext,
            dbContext,
            linkGenerator, 
            dbContext.Employees, 
            employee.ToEFModel(httpContext.User),
            x => new EmployeeViewModel(x)
        );
    })
    .WithName("createEmployee")
    .RequireAuthorization();

app.MapPut("/employee", IResult (HttpContext httpContext, EmployeeManagerContext dbContext, LinkGenerator linkGenerator, [FromBody] EmployeeViewModel employee) =>
    {
        return UpdatePerUserEntity(
            httpContext,  dbContext,
            dbContext.Employees, 
            employee.ToEFModel(httpContext.User),
            x => new EmployeeViewModel(x)
        );
    })
    .WithName("updateEmployee")
    .RequireAuthorization();

app.MapDelete("/employee", IResult (HttpContext httpContext, EmployeeManagerContext dbContext, [FromQuery] int id) 
        => DeletePerUserEntity(httpContext, dbContext.Employees, id)
    )
    .WithName("deleteEmployee")
    .RequireAuthorization();

/*
 *
 *
 * 
 * Endpoints for Task CRUD.
 */

app.MapPost("/work-item", IResult (HttpContext httpContext, EmployeeManagerContext dbContext, LinkGenerator linkGenerator, [FromBody] WorkItemViewModel workItemViewModel) =>
    {
        var workItem = workItemViewModel.ToEFModel(httpContext.User);
        if (!workItem.IsValid())
        {
            return TypedResults.BadRequest("Invalid work item.");
        }
        
        return CreatePerUserEntity(
            httpContext,
            dbContext,
            linkGenerator, 
            dbContext.WorkItems, 
            workItem,
            x => new WorkItemViewModel(x)
        );
    })
    .WithName("createWorkItem")
    .RequireAuthorization();

app.MapPut("/work-item", IResult (HttpContext httpContext, EmployeeManagerContext dbContext, [FromBody] WorkItemViewModel workItemViewModel) =>
    {
        httpContext.VerifyUserHasAnyAcceptedScope(scopeRequiredByApi);
        var workItem = workItemViewModel.ToEFModel(httpContext.User);
        return UpdatePerUserEntity(httpContext, dbContext, dbContext.WorkItems, workItem, x => new WorkItemViewModel(x));
    })
    .WithName("updateWorkItem")
    .RequireAuthorization();

app.MapDelete("/work-item", IResult (HttpContext httpContext, EmployeeManagerContext dbContext, [FromQuery] int id)
    => DeletePerUserEntity(httpContext, dbContext.WorkItems, id)
)
    .WithName("deleteWorkItem")
    .RequireAuthorization();

/*
 *
 *
 * 
 * Endpoints for WorkItemCategory CRUD.
 */

app.MapGet("/work-item-category/list", IResult (HttpContext httpContext, EmployeeManagerContext dbContext) =>
    {
        httpContext.VerifyUserHasAnyAcceptedScope(scopeRequiredByApi);

        ICollection<WorkItemCategoryViewModel> categories = [..((ICollection<WorkItemCategory>)
        [
            // get the user-defined categories
            .. dbContext.WorkItemCategories
                .Where(x => x.UserId == httpContext.User.GetObjectId()),
            // get the unassigned items and build a fake category
            new WorkItemCategory
            {
                Id = -1,
                Name = "UNASSIGNED",
                WorkItems = [.. dbContext.WorkItems.Where(x => x.Category == null)]
            }
        ]).Select(x => new WorkItemCategoryViewModel(x))];
        
        return TypedResults.Ok(categories);
    })
    .WithName("listWorkItemCategory")
    .RequireAuthorization();

app.MapGet("/work-item-category", IResult (HttpContext httpContext, EmployeeManagerContext dbContext, [FromQuery] int id)
    => DetailsPerUserEntity(httpContext, dbContext.WorkItemCategories, id, x => new WorkItemCategoryViewModel(x))
)
    .WithName("getWorkItemCategory")
    .RequireAuthorization();


app.MapPost("/work-item-category", IResult (HttpContext httpContext, EmployeeManagerContext dbContext, LinkGenerator linkGenerator, [FromBody] WorkItemCategoryViewModel category) =>
    {
        if (category.Id == -1 || category.Name == "Unassigned")
        {
            return TypedResults.BadRequest("Reserved work item category.");
        }
        
        return CreatePerUserEntity(
            httpContext, 
            dbContext,
            linkGenerator, 
            dbContext.WorkItemCategories, 
            category.ToEFModel(httpContext.User),
            x => new WorkItemCategoryViewModel(x)
        );
    })
    .WithName("createWorkItemCategory")
    .RequireAuthorization();

app.MapPut("/work-item-category", IResult (HttpContext httpContext, EmployeeManagerContext dbContext, LinkGenerator linkGenerator, [FromBody] WorkItemCategoryViewModel category) =>
    {
        if (category.Id == -1 || category.Name == "UNASSIGNED")
        {
            return TypedResults.BadRequest("Reserved work item category.");
        }
        
        return UpdatePerUserEntity(
            httpContext, dbContext,
            dbContext.WorkItemCategories, 
            category.ToEFModel(httpContext.User),
            x => new WorkItemCategoryViewModel(x)
        );
    })
    .WithName("updateWorkItemCategory")
    .RequireAuthorization();

app.MapDelete("/work-item-category", IResult (HttpContext httpContext, EmployeeManagerContext dbContext, [FromQuery] int id) 
        => DeletePerUserEntity(httpContext, dbContext.WorkItemCategories, id)
    )
    .WithName("deleteWorkItemCategory")
    .RequireAuthorization();
    
app.Run();
return;

IResult DetailsPerUserEntity<TEntity,TView>(HttpContext httpContext, DbSet<TEntity> dbSet, int id, Func<TEntity,TView> mapper) where TEntity : PerUserEntity where TView: ViewModel<TEntity>
{
    httpContext.VerifyUserHasAnyAcceptedScope(scopeRequiredByApi);
    // get the employee with the matching ID, if they're associated with user
    var found = dbSet.Find(id);

    return found != null && found.VisibleToUser(httpContext.User)
        ? TypedResults.Ok(mapper(found)) // respond with it if it exists and the user's authorized to access it
        : TypedResults.NotFound(); // otherwise respond not found. Even if they're unauthorized, we don't want them to know it exists.
}

/*
 * Insert a PerUserEntity into the DB, checking authorization etc.
 */
IResult CreatePerUserEntity<TEntity,TView> ( HttpContext httpContext, DbContext dbContext, LinkGenerator linkGenerator, DbSet<TEntity> dbSet, TEntity x, Func<TEntity,TView> mapper) where TEntity : PerUserEntity where TView: ViewModel<TEntity>
{
    httpContext.VerifyUserHasAnyAcceptedScope(scopeRequiredByApi);

    if (x.Id != 0)
    {
        return TypedResults.BadRequest("Id cannot be provided.");
    }
        
    x.UserId = httpContext.User.GetObjectId();
    var created = dbSet.Add(x).Entity;
    dbContext.SaveChanges();
    
    return TypedResults.Created(
        linkGenerator.GetUriByName(
            httpContext, "getEmployee",
            new { id = created.Id }
        ),
        mapper(created)
    );
}

/*
 * Update a PerUserEntity in the DB, checking authorization etc.
 */
IResult UpdatePerUserEntity<TEntity,TView> ( HttpContext httpContext, DbContext dbContext, DbSet<TEntity> dbSet, TEntity entity, Func<TEntity, TView> mapper) where TEntity : PerUserEntity where TView : ViewModel<TEntity>
{
    httpContext.VerifyUserHasAnyAcceptedScope(scopeRequiredByApi);

    // only perform the update if the user is authorized to access this entity
    var found = dbSet.Find(entity.Id);
    if (found == null || !found.VisibleToUser(httpContext.User))
    {
        return TypedResults.NotFound();
    }
    dbContext.Entry(found).State = EntityState.Detached;
        
    var ret = TypedResults.Ok(mapper(dbSet.Update(entity).Entity));
    dbContext.SaveChanges();
    return ret;

    // otherwise the entity doesn't exist (from their perspective).
}


/*
 * Update a PerUserEntity in the DB, checking authorization etc.
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