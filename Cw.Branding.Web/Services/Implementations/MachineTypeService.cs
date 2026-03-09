using Cw.Branding.Web.Data;
using Cw.Branding.Web.Helpers;
using Cw.Branding.Web.Models.Entities;
using Cw.Branding.Web.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Cw.Branding.Web.Services.Implementations;

public class MachineTypeService(AppDbContext context) : IMachineTypeService
{
    public async Task<List<MachineType>> GetAllAsync() =>
        await context.MachineTypes.OrderBy(m => m.DisplayOrder).ToListAsync();

    public async Task<List<MachineType>> GetActiveAsync() =>
        await context.MachineTypes.Where(m => m.IsActive).OrderBy(m => m.DisplayOrder).ToListAsync();

    public async Task<MachineType?> GetByIdAsync(int id) =>
        await context.MachineTypes.FindAsync(id);

    public async Task CreateAsync(MachineType machineType)
    {
        machineType.SlugVi = SlugHelper.GenerateSlug(machineType.NameVi);
        machineType.SlugEn = SlugHelper.GenerateSlug(machineType.NameEn);
        context.MachineTypes.Add(machineType);
        await context.SaveChangesAsync();
    }

    public async Task UpdateAsync(MachineType machineType)
    {
        machineType.SlugVi = SlugHelper.GenerateSlug(machineType.NameVi);
        machineType.SlugEn = SlugHelper.GenerateSlug(machineType.NameEn);
        context.MachineTypes.Update(machineType);
        await context.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var item = await context.MachineTypes.FindAsync(id);
        if (item != null)
        {
            context.MachineTypes.Remove(item);
            await context.SaveChangesAsync();
        }
    }
}