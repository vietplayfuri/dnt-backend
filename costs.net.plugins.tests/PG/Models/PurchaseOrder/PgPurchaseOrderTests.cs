namespace costs.net.plugins.tests.PG.Models.PurchaseOrder
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using FluentAssertions;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using Newtonsoft.Json.Schema;
    using NUnit.Framework;
    using plugins.PG.Models.PurchaseOrder;

    [TestFixture]
    public class PgPurchaseOrderTests : SchemaTestsBase
    {
        private string PgPurchaseOrderSchemaPath = $"PG{Path.DirectorySeparatorChar}Schemas{Path.DirectorySeparatorChar}PgPurchaseOrder.json";
        private JSchema _schema;

        [OneTimeSetUp]
        public void Init()
        {
            _schema = JSchema.Parse(File.ReadAllText(GetPath(PgPurchaseOrderSchemaPath)));
        }

        [Test]
        public void PgPurchaseOrder_schema_validation()
        {
            // Arrange
            var pgPurchaseOrder = new PgPurchaseOrder
            {
                TotalAmount = 123,
                CostNumber = "TestCostNumber1",
                PaymentAmount = 111,
                AccountCode = "TestAccountCode",
                BasketName = "TestBasketName",
                CategoryId = "MGCCode",
                Currency = "CurrencyName",
                DeliveryDate = DateTime.Parse("2017-08-09T11:26:45.345Z"),
                Description = "TestDescription",
                GL = "GLCode",
                GrNumbers = new[] { "Number1" },
                IONumber = "IONumber",
                ItemIdCode = "ItemIdCode",
                LongText = new PgPurchaseOrder.LongTextField
                {
                    AN = new List<string> { "AN1" },
                    BN = new List<string> { "BN1" },
                    VN = new List<string> { "VN1" }
                },
                TNumber = "TNUmber",
                RequisitionerEmail = "RequisitionerEmail",
                PoNumber = "PONumber",
                StartDate = DateTime.Parse("2017-08-30T11:26:45.345Z"),
                Vendor = "VendorCode",
                Commodity = "CostTypeBuyoutTypeCombined"
            };
            var serialized = JsonConvert.SerializeObject(pgPurchaseOrder, Formatting.None, SerializerSettings);

            // Act
            var jobject = JObject.Parse(serialized);

            // Assert
            jobject.IsValid(_schema, out IList<string> errors).Should().BeTrue(GetSchemaValidationAssertionReason(errors));
        }
    }
}