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
    public class PgPurchaseOrderResponseTests : SchemaTestsBase
    {
        private string PgPurchaseOrderSchemaPath = $"PG{Path.DirectorySeparatorChar}Schemas{Path.DirectorySeparatorChar}PgPurchaseOrderResponse.json";
        private JSchema _schema;

        [OneTimeSetUp]
        public void Init()
        {
            _schema = JSchema.Parse(File.ReadAllText(GetPath(PgPurchaseOrderSchemaPath)));
        }

        [Test]
        public void PgPurchaseOrderResponse_schema_validation()
        {
            // Arrange
            var pgPurchaseOrder = new PgPurchaseOrderResponse
            {
                AccountCode = "TestAccountCode",
                ItemIdCode = "ItemIdCode",
                PoNumber = "PONumber",
                Comments = "Comments",
                Type = "Type",
                GlAccount = "GLAccountCode",
                GrDate = DateTime.Parse("2017-08-09T11:26:45.345Z"),
                GrNumber = "GR number",
                ApproverEmail = "Email address of approver here",
                IoNumberOwner = "Internal order number owner",
                PoDate = DateTime.Parse("2017-08-30T11:26:45.345Z"),
                Requisition = "Requisition",
                ApprovalStatus = "Approved",
                TotalAmount = 123.45m
            };
            var serialized = JsonConvert.SerializeObject(pgPurchaseOrder, Formatting.None, SerializerSettings);

            // Act
            var jobject = JObject.Parse(serialized);

            // Assert
            jobject.IsValid(_schema, out IList<string> errors).Should().BeTrue(GetSchemaValidationAssertionReason(errors));
        }

        [Test]
        public void PgPurchaseOrderResponse_When_TotalAmountIsNull_Should_HaveValidSchema()
        {
            // Arrange
            var pgPurchaseOrder = new PgPurchaseOrderResponse
            {
                AccountCode = "TestAccountCode",
                ItemIdCode = "ItemIdCode",
                PoNumber = "PONumber",
                Comments = "Comments",
                Type = "Type",
                GlAccount = "GLAccountCode",
                GrDate = DateTime.Parse("2017-08-09T11:26:45.345Z"),
                GrNumber = "GR number",
                ApproverEmail = "Email address of approver here",
                IoNumberOwner = "Internal order number owner",
                PoDate = DateTime.Parse("2017-08-30T11:26:45.345Z"),
                Requisition = "Requisition",
                ApprovalStatus = "Approved",
                TotalAmount = null
            };
            var serialized = JsonConvert.SerializeObject(pgPurchaseOrder, Formatting.None, SerializerSettings);

            // Act
            var jobject = JObject.Parse(serialized);

            // Assert
            jobject.IsValid(_schema, out IList<string> errors).Should().BeTrue(GetSchemaValidationAssertionReason(errors));
        }

    }
}