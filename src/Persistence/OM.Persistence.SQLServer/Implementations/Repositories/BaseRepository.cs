using Microsoft.EntityFrameworkCore;

using OM.Domain.Interfaces.IRepositories;
using OM.Persistence.SQLServer.Context;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace OM.Persistence.SQLServer.Implementations.Repositories;


public class BaseRepository<T> : IBaseRepository<T> where T : class
{
    protected readonly DBContext _context;

    public BaseRepository(DBContext context)
    {
        _context = context;
    }

    public async Task<T> CreateAsync(T entity)
    {
        await _context.Set<T>().AddAsync(entity);
        await _context.SaveChangesAsync();
        return entity;
    }

    public async Task<T> DeleteAsync(int Id)
    {
        var entity = await FindByIdAsync(Id);

        if (entity == null)
            throw new Exception($"Entity with id {Id} not found");

        _context.Set<T>().Remove(entity);
        await _context.SaveChangesAsync();
        return entity;
    }

    public IQueryable<T> FindAll()
    {
        return _context.Set<T>().AsNoTracking();
    }

    public IQueryable<T> FindByCondition(Expression<Func<T, bool>> expression)
    {
        return _context.Set<T>().Where(expression).AsNoTracking();

    }

    public async Task<T?> FindByIdAsync(int Id)
    {
        return await _context.Set<T>().FindAsync(Id);
    }

    public async Task<T> UpdateAsync(T entity)
    {
        _context.Set<T>().Update(entity);
        await _context.SaveChangesAsync();
        return entity;
    }
}

