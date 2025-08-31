using System;
using System.Collections.Generic;
using System.Reflection;
using LCH_RTS_MATCHING;
using LCH_RTS_MATCHING.MatchMake;
using Xunit;

namespace LCH_RTS_TEST
{
    public readonly struct TestMatcherInfo(long playerId, int mmr)
    {
        public long PlayerId { get; } = playerId;
        public int Mmr { get; } = mmr;
    }

    public class TestMatchManager
    {
        private readonly Queue<TestMatcherInfo> _matchQueue = new();
        private int _matchWindow = 100;
        private long _lastMatchTriedSecond = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

        public void Enqueue(TestMatcherInfo info)
        {
            _matchQueue.Enqueue(info);
        }

        public int GetQueueCount()
        {
            return _matchQueue.Count;
        }

        public List<(TestMatcherInfo, TestMatcherInfo)>? ProcessMatching()
        {
            var results = new List<(TestMatcherInfo, TestMatcherInfo)>();
            var initialWindow = _matchWindow;
            var currentWindow = _matchWindow;

            while (_matchQueue.Count >= 2)
            {
                // 큐에 2명 이상의 플레이어가 있을 때만 매칭 시도
                var player1 = _matchQueue.Dequeue();

                // 현재 윈도우 내에서 매칭 가능한 두 번째 플레이어를 찾음
                TestMatcherInfo? matchedPlayer = null;
                var tempQueue = new Queue<TestMatcherInfo>();

                while (_matchQueue.Count > 0)
                {
                    var player2 = _matchQueue.Dequeue();

                    // MMR 차이가 현재 윈도우 이내인지 확인
                    if (Math.Abs(player1.Mmr - player2.Mmr) <= currentWindow)
                    {
                        matchedPlayer = player2;
                        break;
                    }
                    else
                    {
                        // 매칭되지 않은 플레이어는 임시 큐에 저장
                        tempQueue.Enqueue(player2);
                    }
                }

                // 매칭된 플레이어가 있으면 결과에 추가
                if (matchedPlayer.HasValue)
                {
                    // 임시 큐에 저장된 나머지 플레이어들을 원래 큐로 복사
                    while (tempQueue.Count > 0)
                    {
                        _matchQueue.Enqueue(tempQueue.Dequeue());
                    }

                    // 매칭 성공 시 윈도우를 초기값으로 리셋
                    currentWindow = initialWindow;

                    results.Add((player1, matchedPlayer.Value));
                }
                else
                {
                    // 매칭되지 않은 플레이어를 다시 큐에 추가
                    _matchQueue.Enqueue(player1);

                    // 임시 큐에 저장된 나머지 플레이어들을 원래 큐로 복사
                    while (tempQueue.Count > 0)
                    {
                        _matchQueue.Enqueue(tempQueue.Dequeue());
                    }

                    currentWindow += 100;
                    break;
                }
            }
            
            _matchWindow = currentWindow;
            _lastMatchTriedSecond = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            return results.Count > 0 ? results : null;
        }
    }

