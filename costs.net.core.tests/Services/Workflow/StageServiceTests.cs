namespace costs.net.core.tests.Services.Workflow
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Builders;
    using Builders.Workflow;
    using core.Services.Workflow;
    using Castle.Components.DictionaryAdapter;
    using core.Models;
    using Moq;
    using NUnit.Framework;

    [TestFixture]
    public class StageServiceTests
    {
        private List<Lazy<IStageBuilder, PluginMetadata>> _stageBuilders;

        private StageService _service;

        [SetUp]
        public void Setup()
        {
            _stageBuilders = new EditableList<Lazy<IStageBuilder, PluginMetadata>>();

            _service = new StageService(_stageBuilders);
        }

        [Test]
        public async Task GetStages_always_shouldRetrieveStageFromBuilder()
        {
            // Arrange
            var stageBuilder = new Mock<IStageBuilder>();
            var costStageRevisionId = Guid.NewGuid();
            _stageBuilders.Add(new Lazy<IStageBuilder, PluginMetadata>(
                () => stageBuilder.Object, new PluginMetadata { BuType =  BuType.Pg }));

            // Act
            await _service.GetAllStages(BuType.Pg, costStageRevisionId);

            // Assert
            stageBuilder.Verify(b => b.GetStages(costStageRevisionId), Times.Once);
        }
    }
}