using BusinessLogic.DTO.Request;
using BusinessLogic.DTO.Response;
using BusinessLogic.Servicies;
using Presentation.Controllers;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace WebApp.UnitTests
{
    public class UnitTestCreator_ErrorCodes
    {
        private readonly Mock<IBaseService<CreatorRequestTo, CreatorResponseTo>> _mockService;
        private readonly CreatorsController _controller;
        private readonly List<CreatorResponseTo> _testCreators;

        public UnitTestCreator_ErrorCodes()
        {
            _mockService = new Mock<IBaseService<CreatorRequestTo, CreatorResponseTo>>();
            _controller = new CreatorsController(_mockService.Object);
            _testCreators = SeedTestData();
        }

        private List<CreatorResponseTo> SeedTestData()
        {
            var creators = new List<CreatorResponseTo>
            {
                new CreatorResponseTo
                {
                    Id = 1,
                    Login = "ivanov",
                    Password = "password123",
                    FirstName = "Иван",
                    LastName = "Иванов"
                },
                new CreatorResponseTo
                {
                    Id = 2,
                    Login = "petrov",
                    Password = "password456",
                    FirstName = "Петр",
                    LastName = "Петров"
                },
                new CreatorResponseTo
                {
                    Id = 3,
                    Login = "sergeev",
                    Password = "password789",
                    FirstName = "Сергей",
                    LastName = "Сергеев"
                }
            };

            foreach (var creator in creators)
            {
                _mockService
                    .Setup(service => service.GetById(creator.Id))
                    .Returns(creator);
            }

            _mockService
                .Setup(service => service.GetById(It.Is<int>(id => id > 100)))
                .Returns((CreatorResponseTo)null);

            _mockService
                .Setup(service => service.GetAll())
                .Returns(creators);

            foreach (var creator in creators)
            {
                var updateRequest = new CreatorRequestTo
                {
                    Id = creator.Id,
                    Login = creator.Login,
                    Password = creator.Password,
                    FirstName = creator.FirstName,
                    LastName = creator.LastName
                };

                _mockService
                    .Setup(service => service.Update(It.Is<CreatorRequestTo>(r => r.Id == creator.Id)))
                    .Returns(creator);
            }

            _mockService
                .Setup(service => service.Update(It.Is<CreatorRequestTo>(r => r.Id > 100)))
                .Returns((CreatorResponseTo)null);

            foreach (var creator in creators)
            {
                _mockService
                    .Setup(service => service.DeleteById(creator.Id))
                    .Returns(true);
            }

            _mockService
                .Setup(service => service.DeleteById(It.Is<int>(id => id > 100)))
                .Returns(false);

            return creators;
        }

        #region GET Tests - Error Codes

        [Fact]
        public void GetById_NonExistingId_Returns404NotFound()
        {
            var nonExistingId = 999;

            var result = _controller.GetById(nonExistingId);

            Assert.IsType<NotFoundResult>(result.Result);
            Assert.Equal(404, (result.Result as NotFoundResult)?.StatusCode);

            _mockService.Verify(service => service.GetById(nonExistingId), Times.Once);
        }

        [Fact]
        public void GetById_ExistingId_ReturnsOkWithCreator()
        {
            var existingId = 1;
            var expectedCreator = _testCreators.First(c => c.Id == existingId);

            var result = _controller.GetById(existingId);

            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            Assert.Equal(200, okResult.StatusCode);

            var returnedCreator = Assert.IsType<CreatorResponseTo>(okResult.Value);
            Assert.Equal(expectedCreator.Id, returnedCreator.Id);
            Assert.Equal(expectedCreator.Login, returnedCreator.Login);
            Assert.Equal(expectedCreator.FirstName, returnedCreator.FirstName);
            Assert.Equal(expectedCreator.LastName, returnedCreator.LastName);

            _mockService.Verify(service => service.GetById(existingId), Times.Once);
        }

        [Fact]
        public void GetAll_ReturnsOkWithAllCreators()
        {
            var result = _controller.GetAll();

            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            Assert.Equal(200, okResult.StatusCode);

            var returnedCreators = Assert.IsType<List<CreatorResponseTo>>(okResult.Value);
            Assert.Equal(_testCreators.Count, returnedCreators.Count);

            for (int i = 0; i < _testCreators.Count; i++)
            {
                Assert.Equal(_testCreators[i].Id, returnedCreators[i].Id);
                Assert.Equal(_testCreators[i].Login, returnedCreators[i].Login);
            }

            _mockService.Verify(service => service.GetAll(), Times.Once);
        }

        #endregion

        #region CREATE Tests - Error Codes

        [Fact]
        public void Create_ValidData_ReturnsCreated()
        {
            var newRequest = new CreatorRequestTo
            {
                Login = "newuser",
                Password = "newpassword123",
                FirstName = "Новый",
                LastName = "Пользователь"
            };

            var createdResponse = new CreatorResponseTo
            {
                Id = 4,
                Login = newRequest.Login,
                Password = newRequest.Password,
                FirstName = newRequest.FirstName,
                LastName = newRequest.LastName
            };

            _mockService
                .Setup(service => service.Create(newRequest))
                .Returns(createdResponse);

            var result = _controller.Create(newRequest);

            var createdResult = Assert.IsType<CreatedResult>(result.Result);
            Assert.Equal(201, createdResult.StatusCode);
            Assert.Equal($"{createdResponse.Id}", createdResult.Location);

            var returnedCreator = Assert.IsType<CreatorResponseTo>(createdResult.Value);
            Assert.Equal(createdResponse.Id, returnedCreator.Id);
            Assert.Equal(createdResponse.Login, returnedCreator.Login);

            _mockService.Verify(service => service.Create(newRequest), Times.Once);
        }

        [Fact]
        public void Create_LoginTooShort_ThrowsException()
        {
            var invalidRequest = new CreatorRequestTo
            {
                Login = "a",
                Password = "password123",
                FirstName = "Иван",
                LastName = "Иванов"
            };

            _mockService
                .Setup(service => service.Create(invalidRequest))
                .Throws(new Exception("Login should be from 2 to 64 symbols"));

            var exception = Assert.Throws<Exception>(() => _controller.Create(invalidRequest));
            Assert.Equal("Login should be from 2 to 64 symbols", exception.Message);

            _mockService.Verify(service => service.Create(invalidRequest), Times.Once);
        }

        [Fact]
        public void Create_LoginTooLong_ThrowsException()
        {
            var invalidRequest = new CreatorRequestTo
            {
                Login = new string('a', 65),
                Password = "password123",
                FirstName = "Иван",
                LastName = "Иванов"
            };

            _mockService
                .Setup(service => service.Create(invalidRequest))
                .Throws(new Exception("Login should be from 2 to 64 symbols"));

            var exception = Assert.Throws<Exception>(() => _controller.Create(invalidRequest));
            Assert.Equal("Login should be from 2 to 64 symbols", exception.Message);

            _mockService.Verify(service => service.Create(invalidRequest), Times.Once);
        }

        [Fact]
        public void Create_PasswordTooShort_ThrowsException()
        {
            var invalidRequest = new CreatorRequestTo
            {
                Login = "validlogin",
                Password = "short",
                FirstName = "Иван",
                LastName = "Иванов"
            };

            _mockService
                .Setup(service => service.Create(invalidRequest))
                .Throws(new Exception("Password should be from 8 to 128 symbols"));

            var exception = Assert.Throws<Exception>(() => _controller.Create(invalidRequest));
            Assert.Equal("Password should be from 8 to 128 symbols", exception.Message);

            _mockService.Verify(service => service.Create(invalidRequest), Times.Once);
        }

        [Fact]
        public void Create_PasswordTooLong_ThrowsException()
        {
            var invalidRequest = new CreatorRequestTo
            {
                Login = "validlogin",
                Password = new string('p', 129),
                FirstName = "Иван",
                LastName = "Иванов"
            };

            _mockService
                .Setup(service => service.Create(invalidRequest))
                .Throws(new Exception("Password should be from 8 to 128 symbols"));

            var exception = Assert.Throws<Exception>(() => _controller.Create(invalidRequest));
            Assert.Equal("Password should be from 8 to 128 symbols", exception.Message);

            _mockService.Verify(service => service.Create(invalidRequest), Times.Once);
        }

        [Fact]
        public void Create_FirstNameTooShort_ThrowsException()
        {
            var invalidRequest = new CreatorRequestTo
            {
                Login = "validlogin",
                Password = "password123",
                FirstName = "I",
                LastName = "Иванов"
            };

            _mockService
                .Setup(service => service.Create(invalidRequest))
                .Throws(new Exception("FirstName should be from 2 to 64 symbols"));

            var exception = Assert.Throws<Exception>(() => _controller.Create(invalidRequest));
            Assert.Equal("FirstName should be from 2 to 64 symbols", exception.Message);

            _mockService.Verify(service => service.Create(invalidRequest), Times.Once);
        }

        [Fact]
        public void Create_FirstNameTooLong_ThrowsException()
        {
            var invalidRequest = new CreatorRequestTo
            {
                Login = "validlogin",
                Password = "password123",
                FirstName = new string('I', 65),
                LastName = "Иванов"
            };

            _mockService
                .Setup(service => service.Create(invalidRequest))
                .Throws(new Exception("FirstName should be from 2 to 64 symbols"));

            var exception = Assert.Throws<Exception>(() => _controller.Create(invalidRequest));
            Assert.Equal("FirstName should be from 2 to 64 symbols", exception.Message);

            _mockService.Verify(service => service.Create(invalidRequest), Times.Once);
        }

        [Fact]
        public void Create_LastNameTooShort_ThrowsException()
        {
            var invalidRequest = new CreatorRequestTo
            {
                Login = "validlogin",
                Password = "password123",
                FirstName = "Иван",
                LastName = "P"
            };

            _mockService
                .Setup(service => service.Create(invalidRequest))
                .Throws(new Exception("LastName should be from 2 to 64 symbols"));

            var exception = Assert.Throws<Exception>(() => _controller.Create(invalidRequest));
            Assert.Equal("LastName should be from 2 to 64 symbols", exception.Message);

            _mockService.Verify(service => service.Create(invalidRequest), Times.Once);
        }

        [Fact]
        public void Create_LastNameTooLong_ThrowsException()
        {
            var invalidRequest = new CreatorRequestTo
            {
                Login = "validlogin",
                Password = "password123",
                FirstName = "Иван",
                LastName = new string('P', 65)
            };

            _mockService
                .Setup(service => service.Create(invalidRequest))
                .Throws(new Exception("LastName should be from 2 to 64 symbols"));

            var exception = Assert.Throws<Exception>(() => _controller.Create(invalidRequest));
            Assert.Equal("LastName should be from 2 to 64 symbols", exception.Message);

            _mockService.Verify(service => service.Create(invalidRequest), Times.Once);
        }

        [Fact]
        public void Create_LoginEmpty_ThrowsException()
        {
            var invalidRequest = new CreatorRequestTo
            {
                Login = "",
                Password = "password123",
                FirstName = "Иван",
                LastName = "Иванов"
            };

            _mockService
                .Setup(service => service.Create(invalidRequest))
                .Throws(new Exception("Login is required"));

            var exception = Assert.Throws<Exception>(() => _controller.Create(invalidRequest));
            Assert.Equal("Login is required", exception.Message);

            _mockService.Verify(service => service.Create(invalidRequest), Times.Once);
        }

        [Fact]
        public void Create_PasswordEmpty_ThrowsException()
        {
            var invalidRequest = new CreatorRequestTo
            {
                Login = "validlogin",
                Password = "",
                FirstName = "Иван",
                LastName = "Иванов"
            };

            _mockService
                .Setup(service => service.Create(invalidRequest))
                .Throws(new Exception("Password is required"));

            var exception = Assert.Throws<Exception>(() => _controller.Create(invalidRequest));
            Assert.Equal("Password is required", exception.Message);

            _mockService.Verify(service => service.Create(invalidRequest), Times.Once);
        }

        [Fact]
        public void Create_FirstNameEmpty_ThrowsException()
        {
            var invalidRequest = new CreatorRequestTo
            {
                Login = "validlogin",
                Password = "password123",
                FirstName = "",
                LastName = "Иванов"
            };

            _mockService
                .Setup(service => service.Create(invalidRequest))
                .Throws(new Exception("FirstName is required"));

            var exception = Assert.Throws<Exception>(() => _controller.Create(invalidRequest));
            Assert.Equal("FirstName is required", exception.Message);

            _mockService.Verify(service => service.Create(invalidRequest), Times.Once);
        }

        [Fact]
        public void Create_LastNameEmpty_ThrowsException()
        {
            var invalidRequest = new CreatorRequestTo
            {
                Login = "validlogin",
                Password = "password123",
                FirstName = "Иван",
                LastName = ""
            };

            _mockService
                .Setup(service => service.Create(invalidRequest))
                .Throws(new Exception("LastName is required"));

            var exception = Assert.Throws<Exception>(() => _controller.Create(invalidRequest));
            Assert.Equal("LastName is required", exception.Message);

            _mockService.Verify(service => service.Create(invalidRequest), Times.Once);
        }

        #endregion

        #region UPDATE Tests - Error Codes

        [Fact]
        public void Update_ExistingId_ReturnsOkWithUpdatedCreator()
        {
            var existingId = 1;
            var updateRequest = new CreatorRequestTo
            {
                Id = existingId,
                Login = "ivanov_updated",
                Password = "newpassword123",
                FirstName = "Иван",
                LastName = "Иванов"
            };

            var updatedResponse = new CreatorResponseTo
            {
                Id = existingId,
                Login = updateRequest.Login,
                Password = updateRequest.Password,
                FirstName = updateRequest.FirstName,
                LastName = updateRequest.LastName
            };

            _mockService
                .Setup(service => service.Update(updateRequest))
                .Returns(updatedResponse);

            var result = _controller.Update(updateRequest);

            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            Assert.Equal(200, okResult.StatusCode);

            var returnedCreator = Assert.IsType<CreatorResponseTo>(okResult.Value);
            Assert.Equal(updateRequest.Id, returnedCreator.Id);
            Assert.Equal(updateRequest.Login, returnedCreator.Login);

            _mockService.Verify(service => service.Update(updateRequest), Times.Once);
        }

        [Fact]
        public void Update_NonExistingId_Returns404NotFound()
        {
            var nonExistingRequest = new CreatorRequestTo
            {
                Id = 999,
                Login = "validlogin",
                Password = "password123",
                FirstName = "Иван",
                LastName = "Иванов"
            };

            _mockService
                .Setup(service => service.Update(nonExistingRequest))
                .Returns((CreatorResponseTo)null);

            var result = _controller.Update(nonExistingRequest);

            Assert.IsType<NotFoundResult>(result.Result);
            Assert.Equal(404, (result.Result as NotFoundResult)?.StatusCode);

            _mockService.Verify(service => service.Update(nonExistingRequest), Times.Once);
        }

        [Fact]
        public void Update_LoginTooShort_ThrowsException()
        {
            var invalidRequest = new CreatorRequestTo
            {
                Id = 1,
                Login = "a",
                Password = "password123",
                FirstName = "Иван",
                LastName = "Иванов"
            };

            _mockService
                .Setup(service => service.Update(invalidRequest))
                .Throws(new Exception("Login should be from 2 to 64 symbols"));

            var exception = Assert.Throws<Exception>(() => _controller.Update(invalidRequest));
            Assert.Equal("Login should be from 2 to 64 symbols", exception.Message);

            _mockService.Verify(service => service.Update(invalidRequest), Times.Once);
        }

        [Fact]
        public void Update_LoginTooLong_ThrowsException()
        {
            var invalidRequest = new CreatorRequestTo
            {
                Id = 1,
                Login = new string('a', 65),
                Password = "password123",
                FirstName = "Иван",
                LastName = "Иванов"
            };

            _mockService
                .Setup(service => service.Update(invalidRequest))
                .Throws(new Exception("Login should be from 2 to 64 symbols"));

            var exception = Assert.Throws<Exception>(() => _controller.Update(invalidRequest));
            Assert.Equal("Login should be from 2 to 64 symbols", exception.Message);

            _mockService.Verify(service => service.Update(invalidRequest), Times.Once);
        }

        [Fact]
        public void Update_PasswordTooShort_ThrowsException()
        {
            var invalidRequest = new CreatorRequestTo
            {
                Id = 1,
                Login = "validlogin",
                Password = "short",
                FirstName = "Иван",
                LastName = "Иванов"
            };

            _mockService
                .Setup(service => service.Update(invalidRequest))
                .Throws(new Exception("Password should be from 8 to 128 symbols"));

            var exception = Assert.Throws<Exception>(() => _controller.Update(invalidRequest));
            Assert.Equal("Password should be from 8 to 128 symbols", exception.Message);

            _mockService.Verify(service => service.Update(invalidRequest), Times.Once);
        }

        [Fact]
        public void Update_PasswordTooLong_ThrowsException()
        {
            var invalidRequest = new CreatorRequestTo
            {
                Id = 1,
                Login = "validlogin",
                Password = new string('p', 129),
                FirstName = "Иван",
                LastName = "Иванов"
            };

            _mockService
                .Setup(service => service.Update(invalidRequest))
                .Throws(new Exception("Password should be from 8 to 128 symbols"));

            var exception = Assert.Throws<Exception>(() => _controller.Update(invalidRequest));
            Assert.Equal("Password should be from 8 to 128 symbols", exception.Message);

            _mockService.Verify(service => service.Update(invalidRequest), Times.Once);
        }

        [Fact]
        public void Update_FirstNameTooShort_ThrowsException()
        {
            var invalidRequest = new CreatorRequestTo
            {
                Id = 1,
                Login = "validlogin",
                Password = "password123",
                FirstName = "I",
                LastName = "Иванов"
            };

            _mockService
                .Setup(service => service.Update(invalidRequest))
                .Throws(new Exception("FirstName should be from 2 to 64 symbols"));

            var exception = Assert.Throws<Exception>(() => _controller.Update(invalidRequest));
            Assert.Equal("FirstName should be from 2 to 64 symbols", exception.Message);

            _mockService.Verify(service => service.Update(invalidRequest), Times.Once);
        }

        [Fact]
        public void Update_FirstNameTooLong_ThrowsException()
        {
            var invalidRequest = new CreatorRequestTo
            {
                Id = 1,
                Login = "validlogin",
                Password = "password123",
                FirstName = new string('I', 65),
                LastName = "Иванов"
            };

            _mockService
                .Setup(service => service.Update(invalidRequest))
                .Throws(new Exception("FirstName should be from 2 to 64 symbols"));

            var exception = Assert.Throws<Exception>(() => _controller.Update(invalidRequest));
            Assert.Equal("FirstName should be from 2 to 64 symbols", exception.Message);

            _mockService.Verify(service => service.Update(invalidRequest), Times.Once);
        }

        [Fact]
        public void Update_LastNameTooShort_ThrowsException()
        {
            var invalidRequest = new CreatorRequestTo
            {
                Id = 1,
                Login = "validlogin",
                Password = "password123",
                FirstName = "Иван",
                LastName = "P"
            };

            _mockService
                .Setup(service => service.Update(invalidRequest))
                .Throws(new Exception("LastName should be from 2 to 64 symbols"));

            var exception = Assert.Throws<Exception>(() => _controller.Update(invalidRequest));
            Assert.Equal("LastName should be from 2 to 64 symbols", exception.Message);

            _mockService.Verify(service => service.Update(invalidRequest), Times.Once);
        }

        [Fact]
        public void Update_LastNameTooLong_ThrowsException()
        {
            var invalidRequest = new CreatorRequestTo
            {
                Id = 1,
                Login = "validlogin",
                Password = "password123",
                FirstName = "Иван",
                LastName = new string('P', 65)
            };

            _mockService
                .Setup(service => service.Update(invalidRequest))
                .Throws(new Exception("LastName should be from 2 to 64 symbols"));

            var exception = Assert.Throws<Exception>(() => _controller.Update(invalidRequest));
            Assert.Equal("LastName should be from 2 to 64 symbols", exception.Message);

            _mockService.Verify(service => service.Update(invalidRequest), Times.Once);
        }

        #endregion

        #region DELETE Tests - Error Codes

        [Fact]
        public void Delete_ExistingId_ReturnsNoContent()
        {
            var existingId = 1;

            var result = _controller.Delete(existingId);

            Assert.IsType<NoContentResult>(result);
            Assert.Equal(204, (result as NoContentResult)?.StatusCode);

            _mockService.Verify(service => service.DeleteById(existingId), Times.Once);
        }

        [Fact]
        public void Delete_NonExistingId_Returns404NotFound()
        {
            var nonExistingId = 999;

            var result = _controller.Delete(nonExistingId);

            Assert.IsType<NotFoundResult>(result);
            Assert.Equal(404, (result as NotFoundResult)?.StatusCode);

            _mockService.Verify(service => service.DeleteById(nonExistingId), Times.Once);
        }

        #endregion
    }
}