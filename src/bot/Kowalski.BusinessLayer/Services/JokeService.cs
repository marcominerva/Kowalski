using Kowalski.DataAccessLayer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kowalski.BusinessLayer.Services
{
    public class JokeService : IJokeService
    {
        private readonly IDbContext dbContext;

        public JokeService(IDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        public async Task<string> GetJokeAsync()
        {
            var query = "SELECT TOP 1 Content FROM JOKES ORDER BY NEWID()";
            var joke = await dbContext.GetAsync<string>(query);

            return joke;
        }
    }
}