    public class MatchManagerTests
    {
        [Fact]
        public void ProcessMatching_WithEmptyQueue_ReturnsNull()
        {
            // Arrange
            var manager = new TestMatchManager();

            // Act
            var result = manager.ProcessMatching();

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void ProcessMatching_WithOnePlayer_ReturnsNull()
        {
            // Arrange
            var manager = new TestMatchManager();
            var player = new TestMatcherInfo(1, 1000);
            manager.Enqueue(player);

            // Act
            var result = manager.ProcessMatching();

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void ProcessMatching_WithTwoPlayersInRange_ReturnsMatch()
        {
            // Arrange
            var manager = new TestMatchManager();
            var player1 = new TestMatcherInfo(1, 1000);
            var player2 = new TestMatcherInfo(2, 1050);

            manager.Enqueue(player1);
            manager.Enqueue(player2);

            // Act
            var result = manager.ProcessMatching();

            // Assert
            Assert.NotNull(result);
            Assert.Single(result);
            Assert.Equal(player1.PlayerId, result[0].Item1.PlayerId);
            Assert.Equal(player2.PlayerId, result[0].Item2.PlayerId);
        }

        [Fact]
        public void ProcessMatching_WithTwoPlayersOutOfRange_ReturnsEmptyList()
        {
            // Arrange
            var manager = new TestMatchManager();
            var player1 = new TestMatcherInfo(1, 1000);
            var player2 = new TestMatcherInfo(2, 1200); // MMR difference: 200, outside default window of 100

            manager.Enqueue(player1);
            manager.Enqueue(player2);

            // Act
            var result = manager.ProcessMatching();

            // Assert
            Assert.NotNull(result);
            Assert.NotEmpty(result);
        }

        [Fact]
        public void ProcessMatching_WithThreePlayersFirstTwoMatch_ReturnsOneMatch()
        {
            // Arrange
            var manager = new TestMatchManager();
            var player1 = new TestMatcherInfo(1, 1000);
            var player2 = new TestMatcherInfo(2, 1050);
            var player3 = new TestMatcherInfo(3, 1500);

            manager.Enqueue(player1);
            manager.Enqueue(player2);
            manager.Enqueue(player3);

            // Act
            var result = manager.ProcessMatching();

            // Assert
            Assert.NotNull(result);
            Assert.Single(result);
            Assert.Equal(player1.PlayerId, result[0].Item1.PlayerId);
            Assert.Equal(player2.PlayerId, result[0].Item2.PlayerId);
        }

        [Fact]
        public void ProcessMatching_WithThreePlayersFirstAndThirdMatch_ReturnsOneMatch()
        {
            // Arrange
            var manager = new TestMatchManager();
            var player1 = new TestMatcherInfo(1, 1000);
            var player2 = new TestMatcherInfo(2, 1200);
            var player3 = new TestMatcherInfo(3, 1050);

            manager.Enqueue(player1);
            manager.Enqueue(player2);
            manager.Enqueue(player3);

            // Act
            var result = manager.ProcessMatching();

            // Assert
            Assert.NotNull(result);
            Assert.Single(result);
            Assert.Equal(player1.PlayerId, result[0].Item1.PlayerId);
            Assert.Equal(player3.PlayerId, result[0].Item2.PlayerId);
        }

        [Fact]
        public void ProcessMatching_WithExactMMRMatch_ReturnsMatch()
        {
            // Arrange
            var manager = new TestMatchManager();
            var player1 = new TestMatcherInfo(1, 1000);
            var player2 = new TestMatcherInfo(2, 1000); // Exact MMR match

            manager.Enqueue(player1);
            manager.Enqueue(player2);

            // Act
            var result = manager.ProcessMatching();

            // Assert
            Assert.NotNull(result);
            Assert.Single(result);
            Assert.Equal(player1.PlayerId, result[0].Item1.PlayerId);
            Assert.Equal(player2.PlayerId, result[0].Item2.PlayerId);
        }

        [Fact]
        public void ProcessMatching_WithBoundaryMMRDifference_ReturnsMatch()
        {
            // Arrange
            var manager = new TestMatchManager();
            var player1 = new TestMatcherInfo(1, 1000);
            var player2 = new TestMatcherInfo(2, 1100); // Exactly 100 MMR difference (boundary case)

            manager.Enqueue(player1);
            manager.Enqueue(player2);

            // Act
            var result = manager.ProcessMatching();

            // Assert
            Assert.NotNull(result);
            Assert.Single(result);
            Assert.Equal(player1.PlayerId, result[0].Item1.PlayerId);
            Assert.Equal(player2.PlayerId, result[0].Item2.PlayerId);
        }

        //매칭윈도우 오버여도 다음 틱에 돌아야 함. 이 케이스 오류
        [Fact]
        public void ProcessMatching_WithMMRDifferenceJustOverBoundary_ReturnsEmptyList()
        {
            // Arrange
            var manager = new TestMatchManager();
            var player1 = new TestMatcherInfo(1, 1000);
            var player2 = new TestMatcherInfo(2, 1101); // 101 MMR difference (just over boundary)

            manager.Enqueue(player1);
            manager.Enqueue(player2);

            // Act
            var result = manager.ProcessMatching();

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Fact]
        public void ProcessMatching_QueueCountAfterSuccessfulMatch_DecreasesBy2()
        {
            // Arrange
            var manager = new TestMatchManager();
            var player1 = new TestMatcherInfo(1, 1000);
            var player2 = new TestMatcherInfo(2, 1050);
            var player3 = new TestMatcherInfo(3, 1500);

            manager.Enqueue(player1);
            manager.Enqueue(player2);
            manager.Enqueue(player3);

            Assert.Equal(3, manager.GetQueueCount());

            // Act
            var result = manager.ProcessMatching();

            // Assert
            Assert.NotNull(result);
            Assert.Single(result);
            Assert.Equal(1, manager.GetQueueCount()); // Only player3 should remain
        }

        [Fact]
        public void ProcessMatching_QueueCountAfterFailedMatch_RemainsUnchanged()
        {
            // Arrange
            var manager = new TestMatchManager();
            var player1 = new TestMatcherInfo(1, 1000);
            var player2 = new TestMatcherInfo(2, 1200); // Too far apart

            manager.Enqueue(player1);
            manager.Enqueue(player2);

            Assert.Equal(2, manager.GetQueueCount());

            // Act
            var result = manager.ProcessMatching();

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
            Assert.Equal(2, manager.GetQueueCount()); // Both players should remain
        }

        [Fact]
        public void ProcessMatching_ManyPlayers_Matching()
        {
            // Arrange
            var manager = new TestMatchManager();
            var player1 = new TestMatcherInfo(1, 1000);
            var player2 = new TestMatcherInfo(2, 1050);
            var player3 = new TestMatcherInfo(3, 1100);
            var player4 = new TestMatcherInfo(4, 1150);

            manager.Enqueue(player1);
            manager.Enqueue(player2);
            manager.Enqueue(player3);
            manager.Enqueue(player4);

            // Act
            var result = manager.ProcessMatching();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(player1.PlayerId, result[0].Item1.PlayerId);
            Assert.Equal(player2.PlayerId, result[0].Item2.PlayerId);

            Assert.Equal(player3.PlayerId, result[1].Item1.PlayerId);
            Assert.Equal(player4.PlayerId, result[1].Item2.PlayerId);
        }

        [Fact]
        public void ProcessMatching_WindowExpansion_AfterFailedMatch()
        {
            // Arrange
            var manager = new TestMatchManager();
            var player1 = new TestMatcherInfo(1, 1000);
            var player2 = new TestMatcherInfo(2, 1150); // MMR 차이 150 (초기 윈도우 100 초과)

            manager.Enqueue(player1);
            manager.Enqueue(player2);

            _ = manager.ProcessMatching();

            // Act - 두 번째 매칭 시도 (윈도우가 200으로 확장되어 성공 예상)
            var result = manager.ProcessMatching();

            // Assert - 두 번째 시도는 성공 (윈도우가 확장되어 MMR 차이 150이 허용됨)
            Assert.NotNull(result);
            Assert.Single(result);
            Assert.Equal(player1.PlayerId, result[0].Item1.PlayerId);
            Assert.Equal(player2.PlayerId, result[0].Item2.PlayerId);
            Assert.Equal(0, manager.GetQueueCount());
        }

        [Fact]
        public void ProcessMatching_WindowReset_AfterSuccessfulMatch()
        {
            // Arrange
            var manager = new TestMatchManager();
            var player1 = new TestMatcherInfo(1, 1100);
            var player2 = new TestMatcherInfo(2, 1050);
            var player3 = new TestMatcherInfo(3, 1000);
            var player4 = new TestMatcherInfo(4, 1150);

            manager.Enqueue(player1);
            manager.Enqueue(player2);
            manager.Enqueue(player3);
            manager.Enqueue(player4);

            // Act
            _ = manager.ProcessMatching();
            var result = manager.ProcessMatching();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(player3.PlayerId, result[0].Item1.PlayerId);
            Assert.Equal(player4.PlayerId, result[0].Item2.PlayerId);
        }

        [Fact]
        public void PlayerIdGen_Success()
        {
            //Arange
            var id1 = PlayerIdGenerator.Instance.NextId();
            var id2 = PlayerIdGenerator.Instance.NextId();
            
            //Act
            
            //Assert
            Assert.NotEqual(id1, id2);
            Assert.Equal(id1 + 1, id2);
        }
    }
}