using Ac4.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ac4.Persistencia.DAOs
{
    public interface IConsumDAO
    {
        public List<ConsumDTO> GetAllRecords();
        public void InsertRecord(ConsumDTO record);
    }
}
