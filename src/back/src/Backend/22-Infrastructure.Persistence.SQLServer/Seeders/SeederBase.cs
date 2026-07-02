using Infrastructure.Persistence.Entities;
using Infrastructure.Persistence.SQLServer.Contexts;
using Infrastructure.Persistence.SQLServer.Exceptions;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.SQLServer.Seeders
{
    public abstract class SeederBase(WritableDbContext context, UserManager<UserDao> userManager)
    {
        public abstract Task SeedDataAsync();

        protected readonly WritableDbContext _context = context;
        protected readonly UserManager<UserDao> _userManager = userManager;

        protected async Task SeedUserAsync(
            string userName,
            string firstName,
            string lastName,
            string email,
            string phoneNumber,
            string password,
            IEnumerable<string> roles
        )
        {
            if (!await _context.Users.AnyAsync(u => u.UserName == userName))
            {
                var userDao = new UserDao()
                {
                    UserName = userName,
                    FirstName = firstName,
                    LastName = lastName,
                    Email = email,
                    PhoneNumber = phoneNumber,
                };
                await SeedUserAsync(userDao, password, roles);
            }
        }

        protected async Task SeedUserAsync(UserDao user, string password, IEnumerable<string> roles)
        {
            if (!await _context.Users.AnyAsync(u => u.UserName == user.UserName))
            {
                var result = await _userManager.CreateAsync(user, password);
                if (!result.Succeeded)
                {
                    throw new DataSeedException($"Could not create user : {user.UserName}");
                }
                foreach (var role in roles)
                {
                    await _userManager.AddToRoleAsync(user, role);
                }
            }
        }
    }
}
