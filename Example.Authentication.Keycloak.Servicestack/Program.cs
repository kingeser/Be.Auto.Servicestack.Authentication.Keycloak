var builder = WebApplication.CreateBuilder(args);
builder.Services.AddAuthentication();
builder.Services.AddHttpContextAccessor();
builder.Services.AddMvc();
builder.Services.AddMvcCore();
builder.Services.AddRazorPages();
builder.Services.AddControllers();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
    app.UseHttpsRedirection();
}
app.UseServiceStack(new AppHost());
app.Run();




