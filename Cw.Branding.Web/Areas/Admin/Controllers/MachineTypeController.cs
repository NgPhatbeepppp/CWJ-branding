using Cw.Branding.Web.Models.Entities;
using Cw.Branding.Web.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Cw.Branding.Web.Areas.Admin.Controllers;

[Area("Admin")]
[Route("{lang}/Admin/[controller]/{action=Index}")]
public class MachineTypeController(IMachineTypeService machineTypeService) : BaseAdminController
{
    public async Task<IActionResult> Index()
    {
        var items = await machineTypeService.GetAllAsync();
        return View(items);
    }
    [HttpGet]
    public IActionResult Create() => View(new MachineType { IsActive = true });

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(MachineType machineType)
    {
        if (ModelState.IsValid)
        {
            await machineTypeService.CreateAsync(machineType);
            return RedirectToAction(nameof(Index));
        }
        return View(machineType);
    }

    // GET: Admin/MachineType/Edit/5
    [HttpGet("{id}")]
    public async Task<IActionResult> Edit(int id)
    {
        var item = await machineTypeService.GetByIdAsync(id);
        if (item == null) return NotFound();
        return View(item);
    }

    // POST: Admin/MachineType/Edit/5
    [HttpPost("{id}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, MachineType machineType)
    {
        if (id != machineType.Id) return NotFound();

        if (ModelState.IsValid)
        {
            await machineTypeService.UpdateAsync(machineType);
            return RedirectToAction(nameof(Index));
        }
        return View(machineType);
    }

    [HttpPost("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        await machineTypeService.DeleteAsync(id);
        return RedirectToAction(nameof(Index));
    }

} 