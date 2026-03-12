using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using FakeItEasy;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Onboarding.Controllers;
using Onboarding.Data;
using Onboarding.Models;
using Onboarding.Interfaces;
using Xunit;
using Task = System.Threading.Tasks.Task;

namespace OnboardingXUnitTests
{
    public class TasksControllerTests : IDisposable
    {
        private readonly TasksController _controller;
        private readonly ApplicationDbContext _context;

        public TasksControllerTests()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            _context = new ApplicationDbContext(options);

            _controller = new TasksController(_context);

            var user = new ClaimsPrincipal(new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, "1"),
                new Claim(ClaimTypes.Name, "test@test.com")
            }, "TestAuthentication"));

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = user }
            };
        }

        [Fact]
        public async Task Index_ReturnsViewResult()
        {
            // Act
            var result = await _controller.Index();

            // Assert
            result.Should().BeOfType<ViewResult>();
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}
