﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pacman.GameEngine
{
    public enum Behaviour { Patrol, Chase, Frightened, Return }

    public abstract class Ghost : GameCharacter
    {
        #region Fields

        private const int RUN_PATH_LENGTH = 5;
        protected const int CHASE_PATH_LENGTH = 3;

        private const float CHASE_TIME = 20.0f;
        private const float PATROL_TIME = 7.0f;
        private const float RETURN_TIME = 7.0f;

        private float _patrolTime;
        private float _chaseTime;
        private float _returnTime;

        private List<Cell> _patrolPath;
        private List<Cell> _runPath;
        protected List<Cell> _chasePath;
        private List<Cell> _returnPath;

        protected int _pathIterator;
        private Cell _homeCell;
        private Cell _startCell;
        protected Cell _targetCell;
        protected int _distance;

        private Behaviour _behaviour;

        private bool _isChanging;

        protected string _name;

        protected Player _pacman;

        protected IPathfindingAlgorithm _algorithm;

        #endregion

        #region Properties

        public float PatrolTime
        {
            get
            {
                return _patrolTime;
            }
            set
            {
                _patrolTime = value;
            }
        }

        public float ChaseTime
        {
            get
            {
                return _chaseTime;
            }
            set
            {
                _chaseTime = value;
            }
        }

        public float ReturnTime
        {
            get
            {
                return _returnTime;
            }
            set
            {
                _returnTime = value;
            }
        }

        public List<Cell> PatrolPath
        {
            get
            {
                return _patrolPath;
            }
            set
            {
                _patrolPath = value;
            }
        }

        public List<Cell> RunPath
        {
            get
            {
                return _runPath;
            }
            set
            {
                _runPath = value;
            }
        }

        public List<Cell> ChasePath
        {
            get
            {
                return _chasePath;
            }
            set
            {
                _chasePath = value;
            }
        }

        public List<Cell> ReturnPath
        {
            get
            {
                return _returnPath;
            }
            set
            {
                _returnPath = value;
            }
        }

        public int PathIterator
        {
            get
            {
                return _pathIterator;
            }
            set
            {
                _pathIterator = value;
            }
        }

        public Cell HomeCell
        {
            get
            {
                return _homeCell;
            }
            set
            {
                _homeCell = value;
            }
        }

        public Cell StartCell
        {
            get
            {
                return _startCell;
            }
            set
            {
                _startCell = value;
            }
        }

        public Cell TargetCell
        {
            get
            {
                return _targetCell;
            }
            set
            {
                _targetCell = value;
            }
        }

        public Behaviour Behaviour
        {
            get
            {
                return _behaviour;
            }
            set
            {
                _behaviour = value;
            }
        }

        public bool IsChanging
        {
            get
            {
                return _isChanging;
            }
            set
            {
                _isChanging = value;
            }
        }

        public string Name
        {
            get
            {
                return _name;
            }
        }

        public IPathfindingAlgorithm Algorithm
        {
            get
            {
                return _algorithm;
            }
        }

        #endregion

        #region Initialize

        public Ghost(Grid grid, float size)
            : base(grid, size)
        {
            _pathIterator = 0;
            _behaviour = Behaviour.Patrol;
            _patrolTime = PATROL_TIME;
            _chaseTime = CHASE_TIME;
            _returnTime = RETURN_TIME;
            _chasePath = new List<Cell>();

            _algorithm = new AStarAlgorithm();
        }

        public Ghost(Grid grid, int x, int y, float size)
            : this(grid, size)
        {
            _otherSpeed = _fullSpeed / 2.0f;
            SetX(x);
            SetY(y);
            _homeCell = _level.Map[x - 1, y - 1];
        }

        public Ghost(Player pacman, Grid grid, int x, int y, float size)
            : this(grid, x, y, size)
        {
            _pacman = pacman;
        }

        public abstract void InitializePatrolPath();

        #endregion

        #region Movement

        public void Patrol()
        {
            FollowPath(_patrolPath);

            if (_pathIterator == _patrolPath.Count - 1)
            {
                _patrolPath.Reverse();
                _pathIterator = 0;
            }
        }

        public void Run()
        {
            FollowPath(_runPath);
        }

        public void Chase()
        {
            FollowPath(_chasePath);
        }

        public void ReturnHome()
        {
            FollowPath(_returnPath);
        }

        private void FollowPath(List<Cell> path)
        {
            #region Validation

            if (path == null)
            {
                throw new ArgumentNullException();
            }

            #endregion

            if (_pathIterator < path.Count && _pathIterator > -1)
            {
                if (MoveTo(path.ElementAt(_pathIterator)))
                {
                    _pathIterator++;
                }
            }
        }

        private bool MoveTo(Cell cell)
        {
            #region Validation

            if (cell == null)
            {
                throw new ArgumentNullException();
            }

            #endregion

            TrySlowerSpeed();
            UpdateDirection(cell);

            if (GetBoundingRect() != cell.GetBoundingRect())
            {
                this.Move();
                return false;
            }
            else
            {
                _direction = Direction.None;
                _speed = _fullSpeed;
                return true;
            }
        }

        private void TrySlowerSpeed()
        {
            if (_behaviour == Behaviour.Frightened)
            {
                _speed = _otherSpeed;
            }
        }

        #endregion

        #region Movement constraints

        public bool IsUpAvailable()
        {
            return _pacman.GetY() - _distance > 0;
        }

        public bool IsDownAvailable()
        {
            return _pacman.GetY() + _distance < _level.Height - 1;
        }

        public bool IsLeftAvailable()
        {
            return _pacman.GetX() - _distance > 0;
        }

        public bool IsRightAvailable()
        {
            return _pacman.GetX() + _distance < _level.Width - 1;
        }

        #endregion

        #region Update direction

        public void UpdateDirection(Cell other)
        {
            #region Validation

            if (other == null)
            {
                throw new ArgumentNullException();
            }

            #endregion

            TryDirectionUp(other);

            TryDirectionDown(other);

            TryDirectionLeft(other);

            TryDirectionRight(other);
        }

        private void TryDirectionUp(Cell cell)
        {
            if (cell.GetBoundingRect().Top < this.GetBoundingRect().Top)
            {
                _direction = Direction.Up;
            }
        }

        private void TryDirectionDown(Cell cell)
        {
            if (cell.GetBoundingRect().Top > this.GetBoundingRect().Top)
            {
                _direction = Direction.Down;
            }
        }

        private void TryDirectionLeft(Cell cell)
        {
            if (cell.GetBoundingRect().Left < this.GetBoundingRect().Left)
            {
                _direction = Direction.Left;
            }
        }

        private void TryDirectionRight(Cell cell)
        {
            if (cell.GetBoundingRect().Left > this.GetBoundingRect().Left)
            {
                _direction = Direction.Right;
            }
        }

        #endregion

        #region Behaviour

        public void DoPatroling()
        {
            if (PatrolTime > 0)
            {
                Patrol();
            }
            else
            {
                _targetCell = CurrentCell();
                _chaseTime = CHASE_TIME;
                _behaviour = Behaviour.Chase;
            }
        }

        public void UpdateRunPath(Cell randomCell)
        {
            #region Validation

            if (randomCell == null)
            {
                throw new ArgumentNullException();
            }

            #endregion

            List<Cell> bestPath = _algorithm.CalculatePath(CurrentCell(), randomCell, _level.Map);
            PathIterator = 0;

            if (bestPath.Count >= RUN_PATH_LENGTH)
            {
                _runPath = bestPath.GetRange(0, RUN_PATH_LENGTH);
            }
            else
            {
                _runPath = bestPath;
            }

            _targetCell = _runPath.Last();
        }

        public virtual void UpdateChasePath()
        {
            List<Cell> bestPath = _algorithm.CalculatePath(CurrentCell(), CalculateTargetCell(), _level.Map);
            _pathIterator = 0;

            SelectChasePath(bestPath);

            _targetCell = _chasePath.Last();
        }

        public void DoChasing()
        {
            if (_chaseTime > 0)
            {
                Chase();
            }
            else
            {
                _returnPath = _algorithm.CalculatePath(CurrentCell(), _startCell, _level.Map);
                _behaviour = Behaviour.Return;
            }
        }

        public void DoReturning()
        {
            if (_returnTime > 0)
            {
                ReturnHome();
            }
            else
            {
                if (!IsAtStart())
                {
                    StartChase();
                }
            }

            if (IsAtStart())
            {
                StartPatrol();
            }
        }

        public virtual Cell CalculateTargetCell()
        {
            return _pacman.CurrentCell();
        }

        protected void SelectChasePath(List<Cell> path)
        {
            if (path.Count >= CHASE_PATH_LENGTH)
            {
                _chasePath = path.GetRange(0, CHASE_PATH_LENGTH);
            }
            else
            {
                _chasePath = path;
            }
        }

        private void StartChase()
        {
            _targetCell = CurrentCell();
            _chaseTime = CHASE_TIME;
            _behaviour = Behaviour.Chase;
        }

        private void StartPatrol()
        {
            _pathIterator = 0;
            _patrolTime = PATROL_TIME;
            _behaviour = Behaviour.Patrol;
        }

        private bool IsAtStart()
        {
            return CurrentCell().GetBoundingRect() == _startCell.GetBoundingRect();
        }

        protected Cell GetUpCell()
        {
            return _level.Map[_pacman.GetX(), _pacman.GetY() - _distance];
        }

        protected Cell GetDownCell()
        {
            return _level.Map[_pacman.GetX(), _pacman.GetY() + _distance];
        }

        protected Cell GetLeftCell()
        {
            return _level.Map[_pacman.GetX() - _distance, _pacman.GetY()];
        }

        protected Cell GetRightCell()
        {
            return _level.Map[_pacman.GetX() + _distance, _pacman.GetY()];
        }

        #endregion

        #region Object methods

        public override string ToString()
        {
            return _name;
        }

        #endregion
    }
}
