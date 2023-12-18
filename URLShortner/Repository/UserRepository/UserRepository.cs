using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using URLShortner.Data;
using URLShortner.Models;

namespace URLShortner.Repository.UserRepository;

public class UserRepository : IUserRepository
{
    private readonly ApplicationDbContext _db;

    public UserRepository(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<User> GetUserAsync(Expression<Func<User, bool>> filter)
    {
        IQueryable<User> queryable = _db.Users;

        queryable = queryable.Where(filter);

        return await queryable.FirstOrDefaultAsync();
    }

    public async Task AddUserAsync(User user)
    {
        await _db.Users.AddAsync(user);
        await _db.SaveChangesAsync();
    }
}
