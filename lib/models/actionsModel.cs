using System.Collections.Generic;

namespace iflix.models
{
    public class ActionsModel
    {
        public List<ActionModel> revocations { get; set;}
        public List<ActionModel> grants { get; set;}
    }
}