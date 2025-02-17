using SharedLibrary.Responses;
using System.Linq.Expressions;
//TepSharedLibrarySolution.Responses; 
namespace SharedLibrary.Interface
{
    public interface IGenericInterface<T> where T : class
    {
        Task <Response> CreateAsync(T entity);
        Task<Response> UpdateAsync(T entity);
        Task<Response> DeleteAsync(T entity);
        Task<IEnumerable<T>> GetAllAsync();
        Task<T> FindByIDAsync(int id);
        Task<T> GetBAsync(Expression<Func<T,bool>> predicate);


    }
}
