using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Space_FaceID.Data.Context;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Space_FaceID.Data.Seed
{
    public static class DataSeeder
    {
        public static async Task SeedDatabase(IHost host)
        {
            using var scope = host.Services.CreateScope();
            var services = scope.ServiceProvider;
            try
            {
                var context = services.GetRequiredService<FaceIDDbContext>();

                // ตรวจสอบว่าต้องทำ migration หรือไม่
                if (context.Database.GetPendingMigrations().Any())
                {
                    // ทำการ migration database
                    context.Database.Migrate();
                }

                // เพิ่มข้อมูลเริ่มต้น
                await SeedData.Initialize(services);
            }
            catch (Exception ex)
            {
                var logger = services.GetRequiredService<ILogger<App>>();
                logger.LogError(ex, "เกิดข้อผิดพลาดในการเพิ่มข้อมูลเริ่มต้น");
            }
        }
    }
}
