using System;
using System.Collections.Generic;
using LCH_RTS_MATCHING;
using Xunit;

namespace LCH_RTS_TEST
{
    public readonly record struct TestMatcherInfo(long playerId, int mmr, long matchReqMsec = 0)
    {
        public long PlayerId { get; } = playerId;
        public int Mmr { get; } = mmr;
        public long MatchReqMsec { get; } = matchReqMsec;
    }

    public class TestMatchManager
    {
        private readonly List<TestMatcherInfo> _matchQueue = new();
        public long CurrentTimeMsec { get; set; }

        public int GetQueueCount()
        {
            return _matchQueue.Count;
        }

        public void Enqueue(TestMatcherInfo info)
        {
            _matchQueue.Add(info);
        }

        public List<(TestMatcherInfo First, TestMatcherInfo Second)> ProcessMatching()
        {
            if (_matchQueue.Count < 2)
            {
                return [];
            }

            var resultList = new List<(TestMatcherInfo First, TestMatcherInfo Second)>();
            var matchedCache = new bool[_matchQueue.Count];
            var needToRemoveIndexes = new List<int>();

            for (var i = 0; i < _matchQueue.Count; i++)
            {
                if (matchedCache[i])
                {
                    continue;
                }

                var firstPlayer = _matchQueue[i];
                var nowMsec = CurrentTimeMsec == 0 ? firstPlayer.MatchReqMsec : CurrentTimeMsec;

                for (var j = i + 1; j < _matchQueue.Count; j++)
                {
                    if (matchedCache[j])
                    {
                        continue;
                    }

                    var candidate = _matchQueue[j];
                    if (Math.Abs(firstPlayer.Mmr - candidate.Mmr) > CalcMatchWindow(nowMsec, firstPlayer.MatchReqMsec, candidate.MatchReqMsec))
                    {
                        continue;
                    }

                    matchedCache[i] = true;
                    matchedCache[j] = true;
                    needToRemoveIndexes.Add(i);
                    needToRemoveIndexes.Add(j);
                    resultList.Add((firstPlayer, candidate));
                    break;
                }
            }

            if (needToRemoveIndexes.Count == 0)
            {
                return resultList;
            }

            needToRemoveIndexes.Sort((a, b) => b.CompareTo(a));
            foreach (var index in needToRemoveIndexes)
            {
                _matchQueue.RemoveAt(index);
            }

            return resultList;
        }

        private static long CalcMatchWindow(long nowMsec, long msec1, long msec2)
        {
            const int matchWindow = 100;
            const int oneSecInMs = 1000;
            const int matchWindowInc = 100;
            var minMsec = Math.Min(msec1, msec2);
            var matchThreshold = Math.Abs(nowMsec - minMsec);
            return matchWindow + matchThreshold / oneSecInMs * matchWindowInc;
        }
    }

    public class MatchManagerTests
    {
        [Fact]
        public void _001_ProcessMatching_WithOnePlayer_ReturnsEmptyList()
        {
            var manager = new TestMatchManager();
            manager.Enqueue(new TestMatcherInfo(1, 1000));
            manager.CurrentTimeMsec = 0;

            var result = manager.ProcessMatching();

            Assert.Empty(result);
        }

        [Fact]
        public void _002_ProcessMatching_WithTwoPlayersInRange_CreatesMatch()
        {
            var manager = new TestMatchManager();
            var player1 = new TestMatcherInfo(1, 1000);
            var player2 = new TestMatcherInfo(2, 1050);

            manager.Enqueue(player1);
            manager.Enqueue(player2);
            manager.CurrentTimeMsec = 0;

            var result = manager.ProcessMatching();

            Assert.Single(result);
            Assert.Equal(player1.PlayerId, result[0].First.PlayerId);
            Assert.Equal(player2.PlayerId, result[0].Second.PlayerId);
            Assert.Equal(0, manager.GetQueueCount());
        }

