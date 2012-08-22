using System;

namespace Dovetail.SDK.ModelMap
{
    public interface ISystemTime
    {
        DateTime Now { get; }
    }

    public class SystemTime : ISystemTime
    {
        public DateTime Now
        {
            get { return DateTime.Now; }
        }
    }
}