using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Moq;
using Onboarding.Controllers;
using Onboarding.Data;
using Onboarding.Data.Enums;
using Onboarding.Hubs;
using Onboarding.Models;
using Onboarding.ViewModels;
using System.Security.Claims;
using Xunit;
using Task = System.Threading.Tasks.Task;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.AspNetCore.Mvc.ViewFeatures;

namespace OnboardingXUnitTests
{
	public class AdminControllerTests
	{
		private readonly ApplicationDbContext _context;
		private readonly Mock<UserManager<User>> _userManagerMock;
		private readonly Mock<RoleManager<IdentityRole<int>>> _roleManagerMock;

		public AdminControllerTests()
		{
			var options = new DbContextOptionsBuilder<ApplicationDbContext>()
				.UseInMemoryDatabase("TestDb")
				.Options;
			_context = new ApplicationDbContext(options);

			var userStoreMock = new Mock<IUserStore<User>>();
			_userManagerMock = new Mock<UserManager<User>>(userStoreMock.Object, null, null, null, null, null, null, null, null);

			var roleStoreMock = new Mock<IRoleStore<IdentityRole<int>>>();
			_roleManagerMock = new Mock<RoleManager<IdentityRole<int>>>(roleStoreMock.Object, null, null, null, null);
		}

		[Fact]
		public async Task ManageRoles_ReturnsCorrectUsersAndRoles()
		{
			// Arrange
			var users = new List<User>
			{
				new User { Id = 1, Name = "Anna", Surname = "Nowak", Email = "anna@example.com" }
			};

			var roles = new List<IdentityRole<int>>
			{
				new IdentityRole<int>("Admin"),
				new IdentityRole<int>("Buddy")
			};

			_userManagerMock.Setup(um => um.Users).Returns(MockDbSetHelper.CreateMockSet(users).Object);
			_userManagerMock.Setup(um => um.GetRolesAsync(It.IsAny<User>())).ReturnsAsync(new List<string> { "Admin" });
			_roleManagerMock.Setup(rm => rm.Roles).Returns(roles.AsQueryable());

			var controller = new AdminController(_context, _roleManagerMock.Object, _userManagerMock.Object);

			// Act
			var result = await controller.ManageRoles("");

			// Assert
			var viewResult = Assert.IsType<ViewResult>(result);
			var model = Assert.IsAssignableFrom<List<ManageRolesViewModel>>(viewResult.Model);
			Assert.Single(model);
			Assert.Equal("anna@example.com", model[0].User.Email);
			Assert.Contains("Admin", model[0].UserRoles);
		}

		[Fact]
		public async Task UpdateRole_AdminCannotRemoveSelf_WhenLastAdmin()
		{
			// Arrange
			var user = new User { Id = 1, Email = "admin@example.com" };
			_context.Users.Add(new User { Id = 1, Email = "admin@example.com" });
			_context.SaveChanges();

			_userManagerMock.Setup(um => um.FindByIdAsync("1")).ReturnsAsync(user);
			_userManagerMock.Setup(um => um.GetRolesAsync(user)).ReturnsAsync(new List<string> { "Admin" });
			_userManagerMock.Setup(um => um.GetUsersInRoleAsync("Admin")).ReturnsAsync(new List<User> { user });
			_userManagerMock.Setup(um => um.GetUserId(It.IsAny<ClaimsPrincipal>())).Returns("1");

			var controller = new AdminController(_context, _roleManagerMock.Object, _userManagerMock.Object);

			// Setup HttpContext.User
			var userClaims = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
			{
		new Claim(ClaimTypes.NameIdentifier, "1")
			}, "mock"));
			controller.ControllerContext = new ControllerContext
			{
				HttpContext = new DefaultHttpContext { User = userClaims }
			};

			// Setup TempData - bardzo ważne, bo jest używane w kontrolerze
			var tempData = new Mock<ITempDataDictionary>();
			controller.TempData = tempData.Object;

			// Act
			var result = await controller.UpdateRole(1, "Buddy");

