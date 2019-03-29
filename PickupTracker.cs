using Sansar.Script;
using Sansar.Simulation;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace ItemCollector
{
    [RegisterReflective]
    public class PickupTracker : SceneObjectScript
    {
        HttpRequestOptions options;
        public override void Init()
        {
            options = new HttpRequestOptions();
            options.Method = HttpRequestMethod.POST;
            options.Headers = new Dictionary<string, string>()
            {
                {"content-type", "application/json" }
            };
        }

        HashSet<Guid> Collectibles = new HashSet<Guid>();
        Dictionary<Guid, HashSet<Guid>> CollectorProgress = new Dictionary<Guid, HashSet<Guid>>();
        Dictionary<Guid, TimedRun> TimedCollectorProgress = new Dictionary<Guid, TimedRun>();

        public void RecordPickup(AgentPrivate collectorAgent, Guid collectedItemId)
        {
            Guid personaId = collectorAgent.AgentInfo.AvatarUuid;
            HashSet<Guid> collectedItems;
            if (!CollectorProgress.TryGetValue(personaId, out collectedItems))
            {
                collectedItems = new HashSet<Guid>();
            }
            collectedItems.Add(collectedItemId);
            CollectorProgress[personaId] = collectedItems;

            Log.Write(LogLevel.Info, $"You've collected {collectedItems.Count} / {Collectibles.Count} items!");
            if (collectedItems.SetEquals(Collectibles))
            {
                Log.Write(LogLevel.Info, $"Congratulations, you collected all {Collectibles.Count} items!");
            }

            if (collectedItems.SetEquals(Collectibles))
            {
                collectorAgent.SendChat($"Congratulations, you collected all {Collectibles.Count} items!");
            }
            else
            {
                collectorAgent.SendChat($"You've collected {collectedItems.Count} / {Collectibles.Count} items!");
            }
        }

        public void RecordPickupTimed(AgentPrivate collectorAgent, Guid collectedItemId)
        {
            Guid personaId = collectorAgent.AgentInfo.AvatarUuid;
            TimedRun timedRun;
            if (!TimedCollectorProgress.TryGetValue(personaId, out timedRun))
            {
                timedRun = new TimedRun(Collectibles);
            }
            bool isNew = timedRun.CollectItem(collectedItemId);
            TimedCollectorProgress[personaId] = timedRun;

            if (!isNew)
            {
                return;
            }
            if (timedRun.IsFinished())
            {
                collectorAgent.SendChat($"Congratulations, you collected all {Collectibles.Count} items, taking {timedRun.ElapsedTime().TotalSeconds}.{timedRun.ElapsedTime().Milliseconds} seconds!");
                PostResults(personaId, timedRun.ElapsedTime());
            }
            else
            {
                collectorAgent.SendChat($"You've collected {timedRun.Collected()} / {Collectibles.Count}, time: {timedRun.ElapsedTime().TotalSeconds}.{timedRun.ElapsedTime().Milliseconds} seconds");
            }
        }

        public void RegisterPresence(Guid collectibleItemId)
        {
            Collectibles.Add(collectibleItemId);
        }

        private void PostResults(Guid personaId, TimeSpan time)
        {
            options.Body = $"{{\"personaId\":\"{personaId}\",\"milliseconds\": \"{time.TotalMilliseconds}\"}}";
            var result = WaitFor(ScenePrivate.HttpClient.Request, "https://postman-echo.com/post", options) as Sansar.Simulation.HttpClient.RequestData;
            if (result.Success)
            {
                Log.Write(LogLevel.Info, $"{result.Response.Body}");
            }
        }
    }

    class TimedRun
    {
        private Stopwatch watch;
        private HashSet<Guid> collectibles;
        private HashSet<Guid> collected;

        public TimedRun(HashSet<Guid> collectibleSet)
        {
            watch = System.Diagnostics.Stopwatch.StartNew();
            collectibles = collectibleSet;
            collected = new HashSet<Guid>();
        }

        public bool CollectItem(Guid collectedItemId)
        {
            int alreadyCollected = collected.Count;
            collected.Add(collectedItemId);
            CheckProgress();
            return collected.Count > alreadyCollected;
        }

        private void CheckProgress()
        {
            if (!watch.IsRunning)
            {
                return;
            }

            if (collectibles.SetEquals(collected))
            {
                watch.Stop();
            }
        }

        public bool IsFinished()
        {
            return !watch.IsRunning;
        }

        public TimeSpan ElapsedTime()
        {
            return watch.Elapsed;
        }

        public int Collected()
        {
            return collected.Count;
        }
    }
}
