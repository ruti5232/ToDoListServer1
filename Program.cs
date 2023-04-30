using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ToDoApi;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddCors(options =>
{
    options.AddPolicy("AddPolicy",
                          policy =>
                          {
                              policy.AllowAnyOrigin()
                                                  .AllowAnyHeader()
                                                  .AllowAnyMethod();
                          });
});
builder.Services.AddDbContext<ToDoDbContext>();
builder.Services.AddSwaggerGen();
var app = builder.Build();
app.UseCors("AddPolicy");
if(app.Environment.IsDevelopment())
{
    app.UseSwagger(options =>
{
    options.SerializeAsV2 = true;
});
    app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "v1");
    options.RoutePrefix = string.Empty;
});
}


// app.MapGet("/", () => "Hello World!");
app.MapGet("/items", async (ToDoDbContext db) =>
      await db.Items.ToListAsync());
app.MapPost("/items", async (ToDoDbContext db,[FromBody]ItemModel item) =>
{
    var myItem=new Item{Name=item.Name,IsComplete=item.IsComplete};
    db.Items.Add(myItem);
    await db.SaveChangesAsync();
    return item;
});
app.MapPut("/items", async (ToDoDbContext db,[FromBody]Item item) =>
{
    var myItem = await db.Items.FindAsync(item.Id);
    if (myItem is null)
        return Results.NotFound();
    myItem.IsComplete = item.IsComplete;
    await db.SaveChangesAsync();
    return Results.NoContent();
});
app.MapDelete("/items/{id}", async (int id, ToDoDbContext db) =>
{
    var myItem = await db.Items.FindAsync(id);
    if (myItem != null)
    {
        db.Items.Remove(myItem);
        await db.SaveChangesAsync();
        return Results.Ok();
    }
    return Results.NotFound();
});
app.Run();