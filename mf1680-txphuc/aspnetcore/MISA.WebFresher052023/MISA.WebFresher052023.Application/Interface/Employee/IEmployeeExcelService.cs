﻿using MISA.WebFresher052023.Application.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MISA.WebFresher052023.Application
{
    public interface IEmployeeExcelService : IExcelService<EmployeeExcelDto, EmployeeExcelInsertDto>
    {
    }
}
