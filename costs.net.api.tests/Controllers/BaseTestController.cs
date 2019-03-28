namespace costs.net.api.tests
{
    using System;
    using System.Collections.Generic;
    using core.Models;
    using core.Models.User;
    using core.Services;
    using core.Services.AdvancedSearch;
    using core.Services.Costs;
    using core.Services.Search;
    using Controllers.AdvancedSearch;
    using Controllers.Watchers;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.Abstractions;
    using Microsoft.AspNetCore.Mvc.Filters;
    using Microsoft.AspNetCore.Routing;
    using Moq;
    using NUnit.Framework;

    public class BaseTestController
    {
        private Mock<ICostService> _costServiceMock;
        private Mock<IPermissionService> _permissionServiceMock;
        protected Mock<IUserSearchService> UserSearchServiceMock;
        protected Mock<IAdvancedSearchService> AdvancedSearchServiceMock;
        protected WatchersController WatchersController;
        protected AdvancedSearchController AdvancedSearchController;
        protected UserIdentity User;

        [SetUp]
        public void Init()
        {
            _costServiceMock = new Mock<ICostService>();
            UserSearchServiceMock = new Mock<IUserSearchService>();
            _permissionServiceMock = new Mock<IPermissionService>();
            AdvancedSearchServiceMock = new Mock<IAdvancedSearchService>(
                );
            _permissionServiceMock.Setup(a => a.GetObjectUserGroups(It.IsAny<Guid>(), It.IsAny<Guid?>()))
                .ReturnsAsync(new[]
                {
                    Guid.NewGuid().ToString()
                });
            var actionDescriptor = new Mock<ActionDescriptor>();
            actionDescriptor.SetupGet(x => x.DisplayName).Returns("Action_With_SomeAttribute");
            User = new UserIdentity
            {
                AgencyId = Guid.NewGuid(),
                BuType = BuType.Pg,
                Email = "email",
                FirstName = "Bruce",
                FullName = "Bat Man",
                LastName = "Wayne",
                GdamUserId = "gdamID",
                Id = Guid.NewGuid(),
                ModuleId = Guid.NewGuid()
            };
            WatchersController = SetupWatcherController(actionDescriptor.Object);
            AdvancedSearchController = SetupAdvancedSearchController(actionDescriptor.Object);
        }

        private AdvancedSearchController SetupAdvancedSearchController(ActionDescriptor actionDescriptor)
        {
            AdvancedSearchController =
                new AdvancedSearchController(_permissionServiceMock.Object, AdvancedSearchServiceMock.Object)
                {
                    ControllerContext = new ControllerContext()
                };
            AdvancedSearchController.ControllerContext.HttpContext = new DefaultHttpContext();
            AdvancedSearchController.HttpContext.Request.Headers["X-Forwarded-For"] = "127.0.0.1";
            AdvancedSearchController.HttpContext.Items["user"] = User;
            var httpActionContext = new ActionExecutingContext(
                new ActionContext
                {
                    HttpContext = AdvancedSearchController.HttpContext,
                    RouteData = new RouteData(),
                    ActionDescriptor = actionDescriptor
                }, new Mock<List<IFilterMetadata>>().Object,
                new Mock<IDictionary<string, object>>().Object,
                AdvancedSearchController);
            AdvancedSearchController.OnActionExecuting(httpActionContext);
            return AdvancedSearchController;
        }

        private WatchersController SetupWatcherController(ActionDescriptor actionDescriptor)
        {
            WatchersController =
                new WatchersController(_costServiceMock.Object, UserSearchServiceMock.Object, _permissionServiceMock.Object)
                {
                    ControllerContext = new ControllerContext()
                };
            WatchersController.ControllerContext.HttpContext = new DefaultHttpContext();

            WatchersController.HttpContext.Request.Headers["X-Forwarded-For"] = "127.0.0.1";
            WatchersController.HttpContext.Items["user"] = User;
            var httpActionContext = new ActionExecutingContext(
                new ActionContext
                {
                    HttpContext = WatchersController.HttpContext,
                    RouteData = new RouteData(),
                    ActionDescriptor = actionDescriptor
                }, new Mock<List<IFilterMetadata>>().Object,
                new Mock<IDictionary<string, object>>().Object,
                WatchersController);
            WatchersController.OnActionExecuting(httpActionContext);
            return WatchersController;
        }
    }
}