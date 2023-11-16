using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CBC.Shared;
public class QueueUser
{
   //public string ConnectionId { get; set; }
    public int Age { get; set; }
    public bool IsFemale { get; set; }
    public int MinAge { get; set; }
    public int MaxAge { get; set; }
    public bool AcceptMale { get; set; }
    public bool AcceptFemale { get; set; }
}
