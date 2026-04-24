using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Velocify.Application.DTOs.Users;
using Velocify.Domain.Entities;
using Velocify.Domain.Enums;
using Velocify.Infrastructure.Data;
using Velocify.Infrastructure.Repositories;
using Xunit;

namespace Velocify.Tests.Infrastructure.Repositories;

/// <summary>
/// Unit tests for UserRepository covering user CRUD operations, password hashing, and soft deletes.
/// 
/// TEST COVERAGE:
/// - Create: Verifies correct default values (timestamps, IsActive, ProductivityScore)
/// - GetByEmail: Verifies email lookup using compiled query
/// - Update: Verifies field updates and timestamp changes
/// - Soft Delete: Verifies IsActive flag is set to false without removing record
/// - Password Hashing: Verifies BCrypt hashing and verification
/// - Query Filters: Verifies only active users are returned by GetList
/// 
/// TESTING APPROACH:
/// - Uses in-memory database provider for fast, isolated testing
/// - Tests business rules: timestamp assignment, soft delete behavior, password security
/// - Validates that inactive users are excluded from list queries
/// 
/// Requirements: 30.1-30.7
/// </summary>
public class UserRepositoryTests : IDisposable
{
    private readonly VelocifyDbContext _context;
    private readonly UserRepository _repository;

    public UserRepositoryTests()
    {
        // Setup in-memory database for realistic testing
        var options = new DbContextOptionsBuilder<VelocifyDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new VelocifyDbContext(options);

        _repository = new UserRepository(_context);
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }

    [Fact]
    public async Task Create_ShouldAssignCorrectDefaults()
    {
        // Arrange
        var beforeCreate = DateTime.UtcNow;
        var userDto = new UserDto
        {
            FirstName = "John",
            LastName = "Doe",
            Email = "john.doe@example.com",
            Role = UserRole.Member
        };

        // Act
        var result = await _repository.Create(userDto);
        var afterCreate = DateTime.UtcNow;

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().NotBeEmpty();
        result.FirstName.Should().Be("John");
        result.LastName.Should().Be("Doe");
        result.Email.Should().Be("john.doe@example.com");
        result.Role.Should().Be(UserRole.Member);

        // Verify the user was persisted with correct defaults
        var persistedUser = await _context.Users.FindAsync(result.Id);
        persistedUser.Should().NotBeNull();
        persistedUser!.IsActive.Should().BeTrue("IsActive should default to true");
        persistedUser.ProductivityScore.Should().Be(0, "ProductivityScore should default to 0 for new users");
        persistedUser.CreatedAt.Should().BeOnOrAfter(beforeCreate).And.BeOnOrBefore(afterCreate);
        persistedUser.UpdatedAt.Should().BeOnOrAfter(beforeCreate).And.BeOnOrBefore(afterCreate);
        persistedUser.LastLoginAt.Should().BeNull("LastLoginAt should be null for new users");
    }

