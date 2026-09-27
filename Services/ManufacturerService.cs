using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VehicleManagement.Models;
using VehicleManagement.Repositories;
using VehicleManagement.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace VehicleManagement.Services
{
    public class ManufacturerService: IManufacturerService
    {
         private readonly IManufacturerRepository _repository;

    public ManufacturerService(
        IManufacturerRepository repository)
    {
        _repository = repository;
    }


    public async Task<ManufacturerListViewModel>
        GetManufacturersAsync(
            string? search,
            string sortBy,
            string sortDirection,
            int page,
            int pageSize)
    {
        var query = _repository.GetQueryable();


        // Search

        if (!string.IsNullOrWhiteSpace(search))
        {
            search = search.Trim();

            query = query.Where(m =>
                m.Name.Contains(search));
        }


        // Sorting

        query = sortDirection == "Descending"
            ? query.OrderByDescending(m => m.Name)
            : query.OrderBy(m => m.Name);


        // Count

        var totalItems =
            await query.CountAsync();


        var totalPages =
            (int)Math.Ceiling(
                totalItems / (double)pageSize);


        if (page < 1)
        {
            page = 1;
        }

        if (totalPages > 0 &&
            page > totalPages)
        {
            page = totalPages;
        }


        // Pagination

        var manufacturers =
            await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(m => new ManufacturerListItemViewModel
                {
                    Id = m.Id,

                    Name = m.Name,

                    VehicleCount = m.Vehicles.Count
                })
                .ToListAsync();


        return new ManufacturerListViewModel
        {
            Manufacturers = manufacturers,

            Search = search,

            SortBy = sortBy,

            SortDirection = sortDirection,

            CurrentPage = page,

            PageSize = pageSize,

            TotalItems = totalItems,

            TotalPages = totalPages
        };
    }


    public async Task<ManufacturerDetailsViewModel?>
        GetDetailsAsync(int id)
    {
        var manufacturer =
            await _repository.GetByIdAsync(id);

        if (manufacturer == null)
        {
            return null;
        }


        return new ManufacturerDetailsViewModel
        {
            Id = manufacturer.Id,

            Name = manufacturer.Name,

            VehicleCount =
                manufacturer.Vehicles.Count,

            Vehicles =
                manufacturer.Vehicles
                    .Select(v => new VehicleListItemViewModel
                    {
                        Id = v.Id,

                        OwnerName = v.OwnerName,

                        ManufacturerName =
                            manufacturer.Name,

                        YearOfManufacture =
                            v.YearOfManufacture,

                        WeightKg =
                            v.WeightKg,

                        CategoryName =
                            v.Category?.Name ?? string.Empty
                    })
                    .ToList()
        };
    }


    public async Task<bool> CreateAsync(
        ManufacturerCreateViewModel model)
    {
        var name = model.Name.Trim();


        if (await _repository.NameExistsAsync(name))
        {
            return false;
        }


        var manufacturer = new Manufacturer
        {
            Name = name
        };


        await _repository.AddAsync(
            manufacturer);

        await _repository.SaveChangesAsync();

        return true;
    }


    public async Task<ManufacturerEditViewModel?>
        GetEditAsync(int id)
    {
        var manufacturer =
            await _repository.GetByIdAsync(id);

        if (manufacturer == null)
        {
            return null;
        }


        return new ManufacturerEditViewModel
        {
            Id = manufacturer.Id,

            Name = manufacturer.Name
        };
    }


    public async Task<bool> UpdateAsync(
        ManufacturerEditViewModel model)
    {
        var manufacturer =
            await _repository.GetByIdAsync(model.Id);

        if (manufacturer == null)
        {
            return false;
        }


        var name = model.Name.Trim();


        if (await _repository.NameExistsAsync(
                name,
                model.Id))
        {
            return false;
        }


        manufacturer.Name = name;


        _repository.Update(manufacturer);

        await _repository.SaveChangesAsync();

        return true;
    }


    public async Task<bool> DeleteAsync(int id)
    {
        var manufacturer =
            await _repository.GetByIdAsync(id);

        if (manufacturer == null)
        {
            return false;
        }


        // Do not delete a manufacturer
        // that is used by vehicles.

        if (await _repository.HasVehiclesAsync(id))
        {
            return false;
        }


        _repository.Delete(manufacturer);

        await _repository.SaveChangesAsync();

        return true;
    }
    }
}