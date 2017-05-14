using System;

namespace iflix.models
{
    public class ActionModel
    {
        public string number { get; set;}
        public DateTime date { get; set;}
        public int? period { get; set;}

        public string action { get; set;} 

        public string partner { get; set;}

    }
}