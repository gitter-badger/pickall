using System.Linq;
using Xunit;
using PickAll.Tests.Fakes;

namespace PickAll.Tests.Unit
{
    public class SearchContextTests
    {
        [Fact]
        public void When_none_searcher_is_set_Search_returns_an_empty_collection()
        {
            var context = new SearchContext();
            var result = context.Search("query").GetAwaiter().GetResult();

            Assert.Empty(result);
        }

        [Fact]
        public void When_two_searches_are_set_Search_returns_a_merged_collection()
        {
            var firstFakeResults = Utilities.GetFakeSearcherResultsCount<Searcher_with_three_results>().GetAwaiter().GetResult();
            var secondFakeResults = Utilities.GetFakeSearcherResultsCount<Searcher_with_five_results>().GetAwaiter().GetResult();
            var context = new SearchContext()
                .With<Searcher_with_three_results>()
                .With<Searcher_with_five_results>();
            var results = context.Search("query").GetAwaiter().GetResult();

            Assert.Equal(firstFakeResults + secondFakeResults, results.Count());
        }

        [Fact]
        public void When_uniqueness_is_set_Search_excludes_duplicates_url()
        {
            var firstFakeResults = Utilities.GetFakeSearcherResultsCount<Searcher_with_three_results>().GetAwaiter().GetResult();
            var secondFakeResults = Utilities.GetFakeSearcherResultsCount<Searcher_with_five_results>().GetAwaiter().GetResult();
            var context = new SearchContext()
                .With<Searcher_with_three_results>()
                .With<Searcher_with_five_results>()
                .With<UniquenessPostProcessor>();
            var results = context.Search("query").GetAwaiter().GetResult();

            Assert.Equal(firstFakeResults + secondFakeResults - 2, results.Count());
        }

        [Fact]
        public void When_order_is_set_Search_results_are_ordered_by_index()
        {
            var firstFakeResults = Utilities.GetFakeSearcherResultsCount<Searcher_with_three_results>().GetAwaiter().GetResult();
            var secondFakeResults = Utilities.GetFakeSearcherResultsCount<Searcher_with_five_results>().GetAwaiter().GetResult();
            var context = new SearchContext()
                .With<Searcher_with_three_results>()
                .With<Searcher_with_five_results>()
                .With<OrderPostProcessor>();
            var results = context.Search("query").GetAwaiter().GetResult();

            Assert.Equal(0, results.ElementAt(0).Index);
            Assert.Equal(0, results.ElementAt(1).Index);
        }
    }
}