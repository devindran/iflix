using System.Collections.Generic;

namespace iflix.models
{
    public class UserModel
    {
        public string number { get; set;}

        public string name { get; set;}

        public string partner { get; set;}
        
        public List<UserOfferModel> offers { get; set;}

        public UserModel()
        {
            offers = new List<UserOfferModel>();
        }
    }
}