    [Fact]
    public async Task GetByEmail_ShouldReturnUser_WhenEmailExists()
    {
        // Arrange
        var user = new User
        {
            Id = Guid.NewGuid(),
            FirstName = "Jane",
            LastName = "Smith",
            Email = "jane.smith@example.com",
            PasswordHash = "hash",
            Role = UserRole.Admin,
            ProductivityScore = 85.5m,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetByEmail("jane.smith@example.com");

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(user.Id);
        result.Email.Should().Be("jane.smith@example.com");
        result.FirstName.Should().Be("Jane");
        result.LastName.Should().Be("Smith");
        result.Role.Should().Be(UserRole.Admin);
        result.ProductivityScore.Should().Be(85.5m);
    }

    [Fact]
    public async Task GetByEmail_ShouldReturnNull_WhenEmailDoesNotExist()
    {
        // Act
        var result = await _repository.GetByEmail("nonexistent@example.com");

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetById_ShouldReturnUser_WhenIdExists()
    {
        // Arrange
        var user = new User
        {
            Id = Guid.NewGuid(),
            FirstName = "Bob",
            LastName = "Johnson",
            Email = "bob.johnson@example.com",
            PasswordHash = "hash",
            Role = UserRole.Member,
            ProductivityScore = 72.3m,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetById(user.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(user.Id);
        result.Email.Should().Be("bob.johnson@example.com");
        result.FirstName.Should().Be("Bob");
        result.LastName.Should().Be("Johnson");
    }

    [Fact]
    public async Task Update_ShouldModifyFields_AndUpdateTimestamp()
    {
        // Arrange
        var user = new User
        {
            Id = Guid.NewGuid(),
            FirstName = "Alice",
            LastName = "Brown",
            Email = "alice.brown@example.com",
            PasswordHash = "hash",
            Role = UserRole.Member,
            ProductivityScore = 50.0m,
            IsActive = true,
            CreatedAt = DateTime.UtcNow.AddDays(-10),
            UpdatedAt = DateTime.UtcNow.AddDays(-10)
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        var originalUpdatedAt = user.UpdatedAt;
        var beforeUpdate = DateTime.UtcNow;

        var updateDto = new UserDto
        {
            Id = user.Id,
            FirstName = "Alice",
            LastName = "Brown-Smith", // Changed last name
            Email = "alice.brown@example.com",
            Role = UserRole.Admin, // Changed role
            ProductivityScore = 75.5m, // Changed score
            IsActive = true
        };

        // Act
        var result = await _repository.Update(updateDto);
        var afterUpdate = DateTime.UtcNow;

        // Assert
        result.Should().NotBeNull();
        result.LastName.Should().Be("Brown-Smith");
        result.Role.Should().Be(UserRole.Admin);
        result.ProductivityScore.Should().Be(75.5m);

        // Verify the user was updated in the database
        var updatedUser = await _context.Users.FindAsync(user.Id);
        updatedUser.Should().NotBeNull();
        updatedUser!.LastName.Should().Be("Brown-Smith");
        updatedUser.Role.Should().Be(UserRole.Admin);
        updatedUser.ProductivityScore.Should().Be(75.5m);
        updatedUser.UpdatedAt.Should().BeAfter(originalUpdatedAt, "UpdatedAt should be updated");
        updatedUser.UpdatedAt.Should().BeOnOrAfter(beforeUpdate).And.BeOnOrBefore(afterUpdate);
    }

    [Fact]
    public async Task Delete_ShouldSetIsActiveToFalse_WithoutRemovingRecord()
    {
        // Arrange
        var user = new User
        {
            Id = Guid.NewGuid(),
            FirstName = "Charlie",
            LastName = "Wilson",
            Email = "charlie.wilson@example.com",
            PasswordHash = "hash",
            Role = UserRole.Member,
            ProductivityScore = 60.0m,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        var userId = user.Id;
        var originalUpdatedAt = user.UpdatedAt;

        // Act
        await _repository.Delete(userId);

        // Assert
        // Verify the record still exists in the database
        var deletedUser = await _context.Users.FindAsync(userId);
        deletedUser.Should().NotBeNull("User record should still exist in database after soft delete");
        deletedUser!.IsActive.Should().BeFalse("IsActive should be set to false");
        deletedUser.UpdatedAt.Should().BeAfter(originalUpdatedAt, "UpdatedAt should be updated on delete");
    }

    [Fact]
    public async Task GetList_ShouldReturnOnlyActiveUsers()
    {
        // Arrange
        var activeUser1 = new User
        {
            Id = Guid.NewGuid(),
            FirstName = "Active",
            LastName = "User1",
            Email = "active1@example.com",
            PasswordHash = "hash",
            Role = UserRole.Member,
            ProductivityScore = 70.0m,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var activeUser2 = new User
        {
            Id = Guid.NewGuid(),
            FirstName = "Active",
            LastName = "User2",
            Email = "active2@example.com",
            PasswordHash = "hash",
            Role = UserRole.Admin,
            ProductivityScore = 80.0m,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var inactiveUser = new User
        {
            Id = Guid.NewGuid(),
            FirstName = "Inactive",
            LastName = "User",
            Email = "inactive@example.com",
            PasswordHash = "hash",
            Role = UserRole.Member,
            ProductivityScore = 50.0m,
            IsActive = false, // Inactive user
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.Users.AddRange(activeUser1, activeUser2, inactiveUser);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetList(page: 1, pageSize: 10);

        // Assert
        result.Should().NotBeNull();
        result.Items.Should().HaveCount(2, "Only active users should be returned");
        result.Items.Should().Contain(u => u.Id == activeUser1.Id);
        result.Items.Should().Contain(u => u.Id == activeUser2.Id);
        result.Items.Should().NotContain(u => u.Id == inactiveUser.Id, "Inactive users should be excluded");
        result.TotalCount.Should().Be(2);
    }

    [Fact]
    public async Task GetList_ShouldRespectPagination()
    {
        // Arrange
        var users = Enumerable.Range(1, 15).Select(i => new User
        {
            Id = Guid.NewGuid(),
            FirstName = $"User{i}",
            LastName = $"Test{i}",
            Email = $"user{i}@example.com",
            PasswordHash = "hash",
            Role = UserRole.Member,
            ProductivityScore = 50.0m,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        }).ToList();

        _context.Users.AddRange(users);
        await _context.SaveChangesAsync();

        // Act
        var page1 = await _repository.GetList(page: 1, pageSize: 5);
        var page2 = await _repository.GetList(page: 2, pageSize: 5);
        var page3 = await _repository.GetList(page: 3, pageSize: 5);

        // Assert
        page1.Items.Should().HaveCount(5);
        page2.Items.Should().HaveCount(5);
        page3.Items.Should().HaveCount(5);
        page1.TotalCount.Should().Be(15);
        page1.TotalPages.Should().Be(3);

        // Verify no overlap between pages
        var allIds = page1.Items.Select(u => u.Id)
            .Concat(page2.Items.Select(u => u.Id))
            .Concat(page3.Items.Select(u => u.Id))
            .ToList();
        allIds.Should().OnlyHaveUniqueItems("Pages should not contain duplicate users");
    }

    [Fact]
    public void HashPassword_ShouldGenerateValidBCryptHash()
    {
        // Arrange
        var password = "SecurePassword123!";

        // Act
        var hash = _repository.HashPassword(password);

        // Assert
        hash.Should().NotBeNullOrEmpty();
        hash.Should().StartWith("$2", "BCrypt hashes start with $2");
        hash.Length.Should().BeGreaterThan(50, "BCrypt hashes are typically 60 characters");
    }

    [Fact]
    public void VerifyPassword_ShouldReturnTrue_ForCorrectPassword()
    {
        // Arrange
        var password = "MyPassword456!";
        var hash = _repository.HashPassword(password);

        // Act
        var result = _repository.VerifyPassword(password, hash);

        // Assert
        result.Should().BeTrue("Correct password should verify successfully");
    }

    [Fact]
    public void VerifyPassword_ShouldReturnFalse_ForIncorrectPassword()
    {
        // Arrange
        var correctPassword = "CorrectPassword789!";
        var incorrectPassword = "WrongPassword123!";
        var hash = _repository.HashPassword(correctPassword);

        // Act
        var result = _repository.VerifyPassword(incorrectPassword, hash);

        // Assert
        result.Should().BeFalse("Incorrect password should fail verification");
    }

    [Fact]
    public async Task GetProductivityHistory_ShouldReturnEmptyList_WhenNoCompletedTasks()
    {
        // Arrange
        var user = new User
        {
            Id = Guid.NewGuid(),
            FirstName = "Test",
            LastName = "User",
            Email = "test@example.com",
            PasswordHash = "hash",
            Role = UserRole.Member,
            ProductivityScore = 0,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetProductivityHistory(user.Id);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEmpty("No completed tasks means no productivity history");
    }
}
