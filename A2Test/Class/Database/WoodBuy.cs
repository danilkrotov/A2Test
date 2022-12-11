using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace A2Test.Class.Database
{
    internal class WoodBuy
    {
        [Key]
        public string DealNumber { get; set; }
        public string SellerName { get; set; }
        public string SellerInn { get; set; }
        public string BuyerName { get; set; }
        public string BuyerInn { get; set; }
        public string DealDate { get; set; }
        public double WoodVolumeBuyer { get; set; }
    }
}
