using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

foreach (var (page, size) in new[] { (200000000, 20), (107374184, 20), (int.MaxValue, 20), (21474837, 100), (30000000, 100) })
    Console.WriteLine($"page={page,-12} pageSize={size,-4} => Skip({(page - 1) * size})");

var opts = new DbContextOptionsBuilder<SonaraDbContext>()
    .UseNpgsql("Host=127.0.0.1;Database=none;Username=none;Password=none")
    .Options;
using var ctx = new SonaraDbContext(opts);
int p = 200000000, ps = 20;
var sql = ctx.Tracks.AsNoTracking().OrderByDescending(t => t.CreatedAt).Skip((p - 1) * ps).Take(ps).Select(t => t.Id).ToQueryString();
Console.WriteLine("---- generated SQL ----");
Console.WriteLine(sql);