        [Fact]
        public void _003_ProcessMatching_WithTwoPlayersOutOfRange_ReturnsEmptyList()
        {
            var manager = new TestMatchManager();
            var player1 = new TestMatcherInfo(1, 1000);
            var player2 = new TestMatcherInfo(2, 1200);

            manager.Enqueue(player1);
            manager.Enqueue(player2);
            manager.CurrentTimeMsec = 0;

            var result = manager.ProcessMatching();

            Assert.Empty(result);
            Assert.Equal(2, manager.GetQueueCount());
        }

        [Fact]
        public void _004_ProcessMatching_WithWaitTime_AllowsBroaderWindow()
        {
            var manager = new TestMatchManager();
            var player1 = new TestMatcherInfo(1, 1000);
            var player2 = new TestMatcherInfo(2, 1200);

            manager.Enqueue(player1);
            manager.Enqueue(player2);

            manager.CurrentTimeMsec = 0;
            var firstAttempt = manager.ProcessMatching();
            Assert.Empty(firstAttempt);
            Assert.Equal(2, manager.GetQueueCount());

            manager.CurrentTimeMsec = 2_000;
            var secondAttempt = manager.ProcessMatching();

            Assert.Single(secondAttempt);
            Assert.Equal(player1.PlayerId, secondAttempt[0].First.PlayerId);
            Assert.Equal(player2.PlayerId, secondAttempt[0].Second.PlayerId);
            Assert.Equal(0, manager.GetQueueCount());
        }

        [Fact]
        public void _005_ProcessMatching_WithThreePlayersFirstTwoMatch_LeavesOneInQueue()
        {
            var manager = new TestMatchManager();
            var player1 = new TestMatcherInfo(1, 1000);
            var player2 = new TestMatcherInfo(2, 1050);
            var player3 = new TestMatcherInfo(3, 1500);

            manager.Enqueue(player1);
            manager.Enqueue(player2);
            manager.Enqueue(player3);
            manager.CurrentTimeMsec = 0;

            var result = manager.ProcessMatching();

            Assert.Single(result);
            Assert.Equal(player1.PlayerId, result[0].First.PlayerId);
            Assert.Equal(player2.PlayerId, result[0].Second.PlayerId);
            Assert.Equal(1, manager.GetQueueCount());
        }

        [Fact]
        public void _006_ProcessMatching_WithThreePlayersFirstAndThirdMatch_LeavesMiddlePlayer()
        {
            var manager = new TestMatchManager();
            var player1 = new TestMatcherInfo(1, 1000);
            var player2 = new TestMatcherInfo(2, 1200);
            var player3 = new TestMatcherInfo(3, 1050);

            manager.Enqueue(player1);
            manager.Enqueue(player2);
            manager.Enqueue(player3);
            manager.CurrentTimeMsec = 0;

            var result = manager.ProcessMatching();

            Assert.Single(result);
            Assert.Equal(player1.PlayerId, result[0].First.PlayerId);
            Assert.Equal(player3.PlayerId, result[0].Second.PlayerId);
            Assert.Equal(1, manager.GetQueueCount());
        }

        [Fact]
        public void _007_ProcessMatching_WithBoundaryDifference_ReturnsMatch()
        {
            var manager = new TestMatchManager();
            var player1 = new TestMatcherInfo(1, 1000);
            var player2 = new TestMatcherInfo(2, 1100);

            manager.Enqueue(player1);
            manager.Enqueue(player2);
            manager.CurrentTimeMsec = 0;

            var result = manager.ProcessMatching();

            Assert.Single(result);
            Assert.Equal(player1.PlayerId, result[0].First.PlayerId);
            Assert.Equal(player2.PlayerId, result[0].Second.PlayerId);
        }

