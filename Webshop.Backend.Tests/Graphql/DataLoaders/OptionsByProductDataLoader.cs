// File: OptionsByProductDataLoader.cs
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GreenDonut;                          // <- hier
using Microsoft.EntityFrameworkCore;
using Webshop.Backend.Data;
using Webshop.Shared.Models;

namespace Webshop.Backend.GraphQL.DataLoaders
{
    public class OptionsByProductDataLoader
        : BatchDataLoader<int, List<ProductOption>>
    {
        private readonly IDbContextFactory<AppDbContext> _dbFactory;

        public OptionsByProductDataLoader(
            IBatchScheduler scheduler,
            IDbContextFactory<AppDbContext> dbFactory)
            : base(scheduler, new DataLoaderOptions()) // <- tweede parameter
        {
            _dbFactory = dbFactory;
        }

        protected override async Task<IReadOnlyDictionary<int, List<ProductOption>>> LoadBatchAsync(
            IReadOnlyList<int> keys,
            CancellationToken cancellationToken)
        {
            await using var db = _dbFactory.CreateDbContext();
            var all = await db.ProductOptions
                .Where(o => keys.Contains(o.ProductID))
                .ToListAsync(cancellationToken);

            return all
                .GroupBy(o => o.ProductID)
                .ToDictionary(g => g.Key, g => g.ToList());
        }
    }
}
