using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OM.UnitTests.TestsModels;
public class DiscountTestScenario
{
    public bool VipApplicable { get; set; }
    public bool LoyaltyApplicable { get; set; }
    public bool BulkApplicable { get; set; }
    public decimal VipAmount { get; set; }
    public decimal LoyaltyAmount { get; set; }
    public decimal BulkAmount { get; set; }
    public decimal ExpectedTotal { get; set; }
    public string Description { get; set; } = string.Empty;
}
