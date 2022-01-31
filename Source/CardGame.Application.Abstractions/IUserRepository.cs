using System;
using System.Threading.Tasks;

namespace CardGame.Application.Abstractions
{
    public interface IUserRepository
    {
        Task Add(string username, string password);
    }
}
