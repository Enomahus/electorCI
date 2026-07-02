using System;
using System.Collections.Generic;
using System.Text;
using Infrastructure.Persistence.Configurations;
using Infrastructure.Persistence.Entities;
using Infrastructure.Persistence.SQLServer.Contexts;
using Infrastructure.Persistence.SQLServer.Contexts.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Tools.Constants;
using Tools.Exceptions;

namespace Infrastructure.Persistence.SQLServer.Seeders
{
    public class DataSeeder(
        WritableDbContext context,
        UserManager<UserDao> userManager,
        RoleManager<RoleDao> roleManager,
        IOptions<DataConfiguration> dataConfig
    ) : SeederBase(context, userManager)
    {
        private static readonly string AdminUserName = "pcea_admin";

        public override async Task SeedDataAsync()
        {
            if (!dataConfig.Value.Seed)
            {
                return;
            }
            var strategy = _context.Database.CreateExecutionStrategy();
            await strategy.ExecuteInTransactionAsync(
                async () =>
                {
                    await SeedRolesAsync();
                    await SeedDistrictsAsync();
                    await SeedDefaultUserAsync();
                },
                () => Task.FromResult(true)
            );
        }

        private async Task SeedDistrictsAsync()
        {
            var districts = DistrictData.GetDistrictData;
            foreach (var district in districts)
            {
                var dbConstituency = await _context.Districts.FirstOrDefaultAsync(c =>
                    c.Wording == district.Wording && c.Level == district.Level
                );

                if (dbConstituency == null)
                {
                    await _context.Districts.AddAsync(district);
                }
                else
                {
                    dbConstituency.Code = district.Code;

                    _context.Districts.Update(dbConstituency);
                }
            }
            await _context.SaveChangesAsync();
        }

        private async Task SeedDefaultUserAsync()
        {
            if (dataConfig.Value.DefaultUserPassword is null)
            {
                throw new ConfigurationMissingException(
                    "Missing configuration : DataConfig.DefaultUserConfig"
                );
            }

            await SeedUserAsync(
                AdminUserName,
                "Pcea",
                "Admin",
                "dev@pcea.com",
                "01 23 45 67 89",
                dataConfig.Value.DefaultUserPassword,
                [AppConstants.SuperAdminRole]
            );
        }

        private async Task SeedRolesAsync()
        {
            // 1. Synchronisation des tables de référence (Enum -> DB)
            // Grâce à tes ConfigureConventions, l'insertion se fera en string
            await SynchronizeEnumTableAsync(
                _context.AppPermissions,
                p => p.PermissionCode,
                code => new AppPermissionDao { PermissionCode = code }
            );

            await SynchronizeEnumTableAsync(
                _context.AppActions,
                a => a.ActionCode,
                code => new AppActionDao { ActionCode = code }
            );

            await _context.SaveChangesAsync();
            _context.ChangeTracker.Clear();

            // 2. Chargement optimisé (Eager Loading)
            var allActions = await _context.AppActions.Include(a => a.Permissions).ToListAsync();

            var allPermissions = await _context.AppPermissions.ToDictionaryAsync(p =>
                p.PermissionCode
            );

            // 3. Update Action <-> Permission (Many-to-Many)
            foreach (var seed in RolesData.ActionsSeed)
            {
                var actionDao = allActions.First(a => a.ActionCode == seed.Key);

                // On transforme les codes du seed en objets trackés
                var targetPerms = seed
                    .Value.Select(code => allPermissions.GetValueOrDefault(code))
                    .OfType<AppPermissionDao>()
                    .ToHashSet();

                // Synchro des relations
                actionDao.Permissions.RomoveWhere(p => !targetPerms.Contains(p));

                foreach (var p in targetPerms.Where(p => !actionDao.Permissions.Contains(p)))
                {
                    actionDao.Permissions.Add(p);
                }
            }

            // 4. Update Role <-> Action (Many-to-Many)
            // Note: On utilise le RoleManager pour la création pour respecter la logique Identity
            var existingRoles = await _context.Roles.Include(r => r.Actions).ToListAsync();

            foreach (var seed in RolesData.RolesSeed)
            {
                var roleDao = existingRoles.FirstOrDefault(r => r.Name == seed.Key);

                if (roleDao == null)
                {
                    roleDao = new RoleDao(seed.Key);
                    var result = await roleManager.CreateAsync(roleDao);
                    if (!result.Succeeded)
                        throw new Exception($"Failed to create role {seed.Key}");

                    // On recharge pour activer le tracking des relations
                    roleDao = await _context
                        .Roles.Include(r => r.Actions)
                        .FirstAsync(r => r.Name == seed.Key);
                }

                var targetActions = allActions
                    .Where(a => seed.Value.Contains(a.ActionCode))
                    .ToHashSet();

                roleDao.Actions.RomoveWhere(a => !targetActions.Contains(a));

                foreach (var a in targetActions.Where(a => !roleDao.Actions.Contains(a)))
                {
                    roleDao.Actions.Add(a);
                }
            }

            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Méthode générique pour synchroniser une table à partir d'un Enum
        /// </summary>
        private static async Task SynchronizeEnumTableAsync<TEnum, TEntity>(
            DbSet<TEntity> dbSet,
            Func<TEntity, TEnum> propertySelector,
            Func<TEnum, TEntity> factory
        )
            where TEnum : struct, Enum
            where TEntity : class
        {
            var enumValues = Enum.GetValues<TEnum>().ToHashSet();
            var existingEntities = await dbSet.ToListAsync();

            // Supprimer les entrées en base qui ne sont plus dans l'Enum (Code cleanup)
            var toDelete = existingEntities
                .Where(e => !enumValues.Contains(propertySelector(e)))
                .ToList();
            if (toDelete.Count != 0)
                dbSet.RemoveRange(toDelete);

            // Ajouter les nouvelles entrées de l'Enum
            var existingCodes = existingEntities.Select(propertySelector).ToHashSet();
            var toAdd = enumValues.Where(v => !existingCodes.Contains(v)).Select(factory).ToList();

            if (toAdd.Count != 0)
                dbSet.AddRange(toAdd);
        }
    }

    public static class CollectionExtensions
    {
        public static void RomoveWhere<T>(this ICollection<T> collection, Func<T, bool> predicate)
        {
            if (collection is List<T> list)
            {
                list.RemoveAll(new Predicate<T>(predicate));
                return;
            }

            var itemsToRemove = collection.Where(predicate).ToList();
            foreach (var item in itemsToRemove)
            {
                collection.Remove(item);
            }
        }
    }
}