        [Fact]
        public void _008_ProcessMatching_WithDifferenceJustOverBoundary_ReturnsEmptyList()
        {
            var manager = new TestMatchManager();
            var player1 = new TestMatcherInfo(1, 1000);
            var player2 = new TestMatcherInfo(2, 1101);

            manager.Enqueue(player1);
            manager.Enqueue(player2);
            manager.CurrentTimeMsec = 0;

            var result = manager.ProcessMatching();

            Assert.Empty(result);
            Assert.Equal(2, manager.GetQueueCount());
        }

        [Fact]
        public void _009_ProcessMatching_QueueCountAfterSuccessfulMatch_DecreasesBy2()
        {
            var manager = new TestMatchManager();
            var player1 = new TestMatcherInfo(1, 1000);
            var player2 = new TestMatcherInfo(2, 1050);
            var player3 = new TestMatcherInfo(3, 1500);

            manager.Enqueue(player1);
            manager.Enqueue(player2);
            manager.Enqueue(player3);
            manager.CurrentTimeMsec = 0;

            var result = manager.ProcessMatching();

            Assert.Single(result);
            Assert.Equal(1, manager.GetQueueCount());
        }

        [Fact]
        public void _010_ProcessMatching_QueueCountAfterFailedMatch_RemainsUnchanged()
        {
            var manager = new TestMatchManager();
            var player1 = new TestMatcherInfo(1, 1000);
            var player2 = new TestMatcherInfo(2, 1200);

            manager.Enqueue(player1);
            manager.Enqueue(player2);
            manager.CurrentTimeMsec = 0;

            var result = manager.ProcessMatching();

            Assert.Empty(result);
            Assert.Equal(2, manager.GetQueueCount());
        }

        [Fact]
        public void _011_ProcessMatching_ManyPlayers_GenerateMultipleMatches()
        {
            var manager = new TestMatchManager();
            var player1 = new TestMatcherInfo(1, 1000);
            var player2 = new TestMatcherInfo(2, 1050);
            var player3 = new TestMatcherInfo(3, 1100);
            var player4 = new TestMatcherInfo(4, 1150);

            manager.Enqueue(player1);
            manager.Enqueue(player2);
            manager.Enqueue(player3);
            manager.Enqueue(player4);
            manager.CurrentTimeMsec = 0;

            var result = manager.ProcessMatching();

            Assert.Equal(2, result.Count);
            Assert.Equal(player1.PlayerId, result[0].First.PlayerId);
            Assert.Equal(player2.PlayerId, result[0].Second.PlayerId);
            Assert.Equal(player3.PlayerId, result[1].First.PlayerId);
            Assert.Equal(player4.PlayerId, result[1].Second.PlayerId);
            Assert.Equal(0, manager.GetQueueCount());
        }

        [Fact]
        public void _012_ProcessMatching_MatchWindowExpandsWithElapsedTime()
        {
            var manager = new TestMatchManager();
            var player1 = new TestMatcherInfo(1, 1000);
            var player2 = new TestMatcherInfo(2, 1200);

            manager.Enqueue(player1);
            manager.Enqueue(player2);

            manager.CurrentTimeMsec = 0;
            var firstAttempt = manager.ProcessMatching();
            Assert.Empty(firstAttempt);
            Assert.Equal(2, manager.GetQueueCount());

            manager.CurrentTimeMsec = 999;
            var almostThreshold = manager.ProcessMatching();
            Assert.Empty(almostThreshold);
            Assert.Equal(2, manager.GetQueueCount());

            manager.CurrentTimeMsec = 1_000;
            var thresholdMatch = manager.ProcessMatching();

            Assert.Single(thresholdMatch);
            Assert.Equal(0, manager.GetQueueCount());
        }

        [Fact]
        public void _013_PlayerIdGen_Success()
        {
            var id1 = PlayerIdGenerator.Instance.NextId();
            var id2 = PlayerIdGenerator.Instance.NextId();

            Assert.NotEqual(id1, id2);
            Assert.Equal(id1 + 1, id2);
        }
    }
}
