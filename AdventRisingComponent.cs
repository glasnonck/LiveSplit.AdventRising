using LiveSplit.Model;
using LiveSplit.TimeFormatters;
using LiveSplit.UI.Components;
using LiveSplit.UI;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Xml;
using System.Windows.Forms;
using System.Diagnostics;

namespace LiveSplit.AdventRising
{
    class AdventRisingComponent : LogicComponent
    {
        public override string ComponentName
        {
            get { return "AdventRising"; }
        }

        private TimerModel _timer;
        private GameMemory _gameMemory;

        public AdventRisingComponent(LiveSplitState state)
        {
            _timer = new TimerModel { CurrentState = state };

            _gameMemory = new GameMemory();
            _gameMemory.OnLoadStarted += gameMemory_OnLoadStarted;
            _gameMemory.OnLoadFinished += gameMemory_OnLoadFinished;
            _gameMemory.StartMonitoring();
        }

        public override void Dispose()
        {
            if (_gameMemory != null)
            {
                _gameMemory.Stop();
            }

        }

        void gameMemory_OnLoadStarted(object sender, EventArgs e)
        {
            _timer.CurrentState.IsGameTimePaused = true;
        }

        void gameMemory_OnLoadFinished(object sender, EventArgs e)
        {
            _timer.CurrentState.IsGameTimePaused = false;
        }

        public override void Update(IInvalidator invalidator, LiveSplitState state, float width, float height, LayoutMode mode) { }
        public override XmlNode GetSettings(XmlDocument document) { return document.CreateElement("Settings"); }
        public override Control GetSettingsControl(LayoutMode mode) { return null;  }
        public override void SetSettings(XmlNode settings) { }
    }
}
