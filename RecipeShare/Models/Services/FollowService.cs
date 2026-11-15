using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RecipeShare.Models.Services
{
    // Services/FollowService.cs
    public class FollowService
    {
        private readonly ApplicationDbContext _context;

        public FollowService(ApplicationDbContext context)
        {
            _context = context;
        }

        public bool FollowUser(string followerId, string followeeId)
        {
            if (followerId == followeeId) return false;

            if (_context.Follows.Any(f => f.FollowerId == followerId && f.FolloweeId == followeeId))
                return false;

            _context.Follows.Add(new Follow { FollowerId = followerId, FolloweeId = followeeId });
            _context.SaveChanges();
            return true;
        }

        public bool UnfollowUser(string followerId, string followeeId)
        {
            var follow = _context.Follows
                .FirstOrDefault(f => f.FollowerId == followerId && f.FolloweeId == followeeId);
            if (follow == null) return false;

            _context.Follows.Remove(follow);
            _context.SaveChanges();
            return true;
        }
    }
}