using LCH_COMMON;

namespace LCH_RTS.Contents
{
    public enum ERoomState
    {
        Waiting,
        PreStart,
        Start,
        End,
        Max
    }

    public abstract class GameRoomState
    {
        protected GameRoomStatus? StateController { get; private set; }
        protected GameRoom? GameRoom { get; private set; }

        public void Initialize(GameRoomStatus stateController, GameRoom gameRoom)
        {
            StateController = stateController;
            GameRoom = gameRoom;
        }

        public abstract void Enter();
        public abstract void Update();
        public abstract void Exit();
        public abstract ERoomState GetStateType();
        
        public virtual ERoomState? Next()
        {
            return null;
        }
    }

    public class WaitingState : GameRoomState
    {
        public override void Enter()
        {
        }

        public override void Update()
        {
        }

        public override void Exit()
        {
        }

        public override ERoomState GetStateType() => ERoomState.Waiting;

        public override ERoomState? Next()
        {
            return ERoomState.PreStart;
        }
    }

    public class PreStartState : GameRoomState
    {
        private long _gameStartTime;

        public override void Enter()
        {
            _gameStartTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds() + 2;
            Logger.Log(ELogType.Console, ELogLevel.Info, $"[GameRoom {GameRoom!.RoomId}] Entered PreStart state. Game will start at {_gameStartTime}");
        }

        public override void Update()
        {
            if (StateController is null)
                return;
            
            var utcNow = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            if (utcNow <= _gameStartTime) 
                return;
            StateController.ChangeState();
            Logger.Log(ELogType.Console, ELogLevel.Info, $"Game Start roomId={GameRoom!.RoomId}");
        }

        public override void Exit()
        {
            Logger.Log(ELogType.Console, ELogLevel.Info, $"[GameRoom {GameRoom!.RoomId}] Exiting PreStart state");
        }

        public override ERoomState GetStateType() => ERoomState.PreStart;

        public override ERoomState? Next()
        {
            return ERoomState.Start;
        }
    }

    public class StartState : GameRoomState
    {
        public override void Enter()
        {
            Logger.Log(ELogType.Console, ELogLevel.Info, $"[GameRoom {GameRoom!.RoomId}] Entered Start state - Game Started!");
            GameRoom.Broadcast(PacketUtil.SC_START_GAME_PACKET());
            GameRoom.GameStart();
        }

        public override void Update()
        {
            if (GameRoom is null)
                return;
            
            GameRoom.UpdatePlayersCost();
            GameRoom.UpdateUnits();
            GameRoom.UpdateTowers();
        }

        public override void Exit()
        {
            Logger.Log(ELogType.Console, ELogLevel.Info, $"[GameRoom {GameRoom!.RoomId}] Exiting Start state");
        }

        public override ERoomState GetStateType() => ERoomState.Start;

        public override ERoomState? Next()
        {
            return ERoomState.End;
        }
    }

    public class EndState : GameRoomState
    {
        public override void Enter()
        {
            Logger.Log(ELogType.Console, ELogLevel.Info, $"[GameRoom {GameRoom!.RoomId}] Entered End state - Game Ended!");
        }

        public override void Update()
        {
        }

        public override void Exit()
        {
            Logger.Log(ELogType.Console, ELogLevel.Info, $"[GameRoom {GameRoom!.RoomId}] Exiting End state");
        }

        public override ERoomState GetStateType() => ERoomState.End;

        public override ERoomState? Next()
        {
            return null;
        }
    }

    public class GameRoomStatus
    {
        private readonly Dictionary<ERoomState, GameRoomState> _states;
        private GameRoomState _currentState;
        private GameRoom _gameRoom;

        public ERoomState CurrentStateType => _currentState?.GetStateType() ?? ERoomState.Waiting;

        public GameRoomStatus(GameRoom gameRoom)
        {
            _gameRoom = gameRoom;
            _states = new Dictionary<ERoomState, GameRoomState>
            {
                { ERoomState.Waiting, new WaitingState() },
                { ERoomState.PreStart, new PreStartState() },
                { ERoomState.Start, new StartState() },
                { ERoomState.End, new EndState() }
            };

            foreach (var state in _states.Values)
            {
                state.Initialize(this, _gameRoom);
            }

            _currentState = _states[ERoomState.Waiting];
            _currentState.Enter();
        }

        public void ChangeState(ERoomState newState)
        {
            if (newState == CurrentStateType)
                return;

            if (!_states.TryGetValue(newState, out var nextState))
            {
                Logger.Log(ELogType.Console, ELogLevel.Error, $"[ERROR] Invalid state transition to {newState}");
                return;
            }
            
            _currentState.Exit();
            _currentState = nextState;
            _currentState.Enter();
        }

        public void ChangeState()
        {
            var nextState = _currentState?.Next();
            if (nextState.HasValue)
            {
                ChangeState(nextState.Value);
            }
        }

        public void Update()
        {
            _currentState?.Update();
        }

        public bool IsInState(ERoomState state)
        {
            return CurrentStateType == state;
        }

        public ERoomState GetRoomStateEnum() => _currentState.GetStateType();
    }
}
