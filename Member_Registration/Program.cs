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

            // Register the DbContext with a connection string
            builder.Services.AddDbContext<iBlueAnts_MembersContext>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
            
            builder.Services.AddAuthentication("MyCookieScheme") // Define your scheme name
           .AddCookie("MyCookieScheme", options =>
           {
                options.LoginPath = "/Account/Login"; // Redirect to login page
                options.LogoutPath = "/Account/Logout"; // Redirect to logout page
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
