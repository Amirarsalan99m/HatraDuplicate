﻿using EFSecondLevelCache.Core;
using Hatra.Common.GuardToolkit;
using Hatra.DataLayer.Context;
using Hatra.Entities;
using Hatra.Services.Contracts;
using Hatra.ViewModels;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Hatra.Services
{
    public class PictureService : IPictureService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly DbSet<Picture> _pictures;

        public PictureService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
            _unitOfWork.CheckArgumentIsNull(nameof(_unitOfWork));

            _pictures = _unitOfWork.Set<Picture>();
            _pictures.CheckArgumentIsNull(nameof(_pictures));
        }

        public async Task<List<PictureViewModel>> GetAllAsync()
        {
            return await _pictures
                .Select(p => new PictureViewModel(p))
                .Cacheable()
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<List<PictureViewModel>> GetAllByFolderIdAsync(int folderId)
        {
            return await _pictures
                .Include(p => p.Folder)
                .Where(p => p.FolderId == folderId)
                .Select(p => new PictureViewModel(p))
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<PictureViewModel> GetByIdAsync(int id)
        {
            var entity = await _pictures.FirstOrDefaultAsync(p => p.Id == id);

            if (entity != null)
            {
                return new PictureViewModel(entity);
            }

            return null;
        }

        public PictureViewModel GetByName(string name)
        {
            var entity = _pictures.FirstOrDefault(p => p.Name == name);

            if (entity != null)
            {
                return new PictureViewModel(entity);
            }

            return null;
        }

        public async Task<PictureViewModel> GetByNameAsync(string name)
        {
            var entity = await _pictures.FirstOrDefaultAsync(p => p.Name == name);

            if (entity != null)
            {
                return new PictureViewModel(entity);
            }

            return null;
        }

        public async Task<AuditableInformationViewModel> GetAuditableInformationByIdAsync(int id)
        {
            var query = _pictures
                .Where(p => p.Id == id)
                .Select(p => new AuditableInformationViewModel()
                {
                    CreatedByBrowserName = EF.Property<string>(p, nameof(AuditableInformationViewModel.CreatedByBrowserName)) ?? "-",
                    ModifiedByBrowserName = EF.Property<string>(p, nameof(AuditableInformationViewModel.ModifiedByBrowserName)) ?? "-",
                    CreatedByIp = EF.Property<string>(p, nameof(AuditableInformationViewModel.CreatedByIp)) ?? "-",
                    ModifiedByIp = EF.Property<string>(p, nameof(AuditableInformationViewModel.ModifiedByIp)) ?? "-",
                    CreatedByUserId = EF.Property<int?>(p, nameof(AuditableInformationViewModel.CreatedByUserId)),
                    ModifiedByUserId = EF.Property<int?>(p, nameof(AuditableInformationViewModel.ModifiedByUserId)),
                    CreatedDateTime = EF.Property<DateTimeOffset?>(p, nameof(AuditableInformationViewModel.CreatedDateTime)),
                    ModifiedDateTime = EF.Property<DateTimeOffset?>(p, nameof(AuditableInformationViewModel.ModifiedDateTime)),
                })
                .AsNoTracking();

            return await query.FirstOrDefaultAsync();
        }

        public async Task<bool> InsertAsync(PictureViewModel viewModel)
        {
            var entity = new Picture()
            {
                Id = viewModel.Id,
                Path = viewModel.Path,
                FolderId = viewModel.FolderId,

                Name = viewModel.Name,
                Size = viewModel.Size,
                Type = viewModel.Type,
                Url = viewModel.Url,
                DeleteUrl = viewModel.DeleteUrl,
                ThumbnailUrl = viewModel.ThumbnailUrl,
                DeleteType = viewModel.DeleteType,
                Extension = viewModel.Extension,
            };

            await _pictures.AddAsync(entity);
            var result = await _unitOfWork.SaveChangesAsync();
            return result != 0;
        }

        public async Task<bool> InsertAllAsync(List<PictureViewModel> viewModels)
        {
            foreach (var viewModel in viewModels)
            {
                var entity = new Picture()
                {
                    Id = viewModel.Id,
                    Path = viewModel.Path,
                    FolderId = viewModel.FolderId,

                    Name = viewModel.Name,
                    Size = viewModel.Size,
                    Type = viewModel.Type,
                    Url = viewModel.Url,
                    DeleteUrl = viewModel.DeleteUrl,
                    ThumbnailUrl = viewModel.ThumbnailUrl,
                    DeleteType = viewModel.DeleteType,
                    Extension = viewModel.Extension,
                };

                await _pictures.AddAsync(entity);
            }

            var result = await _unitOfWork.SaveChangesAsync();
            return result != 0;
        }

        public async Task<bool> UpdateAsync(PictureViewModel viewModel)
        {
            var entity = await _pictures.FirstOrDefaultAsync(p => p.Id == viewModel.Id);

            if (entity != null)
            {
                entity.Path = viewModel.Path;
                entity.FolderId = viewModel.FolderId;

                entity.Name = viewModel.Name;
                entity.Size = viewModel.Size;
                entity.Type = viewModel.Type;
                entity.Url = viewModel.Url;
                entity.DeleteUrl = viewModel.DeleteUrl;
                entity.ThumbnailUrl = viewModel.ThumbnailUrl;
                entity.DeleteType = viewModel.DeleteType;
                entity.Extension = viewModel.Extension;

                var result = await _unitOfWork.SaveChangesAsync();
                return result != 0;
            }

            return await Task.FromResult(false);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var entity = await _pictures.FirstOrDefaultAsync(p => p.Id == id);

            if (entity != null)
            {
                _pictures.Remove(entity);
                var result = await _unitOfWork.SaveChangesAsync();
                return result != 0;
            }

            return await Task.FromResult(false);
        }

        public async Task<(bool isSuccess, string pictureName)> DeleteInTupleAsync(int id)
        {
            var entity = await _pictures.FirstOrDefaultAsync(p => p.Id == id);

            if (entity != null)
            {
                var picName = entity.Name;

                _pictures.Remove(entity);
                var result = await _unitOfWork.SaveChangesAsync();
                return (result != 0, picName);
            }

            return await Task.FromResult((false, ""));
        }
    }
}
