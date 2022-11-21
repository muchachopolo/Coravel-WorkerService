using System;
using System.Collections.Generic;
using System.Text;

namespace Coravel.WorkerService.Interfaces
{
    public interface ICustomExcepcionHandler
    {
        public void CatchException(Exception ex);
    }
}
