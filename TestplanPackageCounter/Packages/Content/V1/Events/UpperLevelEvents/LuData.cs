namespace TestplanPackageCounter.Packages.Content.V1.Events.UpperLevelEvents
{
    using System.Collections.Generic;

    public class LuData : RequestJsonContainerV1
    {        
        public Dictionary<int, LuEvent> LuEvents { get; set; }
    }
}
