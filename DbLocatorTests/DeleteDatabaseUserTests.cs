using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.EntityFrameworkCore;
using DbLocator.Entities;
using DbLocator.Commands;
using DbLocator.Services;

namespace DbLocatorTests
{
    public class DeleteDatabaseUserTests
    {
        private readonly IDbContextFactory<DatabaseContext> _dbContextFactory;
        private readonly IDistributedCache _distributedCache;

        public DeleteDatabaseUserTests(IDbContextFactory<DatabaseContext> dbContextFactory, IDistributedCache distributedCache)
        {
            _dbContextFactory = dbContextFactory;
            _distributedCache = distributedCache;
        }

        [Fact]
        public async Task Handle_WhenUserHasRoles_ClearsCache()
        {
            // Arrange
            var dbContext = await _dbContextFactory.CreateDbContextAsync();
            var cache = new DbLocatorCache(_distributedCache);
            var handler = new DeleteDatabaseUser(_dbContextFactory, cache);

            // Create a database user with roles
            var databaseUser = new DatabaseUserEntity
            {
                UserName = "testuser",
                Password = "testpass"
            };
            dbContext.Set<DatabaseUserEntity>().Add(databaseUser);
            await dbContext.SaveChangesAsync();

            var role = new DatabaseUserRoleEntity
            {
                DatabaseUserId = databaseUser.DatabaseUserId,
                DatabaseRoleId = (int)DatabaseRole.Admin
            };
            dbContext.Set<DatabaseUserRoleEntity>().Add(role);
            await dbContext.SaveChangesAsync();

            // Act
            await handler.Handle(new DeleteDatabaseUserCommand(databaseUser.DatabaseUserId, true));

            // Assert
            var deletedUser = await dbContext.Set<DatabaseUserEntity>().FindAsync(databaseUser.DatabaseUserId);
            Assert.Null(deletedUser);

            var deletedRole = await dbContext.Set<DatabaseUserRoleEntity>().FindAsync(role.DatabaseUserRoleId);
            Assert.Null(deletedRole);
        }
    }
} 