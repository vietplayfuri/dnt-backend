
namespace costs.net.plugins.tests.PG.Services.Budget
{
    using System.Collections.Generic;
    using core.Models.CostTemplate;

    public static class CostFormTestHelper
    {
        public static CostTemplateVersionModel CreateTemplateModel()
        {
            var productionDetailsCollection = new List<ProductionDetailsTemplateModel>();
            var templateModel = new CostTemplateVersionModel
            {
                ProductionDetails = productionDetailsCollection
            };

            var audioProduction = new ProductionDetailsFormDefinitionModel { ProductionType = Constants.ProductionType.FullProduction };
            var audioForms = new List<ProductionDetailsFormDefinitionModel> { audioProduction };
            var audio = new ProductionDetailsTemplateModel
            {
                Forms = audioForms,
                Type = Constants.ContentType.Audio
            };

            var digitalProduction = new ProductionDetailsFormDefinitionModel { ProductionType = Constants.ProductionType.FullProduction };
            var digitalForms = new List<ProductionDetailsFormDefinitionModel> { digitalProduction };
            var digital = new ProductionDetailsTemplateModel
            {
                Forms = digitalForms,
                Type = Constants.ContentType.Digital
            };

            var photographyProduction = new ProductionDetailsFormDefinitionModel { ProductionType = Constants.ProductionType.FullProduction };
            var photographyForms = new List<ProductionDetailsFormDefinitionModel> { photographyProduction };
            var photography = new ProductionDetailsTemplateModel
            {
                Forms = photographyForms,
                Type = Constants.ContentType.Photography
            };

            var videoProduction = new ProductionDetailsFormDefinitionModel { ProductionType = Constants.ProductionType.FullProduction };
            var videoPostProduction = new ProductionDetailsFormDefinitionModel { ProductionType = Constants.ProductionType.PostProductionOnly };
            var videoCgi = new ProductionDetailsFormDefinitionModel { ProductionType = Constants.ProductionType.CgiAnimation };
            var videoForms = new List<ProductionDetailsFormDefinitionModel>
            {
                videoProduction,
                videoPostProduction,
                videoCgi
            };
            var video = new ProductionDetailsTemplateModel
            {
                Forms = videoForms,
                Type = Constants.ContentType.Video
            };

            productionDetailsCollection.AddRange(new[] { audio, digital, photography, video });

            return templateModel;
        }
    }
}
