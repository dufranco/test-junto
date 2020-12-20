using JuntoSegurosTest.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Moq;
using NUnit.Framework;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace JuntoSegurosTest.Controllers.Test
{
    [TestFixture]
    public class UsersControllerTest
    {
        private Mock<IUserStore<ApplicationUser>> _userStore;
        private Mock<UserManager<ApplicationUser>> _userManager;
        private Mock<IHttpContextAccessor> _contextAccessor;
        private Mock<IUserClaimsPrincipalFactory<ApplicationUser>> _userPrincipalFactory;
        private Mock<SignInManager<ApplicationUser>> _signInManager;
        private Mock<IConfiguration> _configuration;
        private Mock<UsersController> _controller;

        [SetUp]
        public void SetUp()
        {
            _userStore = new Mock<IUserStore<ApplicationUser>>();
            _userManager = new Mock<UserManager<ApplicationUser>>(_userStore.Object, null, null, null, null, null, null, null, null);
            _contextAccessor = new Mock<IHttpContextAccessor>();
            _userPrincipalFactory = new Mock<IUserClaimsPrincipalFactory<ApplicationUser>>();
            _signInManager = new Mock<SignInManager<ApplicationUser>>(_userManager.Object, _contextAccessor.Object, _userPrincipalFactory.Object, null, null, null, null);
            _configuration = new Mock<IConfiguration>();
            _controller = new Mock<UsersController>(_userManager.Object, _signInManager.Object, _configuration.Object);
        }

        [Test]
        public async Task FailLoginWithoutEmailAndPassword()
        {
            var userInfo = new UserInfo();
            var controller = new UsersController(_userManager.Object, _signInManager.Object, _configuration.Object);

            var result = await controller.Login(userInfo);
            var processedResult = result.Result as BadRequestObjectResult;
            var expectedResult = new BadRequestObjectResult("Usuário ou senha inválidos.");

            Assert.AreEqual(processedResult.StatusCode, expectedResult.StatusCode);
            Assert.AreEqual(processedResult.Value, expectedResult.Value);
        }

        [Test]
        public async Task FailLoginWithoutEmail()
        {
            var userInfo = new UserInfo() { Password = "Pass@word" };
            var controller = new UsersController(_userManager.Object, _signInManager.Object, _configuration.Object);

            var result = await controller.Login(userInfo);
            var processedResult = result.Result as BadRequestObjectResult;
            var expectedResult = new BadRequestObjectResult("Usuário ou senha inválidos.");

            Assert.AreEqual(processedResult.StatusCode, expectedResult.StatusCode);
            Assert.AreEqual(processedResult.Value, expectedResult.Value);
        }

        [Test]
        public async Task FailLoginWithoutPassword()
        {
            var userInfo = new UserInfo() { Email = "test@test.com" };
            var controller = new UsersController(_userManager.Object, _signInManager.Object, _configuration.Object);

            var result = await controller.Login(userInfo);
            var processedResult = result.Result as BadRequestObjectResult;
            var expectedResult = new BadRequestObjectResult("Usuário ou senha inválidos.");

            Assert.AreEqual(processedResult.StatusCode, expectedResult.StatusCode);
            Assert.AreEqual(processedResult.Value, expectedResult.Value);
        }

        [Test]
        public async Task SuccessLogin()
        {
            var userInfo = new UserInfo() { Email = "test@test.com", Password = "Pass@word" };
            var userToken = new UserToken() { Expiration = DateTime.UtcNow, Token = RandomString() };
            var expectedResult = new ActionResult<UserToken>(userToken);

            _controller.Setup(
                x => x.Login(userInfo)
                ).ReturnsAsync(
                    expectedResult
                );

            var result = await _controller.Object.Login(userInfo);

            Assert.IsInstanceOf(typeof(ActionResult<UserToken>), result);
            Assert.AreEqual(expectedResult.Result, result.Result);
            Assert.AreEqual(expectedResult.Value.Expiration, result.Value.Expiration);
            Assert.AreEqual(expectedResult.Value.Token, result.Value.Token);
        }

        [Test]
        public async Task SuccessCreateUser()
        {
            var email = "test@test.com";
            var userInfo = new UserInfo() { Email = email, Password = "Pass@word" };
            var userToken = new UserToken() { Expiration = DateTime.UtcNow, Token = RandomString() };
            var expectedResult = new ActionResult<UserToken>(userToken);

            _controller.Setup(
                x => x.CreateUser(userInfo)
            ).Returns(
                Task.FromResult(expectedResult)
            );

            var result = await _controller.Object.CreateUser(userInfo);

            Assert.AreEqual(expectedResult.Result, result.Result);
            Assert.AreEqual(expectedResult.Value.Expiration, result.Value.Expiration);
            Assert.AreEqual(expectedResult.Value.Token, result.Value.Token);
        }

        [Test]
        public async Task FailCreateUser()
        {
            var email = "test@test.com";
            var userInfo = new UserInfo() { Email = email, Password = "Pass@word" };
            var userToken = new UserToken();
            var expectedResult = new ActionResult<UserToken>(userToken);

            _controller.Setup(
                x => x.CreateUser(userInfo)
            ).Returns(
                Task.FromResult(expectedResult)
            );

            var result = await _controller.Object.CreateUser(userInfo);

            Assert.AreEqual(expectedResult.Result, result.Result);
            Assert.AreEqual(expectedResult.Value.Expiration, DateTime.MinValue);
            Assert.AreEqual(expectedResult.Value.Token, result.Value.Token);
        }

        [Test]
        public async Task SuccessResetPasswordUser()
        {
            var email = "test@test.com";
            var userInfo = new UserInfo() { Email = email, Password = "Pass@word", NewPassword = "Pass!word" };
            var userResetToken = new UserToken() { Expiration = DateTime.UtcNow, Token = RandomString() };
            var expectedResult = new ActionResult<UserToken>(userResetToken);

            _controller.Setup(
                x => x.ResetPassword(userInfo)
            ).Returns(
                Task.FromResult(expectedResult)
            );

            var result = await _controller.Object.ResetPassword(userInfo);

            Assert.AreEqual(expectedResult.Result, result.Result);
            Assert.AreEqual(expectedResult.Value.Expiration, result.Value.Expiration);
            Assert.AreEqual(expectedResult.Value.Token, result.Value.Token);
        }

        [Test]
        public async Task FailResetPasswordUser()
        {
            var email = "test@test.com";
            var userInfo = new UserInfo() { Email = email, Password = "Pass@word", NewPassword = "pwd" };
            var userToken = new UserToken();
            var expectedResult = new ActionResult<UserToken>(userToken);

            _controller.Setup(
                x => x.ResetPassword(userInfo)
            ).Returns(
                Task.FromResult(expectedResult)
            );

            var result = await _controller.Object.ResetPassword(userInfo);

            Assert.AreEqual(expectedResult.Result, result.Result);
            Assert.AreEqual(expectedResult.Value.Expiration, DateTime.MinValue);
            Assert.AreEqual(expectedResult.Value.Token, result.Value.Token);
        }

        private static readonly Random random = new Random();
        private static string RandomString()
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(Enumerable.Repeat(chars, random.Next(50, 200)).Select(s => s[random.Next(s.Length)]).ToArray());
        }
    }
}
