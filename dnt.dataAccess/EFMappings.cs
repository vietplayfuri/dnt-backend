namespace dnt.dataAccess
{
    using Entity;
    using Microsoft.EntityFrameworkCore;

    public static class EFMappings
    {
        public static void DefineMappings(ModelBuilder modelBuilder)
        {
            //modelBuilder.Entity<Dictionary>(e =>
            //{
            //    e.HasKey(u => u.Id);
            //    e.HasMany(x => x.DictionaryEntries).WithOne(x => x.Dictionary).HasForeignKey(f => f.DictionaryId);
            //    e.Property(x => x.Id).ValueGeneratedOnAdd().HasColumnName("id");
            //    e.ToTable("dictionary");
            //});


            //modelBuilder.Entity<DictionaryEntry>(e =>
            //{
            //    e.HasKey(u => u.Id);
            //    e.Property(x => x.Id).ValueGeneratedOnAdd().HasColumnName("id");
            //    e.ToTable("dictionary_entry");
            //});

            //modelBuilder.Entity<VendorCategory>(e =>
            //{
            //    e.HasKey(u => u.Id);
            //    e.HasOne(ve => ve.Currency).WithMany(x => x.VendorCategories).HasForeignKey(x => x.DefaultCurrencyId);
            //    e.Property(x => x.Id).ValueGeneratedOnAdd().HasColumnName("id");
            //    e.ToTable("vendor_category");
            //});

            //modelBuilder.Entity<Vendor>(e =>
            //{
            //    e.HasKey(u => u.Id);
            //    e.HasMany(x => x.Categories).WithOne(x => x.Vendor).HasForeignKey(f => f.VendorId);
            //    e.Property(x => x.Id).ValueGeneratedOnAdd().HasColumnName("id");
            //    e.ToTable("vendor");
            //});

            //modelBuilder.Entity<VendorRule>(e =>
            //{
            //    e.HasKey(u => u.Id);
            //    e.HasOne(ve => ve.VendorCategory).WithMany(x => x.VendorCategoryRules).HasForeignKey(x => x.VendorCategoryId);
            //    e.HasOne(ve => ve.Rule).WithMany(x => x.VendorRules).HasForeignKey(x => x.RuleId);

            //    e.Property(x => x.Id).ValueGeneratedOnAdd().HasColumnName("id");
            //    e.ToTable("vendor_rule");
            //});

            //modelBuilder.Entity<VendorCategory>(e =>
            //{
            //    e.HasKey(u => u.Id);
            //    e.Property(x => x.Id).ValueGeneratedOnAdd().HasColumnName("id");
            //    e.ToTable("vendor_category");
            //});

            //modelBuilder.Entity<Project>(e =>
            //{
            //    e.HasKey(u => u.Id);
            //    e.HasOne(ve => ve.Brand);
            //    e.HasOne(p => p.CreatedBy).WithOne(cu => cu.Project).HasForeignKey<Project>(f => f.CreatedById);
            //    e.HasMany(x => x.Costs).WithOne(x => x.Project).HasForeignKey(x => x.ProjectId);

            //    e.Property(x => x.Id).ValueGeneratedOnAdd().HasColumnName("id");
            //    e.Property(x => x.Deleted).HasColumnName("deleted");
            //    e.Property(x => x.CreatedById).HasColumnName("created_by_id");
            //    e.ToTable("project");
            //});

            //modelBuilder.Entity<ProjectAdId>(e =>
            //{
            //    e.HasKey(u => u.Id);
            //    e.Property(x => x.Id).ValueGeneratedOnAdd().HasColumnName("id");
            //    e.ToTable("project_ad_id");
            //});

            //modelBuilder.Entity<Currency>(e =>
            //{
            //    e.HasKey(u => u.Id);
            //    e.HasMany(x => x.Costs).WithOne(x => x.PaymentCurrency).HasForeignKey(x => x.PaymentCurrencyId);
            //    e.ToTable("currency");
            //});

            //modelBuilder.Entity<Cost>(e =>
            //{
            //    e.HasKey(u => u.Id);
            //    e.HasOne(c => c.LatestCostStageRevision).WithOne(x => x.Cost).HasForeignKey<Cost>(f => f.LatestCostStageRevisionId);
            //    e.HasOne(x => x.Owner);
            //    e.HasOne(x => x.CreatedBy);
            //    e.HasOne(x => x.Parent).WithMany(x => x.Costs).HasForeignKey(f => f.ParentId);
            //    e.HasMany(x => x.CostOwners).WithOne(x => x.Cost).HasForeignKey(f => f.CostId);
            //    e.HasMany(x => x.NotificationSubscribers).WithOne(x => x.Cost).HasForeignKey(f => f.CostId);
            //    e.Property(x => x.Deleted).HasColumnName("deleted");
            //    e.Property(x => x.Id).ValueGeneratedOnAdd().HasColumnName("id");
            //    e.Property(x => x.CostNumber).HasColumnName("cost_number");
            //    e.Property(x => x.Status).HasColumnName("status");
            //    e.ToTable("cost");
            //});

            //modelBuilder.Entity<CostStage>(e =>
            //{
            //    e.HasKey(u => u.Id);
            //    e.HasOne(x => x.Cost).WithMany(x => x.CostStages).HasForeignKey(f => f.CostId);
            //    e.Property(x => x.Id).ValueGeneratedOnAdd().HasColumnName("id");
            //    e.ToTable("cost_stage");
            //});

            //modelBuilder.Entity<AssociatedAsset>(e =>
            //{
            //    e.HasKey(u => u.Id);
            //    e.Property(x => x.Id).ValueGeneratedOnAdd().HasColumnName("id");
            //    e.ToTable("associated_asset");
            //});

            //modelBuilder.Entity<ExpectedAsset>(e =>
            //{
            //    e.HasKey(u => u.Id);
            //    e.HasOne(x => x.CostStageRevision).WithMany(x => x.ExpectedAssets).HasForeignKey(f => f.CostStageRevisionId);
            //    e.HasOne(x => x.ProjectAdId).WithMany(x => x.ExpectedAssets).HasForeignKey(f => f.ProjectAdIdId);
            //    e.Property(x => x.Id).ValueGeneratedOnAdd().HasColumnName("id");
            //    e.ToTable("expected_asset");
            //});

            //modelBuilder.Entity<CustomObjectData>(e =>
            //{
            //    e.HasKey(u => u.Id);
            //    e.Property(x => x.Id).ValueGeneratedOnAdd().HasColumnName("id");
            //    e.ToTable("custom_object_data");
            //});

            //modelBuilder.Entity<BillingExpense>(e =>
            //{
            //    e.HasKey(u => u.Id);
            //    e.Property(x => x.Id).ValueGeneratedOnAdd().HasColumnName("id");
            //    e.ToTable("billing_expense");
            //});

            //modelBuilder.Entity<PolicyException>(e =>
            //{
            //    e.HasKey(u => u.Id);
            //    e.Property(x => x.Id).ValueGeneratedOnAdd().HasColumnName("id");
            //    e.ToTable("policy_exception");
            //});

            //modelBuilder.Entity<CostStageRevision>(e =>
            //{
            //    e.HasKey(u => u.Id);
            //    e.HasOne(x => x.CostStage).WithMany(x => x.CostStageRevisions).HasForeignKey(f => f.CostStageId);
            //    e.HasOne(x => x.ProductDetails);
            //    e.HasOne(x => x.StageDetails);
            //    e.HasOne(x => x.ValueReportings);

            //    e.HasMany(x => x.CostStageRevisionPaymentTotals).WithOne(x => x.CostStageRevision).HasForeignKey(f => f.CostStageRevisionId);
            //    e.HasMany(x => x.CustomObjectData).WithOne(x => x.CostStageRevision).HasForeignKey(f => f.ObjectId);
            //    e.HasMany(x => x.Approvals).WithOne(x => x.CostStageRevision).HasForeignKey(f => f.CostStageRevisionId);
            //    e.HasMany(x => x.SupportingDocuments).WithOne(x => x.CostStageRevision).HasForeignKey(f => f.CostStageRevisionId);
            //    e.HasMany(x => x.CostLineItems).WithOne(x => x.CostStageRevision).HasForeignKey(f => f.CostStageRevisionId);
            //    e.HasMany(x => x.CostFormDetails).WithOne(x => x.CostStageRevision).HasForeignKey(f => f.CostStageRevisionId);
            //    e.HasMany(x => x.TravelCosts).WithOne(x => x.CostStageRevision).HasForeignKey(f => f.CostStageRevisionId);
            //    e.HasMany(x => x.AssociatedAssets).WithOne(x => x.CostStageRevision).HasForeignKey(f => f.CostStageRevisionId);
            //    e.HasMany(x => x.BillingExpenses).WithOne(x => x.CostStageRevision).HasForeignKey(f => f.CostStageRevisionId);

            //    e.Property(x => x.Id).ValueGeneratedOnAdd().HasColumnName("id");
            //    e.ToTable("cost_stage_revision");
            //});

            //modelBuilder.Entity<BillingExpense>(e =>
            //{
            //    e.HasKey(u => u.Id);
            //    e.ToTable("billing_expense");
            //});

            //modelBuilder.Entity<AssociatedAsset>(e =>
            //{
            //    e.HasKey(u => u.Id);
            //    e.ToTable("associated_asset");
            //});

            //modelBuilder.Entity<TravelCost>(e =>
            //{
            //    e.HasKey(u => u.Id);
            //    e.HasOne(x => x.Region);
            //    e.HasOne(x => x.Country);
            //    e.ToTable("travel_cost");
            //});

            //modelBuilder.Entity<ValueReporting>(e =>
            //{
            //    e.HasKey(u => u.Id);
            //    e.ToTable("value_reporting");
            //});

            //modelBuilder.Entity<CostFormDetails>(e =>
            //{
            //    e.HasKey(u => u.Id);
            //    e.HasOne(cfd => cfd.CustomFormData).WithOne(cfd => cfd.CostFormDetails).HasForeignKey<CostFormDetails>(cfd => cfd.FormDataId);
            //    e.HasOne(cfd => cfd.FormDefinition).WithMany(cfd => cfd.CostFormDetails).HasForeignKey(cfd => cfd.FormDefinitionId);
            //    e.Property(x => x.Id).ValueGeneratedOnAdd().HasColumnName("id");
            //    e.ToTable("cost_form_details");
            //});

            //modelBuilder.Entity<CustomFormData>(e =>
            //{
            //    e.HasKey(u => u.Id);
            //    e.Property(x => x.Id).ValueGeneratedOnAdd().HasColumnName("id");
            //    e.ToTable("custom_form_data");
            //});

            //modelBuilder.Entity<Approval>(e =>
            //{
            //    e.HasKey(u => u.Id);
            //    e.HasMany(x => x.ApprovalMembers).WithOne(x => x.Approval).HasForeignKey(f => f.ApprovalId).OnDelete(DeleteBehavior.Cascade);
            //    e.Property(x => x.Id).ValueGeneratedOnAdd().HasColumnName("id");
            //    e.ToTable("approval");
            //});

            //modelBuilder.Entity<ApprovalMember>(e =>
            //{
            //    e.HasKey(u => u.Id);
            //    e.HasOne(x => x.CostUser).WithMany(am => am.ApprovalMembers).HasForeignKey(am => am.MemberId);
            //    e.Property(x => x.Id).ValueGeneratedOnAdd().HasColumnName("id");
            //    e.ToTable("approval_member");
            //});

            //modelBuilder.Entity<CostLineItem>(e =>
            //{
            //    e.HasKey(u => u.Id);
            //    e.HasOne(x => x.CostLineItemSectionTemplate).WithMany(t => t.CostLineItems).HasForeignKey(cli => cli.TemplateSectionId);
            //    e.Property(x => x.Id).ValueGeneratedOnAdd().HasColumnName("id");
            //    e.ToTable("cost_line_item");
            //});

            //modelBuilder.Entity<Rule>(e =>
            //{
            //    e.HasKey(u => u.Id);
            //    e.Ignore(r => r.Criterion);
            //    e.Property(x => x.Id).ValueGeneratedOnAdd().HasColumnName("id");
            //    e.ToTable("rule");
            //});

            //modelBuilder.Entity<CostUser>(e =>
            //{
            //    e.HasKey(u => u.Id);
            //    e.HasOne(x => x.AbstractType).WithMany(t => t.Users).HasForeignKey(cli => cli.ParentId);
            //    e.HasMany(cu => cu.UserUserGroups).WithOne(uug => uug.CostUser).HasForeignKey(f => f.UserId);
            //    e.HasOne(x => x.Agency).WithMany(t => t.Users).HasForeignKey(cli => cli.AgencyId);
            //    e.HasMany(x => x.NotificationSubscribers).WithOne(t => t.CostUser).HasForeignKey(cli => cli.CostUserId);

            //    e.Property(x => x.Id).ValueGeneratedOnAdd().HasColumnName("id");
            //    e.ToTable("cost_user");
            //});

            //modelBuilder.Entity<NotificationSubscriber>(e =>
            //{
            //    e.HasKey(ns => ns.Id);
            //    e.ToTable("notification_subscriber");
            //});

            //modelBuilder.Entity<SupportingDocument>(e =>
            //{
            //    e.HasKey(u => u.Id);
            //    e.HasMany(x => x.SupportingDocumentRevisions).WithOne(t => t.SupportingDocument).HasForeignKey(cli => cli.SupportingDocumentId);
            //    e.Property(x => x.Id).ValueGeneratedOnAdd().HasColumnName("id");
            //    e.ToTable("supporting_document");
            //});

            //modelBuilder.Entity<SupportingDocumentRevision>(e =>
            //{
            //    e.HasKey(u => u.Id);
            //    e.Property(x => x.Id).ValueGeneratedOnAdd().HasColumnName("id");
            //    e.ToTable("supporting_document_revision");
            //});

            //modelBuilder.Entity<UserFavouriteCost>(e =>
            //{
            //    e.HasKey(u => u.Id);
            //    e.Property(x => x.Id).ValueGeneratedOnAdd().HasColumnName("id");
            //    e.ToTable("user_favourite_cost");
            //});

            //modelBuilder.Entity<CostTemplate>(e =>
            //{
            //    e.HasKey(u => u.Id);
            //    e.Property(x => x.Id).ValueGeneratedOnAdd().HasColumnName("id");
            //    e.ToTable("cost_template");
            //});

            //modelBuilder.Entity<CostTemplateVersion>(e =>
            //{
            //    e.HasKey(u => u.Id);
            //    e.Property(x => x.Id).ValueGeneratedOnAdd().HasColumnName("id");
            //    e.ToTable("cost_template_version");
            //});

            //modelBuilder.Entity<FormDefinition>(e =>
            //{
            //    e.HasKey(u => u.Id);
            //    e.Property(x => x.Id).ValueGeneratedOnAdd().HasColumnName("id");
            //    e.ToTable("form_definition");
            //});

            //modelBuilder.Entity<FormSectionDefinition>(e =>
            //{
            //    e.HasKey(u => u.Id);
            //    e.Property(x => x.Id).ValueGeneratedOnAdd().HasColumnName("id");
            //    e.ToTable("form_section_definition");
            //});

            //modelBuilder.Entity<CostLineItemSectionTemplate>(e =>
            //{
            //    e.HasKey(u => u.Id);
            //    e.Property(x => x.Id).ValueGeneratedOnAdd().HasColumnName("id");
            //    e.ToTable("cost_line_item_section_template");
            //});

            //modelBuilder.Entity<CostLineItemSectionTemplateItem>(e =>
            //{
            //    e.HasKey(u => u.Id);
            //    e.HasOne(x => x.CostLineItemSectionTemplate).WithMany(t => t.CostLineItemSectionTemplateItems).HasForeignKey(cli => cli.SectionTemplateId);
            //    e.Property(x => x.Id).ValueGeneratedOnAdd().HasColumnName("id");
            //    e.ToTable("cost_line_item_section_template_item");
            //});

            //modelBuilder.Entity<UserGroup>(e =>
            //{
            //    e.HasKey(u => u.Id);
            //    e.HasMany(x => x.UserUserGroups).WithOne(t => t.UserGroup).HasForeignKey(cli => cli.UserGroupId);
            //    e.HasOne(x => x.Role).WithMany(t => t.RoleUserGroups).HasForeignKey(cli => cli.RoleId);

            //    e.Property(x => x.Id).ValueGeneratedOnAdd().HasColumnName("id");
            //    e.ToTable("user_group");
            //});

            //modelBuilder.Entity<Role>(e =>
            //{
            //    e.HasKey(u => u.Id);
            //    e.HasMany(x => x.BusinessRoles).WithOne(t => t.Role).HasForeignKey(cli => cli.RoleId);
            //    e.Property(x => x.Id).ValueGeneratedOnAdd().HasColumnName("id");
            //    e.ToTable("role");
            //});

            //modelBuilder.Entity<BusinessRole>(e =>
            //{
            //    e.HasKey(u => u.Id);
            //    e.Property(x => x.Id).ValueGeneratedOnAdd().HasColumnName("id");
            //    e.ToTable("business_role");
            //});

            //modelBuilder.Entity<UserUserGroup>(e =>
            //{
            //    e.HasKey(u => u.Id);
            //    e.Property(x => x.Id).ValueGeneratedOnAdd().HasColumnName("id");
            //    e.ToTable("user_user_group");
            //});

            //modelBuilder.Entity<Permission>(e =>
            //{
            //    e.HasKey(u => u.Id);
            //    e.Property(x => x.Id).ValueGeneratedOnAdd().HasColumnName("id");
            //    e.ToTable("permission");
            //});

            //modelBuilder.Entity<CostFilter>(e =>
            //{
            //    e.HasKey(u => u.Id);
            //    e.HasOne(ug => ug.CostUser).WithMany(a => a.CostFilters).HasForeignKey(r => r.UserId);
            //    e.Property(x => x.Id).ValueGeneratedOnAdd().HasColumnName("id");
            //    e.ToTable("cost_filter");
            //});

            //modelBuilder.Entity<City>(e =>
            //{
            //    e.HasKey(u => u.Id);
            //    e.Property(x => x.Id).ValueGeneratedOnAdd().HasColumnName("id");
            //    e.ToTable("city");
            //});

            //modelBuilder.Entity<Country>(e =>
            //{
            //    e.HasKey(u => u.Id);
            //    e.HasOne(ug => ug.Smo).WithMany(a => a.Countries).HasForeignKey(r => r.SmoId);
            //    e.Property(x => x.Id).ValueGeneratedOnAdd().HasColumnName("id");
            //    e.ToTable("country");
            //});

            //modelBuilder.Entity<Smo>(e =>
            //{
            //    e.HasKey(u => u.Id);
            //    e.Property(x => x.Id).ValueGeneratedOnAdd().HasColumnName("id");
            //    e.ToTable("smo");
            //});

            //modelBuilder.Entity<CountryCapital>(e =>
            //{
            //    e.HasKey(u => u.Id);
            //    e.Property(x => x.Id).ValueGeneratedOnAdd().HasColumnName("id");
            //    e.ToTable("country_capital");
            //});

            //modelBuilder.Entity<BudgetFormTemplate>(e =>
            //{
            //    e.HasKey(u => u.Id);
            //    e.Property(x => x.Id).ValueGeneratedOnAdd().HasColumnName("id");
            //    e.ToTable("budget_form_template");
            //});

            //modelBuilder.Entity<Requisitioner>(e =>
            //{
            //    e.HasKey(u => u.Id);
            //    e.HasOne(r => r.Approval).WithMany(csr => csr.Requisitioners).HasForeignKey(r => r.ApprovalId);
            //    e.HasOne(r => r.CostUser);
            //    e.Property(x => x.Id).ValueGeneratedOnAdd().HasColumnName("id");
            //    e.ToTable("requisitioner");
            //});

            //modelBuilder.Entity<AbstractType>(e =>
            //{
            //    e.HasKey(u => u.Id);
            //    e.HasOne(at => at.Module).WithOne(a => a.AbstractType).HasForeignKey<AbstractType>(at => at.ObjectId);
            //    e.HasOne(at => at.Agency).WithMany(a => a.AbstractTypes).HasForeignKey(at => at.ObjectId);
            //    e.Property(x => x.Id).ValueGeneratedOnAdd().HasColumnName("id");
            //    e.ToTable("abstract_type");
            //});

            //modelBuilder.Entity<Module>(e =>
            //{
            //    e.HasKey(u => u.Id);
            //    e.Property(x => x.Id).ValueGeneratedOnAdd().HasColumnName("id");
            //    e.ToTable("module");
            //});

            //modelBuilder.Entity<AbstractTypeHierarchyView>(e =>
            //{
            //    e.HasKey(u => u.Id);
            //    e.HasOne(at => at.AbstractType).WithOne(a => a.AbstractTypeHierarchyView).HasForeignKey<AbstractTypeHierarchyView>(at => at.Id);
            //    e.HasOne(at => at.AbstractTypeParent).WithOne(a => a.AbstractTypeHierarchyViewChild).HasForeignKey<AbstractTypeHierarchyView>(at => at.ParentId);
            //    e.Property(x => x.Id).ValueGeneratedOnAdd().HasColumnName("id");
            //    e.ToTable("abstract_type_hierarchy_view");
            //});

            //modelBuilder.Entity<Agency>(e =>
            //{
            //    e.HasKey(a => a.Id);
            //    e.HasOne(a => a.GlobalAgencyRegion).WithMany(j => j.Agencies).HasForeignKey(a => a.GlobalAgencyRegionId);
            //    e.HasOne(a => a.Country).WithMany(c => c.Agencies).HasForeignKey(a => a.CountryId);
            //    e.Property(a => a.Id).ValueGeneratedOnAdd().HasColumnName("id");
            //    e.ToTable("agency");
            //});

            //modelBuilder.Entity<GeoRegion>(e =>
            //{
            //    e.HasKey(u => u.Id);
            //    e.ToTable("geo_region");
            //});

            //modelBuilder.Entity<Region>(e =>
            //{
            //    e.HasKey(u => u.Id);
            //    e.Property(x => x.Id).ValueGeneratedOnAdd().HasColumnName("id");
            //    e.ToTable("region");
            //});

            //modelBuilder.Entity<PolicyException>(e =>
            //{
            //    e.HasKey(u => u.Id);
            //    e.HasOne(px => px.CostStageRevision).WithMany(csr => csr.PolicyExceptions).HasForeignKey(f => f.CostStageRevisionId);
            //    e.Property(x => x.Id).ValueGeneratedOnAdd().HasColumnName("id");
            //    e.ToTable("policy_exception");
            //});

            //modelBuilder.Entity<ActivityLog>(e =>
            //{
            //    e.HasKey(u => u.Id);
            //    e.Property(x => x.Id).ValueGeneratedOnAdd().HasColumnName("id");
            //    e.ToTable("activity_log");
            //});

            //modelBuilder.Entity<ActivityLogDelivery>(e =>
            //{
            //    e.HasKey(u => u.Id);
            //    e.HasOne(x => x.ActivityLog).WithOne(d => d.ActivityLogDelivery).HasForeignKey<ActivityLogDelivery>(x => x.ActivityLogId);
            //    e.Property(x => x.Id).ValueGeneratedOnAdd().HasColumnName("id");
            //    e.ToTable("activity_log_delivery");
            //});

            //modelBuilder.Entity<GlobalAgency>(e =>
            //{
            //    e.HasKey(u => u.Id);
            //    e.HasMany(i => i.GlobalAgencyRegions).WithOne(j => j.GlobalAgency).HasForeignKey(a => a.GlobalAgencyId);
            //    e.Property(x => x.Id).ValueGeneratedOnAdd().HasColumnName("id");
            //    e.ToTable("global_agency");
            //});

            //modelBuilder.Entity<GlobalAgencyRegion>(e =>
            //{
            //    e.HasKey(u => u.Id);
            //    e.HasOne(i => i.GlobalAgency).WithMany(j => j.GlobalAgencyRegions).HasForeignKey(a => a.GlobalAgencyId);
            //    e.Property(x => x.Id).ValueGeneratedOnAdd().HasColumnName("id");
            //    e.ToTable("global_agency_region");
            //});

            //modelBuilder.Entity<CostStageRevisionPaymentTotal>(e =>
            //{
            //    e.HasKey(u => u.Id);
            //    e.Property(x => x.Id).ValueGeneratedOnAdd().HasColumnName("id");
            //    e.ToTable("cost_stage_revision_payment_total");
            //});

            //modelBuilder.Entity<RejectionDetails>(e =>
            //{
            //    e.HasKey(u => u.Id);
            //    e.HasOne(cr => cr.ApprovalMember).WithOne(fd => fd.RejectionDetails).HasForeignKey<RejectionDetails>(cr => cr.RejectorId);
            //    e.HasOne(cr => cr.Requisitioner).WithOne(fd => fd.RejectionDetails).HasForeignKey<RejectionDetails>(cr => cr.RejectorId);
            //    e.Property(x => x.Id).ValueGeneratedOnAdd().HasColumnName("id");
            //    e.ToTable("rejection_details");
            //});

            //modelBuilder.Entity<ApprovalDetails>(e =>
            //{
            //    e.HasKey(u => u.Id);
            //    e.HasOne(cr => cr.ApprovalMember).WithOne(fd => fd.ApprovalDetails).HasForeignKey<ApprovalDetails>(cr => cr.ApproverId);
            //    e.HasOne(cr => cr.Requisitioner).WithOne(fd => fd.ApprovalDetails).HasForeignKey<ApprovalDetails>(cr => cr.ApproverId);
            //    e.Property(x => x.Id).ValueGeneratedOnAdd().HasColumnName("id");
            //    e.ToTable("approval_details");
            //});

            //modelBuilder.Entity<SchemaVersion>(e =>
            //{
            //    e.HasKey(u => u.InstalledRank);
            //    e.Property(x => x.InstalledRank).HasColumnName("installed_rank");
            //    e.ToTable("schema_version");
            //});

            //modelBuilder.Entity<ContentTypeAdidMedium>(e =>
            //{
            //    e.HasKey(u => u.Id);
            //    e.ToTable("content_type_adid_medium");
            //});

            //modelBuilder.Entity<ExchangeRate>(e =>
            //{
            //    e.HasKey(u => u.Id);
            //    e.Property(x => x.Id).ValueGeneratedOnAdd().HasColumnName("id");
            //    e.Property(x => x.FromCurrency).ValueGeneratedOnAdd().HasColumnName("from_currency");
            //    e.ToTable("exchange_rate");
            //});

            //modelBuilder.Entity<ApprovalBand>(e =>
            //{
            //    e.HasKey(u => u.Id);
            //    e.HasOne(x => x.BusinessRole).WithOne(x => x.ApprovalBand).HasForeignKey<ApprovalBand>(f => f.BusinessRoleId);
            //    e.Property(x => x.Id).ValueGeneratedOnAdd().HasColumnName("id");
            //    e.ToTable("approval_band");
            //});

            //modelBuilder.Entity<UserBusinessRole>(e =>
            //{
            //    e.HasKey(u => u.Id);
            //    e.HasOne(x => x.BusinessRole).WithMany(x => x.UserBusinessRoles).HasForeignKey(f => f.BusinessRoleId);
            //    e.HasOne(x => x.CostUser).WithMany(x => x.UserBusinessRoles).HasForeignKey(f => f.CostUserId).OnDelete(DeleteBehavior.Cascade);

            //    e.Property(x => x.Id).ValueGeneratedOnAdd().HasColumnName("id");
            //    e.Property(x => x.CostUserId).HasColumnName("cost_user_id");

            //    e.ToTable("user_business_role");
            //});

            //modelBuilder.Entity<CostOwner>(e =>
            //{
            //    e.HasKey(u => u.Id);
            //    e.HasOne(x => x.User);
            //    e.Property(x => x.Id).ValueGeneratedOnAdd().HasColumnName("id");
            //    e.ToTable("cost_owner");
            //});

            //modelBuilder.Entity<Brand>(e =>
            //{
            //    e.HasKey(u => u.Id);
            //    e.HasOne(x => x.Sector).WithMany(x => x.Brands).HasForeignKey(f => f.SectorId);
            //    e.Property(x => x.Id).ValueGeneratedOnAdd().HasColumnName("id");
            //    e.ToTable("brand");
            //});

            //modelBuilder.Entity<Sector>(e =>
            //{
            //    e.HasKey(u => u.Id);
            //    e.Property(x => x.Id).ValueGeneratedOnAdd().HasColumnName("id");
            //    e.ToTable("sector");
            //});

            //modelBuilder.Entity<RolePermission>(e =>
            //{
            //    e.HasKey(u => u.Id);
            //    e.HasOne(x => x.Permission).WithMany(x => x.RolePermissions).HasForeignKey(f => f.PermissionId);
            //    e.HasOne(x => x.Role).WithMany(x => x.RolePermissions).HasForeignKey(f => f.RoleId);

            //    e.Property(x => x.Id).ValueGeneratedOnAdd().HasColumnName("id");

            //    e.ToTable("role_permission");
            //});

            //DataTypesMapping(modelBuilder);
        }

        private static void DataTypesMapping(ModelBuilder modelBuilder)
        {
            //modelBuilder.Entity<CustomFormData>()
            //    .Property(cfd => cfd.Data)
            //    .HasColumnType("jsonb");

            //modelBuilder.Entity<CustomObjectData>()
            //    .Property(cfd => cfd.Data)
            //    .HasColumnType("jsonb");

            //modelBuilder.Entity<CostFilter>()
            //    .Property(cf => cf.SearchQuery)
            //    .HasColumnType("jsonb");
        }
    }
}
