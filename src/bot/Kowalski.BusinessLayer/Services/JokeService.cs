using Kowalski.DataAccessLayer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kowalski.BusinessLayer.Services
{
    public static class JokeService
    {
        public static async Task<string> GetJokeAsync()
        {
            using (var db = new DbContext())
            {
                var query = "SELECT TOP 1 Content FROM JOKES ORDER BY NEWID()";
                var joke = await db.GetAsync<string>(query);

                return joke;
            }
        }
    }
}
