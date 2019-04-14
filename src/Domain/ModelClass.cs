using System;
using Shared;

namespace Domain
{
    public class ModelClass
    {
        public ModelClass()
        {
            RequestData = ServiceLocator.GetService<IRequestData>();
        }

        public IRequestData RequestData { get; set; }
    }

}
