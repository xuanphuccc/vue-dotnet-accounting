﻿using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MISA.AmisMintax.Application;
using MISA.AmisMintax.Controllers.Base;
using MISA.AmisMintax.Domain;
using MISA.AmisMintax.Domain.Interface;

namespace MISA.AmisMintax.Controllers
{
    [Route("api/v1/[controller]")]
    [ApiController]
    public class EmployeesController : BaseCodeController<EmployeeDto, EmployeeCreateDto, EmployeeUpdateDto>
    {
        private readonly IEmployeeService _employeeService;
        public EmployeesController(IEmployeeService employeeService) : base(employeeService)
        {
            _employeeService = employeeService;
        }

        /// <summary>
        /// Tìm kiếm, filter và phân trang nhân viên
        /// </summary>
        /// <param name="employeeFilterDto">Các param cho filter và phân trang</param>
        /// <returns>Danh sách nhân viên được lọc và phân trang</returns>
        /// CreatedBy: txphuc (15/07/2023)
        [HttpPost("Filter")]
        public async Task<IActionResult> FilterAsync([FromBody] EmployeeFilterDto employeeFilterDto)
        {
            var pagedEmployees = await _employeeService.FilterAsync(employeeFilterDto);

            return Ok(pagedEmployees);
        }

        /// <summary>
        /// Đếm số bản ghi đang được sử dụng
        /// </summary>
        /// <returns>Số bản ghi đang được sử dụng và tổng số bản ghi</returns>
        /// CreatedBy: txphuc (25/08/2023)
        [HttpGet("UsageCount")]
        public async Task<IActionResult> GetUsageCountAsync()
        {
            var usageCount = await _employeeService.GetUsageCountAsync();

            return Ok(usageCount);
        }
    }
}