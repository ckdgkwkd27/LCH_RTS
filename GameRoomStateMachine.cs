namespace LCH_RTS.Contents
{
    public enum ERoomState
    {
        Waiting,
        PreStart,
        Start,
        End,
        Reset,
        Max
    }

    // Base state class
    public abstract class GameRoomState
    {
        protected GameRoomStateMachine StateMachine { get; private set; }
        protected GameRoom GameRoom { get; private set; }

        public virtual void Initialize(GameRoomStateMachine stateMachine, GameRoom gameRoom)
        {
            StateMachine = stateMachine;
            GameRoom = gameRoom;
        }

        public abstract void Enter();
        public abstract void Update();
        public abstract void Exit();
        public abstract ERoomState GetStateType();
        
        // Common Next() method for state transitions
        public virtual ERoomState? Next()
        {
            return null; // Default: no automatic transition
        }
    }

    public class WaitingState : GameRoomState
    {
        public override void Enter()
        {
            Console.WriteLine($"[GameRoom {GameRoom.RoomId}] Entered Waiting state");
        }

        public override void Update()
        {
            // No specific update logic for waiting state
        }

        public override void Exit()
        {
            Console.WriteLine($"[GameRoom {GameRoom.RoomId}] Exiting Waiting state");
        }

        public override ERoomState GetStateType() => ERoomState.Waiting;

        public override ERoomState? Next()
        {
            // Transition to PreStart when game is ready to start
            // This will be called from GameRoom.GameReady()
            return ERoomState.PreStart;
        }
    }

    public class PreStartState : GameRoomState
    {
        private long _gameStartTime;

        public override void Enter()
        {
            _gameStartTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds() + 3;
            Console.WriteLine($"[GameRoom {GameRoom.RoomId}] Entered PreStart state. Game will start at {_gameStartTime}");
        }

        public override void Update()
        {
            var utcNow = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            if (utcNow > _gameStartTime)
            {
                StateMachine.TransitionToNext();
            }
        }

        public override void Exit()
        {
            Console.WriteLine($"[GameRoom {GameRoom.RoomId}] Exiting PreStart state");
        }

        public override ERoomState GetStateType() => ERoomState.PreStart;

        public override ERoomState? Next()
        {
            // Automatically transition to Start when countdown is over
            return ERoomState.Start;
        }
    }

    public class StartState : GameRoomState
    {
        public override void Enter()
        {
            Console.WriteLine($"[GameRoom {GameRoom.RoomId}] Entered Start state - Game Started!");
            GameRoom.InitializeGameStart();
        }

        public override void Update()
        {
            // Game update logic is handled in GameRoom.Update()
            // This state just represents that the game is running
        }

        public override void Exit()
        {
            Console.WriteLine($"[GameRoom {GameRoom.RoomId}] Exiting Start state");
        }

        public override ERoomState GetStateType() => ERoomState.Start;

        public override ERoomState? Next()
        {
            // Transition to End when game finishes
            // This will be called when game end conditions are met
            return ERoomState.End;
        }
    }

    public class EndState : GameRoomState
    {
        public override void Enter()
        {
            Console.WriteLine($"[GameRoom {GameRoom.RoomId}] Entered End state - Game Ended!");
        }

        public override void Update()
        {
            // No updates when game is ended
        }

        public override void Exit()
        {
            Console.WriteLine($"[GameRoom {GameRoom.RoomId}] Exiting End state");
        }

        public override ERoomState GetStateType() => ERoomState.End;

        public override ERoomState? Next()
        {
            // Transition to Reset to clean up for next game
            return ERoomState.Reset;
        }
    }

    public class ResetState : GameRoomState
    {
        public override void Enter()
        {
            Console.WriteLine($"[GameRoom {GameRoom.RoomId}] Entered Reset state");
            GameRoom.ResetRoom();
        }

        public override void Update()
        {
            // After reset, go back to waiting
            StateMachine.TransitionToNext();
        }

        public override void Exit()
        {
            Console.WriteLine($"[GameRoom {GameRoom.RoomId}] Exiting Reset state");
        }

        public override ERoomState GetStateType() => ERoomState.Reset;

        public override ERoomState? Next()
        {
            // After reset, go back to waiting for new players
            return ERoomState.Waiting;
        }
    }

    public class GameRoomStateMachine
    {
        private readonly Dictionary<ERoomState, GameRoomState> _states;
        private GameRoomState _currentState;
        private GameRoom _gameRoom;

        public ERoomState CurrentStateType => _currentState?.GetStateType() ?? ERoomState.Waiting;

        public GameRoomStateMachine(GameRoom gameRoom)
        {
            _gameRoom = gameRoom;
            _states = new Dictionary<ERoomState, GameRoomState>
            {
                { ERoomState.Waiting, new WaitingState() },
                { ERoomState.PreStart, new PreStartState() },
                { ERoomState.Start, new StartState() },
                { ERoomState.End, new EndState() },
                { ERoomState.Reset, new ResetState() }
            };

            // Initialize all states
            foreach (var state in _states.Values)
            {
                state.Initialize(this, _gameRoom);
            }

            // Start in waiting state
            _currentState = _states[ERoomState.Waiting];
            _currentState.Enter();
        }

        public void ChangeState(ERoomState newState)
        {
            if (newState == CurrentStateType)
                return;

            if (!_states.TryGetValue(newState, out var nextState))
            {
                Console.WriteLine($"[ERROR] Invalid state transition to {newState}");
                return;
            }

            Console.WriteLine($"[GameRoom {_gameRoom.RoomId}] State transition: {CurrentStateType} -> {newState}");
            
            _currentState?.Exit();
            _currentState = nextState;
            _currentState.Enter();
        }

        // New method for transitioning using Next()
        public void TransitionToNext()
        {
            var nextState = _currentState?.Next();
            if (nextState.HasValue)
            {
                ChangeState(nextState.Value);
            }
        }

        public void ChangeState()
        {
            TransitionToNext();
        }

        public void Update()
        {
            _currentState?.Update();
        }

        public bool IsInState(ERoomState state)
        {
            return CurrentStateType == state;
        }

        public bool CanTransitionTo(ERoomState targetState)
        {
            // Define valid state transitions
            return CurrentStateType switch
            {
                ERoomState.Waiting => targetState == ERoomState.PreStart || targetState == ERoomState.Reset,
                ERoomState.PreStart => targetState == ERoomState.Start || targetState == ERoomState.End,
                ERoomState.Start => targetState == ERoomState.End,
                ERoomState.End => targetState == ERoomState.Reset,
                ERoomState.Reset => targetState == ERoomState.Waiting,
                _ => false
            };
        }
    }
}
