using finalprojectSD340.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace finalprojectSD340.Models
{
    public class SeedData
    {
        public async static System.Threading.Tasks.Task Initialize(IServiceProvider serviceProvider)
        {
            var context = new ApplicationDbContext(serviceProvider.GetRequiredService<DbContextOptions<ApplicationDbContext>>());
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var passwordHasher = new PasswordHasher<ApplicationUser>();

            if (!context.Roles.Any())
            {
                List<string> newRoles = new List<string>()
                {
                    "Admin",
                    "Project Manager",
                    "Developer"
                };
                foreach (string role in newRoles)
                {
                    await roleManager.CreateAsync(new IdentityRole(role));
                }
            }

            if (!context.Users.Any())
            {
                ApplicationUser firstAdmin = new ApplicationUser
                {
                    Email = "admin@mitt.ca",
                    NormalizedEmail = "ADMIN@MITT.CA",
                    UserName = "admin@mitt.ca",
                    NormalizedUserName = "ADMIN@MITT.CA",
                    EmailConfirmed = true
                };
                var hashedPassword = passwordHasher.HashPassword(firstAdmin, "Password!1");
                firstAdmin.PasswordHash = hashedPassword;
                await userManager.CreateAsync(firstAdmin);
                await userManager.AddToRoleAsync(firstAdmin, "Admin");

                ApplicationUser firstProjManager = new ApplicationUser
                {
                    Email = "pm@mitt.ca",
                    NormalizedEmail = "PM@MITT.CA",
                    UserName = "pm@mitt.ca",
                    NormalizedUserName = "PM@MITT.CA",
                    EmailConfirmed = true,
                    Salary = 5000
                };
                var hashed1Password = passwordHasher.HashPassword(firstProjManager, "Password!1");
                firstProjManager.PasswordHash = hashed1Password;
                await userManager.CreateAsync(firstProjManager);
                await userManager.AddToRoleAsync(firstProjManager, "Project Manager");

                ApplicationUser dev1 = new ApplicationUser
                {
                    Email = "dev1@mitt.ca",
                    NormalizedEmail = "DEV1@MITT.CA",
                    UserName = "dev1@mitt.ca",
                    NormalizedUserName = "DEV1@MITT.CA",
                    EmailConfirmed = true,
                    Salary = 100
            };
                var hashed2Password = passwordHasher.HashPassword(dev1, "Password!1");
                dev1.PasswordHash = hashed2Password;
                await userManager.CreateAsync(dev1);
                await userManager.AddToRoleAsync(dev1, "Developer");

                ApplicationUser dev2 = new ApplicationUser
                {
                    Email = "dev2@mitt.ca",
                    NormalizedEmail = "DEV2@MITT.CA",
                    UserName = "dev2@mitt.ca",
                    NormalizedUserName = "DEV2@MITT.CA",
                    EmailConfirmed = true,
                    Salary = 200
                };
                var hashed3Password = passwordHasher.HashPassword(dev2, "Password!1");
                dev1.PasswordHash = hashed3Password;
                await userManager.CreateAsync(dev2);
                await userManager.AddToRoleAsync(dev2, "Developer");

                ApplicationUser dev3 = new ApplicationUser
                {
                    Email = "dev3@mitt.ca",
                    NormalizedEmail = "DEV3@MITT.CA",
                    UserName = "dev3@mitt.ca",
                    NormalizedUserName = "DEV3@MITT.CA",
                    EmailConfirmed = true,
                    Salary = 300
            };
                var hashed4Password = passwordHasher.HashPassword(dev3, "Password!1");
                dev1.PasswordHash = hashed4Password;
                await userManager.CreateAsync(dev3);
                await userManager.AddToRoleAsync(dev3, "Developer");           

                List<Project> newProjects = new List<Project>()
                {
                    new Project { Name = "project 1", Budget = 20000, Deadline = new DateTime(2022, 10, 01), Description = "first project", Priority = Priority.Low, ProjectManagerId = firstProjManager.Id },
                    new Project { Name = "project 2", Budget = 30000, Deadline = new DateTime(2022, 02, 01), Description = "second project", Priority = Priority.Moderate, ProjectManagerId = firstProjManager.Id },
                    new Project { Name = "project 3", Budget = 40000, Deadline = new DateTime(2022, 04, 01), Description = "third project", Priority = Priority.High, ProjectManagerId = firstProjManager.Id },
                    new Project { Name = "project 4", Budget = 40000, Deadline = new DateTime(2022, 03, 01), Description = "fourth project", Priority = Priority.Low, ProjectManagerId = firstProjManager.Id }
                };

                context.Projects.AddRange(newProjects);

                List<Task> newTasks = new List<Task>()
                {
                    new Task { Name = "task 1", Deadline = new DateTime(2022, 01, 15), Project = newProjects[1], Description = "its task 1", DeveloperId = dev1.Id, CompletionPercentage = 40, Priority = Priority.Moderate, StartDate = new DateTime(2022, 01, 01) },
                    new Task { Name = "task 2", Deadline = new DateTime(2022, 04, 01), Project = newProjects[2], Description = "its task 2", DeveloperId = dev2.Id, CompletionPercentage = 0, Priority = Priority.High, StartDate = new DateTime(2022, 03, 01)},
                    new Task { Name = "task 3", Deadline = new DateTime(2022, 02, 27), Project = newProjects[3], Description = "Task 3" , DeveloperId = dev3.Id, CompletionPercentage = 0, Priority = Priority.High, StartDate = new DateTime(2022, 02, 01)}
                };

                context.Tasks.AddRange(newTasks);

                List<Comment> newComments = new List<Comment>()
                {
                    new Comment { Body = "its comment 1", DateCreated = new DateTime(2022, 04, 01), Task = newTasks[0], DeveloperId = dev1.Id, IsUrgent = true },
                    new Comment { Body = "its comment 2", DateCreated = new DateTime(2022, 03, 01), Task = newTasks[0], DeveloperId = dev1.Id, IsUrgent = false }
                };

                context.Comments.AddRange(newComments);

                List<Notification> newNotifications = new List<Notification>()
                {
                    new Notification { Title = "Complete", Body = "this is notif 1", NotificationDate = new DateTime(2022, 01, 01), ProjectId = 1, Type = NotificationType.Completed, User = dev1 }
                };

                context.Notifications.AddRange(newNotifications);
            }

            context.SaveChanges();
        }
    }
}
