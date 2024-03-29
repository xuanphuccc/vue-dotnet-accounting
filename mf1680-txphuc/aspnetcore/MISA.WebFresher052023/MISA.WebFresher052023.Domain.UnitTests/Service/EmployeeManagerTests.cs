﻿using NSubstitute;
using NSubstitute.ExceptionExtensions;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MISA.WebFresher052023.Domain.UnitTests
{
    [TestFixture]
    public class EmployeeManagerTests
    {
        #region Fields
        private IDepartmentRepository _departmentRepository;
        private IPositionRepository _positionRepository;
        private IEmployeeRepository _employeeRepository;
        private EmployeeManager _employeeManager;
        #endregion

        #region Methods
        [SetUp]
        public void Setup()
        {
            _departmentRepository = Substitute.For<IDepartmentRepository>();
            _positionRepository = Substitute.For<IPositionRepository>();
            _employeeRepository = Substitute.For<IEmployeeRepository>();
            _employeeManager = new EmployeeManager(_employeeRepository, _departmentRepository, _positionRepository);
        }

        /// <summary>
        /// Trường hợp mã nhân viên đã tồn tại và khác với mã cũ của nó
        /// </summary>
        /// <returns>Trả về ConflictException</returns>
        /// CreatedBy: txphuc (24/07/2023)
        [Test]
        public async Task CheckExistEmployeeCode_ExistEmployee_ConflictException()
        {
            // Arrange
            var code = "txphuc";
            var oldCode = "unittest"; // Mã cũ (trong trường hợp cập nhật mã)

            // Giá trị trả về khi gọi FindByCodeAsync
            _employeeRepository.FindByCodeAsync(code).Returns(new Employee() { EmployeeCode = code });

            // Act & Assert
            Assert.ThrowsAsync<ConflictException>(async () =>
            await _employeeManager.CheckExistEmployeeCode(code, oldCode));

            // Đảm bảo FindByCodeAsync chỉ được gọi 1 lần
            await _employeeRepository.Received(1).FindByCodeAsync(code);
        }

        /// <summary>
        /// Trường hợp mã nhân viên chưa tồn tại
        /// </summary>
        /// <returns>Validate thành công</returns>
        /// CreatedBy: txphuc (24/07/2023)
        [Test]
        public async Task CheckExistEmployeeCode_NotExistEmployee_Success()
        {
            // Arrange
            var code = "txphuc";

            _employeeRepository.FindByCodeAsync(code).Returns(Task.FromResult<Employee?>(null));

            // Act
            await _employeeManager.CheckExistEmployeeCode(code);

            // Assert
            await _employeeRepository.Received(1).FindByCodeAsync(code);
        }

        /// <summary>
        /// Trường hợp đầu vào hợp lệ
        /// </summary>
        /// <returns></returns>
        /// CreatedBy: txphuc (24/07/2023)
        [Test]
        public async Task CheckValidConstraint_ValidInput_Success()
        {
            // Arrange
            var departmentId = Guid.NewGuid();
            var positionId = Guid.NewGuid();

            _departmentRepository.GetByIdAsync(departmentId).Returns(new Department());
            _positionRepository.GetByIdAsync(positionId).Returns(new Position());

            // Act
            await _employeeManager.CheckValidConstraint(departmentId, positionId);

            // Assert
            await _departmentRepository.Received(1).GetByIdAsync(departmentId);
            await _positionRepository.Received(1).GetByIdAsync(positionId);
        }

        /// <summary>
        /// Trường hợp đầu vào không tồn tại
        /// </summary>
        /// <returns></returns>
        /// CreatedBy: txphuc (24/07/2023)
        [Test]
        public void CheckValidConstraint_InValidInput_ThrowException()
        {
            // Arrange
            var departmentId = Guid.NewGuid();
            var positionId = Guid.NewGuid();

            _departmentRepository.GetByIdAsync(departmentId).ThrowsAsync(new NotFoundException());
            _positionRepository.GetByIdAsync(positionId).ThrowsAsync(new NotFoundException());

            // Act & Assert
            Assert.ThrowsAsync<NotFoundException>(async () => await _employeeManager.CheckValidConstraint(departmentId, positionId));
        } 
        #endregion
    }
}
