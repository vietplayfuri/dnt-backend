
namespace costs.net.core.tests.Analysis
{
    using System;
    using System.Collections.Generic;
    using core.Analysis;
    using FluentAssertions;
    using NUnit.Framework;

    [TestFixture]
    public class CollectionAnalyserTests
    {
        [Test]
        public void Analyse_EmptyX_Returns_AllOfY_In_Added()
        {
            //Arrange
            var x = new List<string>();
            var y = new List<string> { "a" };
            var comparer = StringComparer.CurrentCulture;
            var target = new CollectionAnalyser();

            //Act
            var result = target.Analyse(x, y, comparer);

            //Assert
            result.Should().NotBeNull();
            result.Added.Should().BeEquivalentTo(y);
            result.Removed.Should().BeNullOrEmpty();
            result.Unchanged.Should().BeNullOrEmpty();
        }

        [Test]
        public void Analyse_HasNoDifferences_Returns_AllInUnchanged()
        {
            //Arrange
            var x = new List<string> { "a" };
            var y = new List<string> { "a" };
            var comparer = StringComparer.CurrentCulture;
            var target = new CollectionAnalyser();

            //Act
            var result = target.Analyse(x, y, comparer);

            //Assert
            result.Should().NotBeNull();
            result.Unchanged.Should().BeEquivalentTo(y);
            result.Added.Should().BeNullOrEmpty();
            result.Removed.Should().BeNullOrEmpty();
        }

        [Test]
        public void Analyse_EmptyY_Returns_AllOfX_In_Removed()
        {
            //Arrange
            var x = new List<string> { "a" };
            var y = new List<string>();
            var comparer = StringComparer.CurrentCulture;
            var target = new CollectionAnalyser();

            //Act
            var result = target.Analyse(x, y, comparer);

            //Assert
            result.Should().NotBeNull();
            result.Unchanged.Should().BeNullOrEmpty();
            result.Added.Should().BeNullOrEmpty();
            result.Removed.Should().BeEquivalentTo(x);
        }        
    }
}
