using BusinessLogic.DTO.Request;
using BusinessLogic.DTO.Response;
using BusinessLogic.Servicies;
using FakeItEasy;
using Microsoft.AspNetCore.Mvc;
using Presentation.Controllers;
using TestUnit.Asserts;
using TestUnit.Attributes;
using TestUnit.Attributes.ClassAttributes;
using TestUnit.Attributes.MethodAttributes.SetupCleanup;
using TestUnit.Attributes.MethodAttributes.Fact;
using TestUnit.Attributes.MethodAttributes.Theory;

namespace WebApp.Tests.Controllers
{
    [TestClass]
    [TestTrait("DisplayName", "MarksController")]
    [TestTrait("Priority", "P1")]
    public class MarksControllerTests
    {
        private IBaseService<MarkRequestTo, MarkResponseTo> _fakeService;
        private MarksController _controller;
        private Dictionary<int, MarkResponseTo> _testData = new();

        public MarksControllerTests()
        {
            _fakeService = A.Fake<IBaseService<MarkRequestTo, MarkResponseTo>>();
            _controller = new MarksController(_fakeService);
        }

        [TestSetup]
        public void MarksControllerSetup()
        {
            _testData.Add(1, new MarkResponseTo
            {
                Id = 1,
                Name = "good"
            });
            _testData.Add(2, new MarkResponseTo
            {
                Id = 2,
                Name = "bad"
            });
            _testData.Add(3, new MarkResponseTo
            {

                Id = 3,
                Name = "normal"
            });
        }

        [TestCleanup]
        public void MarksControllerCleanup()
        {
            _testData?.Clear();
        }

        [TheoryMethod]
        [TheoryInlineData(1)]
        [TheoryInlineData(2, 3)]
        [TestTrait("DisplayName", "Get mark by existing id")]
        [TestTrait("Priority", "P0")]
        public void MarksController_GetById_ExistingId_ReturnsOk(int id)
        {
            //arrange
            var expected = _testData[id];
            A.CallTo(() => _fakeService.GetById(id))
               .Returns(expected);

            //act
            var actionResult = _controller.GetById(id);
            var okResult = actionResult.Result as OkObjectResult;
            var actual = okResult?.Value as MarkResponseTo;

            //assert
            TestAssert.NotNull(okResult);
            TestAssert.Equal(200, okResult?.StatusCode);
            TestAssert.NotNull(actual);
            TestAssert.Equal(expected, actual);
        }
    }
}
