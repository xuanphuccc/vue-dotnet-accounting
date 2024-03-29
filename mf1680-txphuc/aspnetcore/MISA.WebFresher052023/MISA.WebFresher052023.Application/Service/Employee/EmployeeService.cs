﻿using AutoMapper;
using MISA.WebFresher052023.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MISA.WebFresher052023.Application
{
    public class EmployeeService : BaseCodeService<Employee, EmployeeModel, EmployeeDto, EmployeeCreateDto, EmployeeUpdateDto>, IEmployeeService
    {
        #region Fields
        private readonly IEmployeeRepository _employeeRepository;
        private readonly IEmployeeManager _employeeManager;

        #endregion

        #region Constructors
        public EmployeeService(
            IEmployeeRepository employeeRepository,
            IEmployeeManager employeeManager,
            IUnitOfWork unitOfWork,
            IMapper mapper
            ) : base(employeeRepository, unitOfWork, mapper)
        {
            _employeeRepository = employeeRepository;
            _employeeManager = employeeManager;
        }
        #endregion

        #region Methods
        /// <summary>
        /// Tìm kiếm, filter và phân trang
        /// </summary>
        /// <param name="search">Search theo tên hoặc mã nhân viên</param>
        /// <param name="currentPage">trang hiện tại</param>
        /// <param name="pageSize">Số phần tử trên trang</param>
        /// <returns>Danh sách nhân viên đã được filter và phân trang</returns>
        /// CreatedBy: txphuc (15/07/2023)
        public async Task<Pagination> FilterAsync(EmployeeFilterDto employeeFilterDto)
        {
            var employeeFilterModel = _mapper.Map<EmployeeFilterModel>(employeeFilterDto);

            var pagedEmployees = await _employeeRepository.FilterAsync(employeeFilterModel);

            pagedEmployees.Data = _mapper.Map<IEnumerable<EmployeeDto>>(pagedEmployees.Data);

            return pagedEmployees;
        }

        /// <summary>
        /// Validate nghiệp vụ cho Insert
        /// </summary>
        /// <param name="employeeCreateDto">CreateDto</param>
        /// <returns>Entity</returns>
        /// CreatedBy: txphuc (18/07/2023)
        protected override async Task<Employee> MapCreateDtoToEntityAsync(EmployeeCreateDto employeeCreateDto)
        {
            // Check mã nhân viên tối đa có thể nhập
            await _employeeManager.CheckMaxEmployeeCode(employeeCreateDto.EmployeeCode);

            // Check trùng mã nhân viên
            await _employeeManager.CheckExistEmployeeCode(employeeCreateDto.EmployeeCode);

            // Check departmentId và positionId có tồn tại hay không
            await _employeeManager.CheckValidConstraint(employeeCreateDto.DepartmentId, employeeCreateDto.PositionId);

            var employee = _mapper.Map<Employee>(employeeCreateDto);

            // Set Id cho bản ghi
            employee.EmployeeId = Guid.NewGuid();

            return employee;
        }

        /// <summary>
        /// Validate nghiệp vụ cho Update
        /// </summary>
        /// <param name="employeeId">Id của bản ghi</param>
        /// <param name="employeeUpdateDto">UpdateDto</param>
        /// <returns>Entity</returns>
        /// CreatedBy: txphuc (18/07/2023)
        protected override async Task<Employee> MapUpdateDtoToEntityAsync(Guid employeeId, EmployeeUpdateDto employeeUpdateDto)
        {
            // Check nhân viên có tồn tại hay không
            var oldEmployee = await _employeeRepository.GetByIdAsync(employeeId);

            // Check mã nhân viên tối đa có thể nhập
            await _employeeManager.CheckMaxEmployeeCode(employeeUpdateDto.EmployeeCode);

            // Check trùng mã nhân viên
            await _employeeManager.CheckExistEmployeeCode(employeeUpdateDto.EmployeeCode, oldEmployee.EmployeeCode);

            // Check departmentId và positionId có tồn tại hay không
            await _employeeManager.CheckValidConstraint(employeeUpdateDto.DepartmentId, employeeUpdateDto.PositionId);

            var newEmployee = _mapper.Map(employeeUpdateDto, oldEmployee);

            return newEmployee;
        }
        #endregion
    }
}
