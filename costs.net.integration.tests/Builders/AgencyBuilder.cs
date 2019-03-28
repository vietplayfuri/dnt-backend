namespace costs.net.integration.tests.Builders
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using dataAccess.Entity;

    public class AgencyBuilder : BaseBuilder<Agency>
    {
        public AgencyBuilder()
        {
            Object.Labels = new string[0];
        }

        public AgencyBuilder WithCycloneLabel(bool add = false)
        {
            if (add)
            {
                return WithLabels(plugins.Constants.Agency.CycloneLabel);
            }

            Object.Labels = Object.Labels ?? new string[0];
            return this;
        }

        public AgencyBuilder WithPandGLabel()
        {
            return WithLabels(plugins.Constants.Agency.PAndGLabel);
        }

        public AgencyBuilder WithGdamAgencyId(string gdamId = null)
        {
            Object.GdamAgencyId = gdamId ?? new string(Guid.NewGuid().ToString().Take(24).ToArray());
            return this;
        }

        public AgencyBuilder WithName(string name = "agencyName")
        {
            Object.Name = name;
            return this;
        }

        public AgencyBuilder WithCountry(Country country)
        {
            Object.Country = country;
            return this;
        }

        public AgencyBuilder MakePrime(AbstractType parent)
        {
            WithLabels(plugins.Constants.Agency.PgOwnerLabel);
            Object.AbstractTypes = new List<AbstractType>
            {
                new AbstractType
                {
                    ObjectId = Object.Id,
                    Parent = parent,
                    Type = AbstractObjectType.Agency.ToString()
                }
            };

            return this;
        }

        public AgencyBuilder WithPrimaryCurrency(Guid id)
        {
            Object.PrimaryCurrency = id;
            return this;
        }

        public AgencyBuilder WithLabels(params string[] labels)
        {
            if (labels == null)
            {
                return this;
            }

            var labelsList = Object.Labels.ToList();
            labelsList.AddRange(labels);
            Object.Labels = labelsList.ToArray();
            return this;
        }
    }
}
