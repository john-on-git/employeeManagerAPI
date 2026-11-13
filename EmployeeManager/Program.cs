using System.Net;
using EmployeeManager.Models.EF;
using EmployeeManager.Models.ViewModel;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Web;
using Microsoft.Identity.Web.Resource;

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
        return PostPerUserEntity(
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
        return PutPerUserEntityDefaultUpdater(
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
 * Endpoints for simultaneously updating a Task & Employee.
 */
app.MapPost("/assigned-work-item", IResult (HttpContext httpContext, EmployeeManagerContext dbContext, [FromQuery] int taskId, [FromBody] int employeeId) =>
    {
        httpContext.VerifyUserHasAnyAcceptedScope(scopeRequiredByApi);
        return TypedResults.StatusCode((int)HttpStatusCode.NotImplemented); // TODO
    })
    .WithName("assignUserWorkItem")
    .RequireAuthorization();

app.MapDelete("/assigned-work-item", IResult (HttpContext httpContext, EmployeeManagerContext dbContext, [FromQuery] int taskId) =>
    {
        httpContext.VerifyUserHasAnyAcceptedScope(scopeRequiredByApi);
        return TypedResults.StatusCode((int)HttpStatusCode.NotImplemented); // TODO
    })
    .WithName("unassignUserWorkItem")
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
        
        return PostPerUserEntity(
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
        return PutPerUserEntity(httpContext, dbContext, dbContext.WorkItems, workItem.Id,
        updaterDbContext =>
        {
            Console.WriteLine($"workItem.UserId = {workItem.UserId}, {httpContext.User.GetObjectId()}");
            var updated = updaterDbContext.Update(workItem);
            // This endpoint only allows us to update the title & desc.
            // (These don't get updated if set=null. Weird.)
            if(workItem.AssignedTo != null)
                updated.Property(item => item.AssignedTo).IsModified = false;
            if(workItem.Category != null)
                updated.Property(item => item.Category).IsModified = false;
            // these too, but they're already impossible to modify
            // ret.Property(item => item.Id).IsModified = false;
            // ret.Property(item => item.UserId).IsModified = false;
            return updated.Entity;
        },
        x => new WorkItemViewModel(x)
        );
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
 * Endpoints for simultaneously updating a Task & WorkItemCategory.
 */

app.MapPost("/categorise-work-item", IResult (HttpContext httpContext, EmployeeManagerContext dbContext, [FromQuery] int taskId, [FromBody] int categoryId) =>
    {
        httpContext.VerifyUserHasAnyAcceptedScope(scopeRequiredByApi);
        return TypedResults.StatusCode((int)HttpStatusCode.NotImplemented); // TODO
    })
    .WithName("assignWorkItemCategory")
    .RequireAuthorization();

app.MapDelete("/categorise-work-item", IResult (HttpContext httpContext, EmployeeManagerContext dbContext, [FromQuery] int taskId) =>
    {
        httpContext.VerifyUserHasAnyAcceptedScope(scopeRequiredByApi);
        return TypedResults.StatusCode((int)HttpStatusCode.NotImplemented); // TODO
    })
    .WithName("unassignWorkItemCategory")
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
        
        return PostPerUserEntity(
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
        
        return PutPerUserEntityDefaultUpdater(
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
IResult PostPerUserEntity<TEntity,TView> ( HttpContext httpContext, DbContext dbContext, LinkGenerator linkGenerator, DbSet<TEntity> dbSet, TEntity x, Func<TEntity,TView> mapper) where TEntity : PerUserEntity where TView: ViewModel<TEntity>
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
IResult PutPerUserEntity<TEntity,TView> ( HttpContext httpContext, DbContext dbContext, DbSet<TEntity> dbSet, int id, Func<DbSet<TEntity>, TEntity> updater, Func<TEntity, TView> mapper) where TEntity : PerUserEntity where TView : ViewModel<TEntity>
{
    httpContext.VerifyUserHasAnyAcceptedScope(scopeRequiredByApi);

    // only perform the update if the user is authorized to access this entity
    var found = dbSet.Find(id);
    
    if (found == null || !found.VisibleToUser(httpContext.User))
    {
        return TypedResults.NotFound();
    }
    
    // stop tracking found, because
    // the updater might also try to track the entity
    dbContext.Entry(found).State = EntityState.Detached;
        
    var ret = TypedResults.Ok(mapper(updater(dbSet)));
    dbContext.SaveChanges();
    return ret;

    // otherwise the entity doesn't exist (from their perspective).
}

IResult PutPerUserEntityDefaultUpdater<TEntity, TView>(HttpContext httpContext, DbContext dbContext, DbSet<TEntity> dbSet, TEntity x, Func<TEntity, TView> mapper) where TEntity : PerUserEntity where TView : ViewModel<TEntity>
{
    return PutPerUserEntity(
        httpContext, dbContext, dbSet, x.Id,
    updaterDbSet => updaterDbSet.Update(x).Entity,
        mapper
    );
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