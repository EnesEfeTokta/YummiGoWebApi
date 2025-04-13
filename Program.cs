using Microsoft.EntityFrameworkCore;
using YummiGoWebApi.Data;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

var builder = WebApplication.CreateBuilder(args);

// --- Basitleþtirilmiþ CORS Politikasý (Tüm origin'lere izin) ---
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

// Servisleri Konteynera Ekleme
builder.Services.AddDbContext<DataContext>(options =>
{
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"),
        npqsqlOptions =>
        {
            npqsqlOptions.EnableRetryOnFailure(
                maxRetryCount: 3,
                maxRetryDelay: TimeSpan.FromSeconds(5),
                errorCodesToAdd: null);
        });
});

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Session yapýlandýrmasý (basitleþtirilmiþ örnek)
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(60);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

var app = builder.Build();

// Uygulama baþlangýcýnda Recipe ID sequence ayarlanmasý
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var logger = services.GetRequiredService<ILogger<Program>>();
    try
    {
        var context = services.GetRequiredService<DataContext>();
        logger.LogInformation("Veritabaný baðlantýsý kontrol ediliyor ve Recipe ID sequence ayarlanýyor...");

        string schemaName = "public";
        string tableName = "\"Recipes\"";
        string idColumnName = "\"Id\"";
        string sequenceNameFunction = $"pg_get_serial_sequence('{schemaName}.{tableName}', {idColumnName.Trim('"')})";
        string sql = $"SELECT setval({sequenceNameFunction}, COALESCE((SELECT MAX({idColumnName}) + 1 FROM {schemaName}.{tableName}), 1), false);";

        logger.LogInformation("Çalýþtýrýlacak SQL (Sequence Reset): {SQL}", sql);
        await context.Database.ExecuteSqlRawAsync(sql);
        logger.LogInformation("Recipe ID sequence baþarýyla ayarlandý.");
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Uygulama baþlangýcýnda Recipe ID sequence ayarlanamadý.");
    }
}

// HTTP Request Pipeline yapýlandýrmasý
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.UseCors("AllowAll"); // Tüm origin'lere izin veren CORS politikasý
app.UseSession();

app.UseAuthorization();
app.MapControllers();

app.Run();
