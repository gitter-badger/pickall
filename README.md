# PickAll

.NET agile and extensible web searching API. Built with [AngleSharp](https://github.com/AngleSharp/AngleSharp).

## Build and sample
**NOTE**: .NET Core 3.0 or higher is required.
```sh
# clone the repository
$ git clone https://github.com/gsscoder/pickall.git

# change the working directory
$ cd pickall

# build the package
$ dotnet build -c Release.

# execute sample
$ ./artifacts/PickAll.Simple/Debug/netcoreapp3.0/PickAll.Simple
[2] DUCKDUCKGO: "Steve Jobs - IMDb": "https://www.imdb.com/name/nm0423418/"
[3] GOOGLE: "Steve Jobs (film)": "https://it.wikipedia.org/wiki/Steve_Jobs_(film)"
[4] GOOGLE: "Steve Jobs (libro)": "https://it.wikipedia.org/wiki/Steve_Jobs_(libro)"
[7] GOOGLE: "Steve Jobs - Wikipedia": "https://en.wikipedia.org/wiki/Steve_Jobs"
[28] DUCKDUCKGO: "Steve Jobs - Forbes": "https://www.forbes.com/profile/steve-jobs/"
```

# Test
```sh
# change to tests directory
$ cd pickall/tests/PickAll.Tests

# build with debug configuration
$ dotnet build -c Debug
...

# execute tests
$ dotnet test
...
```

## At a glance
```csharp
using PickAll;
using PickAll.Searchers;
using PickAll.PostProcessors;

var context = new SearchContext()
    .With<Google>() // search on google.com
    .With<DuckDuckGo>() // search on duckduckgo.com
    .With<Uniqueness>() // remove duplicates
    .With<Order>() // order results by index
    // match Levenshtein distance with maximum of 15
    .With(new FuzzyMatch("mechanics", 15));
// execute services (order of addition)
var results = await context.Search("quantum physics");
// do anything you need with LINQ
var scientific = results.Where(result => result.Url.Contains("wikipedia"));

foreach (var result in scientific) {
    Console.WriteLine($"{result.Url} ${result.Description}");
}
```

### Notes
- This is a pre-release, since it's under development API can change until stable version.