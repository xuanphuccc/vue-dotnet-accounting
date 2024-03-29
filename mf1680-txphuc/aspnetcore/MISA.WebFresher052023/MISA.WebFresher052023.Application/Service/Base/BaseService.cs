﻿using AutoMapper;
using MISA.WebFresher052023.Domain;
using MISA.WebFresher052023.Domain.Resources.Common;
using MISA.WebFresher052023.Domain.Resources.ErrorMessage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MISA.WebFresher052023.Application
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
        public virtual async Task<int> InsertAsync(TEntityCreateDto entityCreateDto)
        {
            var entity = await MapCreateDtoToEntityAsync(entityCreateDto);

            if (entity is BaseAuditEntity baseAuditEntity)
            {
                baseAuditEntity.CreatedDate = DateTime.Now;
                baseAuditEntity.CreatedBy = "txphuc";
                baseAuditEntity.ModifiedDate = DateTime.Now;
                baseAuditEntity.ModifiedBy = "txphuc";
            }

            var result = await _baseRepository.InsertAsync(entity);

            return result;
        }

        /// <summary>
        /// Cập nhật đối tượng
        /// </summary>
        /// <param name="entityId">Id đối tượng</param>
        /// <param name="entityUpdateDto">Thông tin đối tượng</param>
        /// CreatedBy: txphuc (18/07/2023)
        public virtual async Task<int> UpdateAsync(Guid entityId, TEntityUpdateDto entityUpdateDto)
        {
            var entity = await MapUpdateDtoToEntityAsync(entityId, entityUpdateDto);

            if (entity is BaseAuditEntity baseAuditEntity)
            {
                baseAuditEntity.ModifiedDate = DateTime.Now;
                baseAuditEntity.ModifiedBy = "txphuc";
            }

            var result = await _baseRepository.UpdateAsync(entity);

            return result;
        }

        /// <summary>
        /// Xoá đối tượng theo Id
        /// </summary>
        /// <param name="entityId">Id của đối tượng</param>
        /// CreatedBy: txphuc (18/07/2023)
        public virtual async Task<int> DeleteByIdAsync(Guid entityId)
        {
            // Check đối tượng có tồn tại hay không
            var existEntity = await _baseRepository.GetByIdAsync(entityId);

            // Check các bản ghi phụ thuộc
            await CheckConstraintForDeleteAsync(entityId);

            var result = await _baseRepository.DeleteByIdAsync(existEntity);

            return result;
        }

        /// <summary>
        /// Xoá nhiều đối tượng
        /// </summary>
        /// <param name="entityIds">Danh sách Id của các đối tượng cần xoá</param>
        /// <returns>Số bản ghi bị ảnh hưởng</returns>
        /// CreatedBy: txphuc (18/07/2023)
        public virtual async Task<int> DeleteAsync(List<Guid> entityIds)
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

                if(invalidEntities != null && invalidEntities.ToList().Count > 0)
                {
                    // Loại bỏ các id không hợp lệ nếu có
                    validIds = entityIds.Where(entityId => !invalidIds.Contains(entityId)).ToList();
                }

                // Xoá các bản ghi hợp lệ
                var result = await _baseRepository.DeleteAsync(validIds);

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
        #endregion
    }
}
