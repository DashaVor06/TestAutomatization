using BusinessLogic.DTO.Request;
using BusinessLogic.DTO.Response;
using BusinessLogic.Servicies;
using FakeItEasy;
using Microsoft.AspNetCore.Mvc;
using Presentation.Controllers;
using TestUnit.Asserts;
using TestUnit.Attributes;
using TestUnit.Attributes.ClassAttributes;
using TestUnit.Attributes.ClassAttributes.SetupCleanup;
using TestUnit.Attributes.MethodAttributes.Fact;
using TestUnit.Attributes.MethodAttributes.Theory;


namespace WebApp.Tests.Controllers
{
    [TestClass]
    public class CreatorControllerTests
    {
        private readonly IBaseService<CreatorRequestTo, CreatorResponseTo> _fakeService;
        private readonly CreatorsController _controller;
        private static Dictionary<int, CreatorResponseTo> _testData = new();

        public CreatorControllerTests()
        {
            _fakeService = A.Fake<IBaseService<CreatorRequestTo, CreatorResponseTo>>();
            _controller = new CreatorsController(_fakeService);
        }

        [TestClassSetup]
        public void CreatorControllerSetup()
        {
            _testData.Clear();
            _testData.Add(1, new CreatorResponseTo
            {
                Id = 1,
                Login = "ivanov",
                Password = "pass123",
                FirstName = "Иван",
                LastName = "Иванов"
            });
            _testData.Add(2, new CreatorResponseTo
            {
                Id = 2,
                Login = "petrov",
                Password = "pass456",
                FirstName = "Петр",
                LastName = "Петров"
            });
            _testData.Add(3, new CreatorResponseTo
            {

                Id = 3,
                Login = "sidorov",
                Password = "pass789",
                FirstName = "Сидор",
                LastName = "Сидоров"
            });
        }

        [TestClassCleanup]
        public static void CreatorControllerCleanup() 
        {
            _testData?.Clear();
        }

        [TheoryMethod]
        [TheoryInlineData(1)]
        [TheoryInlineData(2)]
        [TestTrait("DisplayName", "Get creator by existing id")]
        [TestTrait("Priority", "P0")]
        public void CreatorController_GetById_ExistingId_ReturnsOk(int id)
        {
            //arrange
            var expected = _testData[id];
            A.CallTo(() => _fakeService.GetById(id))
               .Returns(expected);

            //act
            var actionResult = _controller.GetById(id); 
            var okResult = actionResult.Result as OkObjectResult;
            var actual = okResult?.Value as CreatorResponseTo;

            //assert
            TestAssert.NotNull(okResult);
            TestAssert.Equal(200, okResult?.StatusCode);
            TestAssert.NotNull(actual); 
            TestAssert.Equal(expected, actual); 
        }

        [TheoryMethod]
        [TheoryInlineData(999)]
        [TheoryInlineData(-1)]
        [TestTrait("DisplayName", "Get creator by not existing id")]
        [TestTrait("Priority", "P1")]
        public void CreatorController_GetById_NonExistingId_ReturnsNotFound(int id)
        {
            //arrange
            A.CallTo(() => _fakeService.GetById(id))
               .Returns((CreatorResponseTo)null!);

            //act
            var actionResult = _controller.GetById(id);

            //assert
            TestAssert.IsType<NotFoundResult>(actionResult.Result);
            var notFoundResult = actionResult.Result as NotFoundResult;
            TestAssert.NotNull(notFoundResult);                  
            TestAssert.Equal(404, notFoundResult?.StatusCode);      
            TestAssert.IsNotType<OkObjectResult>(actionResult.Result);
                  
            TestAssert.True(notFoundResult?.StatusCode == 404);           
            TestAssert.False(notFoundResult?.StatusCode == 200);     
            var okResult = actionResult.Result as OkObjectResult;
            TestAssert.Null(okResult);

            A.CallTo(() => _fakeService.GetById(id)).MustHaveHappenedOnceExactly(); 
        }

        [FactMethod]
        [TestTrait("DisplayName", "Get all creators")]
        [TestTrait("Priority", "P1")]
        public async Task CreatorController_GetAll_ReturnsListOfCreatorResponseTo()
        {
            //arrange
            var expectedList = _testData.Values.ToList();
            A.CallTo(() => _fakeService.GetAllAsync())
                .Returns(expectedList);

            //act
            var actionResult = await _controller.GetAllAsync();
            var okResult = actionResult.Result as OkObjectResult;
            var actualList = okResult?.Value as List<CreatorResponseTo>;

            //assert
            TestAssert.NotNull(okResult); 
            TestAssert.Equal(200, okResult?.StatusCode); 
            TestAssert.NotNull(actualList);   
            TestAssert.Equal(expectedList.Count, actualList?.Count);
            TestAssert.SequenceEqual(actualList, expectedList);

            TestAssert.True(actualList?.Count > 0);       
            TestAssert.False(actualList?.Count == 0);    
            TestAssert.NotEqual(actualList?.Count, 999); 
            TestAssert.IsType<List<CreatorResponseTo>>(actualList); 
            TestAssert.IsNotType<CreatorResponseTo>(actualList);

            A.CallTo(() => _fakeService.GetAllAsync()).MustHaveHappenedOnceExactly();
        }

        [FactMethod]
        [TestTrait("DisplayName", "Throw exception")]
        [TestTrait("Priority", "P2")]
        public void CreatorController_GetById_ServiceThrowsException_ThrowsException()
        {
            //arrange
            int id = 1;
            A.CallTo(() => _fakeService.GetById(id))
               .Throws(new InvalidOperationException("Database connection failed"));

            //act & assert
            TestAssert.Throws<InvalidOperationException>(() =>
            {
                _controller.GetById(id);
            });
        }
    }
}
