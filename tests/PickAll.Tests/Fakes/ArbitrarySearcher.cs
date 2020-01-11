using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PickAll.Tests.Fakes
{
    class ArbitrarySearcherSettings
    {
        public ushort Samples;
    }

    class ArbitrarySearcher : Searcher
    {
        private readonly ArbitrarySearcherSettings _settings;

        public ArbitrarySearcher(object settings, RuntimePolicy policy) : base(settings, policy)  
        {
            _settings = Settings as ArbitrarySearcherSettings;
            if (_settings == null) {
                throw new NotSupportedException();
            }
        }

        public override async Task<IEnumerable<ResultInfo>> SearchAsync(string query)
        {
            return await Task.Run(() => _());
            IEnumerable<ResultInfo> _() {
                var originator = Guid.NewGuid().ToString();
                return ResultInfoBuilder.Generate(originator, _settings.Samples);
            }
        }
    }
}