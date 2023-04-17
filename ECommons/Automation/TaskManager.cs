﻿using ECommons.DalamudServices;
using ECommons.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ECommons.Automation
{
    public class TaskManager : IDisposable
    {
        private static readonly List<TaskManager> Instances = new();
        public int TimeLimitMS = 10000;
        public bool AbortOnTimeout = false;
        public long AbortAt { get; private set; } = 0;
        TaskManagerTask CurrentTask = null;
        public int NumQueuedTasks => Tasks.Count + ImmediateTasks.Count + (CurrentTask == null ? 0 : 1);
        public bool TimeoutSilently = false;
        Action<string> LogTimeout => TimeoutSilently ? PluginLog.Verbose : PluginLog.Warning;
        string StackTrace => new StackTrace().GetFrames().Select(x => x.GetMethod()?.Name ?? "<unknown>").Join(" <- ");

        Queue<TaskManagerTask> Tasks = new();
        Queue<TaskManagerTask> ImmediateTasks = new();

        public TaskManager()
        {
            Svc.Framework.Update += Tick;
            Instances.Add(this);
        }

        [Obsolete($"Task managers will be disposed automatically on {nameof(ECommonsMain.Dispose)} call. Use this if you need to dispose task manager before that.")]
        public void Dispose()
        {
            Svc.Framework.Update -= Tick;
            Instances.Remove(this);
        }

        internal static void DisposeAll()
        {
            int i = 0;
            foreach(var manager in Instances)
            {
                i++;
                Svc.Framework.Update -= manager.Tick;
            }
            if(i>0)
            {
                PluginLog.Debug($"Auto-disposing {i} task managers");
            }
            Instances.Clear();
        }

        public bool IsBusy => CurrentTask != null || Tasks.Count > 0 || ImmediateTasks.Count > 0;

        public void Enqueue(Func<bool?> task)
        {
            Tasks.Enqueue(new(task, TimeLimitMS, AbortOnTimeout));
        }

        public void Enqueue(Func<bool?> task, int timeLimitMs)
        {
            Tasks.Enqueue(new(task, timeLimitMs, AbortOnTimeout));
        }

        public void Enqueue(Func<bool?> task, bool abortOnTimeout)
        {
            Tasks.Enqueue(new(task, TimeLimitMS, abortOnTimeout));
        }

        public void Enqueue(Func<bool?> task, int timeLimitMs, bool abortOnTimeout)
        {
            Tasks.Enqueue(new(task, timeLimitMs, abortOnTimeout));
        }
        public void Enqueue(Action task)
        {
            Tasks.Enqueue(new(() => { task(); return true; }, TimeLimitMS, AbortOnTimeout));
        }

        public void Enqueue(Action task, int timeLimitMs)
        {
            Tasks.Enqueue(new(() => { task(); return true; }, timeLimitMs, AbortOnTimeout));
        }

        public void Enqueue(Action task, bool abortOnTimeout)
        {
            Tasks.Enqueue(new(() => { task(); return true; }, TimeLimitMS, abortOnTimeout));
        }

        public void Enqueue(Action task, int timeLimitMs, bool abortOnTimeout)
        {
            Tasks.Enqueue(new(() => { task(); return true; }, timeLimitMs, abortOnTimeout));
        }

        public void Abort()
        {
            Tasks.Clear();
            ImmediateTasks.Clear();
            CurrentTask = null;
        }

        public void EnqueueImmediate(Func<bool?> task)
        {
            ImmediateTasks.Enqueue(new(task, TimeLimitMS, AbortOnTimeout));
        }

        public void EnqueueImmediate(Func<bool?> task, int timeLimitMs)
        {
            ImmediateTasks.Enqueue(new(task, timeLimitMs, AbortOnTimeout));
        }

        public void EnqueueImmediate(Func<bool?> task, bool abortOnTimeout)
        {
            ImmediateTasks.Enqueue(new(task, TimeLimitMS, abortOnTimeout));
        }

        public void EnqueueImmediate(Func<bool?> task, int timeLimitMs, bool abortOnTimeout)
        {
            ImmediateTasks.Enqueue(new(task, timeLimitMs, abortOnTimeout));
        }

        public void EnqueueImmediate(Action task)
        {
            ImmediateTasks.Enqueue(new(() => { task(); return true; }, TimeLimitMS, AbortOnTimeout));
        }

        public void EnqueueImmediate(Action task, int timeLimitMs)
        {
            ImmediateTasks.Enqueue(new(() => { task(); return true; }, timeLimitMs, AbortOnTimeout));
        }

        public void EnqueueImmediate(Action task, bool abortOnTimeout)
        {
            ImmediateTasks.Enqueue(new(() => { task(); return true; }, TimeLimitMS, abortOnTimeout));
        }

        public void EnqueueImmediate(Action task, int timeLimitMs, bool abortOnTimeout)
        {
            ImmediateTasks.Enqueue(new(() => { task(); return true; }, timeLimitMs, abortOnTimeout));
        }

        void Tick(object _)
        {
            if (CurrentTask == null)
            {
                if (ImmediateTasks.TryDequeue(out CurrentTask))
                {
                    PluginLog.Debug($"Starting to execute immediate task: {CurrentTask.Action.GetMethodInfo()?.Name}");
                    AbortAt = Environment.TickCount64 + CurrentTask.TimeLimitMS;
                }
                else if (Tasks.TryDequeue(out CurrentTask))
                {
                    PluginLog.Debug($"Starting to execute task: {StackTrace}");
                    AbortAt = Environment.TickCount64 + CurrentTask.TimeLimitMS;
                }
            }
            else
            {
                try
                {
                    var result = CurrentTask.Action();
                    if (result == true)
                    {
                        PluginLog.Debug($"Task {StackTrace} completed successfully");
                        CurrentTask = null; 
                    }
                    else if(result == false)
                    {
                        if (Environment.TickCount64 > AbortAt)
                        {
                            if (CurrentTask.AbortOnTimeout)
                            {
                                LogTimeout($"Clearing {Tasks.Count} remaining tasks because of timeout");
                                Tasks.Clear();
                                ImmediateTasks.Clear();
                            }
                            throw new TimeoutException($"Task {StackTrace} took too long to execute");
                        }
                    }
                    else
                    {
                        PluginLog.Warning($"Clearing {Tasks.Count} remaining tasks because there was a signal from task {CurrentTask.Action.GetMethodInfo()?.Name} to abort");
                        Abort();
                    }
                }
                catch (TimeoutException e)
                {
                    LogTimeout($"{e.Message}\n{e.StackTrace}");
                    CurrentTask = null;
                }
                catch (Exception e)
                {
                    e.Log();
                    CurrentTask = null;
                }
            }
        }
    }
}
