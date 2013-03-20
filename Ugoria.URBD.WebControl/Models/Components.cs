using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Ugoria.URBD.WebControl.Models
{
    public interface IComponentsRepository
    {
        IEnumerable<IComponent> GetComponents();
    }

    public class ComponentsRepository : IComponentsRepository
    {
        private URBD2Entities dataContext;
        public ComponentsRepository(URBD2Entities dataContext)
        {
            this.dataContext = dataContext;
        }

        public IEnumerable<IComponent> GetComponents()
        {
            return dataContext.Component.OrderBy(c => c.component_id).Select(c => c);
        }
    }

    public interface IComponent
    {
        int ComponentId { get; }
        string Name { get; }
        string Description { get; }
    }

    public partial class Component : IComponent
    {
        public int ComponentId
        {
            get { return component_id; }
        }

        public string Name
        {
            get { return name; }
        }

        public string Description
        {
            get { return description; }
        }
    }
}