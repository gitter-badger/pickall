using System.Linq;
using Xunit;
using PickAll.PostProcessors;
using PickAll.Tests.Fakes;

namespace PickAll.Tests.Unit
{
    public class FuzzyMatchTests
    {
        [Fact]
        public void Matching_text_with_minimum_distance_of_zero_excludes_other_results()
        {
            var description = Utilities.RandomDescriptionOf<Searcher_with_five_results>();

            var context = new SearchContext()
                .With<Searcher_with_five_results>()
                .With<FuzzyMatch>(new FuzzyMatchSettings { Text = description });
            var results = context.Search();

            Assert.Single(results);
            Assert.Equal(description, results.First().Description);
        }
    }
}