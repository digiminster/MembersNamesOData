using System;

namespace MNISOData.Models
{
    public class MemberSeniority
    {
        public int MNIS_Id { get; set; }
        public string ListName { get; set; }
        public string DisplayName { get; set; }
        public int LengthOfService { get; set; }
        public DateTime ContinuousElectedDate { get; set; }
        public int? SwornInOrder { get; set; }
    }
}
