namespace costs.net.plugins.tests.PG.Services.PurchaseOrder.PurchaseOrderService
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using core.Events.Cost;
    using core.Models.Payments;
    using dataAccess.Entity;
    using FluentAssertions;
    using Moq;
    using NUnit.Framework;
    using plugins.PG.Form;
    using plugins.PG.Models;
    using plugins.PG.Models.PurchaseOrder;
    using plugins.PG.Models.Stage;

    [TestFixture]
    public class PgPurchaseOrderServiceTests : PgPurchaseOrderServiceTestsBase
    {        
        [Test]
        public async Task GetPurchaseOrder_always_shouldReturnPgPurchaseOrder()
        {
            // Arrange
            var costSubmitted = GetCostRevisionStatusChanged(CostStageRevisionStatus.PendingBrandApproval);
            var costId = costSubmitted.AggregateId;
            SetupPurchaseOrderView(costId);

            // Act
            var purchase = await PgPurchaseOrderService.GetPurchaseOrder(costSubmitted);

            // Assert
            purchase.Should().BeOfType<PgPurchaseOrder>();
        }

        [Test]
        public async Task GetPurchaseOrder_always_deliveryDateShouldBeEventDatePlus3Months()
        {
            // Arrange
            var costSubmitted = GetCostRevisionStatusChanged(CostStageRevisionStatus.PendingBrandApproval);
            var costId = costSubmitted.AggregateId;
            SetupPurchaseOrderView(costId);

            // Act
            var purchase = await PgPurchaseOrderService.GetPurchaseOrder(costSubmitted);

            // Assert
            purchase.DeliveryDate.Should().Be(costSubmitted.TimeStamp.AddMonths(3));
        }

        [Test]
        public async Task GetPurchaseOrder_always_mapBasketNameFromCorrespondingFields()
        {
            // Arrange
            const string description = "Cost description";
            const string brandName = "PG";
            const string costNumber = "123812938102938";
            var stageDetails = new Dictionary<string, dynamic>
            {
                { "description", description },
                {"agencyCurrency", "USD" }
            };

            var costSubmitted = GetCostRevisionStatusChanged(CostStageRevisionStatus.PendingBrandApproval);
            var costId = costSubmitted.AggregateId;

            var purchaseView = SetupPurchaseOrderView(costId, stageDetails, brandName: brandName);
            purchaseView.CostNumber = costNumber;

            var expected = $"ADCOST{costNumber}{brandName}:{description}";

            // Act
            var purchase = await PgPurchaseOrderService.GetPurchaseOrder(costSubmitted);

            // Assert
            purchase.BasketName.Should().Be(expected);
        }

        [Test]
        public async Task GetPurchaseOrder_whenBasketNamecontainsAmp_itShouldBeReplacedWithAnd()
        {
            // Arrange
            const string description = "Cost description";
            const string brandName = "P&G";
            const string costNumber = "1238129";
            var stageDetails = new Dictionary<string, dynamic> {
                { "description", description },
                {"agencyCurrency", "USD" }
            };

            var costSubmitted = GetCostRevisionStatusChanged(CostStageRevisionStatus.PendingBrandApproval);
            var costId = costSubmitted.AggregateId;

            var purchaseView = SetupPurchaseOrderView(costId, stageDetails, brandName: brandName);
            purchaseView.CostNumber = costNumber;

            var expected = $"ADCOST{costNumber}{brandName}:{description}".Replace("&", "and");

            // Act
            var purchase = await PgPurchaseOrderService.GetPurchaseOrder(costSubmitted);

            // Assert
            purchase.BasketName.Should().Be(expected);
        }

        [Test]
        public void GetPurchaseOrder_always_shouldBuildPurchse()
        {
            // Arrange

            // Act
            var purchase = PgPurchaseOrderService.GetPurchaseOrder(It.IsAny<CostStageRevisionStatusChanged>());

            // Assert
            purchase.Should().NotBeNull();
        }

        [Test]
        public async Task GetPurchaseOrder_always_startDateShouldBeEventDate()
        {
            // Arrange
            var costSubmitted = GetCostRevisionStatusChanged(CostStageRevisionStatus.PendingBrandApproval);
            var costId = costSubmitted.AggregateId;
            SetupPurchaseOrderView(costId);

            // Act
            var purchase = await PgPurchaseOrderService.GetPurchaseOrder(costSubmitted);

            // Assert
            purchase.StartDate.Should().Be(costSubmitted.TimeStamp);
        }

        [Test]
        public async Task GetPurchaseOrder_costNumber_Mapping()
        {
            // Arrange
            const string costNumber = "456456456";
            var costSubmitted = GetCostRevisionStatusChanged(CostStageRevisionStatus.PendingBrandApproval);
            var costId = costSubmitted.AggregateId;
            var purchaseView = SetupPurchaseOrderView(costId);
            purchaseView.CostNumber = costNumber;

            // Act
            var purchase = await PgPurchaseOrderService.GetPurchaseOrder(costSubmitted);

            // Assert
            purchase.CostNumber.Should().Be(costNumber);
        }

        [Test]
        public async Task GetPurchaseOrder_description_mapping()
        {
            // Arrange
            const string contentTypeKey = "Video Key";
            const string contentTypeValue = "Video Value";
            const string costNumber = "423489273";
            const string brandName = "PG";
            var budgetRegion = new AbstractTypeValue { Key = "Budget Region", Name = "Budget Region Name" };

            var costSubmitted = GetCostRevisionStatusChanged(CostStageRevisionStatus.PendingBrandApproval);
            var costId = costSubmitted.AggregateId;

            var purchaseView = SetupPurchaseOrderView(costId, new Dictionary<string, dynamic>
            {
                { "contentType", new { id = Guid.NewGuid(), key = contentTypeKey, value = contentTypeValue } },
                { "budgetRegion", budgetRegion }
            },
            brandName: brandName);

            purchaseView.CostNumber = costNumber;

            var expected = $"{contentTypeValue}/{budgetRegion.Name} {costNumber}";

            // Act
            var purchase = await PgPurchaseOrderService.GetPurchaseOrder(costSubmitted);

            // Assert
            purchase.Description.Should().Be(expected);
        }

        [Test]
        public async Task GetPurchaseOrder_GL_Mapping()
        {
            // Arrange
            var costSubmitted = GetCostRevisionStatusChanged(CostStageRevisionStatus.PendingBrandApproval);
            var costId = costSubmitted.AggregateId;
            const string contentType = "Video";
            const string productionType = "Full Production";
            var generalLedgerCode = "33500001";

            SetupPurchaseOrderView(costId, 
                new Dictionary<string, dynamic>
                {
                    { "contentType", new { id = Guid.NewGuid(), value = contentType } },
                    { "productionType", new { id = Guid.NewGuid(), value = productionType } }
                },
                new Dictionary<string, dynamic>
                {
                    { "postProductionDirectBilling", "false" }
                }
            );
            _ledgerMaterialCodeServiceMock.Setup(c => c.GetLedgerMaterialCodes(It.IsAny<Guid>())).ReturnsAsync(new PgLedgerMaterialCodeModel
            {
                GlCode = generalLedgerCode
            });

            // Act
            var purchase = await PgPurchaseOrderService.GetPurchaseOrder(costSubmitted);

            // Assert
            purchase.GL.Should().Be(generalLedgerCode);
        }

        [Test]
        public async Task GetPurchaseOrder_ioNumber_Mapping()
        {
            // Arrange
            const string ioNumber = "234728934sdf";
            const string contentType = "Video";
            const string productionType = "Full Production";
            var costSubmitted = GetCostRevisionStatusChanged(CostStageRevisionStatus.PendingBrandApproval);
            var costId = costSubmitted.AggregateId;

            var paymentDetails = new PgPaymentDetails
            {
                IoNumber = ioNumber
            };

            _customDataServiceMock.Setup(c => c.GetCustomData<PgPaymentDetails>(It.IsAny<Guid>(), CustomObjectDataKeys.PgPaymentDetails))
                .ReturnsAsync(paymentDetails);

            SetupPurchaseOrderView(costId, 
                new Dictionary<string, dynamic>
                {
                    { "contentType", new { id = Guid.NewGuid(), value = contentType } },
                    { "productionType", new { id = Guid.NewGuid(), value = productionType } }
                }
            );

            // Act
            var purchase = await PgPurchaseOrderService.GetPurchaseOrder(costSubmitted);

            // Assert
            purchase.IONumber.Should().Be($"00{ioNumber}");
        }

        [Test] //TODO fix this stage so that currency dependent 
        public async Task GetPurchaseOrder_quantity_mapping()
        {
            // Arrange
            const string costNumber = "423489273";
            const string brandName = "PG";
            var costSubmitted = GetCostRevisionStatusChanged(CostStageRevisionStatus.PendingBrandApproval);
            var costId = costSubmitted.AggregateId;
            var stageDetails = new Dictionary<string, dynamic> { { "AgencyCurrency", "USD" } };
            var productionDetails = new Dictionary<string, dynamic> { { "DirectPaymentVendor", new PgProductionDetailsForm.Vendor { CurrencyId = _usdId } } };

            SetupPurchaseOrderView(costId, brandName: brandName, costNumber: costNumber, stageDetails: stageDetails, productionDetailsData: productionDetails);

            _paymentServiceMock.Setup(x => x.GetPaymentAmount(It.IsAny<Guid>(), false)).ReturnsAsync(new PaymentAmountResult { TotalCostAmount = 1000m });
            

            var expected = 1000;

            // Act
            var purchase = await PgPurchaseOrderService.GetPurchaseOrder(costSubmitted);

            // Assert
            purchase.TotalAmount.Should().Be(expected);
        }

        [Test]
        public async Task GetPurchaseOrder_tnumber_Mapping()
        {
            // Arrange
            const string tnumber = "CD5141"; 
            var costSubmitted = GetCostRevisionStatusChanged(CostStageRevisionStatus.PendingBrandApproval);
            var costId = costSubmitted.AggregateId;
            SetupPurchaseOrderView(costId, tNumber: tnumber);

            // Act
            var purchase = await PgPurchaseOrderService.GetPurchaseOrder(costSubmitted);

            // Assert
            purchase.TNumber.Should().Be(tnumber);
        }

        [Test]
        public async Task GetPurchaseOrder_requisitionerEmail_Mapping()
        {
            // Arrange
            const string email = "test@test.email.com";
            var costSubmitted = GetCostRevisionStatusChanged(CostStageRevisionStatus.PendingBrandApproval);
            var costId = costSubmitted.AggregateId;
            SetupPurchaseOrderView(costId, requisitionerEmail: email);

            // Act
            var purchase = await PgPurchaseOrderService.GetPurchaseOrder(costSubmitted);

            // Assert
            purchase.RequisitionerEmail.Should().Be(email);
        }

        [Test]
        public async Task GetPurchaseOrder_whenLengthOfBasketNameMoreThan40_shouldTruncateIt()
        {
            // Arrange
            const string description = "Cost description bla bla bla bla bla bla bla bla bla bla bla bla";
            const string brandName = "PG";
            const string costNumber = "123812938102938";
            const byte maximumLength = 40;
            var stageDetails = new Dictionary<string, dynamic> { { "description", description } };

            var costSubmitted = GetCostRevisionStatusChanged(CostStageRevisionStatus.PendingBrandApproval);
            var costId = costSubmitted.AggregateId;

            SetupPurchaseOrderView(costId, stageDetails, brandName: brandName, costNumber: costNumber);

            // Act
            var purchase = await PgPurchaseOrderService.GetPurchaseOrder(costSubmitted);

            // Assert
            purchase.BasketName.Should().HaveLength(maximumLength);
        }

        [Test]
        public async Task GetPurchaseOrder_whenLengthOfDescriptionMoreThan50_shouldTruncateIt()
        {
            const string contentType = "Video bla bla bla bla bla bla bla bla bla bla bla bla";
            const string costNumber = "Cost number 423489273 is long as well";
            const string brandName = "PG";
            const string budgetRegionName = "Budget Region bla bla bla bla bla bla bla bla bla bla bla bla";
            var budgetRegion = new AbstractTypeValue { Key = budgetRegionName, Name = budgetRegionName };
            const byte maximumLength = 50;

            var costSubmitted = GetCostRevisionStatusChanged(CostStageRevisionStatus.PendingBrandApproval);
            var costId = costSubmitted.AggregateId;

            SetupPurchaseOrderView(costId, new Dictionary<string, dynamic>
                {
                    { "contentType", new { id = Guid.NewGuid(), value = contentType } },
                    { "budgetRegion", budgetRegion }
                },
                brandName: brandName,
                costNumber: costNumber
                );

            // Act
            var purchase = await PgPurchaseOrderService.GetPurchaseOrder(costSubmitted);

            // Assert
            purchase.Description.Should().HaveLength(maximumLength);
        }

        [Test]
        public async Task GetPurchaseOrder_whenCostSubmitted_LongText_mapping()
        {
            // Arrange
            var costSubmitted = GetCostRevisionStatusChanged(CostStageRevisionStatus.PendingBrandApproval);
            var costId = costSubmitted.AggregateId;
            const CostStages costStage = CostStages.FirstPresentation;
            const string productionType = "Full Production";

            const string expectedVNLine1 = "Purchase order does not authorize committing funds without approved EPCAT sheet.";
            const string expectedVNLine2 = "The services within this Purchase Order can only be ordered from 3rd parties after EPCAT approval.";
            var expectedBN = $"{costStage} APPROVED Production {productionType}";
            var expectedAN = $"{FrontEndUrl.TrimEnd('/')}/#/cost/{costId}/review";

            SetupPurchaseOrderView(costId, new Dictionary<string, dynamic>
                {
                    { "productionType", new { id = Guid.NewGuid(), key = productionType, value = productionType } },
                    { "costType", CostType.Production.ToString() }
                },
                costStage: costStage
                );

            // Act
            var purchase = await PgPurchaseOrderService.GetPurchaseOrder(costSubmitted);

            // Assert
            purchase.LongText.Should().NotBeNull();
            purchase.LongText.VN.Should().Contain(expectedVNLine1);
            purchase.LongText.VN.Should().Contain(expectedVNLine2);
            purchase.LongText.BN.Should().Contain(expectedBN);
            purchase.LongText.AN.Should().Contain(expectedAN);
        }

        [Test]
        public async Task GetPurchaseOrder_whenCostSubmittedAndNonProductionCostType_LongText_mapping()
        {
            // Arrange
            var costSubmitted = GetCostRevisionStatusChanged(CostStageRevisionStatus.PendingBrandApproval);
            var costId = costSubmitted.AggregateId;
            const CostStages costStage = CostStages.FirstPresentation;
            const CostType costType = CostType.Buyout;

            const string expectedVNLine1 = "Purchase order does not authorize committing funds without approved EPCAT sheet.";
            const string expectedVNLine2 = "The services within this Purchase Order can only be ordered from 3rd parties after EPCAT approval.";
            var expectedBN = $"{costStage} APPROVED {costType}";
            var expectedAN = $"{FrontEndUrl.TrimEnd('/')}/#/cost/{costId}/review";

            SetupPurchaseOrderView(costId, 
                costStage: costStage,
                costType: costType
            );

            // Act
            var purchase = await PgPurchaseOrderService.GetPurchaseOrder(costSubmitted);

            // Assert
            purchase.LongText.Should().NotBeNull();
            purchase.LongText.VN.Should().Contain(expectedVNLine1);
            purchase.LongText.VN.Should().Contain(expectedVNLine2);
            purchase.LongText.BN.Should().Contain(expectedBN);
            purchase.LongText.AN.Should().Contain(expectedAN);
        }

        [Test]
        [TestCase(0)]
        [TestCase(100.0)]
        public async Task GetPurchaseOrder_whenSubmittedAtFinalActualAndNotCreditNote_LongText_mapping(decimal creditAmount)
        {
            // Arrange
            var costSubmitted = GetCostRevisionStatusChanged(CostStageRevisionStatus.PendingBrandApproval);
            var costId = costSubmitted.AggregateId;
            const CostStages costStage = CostStages.FinalActual;
            const string productionType = "Full Production";
            const string poNumber = "P&G PO_NUMBER";
            _paymentServiceMock.Setup(p => p.GetPaymentAmount(costSubmitted.CostStageRevisionId, false))
                .ReturnsAsync(new PaymentAmountResult
                {
                    TotalCostAmountPayment = creditAmount
                });

            var expectedBN1 = $"{costStage} APPROVED Production {productionType} {poNumber}";

            SetupPurchaseOrderView(costId,
                new Dictionary<string, dynamic>
                {
                    { "productionType", new { id = Guid.NewGuid(), key = productionType, value = productionType } },
                    { "costType", CostType.Production.ToString() }
                },
                costStage: costStage
            );


            SetupCustomObjectData<PgPurchaseOrderResponse>(CustomObjectDataKeys.PgPurchaseOrderResponse, new Dictionary<string, dynamic> { { "poNumber", poNumber } });

            // Act
            var purchase = await PgPurchaseOrderService.GetPurchaseOrder(costSubmitted);

            // Assert
            purchase.LongText.Should().NotBeNull();
            purchase.LongText.BN.Should().HaveCount(1);
            purchase.LongText.BN.Should().Contain(expectedBN1);
        }

        [Test]
        [TestCase(-100.0)]
        public async Task GetPurchaseOrder_whenSubmittedAtFinalActualAndCreditNote_LongText_mapping(decimal creditAmount)
        {
            // Arrange
            var costSubmitted = GetCostRevisionStatusChanged(CostStageRevisionStatus.Approved);
            var costId = costSubmitted.AggregateId;
            const CostStages costStage = CostStages.FinalActual;
            const string productionType = "Full Production";
            const string poNumber = "P&G PO_NUMBER";
            _paymentServiceMock.Setup(p => p.GetPaymentAmount(costSubmitted.CostStageRevisionId, false))
                .ReturnsAsync(new PaymentAmountResult
                {
                    TotalCostAmountPayment = creditAmount
                });

            var expectedBN2 = $"A credit note of {creditAmount} USD is needed, please update PO accordingly.";

            SetupPurchaseOrderView(costId, 
                new Dictionary<string, dynamic> {
                    { "productionType", new { id = Guid.NewGuid(), key = productionType, value = productionType } },
                    { "costType", CostType.Production.ToString() }},
                costStage: costStage
            );

            SetupCustomObjectData<PgPurchaseOrderResponse>(CustomObjectDataKeys.PgPurchaseOrderResponse, new Dictionary <string, dynamic> { { "poNumber", poNumber } });

            // Act
            var purchase = await PgPurchaseOrderService.GetPurchaseOrder(costSubmitted);

            // Assert
            purchase.LongText.Should().NotBeNull();
            purchase.LongText.BN.Should().HaveCount(1);
            purchase.LongText.BN.Should().Contain(expectedBN2);
        }

        [Test]
        public async Task GetPurchaseOrder_whenCostCancelled_LongText_mapping()
        {
            // Arrange
            var costSubmitted = GetCostRevisionStatusChanged(CostStageRevisionStatus.PendingCancellation);
            var costId = costSubmitted.AggregateId;
            const string productionType = "Full Production";
            const string poNumber = "P&G PO_NUMBER";
            
            const string expectedVN = "PROJECT CANCELLED. PLEASE CANCEL PO AND REQUEST CN FOR ANY AMOUNTS PAID";
            var expectedBN = $"PROJECT CANCELLED. PLEASE CANCEL PO {poNumber} AND REQUEST CN FOR ANY AMOUNTS PAID";

            SetupPurchaseOrderView(costId, new Dictionary<string, dynamic> { { "productionType", new { id = Guid.NewGuid(), value = productionType } } });
            SetupCustomObjectData<PgPurchaseOrderResponse>(CustomObjectDataKeys.PgPurchaseOrderResponse, new Dictionary<string, dynamic> { { "poNumber", poNumber } });

            // Act
            var purchase = await PgPurchaseOrderService.GetPurchaseOrder(costSubmitted);

            // Assert
            purchase.LongText.Should().NotBeNull();
            purchase.LongText.VN.Should().Contain(expectedVN);
            purchase.LongText.BN.Should().Contain(expectedBN);
        }


        [Test]
        public async Task GetPurchaseOrder_whenVendorDirectPayment_shouldReturnVendorCurrency()
        {
            // Arrange
            var costSubmitted = GetCostRevisionStatusChanged(CostStageRevisionStatus.PendingBrandApproval);
            var costId = costSubmitted.AggregateId;
            var vendorCurrency = "EUR";
            var vendorCurrencyId = _eurId;
            var agencyCurrency = "USD";

            SetupCurrencies(vendorCurrency);

            SetupPurchaseOrderView(costId, 
                new Dictionary<string, dynamic>
                {
                    { "agencyCurrency", agencyCurrency },
                }, 
                new Dictionary<string, dynamic>
                {
                    { "directPaymentVendor", new { currencyId = vendorCurrencyId }}
                });

            // Act
            var purchaseOrder = await PgPurchaseOrderService.GetPurchaseOrder(costSubmitted);

            // Assert
            purchaseOrder.Currency.Should().Be(vendorCurrency);
        }

        [Test]
        public async Task GetPurchaseOrder_whenNotVendorDirectPayment_shouldReturnAgencyCurrency()
        {
            // Arrange
            var costSubmitted = GetCostRevisionStatusChanged(CostStageRevisionStatus.PendingBrandApproval);
            var costId = costSubmitted.AggregateId;
            var agencyCurrency = "USD";

            SetupCurrencies();

            SetupPurchaseOrderView(costId,
                new Dictionary<string, dynamic>
                {
                    { "agencyCurrency", agencyCurrency }
                }, 
                new Dictionary<string, dynamic>
                {
                    { "directPaymentVendor", null}
                });

            // Act
            var purchaseOrder = await PgPurchaseOrderService.GetPurchaseOrder(costSubmitted);

            // Assert
            purchaseOrder.Currency.Should().Be(agencyCurrency);
        }

        [Test]
        public async Task GetPurchaseOrder_whenNotVendorDirectPayment_shouldReturnAgencySAPId()
        {
            // Arrange
            var costSubmitted = GetCostRevisionStatusChanged(CostStageRevisionStatus.PendingBrandApproval);
            var costId = costSubmitted.AggregateId;
            const string agencySapVendorId = "1238712638172";

            SetupPurchaseOrderView(costId, agencyLabels: new[] { $"PGSAPVENDORID_{agencySapVendorId}" });

            // Act
            var purchaseOrder = await PgPurchaseOrderService.GetPurchaseOrder(costSubmitted);

            // Assert
            purchaseOrder.Vendor.Should().Be(agencySapVendorId);
        }

        [Test]
        public async Task GetPurchaseOrder_whenVendorDirectPayment_shouldReturnVendorSapCode()
        {
            // Arrange
            var costSubmitted = GetCostRevisionStatusChanged(CostStageRevisionStatus.PendingBrandApproval);
            var costId = costSubmitted.AggregateId;
            const string sapVendorCode = "1238712638172";

            SetupCurrencies("EUR");
            SetupPurchaseOrderView(costId, null, new Dictionary<string, dynamic>
            {
                { "directPaymentVendor", new { sapVendorCode, eurId = _eurId } }
            });

            // Act
            var purchaseOrder = await PgPurchaseOrderService.GetPurchaseOrder(costSubmitted);

            // Assert
            purchaseOrder.Vendor.Should().Be(sapVendorCode);
        }

        [Test]
        public async Task GetPurchaseOrder_whenNoLedgerMaterialCodes_shouldReturnEmptyCategoryIdAndGLCode()
        {
            // Arrange
            var costSubmitted = GetCostRevisionStatusChanged(CostStageRevisionStatus.PendingBrandApproval);
            var costId = costSubmitted.AggregateId;
            SetupPurchaseOrderView(costId);
            _ledgerMaterialCodeServiceMock.Setup(s => s.GetLedgerMaterialCodes(It.IsAny<Guid>()))
                .ReturnsAsync((PgLedgerMaterialCodeModel)null);

            // Act
            var purchase = await PgPurchaseOrderService.GetPurchaseOrder(costSubmitted);

            // Assert
            purchase.Should().NotBeNull();
            purchase.CategoryId.Should().BeEmpty();
            purchase.GL.Should().BeEmpty();
        }
    }
}
