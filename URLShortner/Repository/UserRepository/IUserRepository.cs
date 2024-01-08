using System.Linq.Expressions;
using URLShortner.Models;
using URLShortner.Models.DomainModels;

namespace URLShortner.Repository.UserRepository;

public interface IUserRepository
{
    Task<User> GetUserAsync(Expression<Func<User, bool>> filter);

    Task AddUserAsync(User user);
}
