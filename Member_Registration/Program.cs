using Member_Registration.Models;
using Microsoft.EntityFrameworkCore;

namespace Member_Registration
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddControllersWithViews();

            // Register the iBlueAnts_MembersContext with its connection string
            builder.Services.AddDbContext<iBlueAnts_MembersContext>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

            // Register the UserAdminDBContext with its connection string
            builder.Services.AddDbContext<UserAdminDBContext>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("UserAdminConnection")));

            // Configure authentication
            builder.Services.AddAuthentication("MyCookieScheme")
            .AddCookie("MyCookieScheme", options =>
            {
                options.LoginPath = "/Account/Login";
                options.LogoutPath = "/Account/Logout";
            });

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Members}/{action=ShowMembers}/{id?}");

            app.Run();
        }
    }
}
