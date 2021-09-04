using OoloWebCrawl.Storage.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OoloWebCrawl.Storage
{
    public class TitlesRepository : ITitlesRepository
    {
        //Doing it like that is not scalable
        //in the assignment it said to save internally but I would actually use mongo to hold this
        private readonly Dictionary<string, TitleModel> _titlesCache;

        public TitlesRepository()
        {
            _titlesCache = new Dictionary<string, TitleModel>();
        }

        public Task<bool> Save(TitleModel title)
        {
            if (!_titlesCache.TryGetValue(title.Url, out _))
            {
                _titlesCache.Add(title.Url, title);
            }

            return Task.FromResult(true);
        }

        public Task<List<TitleModel>> GetTitlesFrom(int lastMinutes)
        {
            var date = (DateTime.Now).AddMinutes(-lastMinutes);
            var titleModels = _titlesCache
                .Where(x => x.Value.DateCreated > date)
                .Select(g => g.Value)
                .ToList();

            return Task.FromResult(titleModels);
        }
    }

    public interface ITitlesRepository
    {
        Task<bool> Save(TitleModel title);
        Task<List<TitleModel>> GetTitlesFrom(int lastMinutes);
    }
}
