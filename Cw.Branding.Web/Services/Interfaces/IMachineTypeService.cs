using Cw.Branding.Web.Models.Entities;

namespace Cw.Branding.Web.Services.Interfaces;

public interface IMachineTypeService
{
    Task<List<MachineType>> GetAllAsync();
    Task<List<MachineType>> GetActiveAsync();
    Task<MachineType?> GetByIdAsync(int id);
    Task CreateAsync(MachineType machineType);
    Task UpdateAsync(MachineType machineType);
    Task DeleteAsync(int id);
}