			// Assert
			var redirect = Assert.IsType<RedirectToActionResult>(result);
			Assert.Equal("ManageRoles", redirect.ActionName);
		}


		[Fact]
		public async Task EditUser_ReturnsViewWithBuddyList_ForNowyRole()
		{
			// Arrange
			var user = new User { Id = 1, Name = "Kasia", BuddyId = null };
			var buddy = new User { Id = 2, Name = "Marek", Surname = "Kowalski" };
			var users = new List<User> { user, buddy };

			_userManagerMock.Setup(um => um.Users).Returns(MockDbSetHelper.CreateMockSet(users).Object);
			_userManagerMock.Setup(um => um.GetRolesAsync(It.Is<User>(u => u.Id == 1))).ReturnsAsync(new List<string> { "Nowy" });
			_userManagerMock.Setup(um => um.GetUsersInRoleAsync("Buddy")).ReturnsAsync(new List<User> { buddy });

			var controller = new AdminController(_context, _roleManagerMock.Object, _userManagerMock.Object);

			// Act
			var result = await controller.EditUser(1);

			// Assert
			var view = Assert.IsType<ViewResult>(result);
			var model = Assert.IsAssignableFrom<EditUserViewModel>(view.Model);
			Assert.Single(model.AvailableBuddies);
		}
	}

	// Helper klasy do obsługi async LINQ (np. ToListAsync)
	public static class MockDbSetHelper
	{
		public static Mock<DbSet<T>> CreateMockSet<T>(IEnumerable<T> data) where T : class
		{
			var queryable = data.AsQueryable();

			var mockSet = new Mock<DbSet<T>>();
			mockSet.As<IQueryable<T>>().Setup(m => m.Provider).Returns(new TestAsyncQueryProvider<T>(queryable.Provider));
			mockSet.As<IQueryable<T>>().Setup(m => m.Expression).Returns(queryable.Expression);
			mockSet.As<IQueryable<T>>().Setup(m => m.ElementType).Returns(queryable.ElementType);
			mockSet.As<IQueryable<T>>().Setup(m => m.GetEnumerator()).Returns(queryable.GetEnumerator());
			mockSet.As<IAsyncEnumerable<T>>().Setup(m => m.GetAsyncEnumerator(It.IsAny<CancellationToken>())).Returns(new TestAsyncEnumerator<T>(queryable.GetEnumerator()));

			return mockSet;
		}

		private class TestAsyncEnumerator<T> : IAsyncEnumerator<T>
		{
			private readonly IEnumerator<T> _inner;
			public TestAsyncEnumerator(IEnumerator<T> inner) => _inner = inner;
			public T Current => _inner.Current;
			public ValueTask DisposeAsync() => ValueTask.CompletedTask;
			public ValueTask<bool> MoveNextAsync() => ValueTask.FromResult(_inner.MoveNext());
		}

		private class TestAsyncQueryProvider<TEntity> : IAsyncQueryProvider
		{
			private readonly IQueryProvider _inner;
			internal TestAsyncQueryProvider(IQueryProvider inner) => _inner = inner;

			public IQueryable CreateQuery(Expression expression)
			{
				return new TestAsyncEnumerable<TEntity>(expression, this);
			}

			public IQueryable<TElement> CreateQuery<TElement>(Expression expression)
			{
				return new TestAsyncEnumerable<TElement>(expression, new TestAsyncQueryProvider<TElement>(this));
			}

			public object Execute(Expression expression) => _inner.Execute(expression);
			public TResult Execute<TResult>(Expression expression) => _inner.Execute<TResult>(expression);

			public TResult ExecuteAsync<TResult>(Expression expression, CancellationToken cancellationToken = default)
			{
				var result = Execute(expression);

				if (typeof(TResult).IsGenericType && typeof(TResult).GetGenericTypeDefinition() == typeof(Task<>))
				{
					var taskResult = typeof(Task).GetMethod(nameof(Task.FromResult))
						.MakeGenericMethod(typeof(TResult).GetGenericArguments()[0])
						.Invoke(null, new[] { result });
					return (TResult)taskResult;
				}

				return (TResult)result;
			}
		}

		private class TestAsyncEnumerable<T> : EnumerableQuery<T>, IAsyncEnumerable<T>, IQueryable<T>
		{
			private readonly IQueryProvider _provider;

			public TestAsyncEnumerable(IEnumerable<T> enumerable) : base(enumerable)
			{
				_provider = new TestAsyncQueryProvider<T>(this);
			}

			public TestAsyncEnumerable(Expression expression) : base(expression)
			{
				_provider = new TestAsyncQueryProvider<T>(this);
			}

			public TestAsyncEnumerable(Expression expression, IQueryProvider provider) : base(expression)
			{
				_provider = provider;
			}

			public IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = default)
				=> new TestAsyncEnumerator<T>(this.AsEnumerable().GetEnumerator());

			IQueryProvider IQueryable.Provider => _provider;
		}


	}
}
