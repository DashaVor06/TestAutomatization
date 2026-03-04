namespace BusinessLogic.Repositories
{
    public interface IRepository<TEntity> where TEntity : class
    {
        Task<List<TEntity>> GetAllAsync();
        TEntity GetById(int id);
        TEntity Create(TEntity entity);
        TEntity Update(TEntity entity);
        void Delete(int id);    
        bool Exists(int id);
        int GetLastId();
    }
}
