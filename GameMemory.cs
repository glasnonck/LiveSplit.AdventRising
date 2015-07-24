using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace LiveSplit.AdventRising
{
    public class GameMemory
    {
        public event EventHandler OnLoadStarted;
        public event EventHandler OnLoadFinished;

        private Task _thread;
        private CancellationTokenSource _cancelSource;
        private SynchronizationContext _uiThread;
        private List<int> _ignorePIDs;

        private DeepPointer IsNotLoadingPtr { get; set; }

        private enum ExpectedDllSizes
        {
            AdventRisingGoG = 290816,
        }

        public GameMemory()
        {
            IsNotLoadingPtr = new DeepPointer("Engine.dll", 0x657AB0); // == 1 if not loading
            _ignorePIDs = new List<int>();
        }

        public void StartMonitoring()
        {
            if (_thread != null && _thread.Status == TaskStatus.Running)
            {
                throw new InvalidOperationException();
            }
            if (!(SynchronizationContext.Current is WindowsFormsSynchronizationContext))
            {
                throw new InvalidOperationException("SynchronizationContext.Current is not a UI thread.");
            }

            _uiThread = SynchronizationContext.Current;
            _cancelSource = new CancellationTokenSource();
            _thread = Task.Factory.StartNew(MemoryReadThread);
        }

        public void Stop()
        {
            if (_cancelSource == null || _thread == null || _thread.Status != TaskStatus.Running)
            {
                return;
            }

            _cancelSource.Cancel();
            _thread.Wait();
        }

        void MemoryReadThread()
        {
            Debug.WriteLine("[NoLoads] MemoryReadThread");

            while (!_cancelSource.IsCancellationRequested)
            {
                try
                {
                    Debug.WriteLine("[NoLoads] Waiting for advent.exe...");

                    Process game;
                    while ((game = GetGameProcess()) == null)
                    {
                        Thread.Sleep(250);
                        if (_cancelSource.IsCancellationRequested)
                        {
                            return;
                        }
                    }

                    Debug.WriteLine("[NoLoads] Got advent.exe!");

                    uint frameCounter = 0;

                    bool prevIsNotLoading = false;
                    bool loadingStarted = false;

                    while (!game.HasExited)
                    {
                        bool IsNotLoading;
                        IsNotLoadingPtr.Deref(game, out IsNotLoading);

                        if (IsNotLoading != prevIsNotLoading)
                        {
                            if (!IsNotLoading)
                            {
                                Debug.WriteLine(String.Format("[NoLoads] Load Start - {0}", frameCounter));

                                loadingStarted = true;

                                // pause game timer
                                _uiThread.Post(d =>
                                {
                                    if (this.OnLoadStarted != null)
                                    {
                                        this.OnLoadStarted(this, EventArgs.Empty);
                                    }
                                }, null);
                            }
                            else
                            {
                                Debug.WriteLine(String.Format("[NoLoads] Load End - {0}", frameCounter));

                                if (loadingStarted)
                                {
                                    loadingStarted = false;

                                    // unpause game timer
                                    _uiThread.Post(d =>
                                    {
                                        if (this.OnLoadFinished != null)
                                        {
                                            this.OnLoadFinished(this, EventArgs.Empty);
                                        }
                                    }, null);
                                }
                            }
                        }

                        prevIsNotLoading = IsNotLoading;
                        frameCounter++;

                        Thread.Sleep(15);

                        if (_cancelSource.IsCancellationRequested)
                        {
                            return;
                        }
                    }

                    // Once the game has exited, unpause the game timer if necessary
                    if (loadingStarted)
                    {
                        loadingStarted = false;

                        // unpause game timer
                        _uiThread.Post(d =>
                        {
                            if (this.OnLoadFinished != null)
                            {
                                this.OnLoadFinished(this, EventArgs.Empty);
                            }
                        }, null);
                    }

                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.ToString());
                    Thread.Sleep(1000);
                }
            }
        }

        Process GetGameProcess()
        {
            Process game = Process.GetProcesses().FirstOrDefault(p => p.ProcessName.ToLower() == "advent"
                && !p.HasExited && !_ignorePIDs.Contains(p.Id));
            if (game == null)
            {
                return null;
            }

            Debug.WriteLine(String.Format("[NoLoads] Found Advent Rising with size {0}", game.MainModule.ModuleMemorySize));

            //if (game.MainModule.ModuleMemorySize != (int)ExpectedDllSizes.AdventRisingGoG)
            //{
            //    _ignorePIDs.Add(game.Id);
            //    _uiThread.Send(d => MessageBox.Show("Unexpected game version. Advent Rising 1.03.514077 is required.", "LiveSplit.AdventRising",
            //        MessageBoxButtons.OK, MessageBoxIcon.Error), null);
            //    return null;
            //}

            return game;
        }
    }
}
