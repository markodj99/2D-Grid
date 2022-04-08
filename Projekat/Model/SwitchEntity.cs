using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfApp1.Model
{
    public class SwitchEntity : PowerEntity
    {
        private string _status;

        public string Status { get => _status; set => _status = value; }
    }
}
