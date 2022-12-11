using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace A2Test.Class
{
    internal class WoodDeal
    {
        public Data data { get; set; }
    }

    public class Deal
    {
        [Key]
        public string dealNumber { get; set; }
        public string sellerName { get; set; }
        public string sellerInn { get; set; }
        public string buyerName { get; set; }
        public string buyerInn { get; set; }
        public string dealDate { get; set; }
        public double woodVolumeSeller { get; set; }
        public double woodVolumeBuyer { get; set; }
        public string __typename { get; set; }
    }

    public class Data
    {
        public SearchReportWoodDeal searchReportWoodDeal { get; set; }
    }

    public class SearchReportWoodDeal
    {
        public List<Deal> content { get; set; }
        public string __typename { get; set; }
    }
}
