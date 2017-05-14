using System;

namespace iflix.models
{
    public class UserOfferModel
    {
        public string partner { get; set;}
        public DateTime startDate { get; set;}

        public DateTime endDate { get; set;}

        public long offer { 
            get {
                var d=  (endDate - startDate).TotalDays;                
                return (long)Math.Floor(d);
            }
        }
    }
}