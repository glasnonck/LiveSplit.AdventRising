using System.Reflection;
using LiveSplit.AdventRising;
using LiveSplit.UI.Components;
using System;
using LiveSplit.Model;

[assembly: ComponentFactory(typeof(AdventRisingFactory))]

namespace LiveSplit.AdventRising
{
    class AdventRisingFactory : IComponentFactory
    {
        public string ComponentName
        {
            get { return "AdventRising"; }
        }

        public string Description
        {
            get { return "Automates load removal for Advent Rising."; }
        }

        public ComponentCategory Category
        {
            get { return ComponentCategory.Control; }
        }

        public IComponent Create(LiveSplitState state)
        {
            return new AdventRisingComponent(state);
        }

        public string UpdateName
        {
            get { return this.ComponentName; }
        }

        public string UpdateURL
        {
            get { return "https://raw.githubusercontent.com/glasnonck/LiveSplit.AdventRising/master/"; }
        }

        public Version Version
        {
            get { return Assembly.GetExecutingAssembly().GetName().Version; }
        }

        public string XMLURL
        {
            get { return this.UpdateURL + "Components/update.LiveSplit.AdventRising.xml"; }
        }
    }
}
