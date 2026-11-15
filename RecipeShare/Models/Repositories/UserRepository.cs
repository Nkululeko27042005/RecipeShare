using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RecipeShare.Models.Repositories
{
    public class UserRepository
    {
        private readonly ApplicationDbContext _context;

        public UserRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public ApplicationUser GetById(string id) => _context.Users.Find(id);

        public IQueryable<ApplicationUser> GetRisingUsers(int count)
        {
            // "Rising" = most followers gained recently (simplified: most followers overall)
            return _context.Users
                .OrderByDescending(u => u.Followers.Count)
                .Take(count);
        }

        public bool IsFollowing(string followerId, string followeeId)
        {
            return _context.Follows.Any(f => f.FollowerId == followerId && f.FolloweeId == followeeId);
        }
    }
}