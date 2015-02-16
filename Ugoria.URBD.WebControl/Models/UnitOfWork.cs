using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Ugoria.URBD.WebControl.Models
{
    public class UnitOfWork:IDisposable
    {
        private URBD2Entities dataContext;

        public URBD2Entities DataContext
        {
            get { return dataContext; }
        }
        public UnitOfWork()
        {
            dataContext = new URBD2Entities();
        }

        public void Commit()
        {
            dataContext.SaveChanges();
        }

        public void Dispose()
        {
            dataContext.Dispose();
        }
    }
}