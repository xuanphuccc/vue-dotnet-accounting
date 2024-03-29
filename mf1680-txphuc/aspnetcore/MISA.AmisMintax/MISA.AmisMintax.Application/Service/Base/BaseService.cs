﻿using AutoMapper;
using MISA.AmisMintax.Domain;
using MISA.AmisMintax.Domain.Resources.Common;
using MISA.AmisMintax.Domain.Resources.ErrorMessage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MISA.AmisMintax.Application
{
    public abstract class BaseService<TEntity, TModel, TEntityDto, TEntityCreateDto, TEntityUpdateDto> :
        BaseReadOnlyService<TEntity, TModel, TEntityDto>,
        IBaseService<TEntityDto, TEntityCreateDto, TEntityUpdateDto> where TEntity : IHasKey
    {
        #region Fields
        protected readonly IBaseRepository<TEntity, TModel> _baseRepository;
        protected readonly IUnitOfWork _unitOfWork;
        #endregion

        #region Constructor
        protected BaseService(
            IBaseRepository<TEntity, TModel> baseRepository,
            IUnitOfWork unitOfWork,
            IMapper mapper
            ) : base(baseRepository, mapper)
        {
            _baseRepository = baseRepository;
            _unitOfWork = unitOfWork;
        }
        #endregion

        #region Methods
        /// <summary>
        /// Tạo đối tượng mới
        /// </summary>
        /// <param name="entityCreateDto">Thông tin đối tượng</param>
        /// CreatedBy: txphuc (18/07/2023)
        public virtual async Task<Guid> InsertAsync(TEntityCreateDto entityCreateDto)
        {
            try
            {
                // Mở một transaction
                await _unitOfWork.BeginTransactionAsync();

                var entity = await MapCreateDtoToEntityAsync(entityCreateDto);

                if (entity is BaseAuditEntity baseAuditEntity)
                {
                    baseAuditEntity.CreatedDate = DateTime.Now;
                    baseAuditEntity.CreatedBy = "txphuc";
                    baseAuditEntity.ModifiedDate = DateTime.Now;
                    baseAuditEntity.ModifiedBy = "txphuc";
                }

                var result = await _baseRepository.InsertAsync(entity);

                // Insert các bảng phụ (nếu có)
                await InsertDetailTableAsync(entityCreateDto, entity.GetKey());

                // Xác nhận thay đổi
                await _unitOfWork.CommitAsync();

                return result;
            }
            catch (Exception)
            {
                // Hoàn tác thay đổi
                await _unitOfWork.RollBackAsync();

                throw;
            }
        }

        /// <summary>
        /// Cập nhật đối tượng
        /// </summary>
        /// <param name="entityId">Id đối tượng</param>
        /// <param name="entityUpdateDto">Thông tin đối tượng</param>
        /// CreatedBy: txphuc (18/07/2023)
        public virtual async Task<int> UpdateAsync(Guid entityId, TEntityUpdateDto entityUpdateDto)
        {
            try
            {
                // Mở một transaction
                await _unitOfWork.BeginTransactionAsync();

                var entity = await MapUpdateDtoToEntityAsync(entityId, entityUpdateDto);

                if (entity is BaseAuditEntity baseAuditEntity)
                {
                    baseAuditEntity.ModifiedDate = DateTime.Now;
                    baseAuditEntity.ModifiedBy = "txphuc";
                }

                var result = await _baseRepository.UpdateAsync(entity);

                // Cập nhật bảng phụ (nếu có)
                if (result > 0)
                {
                    await UpdateDetailTableAsync(entityUpdateDto, entity.GetKey());
                }

                // Xác nhận thay đổi
                await _unitOfWork.CommitAsync();

                return result;
            }
            catch (Exception)
            {
                // Hoàn tác thay đổi
                await _unitOfWork.RollBackAsync();

                throw;
            }
        }

        /// <summary>
        /// Cập nhật nhiều đối tượng
        /// </summary>
        /// <param name="entityUpdateDtos">Danh sách đối tượng cần update</param>
        /// <returns>Số bản ghi bị ảnh hưởng</returns>
        /// CreatedBy: txphuc (29/08/2023)
        public async Task<int> UpdateMultipleAsync(IEnumerable<TEntityUpdateDto> entityUpdateDtos)
        {
            var entities = await MapUpdateMultipleAsync(entityUpdateDtos);

            // Cập nhật ngày, người chỉnh sửa
            foreach (var entity in entities)
            {
                if (entity is BaseAuditEntity baseAuditEntity)
                {
                    baseAuditEntity.ModifiedDate = DateTime.Now;
                    baseAuditEntity.ModifiedBy = "txphuc";
                }
            }

            var result = await _baseRepository.UpdateMultipleAsync(entities);

            return result;
        }

        /// <summary>
        /// Xoá đối tượng theo Id
        /// </summary>
        /// <param name="entityId">Id của đối tượng</param>
        /// CreatedBy: txphuc (18/07/2023)
        public virtual async Task<int> DeleteAsync(Guid entityId)
        {
            // Check đối tượng có tồn tại hay không
            var existEntity = await _baseRepository.GetByIdAsync(entityId);

            // Check các bản ghi phụ thuộc
            await CheckConstraintForDeleteAsync(entityId);

            var result = await _baseRepository.DeleteAsync(existEntity);

            return result;
        }

        /// <summary>
        /// Xoá nhiều đối tượng
        /// </summary>
        /// <param name="entityIds">Danh sách Id của các đối tượng cần xoá</param>
        /// <returns>Số bản ghi bị ảnh hưởng</returns>
        /// CreatedBy: txphuc (18/07/2023)
        public virtual async Task<int> DeleteMultipleAsync(List<Guid> entityIds)
        {
            try
            {
                //await _unitOfWork.BeginTransactionAsync();

                // Lấy các bản ghi có phụ thuộc
                var invalidEntities = await CheckConstraintForDeleteManyAsync(entityIds);

                // Lấy các id không hợp lệ
                var invalidIds = invalidEntities.Select(entity => entity.GetKey());

                // Lấy các Id hợp lệ có thể xoá
                var validIds = entityIds;

                if (invalidEntities != null && invalidEntities.ToList().Count > 0)
                {
                    // Loại bỏ các id không hợp lệ nếu có
                    validIds = entityIds.Where(entityId => !invalidIds.Contains(entityId)).ToList();
                }

                // Xoá các bản ghi hợp lệ
                var result = await _baseRepository.DeleteMultipleAsync(validIds);

                // Trả về lỗi các mã không hợp lệ nếu có
                if (invalidEntities != null && invalidEntities.ToList().Count > 0)
                {
                    var invalidCodes = invalidEntities.Select(entity =>
                    {
                        if (entity is IHasCode entityHasCode)
                        {
                            return entityHasCode.GetCode();
                        }
                        else
                        {
                            return "";
                        }
                    });

                    var errorEntityCodes = String.Join(", ", invalidCodes.ToList());

                    var errorMessage = String.Format(ErrorMessage.ConstraintError, errorEntityCodes);

                    throw new ConstraintException(errorMessage, ErrorCode.ConstraintError);
                }

                //await _unitOfWork.CommitAsync();

                return result;
            }
            catch (ConstraintException)
            {
                //await _unitOfWork.RollBackAsync();
                throw;
            }
        }

        /// <summary>
        /// Validate nghiệp vụ cho Insert
        /// </summary>
        /// <param name="entityCreateDto">CreateDto</param>
        /// <returns>Entity</returns>
        /// CreatedBy: txphuc (18/07/2023)
        protected abstract Task<TEntity> MapCreateDtoToEntityAsync(TEntityCreateDto entityCreateDto);

        /// <summary>
        /// Validate nghiệp vụ cho Update
        /// </summary>
        /// <param name="entityId">Id của bản ghi</param>
        /// <param name="entityUpdateDto">UpdateDto</param>
        /// <returns>Entity</returns>
        /// CreatedBy: txphuc (18/07/2023)
        protected abstract Task<TEntity> MapUpdateDtoToEntityAsync(Guid entityId, TEntityUpdateDto entityUpdateDto);

        /// <summary>
        /// Xử lý validate cho update nhiều bản ghi
        /// </summary>
        /// <param name="entityUpdateDtos">Danh sách các đối tượng cần update</param>
        /// <returns>Danh sách các đối tượng đã được validate</returns>
        /// CreatedBy: txphuc (23/08/2023)
        protected virtual Task<IEnumerable<TEntity>> MapUpdateMultipleAsync(IEnumerable<TEntityUpdateDto> entityUpdateDtos)
        {
            var entities = _mapper.Map<IEnumerable<TEntity>>(entityUpdateDtos);

            return Task.FromResult(entities);
        }

        /// <summary>
        /// Kiểm tra có bản ghi phụ thuộc hay không (cho trường hợp xoá nhiều)
        /// </summary>
        /// <param name="entityIds">Danh sách Id của bản ghi</param>
        /// <returns></returns>
        /// CreatedBy: txphuc (18/07/2023)
        protected virtual Task<IEnumerable<TEntity>> CheckConstraintForDeleteManyAsync(List<Guid> entityIds)
        {
            return Task.FromResult(Enumerable.Empty<TEntity>());
        }

        /// <summary>
        /// Kiểm tra có bản ghi phụ thuộc hay không (cho trường hợp xoá một bản ghi)
        /// </summary>
        /// <param name="entityId">Id của bản ghi</param>
        /// <returns></returns>
        /// CreatedBy: txphuc (18/07/2023)
        protected virtual Task CheckConstraintForDeleteAsync(Guid entityId)
        {
            return Task.CompletedTask;
        }

        /// <summary>
        /// Insert các bảng phụ (nếu có)
        /// </summary>
        /// <param name="entityCreateDto">Data</param>
        /// <param name="entityId">Id của bảng chính</param>
        /// <returns></returns>
        /// CreatedBy: txphuc (23/08/2023)
        protected virtual Task InsertDetailTableAsync(TEntityCreateDto entityCreateDto, Guid entityId)
        {
            return Task.CompletedTask;
        }

        /// <summary>
        /// Update các bảng phụ (nếu có)
        /// </summary>
        /// <param name="entityCreateDto">Data</param>
        /// <param name="entityId">Id của bảng chính</param>
        /// <returns></returns>
        /// CreatedBy: txphuc (23/08/2023)
        protected virtual Task UpdateDetailTableAsync(TEntityUpdateDto entityUpdateDto, Guid entityId)
        {
            return Task.CompletedTask;
        }
        #endregion
    }
}
