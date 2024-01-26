using System.ComponentModel;

namespace Wanadi.MySql.Examples.Enums;

public enum Status
{
	[Description("Status 1")]
	Status1 = 1,

    [Description("Status 2")]
    Status2 = 2,

    [Description("Status 3")]
    Status3 = 3,
}