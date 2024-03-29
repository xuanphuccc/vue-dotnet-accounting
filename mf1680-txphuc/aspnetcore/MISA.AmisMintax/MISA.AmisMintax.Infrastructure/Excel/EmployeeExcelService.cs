﻿using AutoMapper;
using MISA.AmisMintax.Application;
using MISA.AmisMintax.Domain;
using MISA.AmisMintax.Domain.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MISA.AmisMintax.Infrastructure
{
    public class EmployeeExcelService : ExcelCore, IEmployeeExcelService
    {
        #region Fields
        private readonly IEmployeeRepository _employeeRepository;
        private readonly IEmployeeRelationshipRepository _employeeRelationshipRepository;
        private readonly IMapper _mapper;
        #endregion

        #region Fields
        public EmployeeExcelService(IEmployeeRepository employeeRepository, IEmployeeRelationshipRepository employeeRelationshipRepository, IMapper mapper)
        {
            _employeeRepository = employeeRepository;
            _employeeRelationshipRepository = employeeRelationshipRepository;
            _mapper = mapper;
        }
        #endregion

        #region Methods
        /// <summary>
        /// Xuất toàn bộ dữ liệu ra file Excel
        /// </summary>
        /// <returns>Mảng bytes của file Excel</returns>
        /// CreatedBy: txphuc (28/08/2023)
        public async Task<byte[]> ExportAllToExcelAsync(List<ExcelExportSheetDto> exportSheetDtos)
        {
            foreach (var exportSheet in exportSheetDtos)
            {
                if (exportSheet.SheetKey == "Employee")
                {
                    var employees = await _employeeRepository.GetAllAsync();

                    var employeeDtos = _mapper.Map<List<EmployeeDto>>(employees);

                    // Thêm trang tính nhân viên
                    AddWorkSheet(employeeDtos, exportSheet);
                }
                if (exportSheet.SheetKey == "EmployeeRelationship")
                {
                    var employeeRelationships = await _employeeRelationshipRepository.GetAllAsync();

                    var employeeDtos = _mapper.Map<List<EmployeeRelationshipDto>>(employeeRelationships);

                    // Thêm trang tính nhân viên
                    AddWorkSheet(employeeDtos, exportSheet);
                }
            }

            // Lấy mảng bytes của file Excel
            var bytes = GetExcelBytes();

            return bytes;
        }

        /// <summary>
        /// Xuất danh sách theo ID ra file Excel
        /// </summary>
        /// <returns>Mảng bytes của file Excel</returns>
        /// CreatedBy: txphuc (28/08/2023)
        public async Task<byte[]> ExportListToExcelAsync(List<Guid> employeeIds, List<ExcelExportSheetDto> exportSheetDtos)
        {
            foreach (var exportSheet in exportSheetDtos)
            {
                if (exportSheet.SheetKey == "Employee")
                {
                    var employees = await _employeeRepository.GetListByIdsAsync(employeeIds);

                    var employeeDtos = _mapper.Map<List<EmployeeDto>>(employees);

                    // Thêm trang tính nhân viên
                    AddWorkSheet(employeeDtos, exportSheet);
                }
                if (exportSheet.SheetKey == "EmployeeRelationship")
                {
                    var employeeRelationships = await _employeeRelationshipRepository.GetListByEmployeeIdAsync(employeeIds);

                    var employeeDtos = _mapper.Map<List<EmployeeRelationshipDto>>(employeeRelationships);

                    // Thêm trang tính nhân viên
                    AddWorkSheet(employeeDtos, exportSheet);
                }
            }

            // Lấy mảng bytes của file Excel
            var bytes = GetExcelBytes();

            return bytes;
        } 
        #endregion
    }
}
