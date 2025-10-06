using FinalProjApp.Data;
using FinalProjApp.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinalProjApp.Services
{
    public interface ICRUD<T>
    {
        Task Add(T person);
        Task Update(int ID, T person);
        Task Delete(T person);
        Task<List<T>> GetList();
        Task<T> GetbyID(int person);
    }
    public class ChildCRUD : ICRUD<Child>
    {
        private DataContext _dataContext;

        public ChildCRUD(DataContext dataContext)
        {
            _dataContext = dataContext;
        }

        public async Task Add(Child child)
        {
            await _dataContext.children.AddAsync(child);
            await _dataContext.SaveChangesAsync();
        }

        public async Task Update(int Id, Child updatedChild)
        {
            var existingChild = await _dataContext.children.FindAsync(Id);
            if (existingChild != null)
            {
                existingChild.FirstName = updatedChild.FirstName;
                existingChild.LastName = updatedChild.LastName;
                existingChild.DateofBirth = updatedChild.DateofBirth;
                existingChild.GuardianID = updatedChild.GuardianID;

                await _dataContext.SaveChangesAsync();
            }
        }

        public async Task Delete(Child child)
        {
            if (child != null)
            {
                _dataContext.children.Remove(child);
                await _dataContext.SaveChangesAsync();
            }
        }

        public async Task<List<Child>> GetList()
        {
            return await _dataContext.children.ToListAsync();
        }

        public async Task<Child> GetbyID(int ID)
        {
            return await _dataContext.children.FindAsync(ID);
        }
    }

    public class GuardianCRUD : ICRUD<Guardian>
    {
        private readonly DataContext _dataContext;

        public GuardianCRUD(DataContext dataContext)
        {
            _dataContext = dataContext;
        }

        public async Task Add(Guardian guardian)
        {
            await _dataContext.guardian.AddAsync(guardian);
            await _dataContext.SaveChangesAsync();
        }

        public async Task Update(int Id, Guardian guardian)
        {
            var existingGuardian = await _dataContext.guardian.FindAsync(Id);
            if (existingGuardian != null)
            {
                existingGuardian.FullName = guardian.FullName;
                existingGuardian.PhoneNumber = guardian.PhoneNumber;
                existingGuardian.Address = guardian.Address;

                await _dataContext.SaveChangesAsync();
            }
        }

        public async Task Delete(Guardian guardian)
        {
            if (guardian != null)
            {
                _dataContext.guardian.Remove(guardian);
                await _dataContext.SaveChangesAsync();
            }
        }

        public async Task<List<Guardian>> GetList()
        {
            return await _dataContext.guardian.ToListAsync();
        }

        public async Task<Guardian> GetbyID(int ID)
        {
            return await _dataContext.guardian.FindAsync(ID);
        }
    }
}
