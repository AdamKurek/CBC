using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CBC.Shared;
public class QueueUser
{
    public bool IsFemale { get; set; }
    public int Age { get; set; }

}

public class UserPreferences
{
    public override string ToString()
    {
        return $"Filters - ConnectionId: {ConnectionId}, MinAge: {MinAge}, MaxAge: {MaxAge}, AcceptMale: {AcceptMale}, AcceptFemale: {AcceptFemale}";
    }

    public UserPreferences(string id) {
        ConnectionId = id;
    }
    public int MinAge { get; set; } = 18;
    public int MaxAge { get; set; } = 60;
    public bool AcceptMale { get; set; } = true;
    public bool AcceptFemale { get; set; } = true;
    public string ConnectionId { get; set; }

}