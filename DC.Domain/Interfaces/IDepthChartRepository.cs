using DC.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DC.Domain.Interfaces
{
    public interface IDepthChartRepository : IRepository<Player>
    {
        // Additional methods specific to Player can be added here
    }
}